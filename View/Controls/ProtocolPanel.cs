using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Guna.UI2.WinForms;
using Guna.UI2.WinForms.Enums;

using VrdoPdg.Model.Protocol;
using VrdoPdg.View.Theme;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Панель «Протокол расчета»: записи протокола в реальном времени в виде дерева
    /// «КП → час → действия с СКРМ и результаты расчета УР» с фильтром по меткам.
    /// </summary>
    internal sealed class ProtocolPanel : UserControl
    {
        /// <summary>
        /// Высота строки протокола, пикселей.
        /// </summary>
        private const int RowHeight = 32;

        /// <summary>
        /// Отступ вложенного уровня, пикселей.
        /// </summary>
        private const int LevelIndent = 30;

        /// <summary>
        /// Интервал вывода новых записей, мс.
        /// </summary>
        private const int FlushInterval = 150;

        /// <summary>
        /// Записи, полученные от сервисов и еще не выведенные на панель.
        /// </summary>
        private readonly ConcurrentQueue<ProtocolEntry> _pending = new ConcurrentQueue<ProtocolEntry>();

        /// <summary>
        /// Узлы дерева по записям протокола.
        /// </summary>
        private readonly Dictionary<ProtocolEntry, TreeNode> _nodes = new Dictionary<ProtocolEntry, TreeNode>();

        /// <summary>
        /// Количество вложенных записей групп.
        /// </summary>
        private readonly Dictionary<ProtocolEntry, int> _descendantCounts = new Dictionary<ProtocolEntry, int>();

        /// <summary>
        /// Номера обработанных записей.
        /// </summary>
        private readonly HashSet<int> _processed = new HashSet<int>();

        /// <summary>
        /// Развернутые группы.
        /// </summary>
        private readonly HashSet<ProtocolEntry> _expanded = new HashSet<ProtocolEntry>();

        /// <summary>
        /// Отображаемые метки.
        /// </summary>
        private readonly HashSet<ProtocolTag> _enabledTags = new HashSet<ProtocolTag>(ProtocolTags.All);

        /// <summary>
        /// Переключатели меток на панели фильтров.
        /// </summary>
        private readonly Dictionary<ProtocolTag, Guna2Button> _tagButtons = new Dictionary<ProtocolTag, Guna2Button>();

        /// <summary>
        /// Таймер вывода новых записей.
        /// </summary>
        private readonly Timer _flushTimer;

        /// <summary>
        /// Рамка панели.
        /// </summary>
        private readonly Guna2Panel _frame;

        /// <summary>
        /// Кнопка «Развернуть всё» / «Свернуть всё».
        /// </summary>
        private readonly Guna2Button _expandButton;

        /// <summary>
        /// Кнопка «Все» с общим количеством записей.
        /// </summary>
        private readonly Guna2Button _allButton;

        /// <summary>
        /// Дерево записей.
        /// </summary>
        private readonly ProtocolTreeView _tree;

        /// <summary>
        /// Сообщение об отсутствии записей выбранного типа.
        /// </summary>
        private readonly Label _emptyLabel;

        /// <summary>
        /// Протокол расчета.
        /// </summary>
        private CalculationProtocol? _protocol;

        /// <summary>
        /// Признак режима «Развернуть всё».
        /// </summary>
        private bool _isExpandAllMode;

        /// <summary>
        /// Создает панель протокола.
        /// </summary>
        public ProtocolPanel()
        {
            BackColor = Palette.Background;
            DoubleBuffered = true;

            _frame = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 14,
                BorderColor = Palette.Border,
                BorderThickness = 1,
                FillColor = Palette.SurfaceSunken,
                Padding = new Padding(2),
                UseTransparentBackground = true
            };

            _tree = new ProtocolTreeView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Palette.SurfaceSunken,
                ForeColor = Palette.Text,
                DrawMode = TreeViewDrawMode.OwnerDrawAll,
                FullRowSelect = true,
                HideSelection = false,
                ItemHeight = RowHeight,
                Indent = LevelIndent,
                ShowLines = false,
                ShowPlusMinus = false,
                ShowRootLines = false,
                Font = UiFonts.Monospace(10f)
            };
            _tree.DrawNode += Tree_DrawNode;
            _tree.NodeMouseClick += Tree_NodeMouseClick;
            _tree.AfterExpand += Tree_AfterExpand;
            _tree.AfterCollapse += Tree_AfterCollapse;

            _emptyLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 48,
                Text = "Нет записей выбранного типа",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Palette.TextSecondary,
                BackColor = Palette.SurfaceSunken,
                Font = UiFonts.Regular(10f),
                Visible = false
            };

            Panel filterPanel = CreateFilterPanel(out _allButton);
            Panel header = CreateHeader(out _expandButton);

            _frame.Controls.Add(_tree);
            _frame.Controls.Add(_emptyLabel);
            _frame.Controls.Add(CreateLine());
            _frame.Controls.Add(filterPanel);
            _frame.Controls.Add(CreateLine());
            _frame.Controls.Add(header);
            Controls.Add(_frame);

            _flushTimer = new Timer { Interval = FlushInterval };
            _flushTimer.Tick += FlushTimer_Tick;
        }

        /// <summary>
        /// Подключает панель к протоколу расчета.
        /// </summary>
        /// <param name="protocol">Протокол расчета.</param>
        public void Attach(CalculationProtocol protocol)
        {
            Detach();
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _protocol.EntryAdded += Protocol_EntryAdded;
            Rebuild();
            _flushTimer.Start();
        }

        /// <summary>
        /// Освобождает ресурсы панели.
        /// </summary>
        /// <param name="disposing">Признак освобождения управляемых ресурсов.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Detach();
                _flushTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Создает горизонтальную линию-разделитель.
        /// </summary>
        /// <returns>Разделитель.</returns>
        private static Control CreateLine()
        {
            return new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Palette.Border
            };
        }

        /// <summary>
        /// Возвращает текст количества записей.
        /// </summary>
        /// <param name="count">Количество записей.</param>
        /// <returns>Строка вида «43 записи».</returns>
        private static string FormatRecordCount(int count)
        {
            return TextFormat.Plural(count, "запись", "записи", "записей");
        }

        /// <summary>
        /// Создает заголовок панели.
        /// </summary>
        /// <param name="expandButton">Кнопка «Развернуть всё».</param>
        /// <returns>Заголовок.</returns>
        private Panel CreateHeader(out Guna2Button expandButton)
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Palette.SurfaceSunken,
                Padding = new Padding(16, 0, 12, 0)
            };

            Label title = new Label
            {
                Dock = DockStyle.Left,
                AutoSize = false,
                Width = 320,
                Text = "›_  ПРОТОКОЛ РАСЧЁТА",
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Palette.TextSecondary,
                Font = UiFonts.Monospace(10.5f)
            };

            expandButton = new Guna2Button
            {
                Dock = DockStyle.Right,
                Width = 170,
                Text = "Развернуть всё"
            };
            UiStyle.ApplyFlat(expandButton, null);
            expandButton.Click += ExpandButton_Click;

            header.Controls.Add(expandButton);
            header.Controls.Add(title);
            return header;
        }

        /// <summary>
        /// Создает панель фильтров по меткам.
        /// </summary>
        /// <param name="allButton">Кнопка «Все».</param>
        /// <returns>Панель фильтров.</returns>
        private Panel CreateFilterPanel(out Guna2Button allButton)
        {
            FlowLayoutPanel panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BackColor = Palette.SurfaceSunken,
                Padding = new Padding(14, 10, 14, 8),
                WrapContents = false,
                AutoScroll = false
            };

            allButton = new Guna2Button
            {
                Size = new Size(96, 34),
                Margin = new Padding(0, 0, 10, 0),
                Text = "Все 0"
            };
            UiStyle.ApplyPill(allButton, Palette.Text, Palette.SurfaceRaised, null);
            allButton.Cursor = Cursors.Hand;
            allButton.Font = UiFonts.MonospaceBold(10f);
            allButton.Click += AllButton_Click;
            panel.Controls.Add(allButton);

            foreach (ProtocolTag tag in ProtocolTags.All)
            {
                Guna2Button button = new Guna2Button
                {
                    Size = new Size(108, 34),
                    Margin = new Padding(0, 0, 10, 0),
                    ButtonMode = ButtonMode.ToogleButton,
                    Checked = true,
                    Tag = tag,
                    Text = ProtocolTags.GetLabel(tag) + " 0"
                };
                StyleTagButton(button, tag);
                button.Click += TagButton_Click;
                _tagButtons[tag] = button;
                panel.Controls.Add(button);
            }

            return panel;
        }

        /// <summary>
        /// Оформляет переключатель метки.
        /// </summary>
        /// <param name="button">Переключатель.</param>
        /// <param name="tag">Метка.</param>
        private void StyleTagButton(Guna2Button button, ProtocolTag tag)
        {
            Color color = ProtocolTagStyle.GetForeColor(tag);
            button.Animated = false;
            button.BorderRadius = 8;
            button.Cursor = Cursors.Hand;
            button.Font = UiFonts.MonospaceBold(9.5f);
            button.UseTransparentBackground = true;
            button.FillColor = Color.Transparent;
            button.ForeColor = Palette.TextMuted;
            button.BorderColor = Palette.Border;
            button.BorderThickness = 1;
            button.HoverState.FillColor = Palette.SurfaceRaised;
            button.CheckedState.FillColor = ProtocolTagStyle.GetBackColor(tag);
            button.CheckedState.ForeColor = color;
            button.CheckedState.BorderColor = color;
            button.CheckedState.Image = IconFactory.Create(IconGlyph.CheckMark, color, 14);
            button.ImageSize = new Size(14, 14);
        }

        /// <summary>
        /// Отключает панель от протокола.
        /// </summary>
        private void Detach()
        {
            if (_protocol != null)
            {
                _protocol.EntryAdded -= Protocol_EntryAdded;
                _protocol = null;
            }
        }

        /// <summary>
        /// Перестраивает дерево записей с учетом фильтра.
        /// </summary>
        private void Rebuild()
        {
            if (_protocol == null)
            {
                return;
            }

            _pending.Clear();
            _tree.BeginUpdate();
            try
            {
                _tree.Nodes.Clear();
                _nodes.Clear();
                _processed.Clear();
                _descendantCounts.Clear();
                foreach (ProtocolEntry entry in _protocol.GetEntries())
                {
                    AddEntry(entry);
                }

                if (_isExpandAllMode)
                {
                    _tree.ExpandAll();
                }
                else
                {
                    foreach (KeyValuePair<ProtocolEntry, TreeNode> pair in _nodes)
                    {
                        if (_expanded.Contains(pair.Key))
                        {
                            pair.Value.Expand();
                        }
                    }
                }
            }
            finally
            {
                _tree.EndUpdate();
            }

            UpdateCounters();
        }

        /// <summary>
        /// Учитывает новую запись: обновляет количество вложенных записей групп
        /// и добавляет запись в дерево, если ее метка выбрана в фильтре.
        /// </summary>
        /// <param name="entry">Запись протокола.</param>
        private void AddEntry(ProtocolEntry entry)
        {
            if (!_processed.Add(entry.Id))
            {
                return;
            }

            for (ProtocolEntry? parent = entry.Parent; parent != null; parent = parent.Parent)
            {
                int count;
                _descendantCounts.TryGetValue(parent, out count);
                _descendantCounts[parent] = count + 1;
            }

            if (_enabledTags.Contains(entry.Tag))
            {
                EnsureNode(entry);
            }
        }

        /// <summary>
        /// Возвращает узел дерева записи, создавая его и узлы родительских записей,
        /// которые сохраняются для контекста.
        /// </summary>
        /// <param name="entry">Запись протокола.</param>
        /// <returns>Узел дерева.</returns>
        private TreeNode EnsureNode(ProtocolEntry entry)
        {
            TreeNode? node;
            if (_nodes.TryGetValue(entry, out node))
            {
                return node;
            }

            TreeNode? parentNode = entry.Parent == null
                ? null
                : EnsureNode(entry.Parent);
            TreeNodeCollection collection = parentNode == null
                ? _tree.Nodes
                : parentNode.Nodes;

            node = new TreeNode(entry.Message)
            {
                Tag = entry
            };
            int index = collection.Count;
            while (index > 0 && ((ProtocolEntry)collection[index - 1].Tag!).Id > entry.Id)
            {
                index--;
            }

            collection.Insert(index, node);
            _nodes[entry] = node;
            if (parentNode != null && (_isExpandAllMode || _expanded.Contains(entry.Parent!)))
            {
                parentNode.Expand();
            }

            return node;
        }

        /// <summary>
        /// Обновляет количество записей на кнопках фильтра и сообщение об отсутствии записей.
        /// </summary>
        private void UpdateCounters()
        {
            if (_protocol == null)
            {
                return;
            }

            _allButton.Text = "Все " + _protocol.Count;
            foreach (KeyValuePair<ProtocolTag, Guna2Button> pair in _tagButtons)
            {
                pair.Value.Text = ProtocolTags.GetLabel(pair.Key) + " " + _protocol.GetCount(pair.Key);
                pair.Value.Checked = _enabledTags.Contains(pair.Key);
            }

            _emptyLabel.Visible = _tree.Nodes.Count == 0 && _protocol.Count > 0;
        }

        /// <summary>
        /// Принимает новую запись протокола из потока сервиса.
        /// </summary>
        /// <param name="sender">Протокол расчета.</param>
        /// <param name="e">Аргументы события.</param>
        private void Protocol_EntryAdded(object? sender, ProtocolEntryEventArgs e)
        {
            _pending.Enqueue(e.Entry);
        }

        /// <summary>
        /// Выводит на панель накопленные записи.
        /// </summary>
        /// <param name="sender">Таймер.</param>
        /// <param name="e">Аргументы события.</param>
        private void FlushTimer_Tick(object? sender, EventArgs e)
        {
            if (_pending.IsEmpty)
            {
                return;
            }

            _tree.BeginUpdate();
            try
            {
                ProtocolEntry? entry;
                while (_pending.TryDequeue(out entry))
                {
                    AddEntry(entry);
                }
            }
            finally
            {
                _tree.EndUpdate();
            }

            UpdateCounters();
            if (_tree.Nodes.Count > 0 && !_isExpandAllMode && _tree.SelectedNode == null)
            {
                _tree.Nodes[_tree.Nodes.Count - 1].EnsureVisible();
            }

            _tree.Invalidate();
        }

        /// <summary>
        /// Переключает режим «Развернуть всё» / «Свернуть всё».
        /// </summary>
        /// <param name="sender">Кнопка.</param>
        /// <param name="e">Аргументы события.</param>
        private void ExpandButton_Click(object? sender, EventArgs e)
        {
            _isExpandAllMode = !_isExpandAllMode;
            _tree.BeginUpdate();
            if (_isExpandAllMode)
            {
                _tree.ExpandAll();
            }
            else
            {
                _tree.CollapseAll();
                _expanded.Clear();
            }

            _tree.EndUpdate();
            _expandButton.Text = _isExpandAllMode
                ? "Свернуть всё"
                : "Развернуть всё";
        }

        /// <summary>
        /// Включает отображение записей всех типов.
        /// </summary>
        /// <param name="sender">Кнопка «Все».</param>
        /// <param name="e">Аргументы события.</param>
        private void AllButton_Click(object? sender, EventArgs e)
        {
            foreach (ProtocolTag tag in ProtocolTags.All)
            {
                _enabledTags.Add(tag);
            }

            Rebuild();
        }

        /// <summary>
        /// Включает или отключает отображение записей с меткой.
        /// </summary>
        /// <param name="sender">Переключатель метки.</param>
        /// <param name="e">Аргументы события.</param>
        private void TagButton_Click(object? sender, EventArgs e)
        {
            Guna2Button? button = sender as Guna2Button;
            if (button == null || !(button.Tag is ProtocolTag))
            {
                return;
            }

            ProtocolTag tag = (ProtocolTag)button.Tag;
            if (!_enabledTags.Remove(tag))
            {
                _enabledTags.Add(tag);
            }

            Rebuild();
        }

        /// <summary>
        /// Раскрывает или сворачивает группу записей по щелчку.
        /// </summary>
        /// <param name="sender">Дерево.</param>
        /// <param name="e">Аргументы события.</param>
        private void Tree_NodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Left || e.Node == null)
            {
                return;
            }

            _tree.SelectedNode = e.Node;
            if (e.Node.Nodes.Count > 0)
            {
                e.Node.Toggle();
            }
        }

        /// <summary>
        /// Запоминает развернутую группу.
        /// </summary>
        /// <param name="sender">Дерево.</param>
        /// <param name="e">Аргументы события.</param>
        private void Tree_AfterExpand(object? sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is ProtocolEntry entry)
            {
                _expanded.Add(entry);
            }
        }

        /// <summary>
        /// Забывает свернутую группу.
        /// </summary>
        /// <param name="sender">Дерево.</param>
        /// <param name="e">Аргументы события.</param>
        private void Tree_AfterCollapse(object? sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is ProtocolEntry entry)
            {
                _expanded.Remove(entry);
            }
        }

        /// <summary>
        /// Рисует строку протокола: стрелку группы, время, цветную метку,
        /// текст сообщения, пояснение и количество записей свернутой группы.
        /// </summary>
        /// <param name="sender">Дерево.</param>
        /// <param name="e">Аргументы события.</param>
        private void Tree_DrawNode(object? sender, DrawTreeNodeEventArgs e)
        {
            ProtocolEntry? entry = e.Node?.Tag as ProtocolEntry;
            if (e.Node == null || entry == null || e.Bounds.Height <= 0)
            {
                return;
            }

            Graphics graphics = e.Graphics;
            Rectangle row = new Rectangle(0, e.Bounds.Top, _tree.ClientSize.Width, e.Bounds.Height);
            bool isSelected = (e.State & TreeNodeStates.Selected) != 0;
            Color background = isSelected
                ? Palette.SurfaceRaised
                : entry.Tag == ProtocolTag.Vrdo
                    ? Color.FromArgb(22, 22, 22)
                    : Palette.SurfaceSunken;
            using (SolidBrush brush = new SolidBrush(background))
            {
                graphics.FillRectangle(brush, row);
            }

            bool hasChildren = e.Node.Nodes.Count > 0;
            int x = 16 + e.Node.Level * LevelIndent;
            int centerY = row.Top + row.Height / 2;
            if (hasChildren)
            {
                UiDrawing.DrawChevron(graphics, Palette.TextSecondary, new Point(x + 4, centerY), e.Node.IsExpanded);
            }

            x += 22;
            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;

            Font timeFont = UiFonts.Monospace(9.5f);
            string time = entry.Time.ToString("HH:mm:ss");
            Size timeSize = TextRenderer.MeasureText(graphics, time, timeFont, Size.Empty, flags);
            TextRenderer.DrawText(graphics, time, timeFont, new Rectangle(x, row.Top, timeSize.Width, row.Height),
                Palette.TextMuted, flags);
            x += timeSize.Width + 14;

            Font badgeFont = UiFonts.MonospaceBold(9f);
            string label = ProtocolTags.GetLabel(entry.Tag);
            Size labelSize = TextRenderer.MeasureText(graphics, label, badgeFont, Size.Empty, flags);
            Rectangle badge = new Rectangle(x, centerY - 11, Math.Max(labelSize.Width + 16, 48), 22);
            UiDrawing.FillRoundedRectangle(graphics, ProtocolTagStyle.GetBackColor(entry.Tag), badge, 5);
            TextRenderer.DrawText(graphics, label, badgeFont, badge, ProtocolTagStyle.GetForeColor(entry.Tag),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
                | TextFormatFlags.NoPrefix);
            x = badge.Right + 12;

            int rightReserve = 16;
            string countText = string.Empty;
            int count;
            if (hasChildren && !e.Node.IsExpanded && _descendantCounts.TryGetValue(entry, out count))
            {
                countText = FormatRecordCount(count);
                rightReserve = TextRenderer.MeasureText(graphics, countText, UiFonts.Regular(9.5f)).Width + 28;
                TextRenderer.DrawText(graphics, countText, UiFonts.Regular(9.5f),
                    new Rectangle(row.Right - rightReserve, row.Top, rightReserve - 16, row.Height),
                    Palette.TextMuted,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }

            Font messageFont = entry.Tag == ProtocolTag.Vrdo
                ? UiFonts.Semibold(11f)
                : UiFonts.Monospace(10f);
            Color messageColor = entry.Tag == ProtocolTag.Warning
                ? Palette.Warning
                : entry.Tag == ProtocolTag.Error
                    ? Palette.Danger
                    : Palette.Text;
            int available = row.Right - rightReserve - x;
            if (available <= 0)
            {
                return;
            }

            Size messageSize = TextRenderer.MeasureText(graphics, entry.Message, messageFont, Size.Empty, flags);
            int messageWidth = Math.Min(messageSize.Width, available);
            Rectangle messageBounds = new Rectangle(x, row.Top, messageWidth, row.Height);
            TextRenderer.DrawText(graphics, entry.Message, messageFont, messageBounds, messageColor,
                flags | TextFormatFlags.EndEllipsis);
            x += messageWidth + 12;

            available = row.Right - rightReserve - x;
            if (entry.Details.Length > 0 && available > 40)
            {
                TextRenderer.DrawText(graphics, entry.Details, UiFonts.Regular(9.5f),
                    new Rectangle(x, row.Top, available, row.Height), Palette.TextSecondary,
                    flags | TextFormatFlags.EndEllipsis);
            }
        }
    }
}

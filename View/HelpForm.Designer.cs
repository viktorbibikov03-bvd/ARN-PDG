#nullable disable

namespace VrdoPdg.View
{
    partial class HelpForm
    {
        /// <summary>
        /// Контейнер компонентов формы.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освобождает ресурсы формы.
        /// </summary>
        /// <param name="disposing">Признак освобождения управляемых ресурсов.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Создает элементы управления формы.
        /// </summary>
        private void InitializeComponent()
        {
            _sectionList = new System.Windows.Forms.ListBox();
            _separator = new System.Windows.Forms.Panel();
            _contentPanel = new System.Windows.Forms.Panel();
            _contentBox = new System.Windows.Forms.RichTextBox();
            _contentPanel.SuspendLayout();
            SuspendLayout();
            //
            // _sectionList
            //
            _sectionList.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            _sectionList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            _sectionList.Dock = System.Windows.Forms.DockStyle.Left;
            _sectionList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            _sectionList.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            _sectionList.ForeColor = System.Drawing.Color.FromArgb(236, 236, 236);
            _sectionList.IntegralHeight = false;
            _sectionList.ItemHeight = 40;
            _sectionList.Name = "_sectionList";
            _sectionList.Size = new System.Drawing.Size(290, 640);
            _sectionList.TabIndex = 0;
            _sectionList.DrawItem += SectionList_DrawItem;
            _sectionList.SelectedIndexChanged += SectionList_SelectedIndexChanged;
            //
            // _separator
            //
            _separator.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
            _separator.Dock = System.Windows.Forms.DockStyle.Left;
            _separator.Name = "_separator";
            _separator.Size = new System.Drawing.Size(1, 640);
            _separator.TabIndex = 1;
            //
            // _contentPanel
            //
            _contentPanel.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
            _contentPanel.Controls.Add(_contentBox);
            _contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _contentPanel.Name = "_contentPanel";
            _contentPanel.Padding = new System.Windows.Forms.Padding(28, 24, 20, 20);
            _contentPanel.TabIndex = 2;
            //
            // _contentBox
            //
            _contentBox.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
            _contentBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            _contentBox.Dock = System.Windows.Forms.DockStyle.Fill;
            _contentBox.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            _contentBox.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            _contentBox.Name = "_contentBox";
            _contentBox.ReadOnly = true;
            _contentBox.TabIndex = 0;
            _contentBox.Text = "";
            //
            // HelpForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
            ClientSize = new System.Drawing.Size(1000, 640);
            Controls.Add(_contentPanel);
            Controls.Add(_separator);
            Controls.Add(_sectionList);
            KeyPreview = true;
            MinimumSize = new System.Drawing.Size(720, 420);
            Name = "HelpForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "ВРДО ПДГ — Справка";
            KeyDown += HelpForm_KeyDown;
            _contentPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        /// <summary>
        /// Оглавление руководства пользователя.
        /// </summary>
        private System.Windows.Forms.ListBox _sectionList;

        /// <summary>
        /// Вертикальный разделитель.
        /// </summary>
        private System.Windows.Forms.Panel _separator;

        /// <summary>
        /// Область содержимого раздела.
        /// </summary>
        private System.Windows.Forms.Panel _contentPanel;

        /// <summary>
        /// Содержимое выбранного раздела.
        /// </summary>
        private System.Windows.Forms.RichTextBox _contentBox;
    }
}

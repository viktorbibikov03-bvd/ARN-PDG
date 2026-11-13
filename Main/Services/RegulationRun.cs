using System.Collections.Generic;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Protocol;
using VrdoPdg.Model.Regime;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Данные выполняемого расчета: исходные данные, таблицы эффективности,
    /// режим после расчета, невведенные КП и группы записей протокола.
    /// </summary>
    internal sealed class RegulationRun
    {
        /// <summary>
        /// Протокол расчета.
        /// </summary>
        private readonly CalculationProtocol _protocol;

        /// <summary>
        /// Записи протокола по КП.
        /// </summary>
        private readonly Dictionary<int, ProtocolEntry> _controlPointEntries = new Dictionary<int, ProtocolEntry>();

        /// <summary>
        /// Записи протокола по КП и часам.
        /// </summary>
        private readonly Dictionary<(int ControlPointId, int Hour), ProtocolEntry> _hourEntries =
            new Dictionary<(int ControlPointId, int Hour), ProtocolEntry>();

        /// <summary>
        /// Создает данные расчета.
        /// </summary>
        /// <param name="session">Исходные данные сеанса.</param>
        /// <param name="efficiencyTables">Таблицы эффективности по коду КП.</param>
        /// <param name="controlPoints">КП, участвующие в расчете: с основным узлом и графиком на все часы.</param>
        /// <param name="protocol">Протокол расчета.</param>
        public RegulationRun(
            SessionData session,
            IReadOnlyDictionary<int, IReadOnlyList<EfficiencyItem>> efficiencyTables,
            IReadOnlyList<ControlPoint> controlPoints,
            CalculationProtocol protocol)
        {
            Session = session;
            EfficiencyTables = efficiencyTables;
            ControlPoints = controlPoints;
            _protocol = protocol;
        }

        /// <summary>
        /// Исходные данные сеанса.
        /// </summary>
        public SessionData Session { get; }

        /// <summary>
        /// Таблицы эффективности по коду КП.
        /// </summary>
        public IReadOnlyDictionary<int, IReadOnlyList<EfficiencyItem>> EfficiencyTables { get; }

        /// <summary>
        /// КП, участвующие в расчете.
        /// </summary>
        public IReadOnlyList<ControlPoint> ControlPoints { get; }

        /// <summary>
        /// Режим после расчета.
        /// </summary>
        public RegimeSnapshot After { get; } = new RegimeSnapshot();

        /// <summary>
        /// КП и часы, в которых напряжение не введено в допустимую область.
        /// </summary>
        public List<UnresolvedViolation> Unresolved { get; } = new List<UnresolvedViolation>();

        /// <summary>
        /// СКРМ, добавленные в БД после загрузки данных: их исходный режим считывается из МТ
        /// перед вводом режима в каждом часе
        /// </summary>
        public List<Device> AddedDevices { get; } = new List<Device>();

        /// <summary>
        /// Количество часов КП, в которых напряжение выходило за границы графика.
        /// </summary>
        public int ViolationCount => _hourEntries.Count;

        /// <summary>
        /// Возвращает действующую таблицу эффективности КП.
        /// </summary>
        /// <param name="controlPointId">Код КП.</param>
        /// <returns>СКРМ в порядке перебора; пустой список, если таблица не задана.</returns>
        public IReadOnlyList<EfficiencyItem> GetEfficiencyTable(int controlPointId)
        {
            IReadOnlyList<EfficiencyItem>? items;
            return EfficiencyTables.TryGetValue(controlPointId, out items)
                ? items
                : new EfficiencyItem[0];
        }

        /// <summary>
        /// Возвращает запись протокола о часе КП, создавая при необходимости
        /// записи о КП (метка ВРДО) и о часе (метка ЧАС).
        /// </summary>
        /// <param name="controlPoint">КП расчета.</param>
        /// <param name="hour">Час суток (1–24).</param>
        /// <param name="voltage">Напряжение в основном узле КП, кВ.</param>
        /// <param name="bounds">Границы графика напряжения.</param>
        /// <returns>Запись протокола о часе КП.</returns>
        public ProtocolEntry GetHourEntry(ControlPoint controlPoint, int hour, double voltage, VoltageBounds bounds)
        {
            ProtocolEntry? hourEntry;
            if (_hourEntries.TryGetValue((controlPoint.Id, hour), out hourEntry))
            {
                return hourEntry;
            }

            ProtocolEntry? controlPointEntry;
            if (!_controlPointEntries.TryGetValue(controlPoint.Id, out controlPointEntry))
            {
                controlPointEntry = _protocol.Add(
                    ProtocolTag.Vrdo,
                    controlPoint.Name + " · узел " + controlPoint.MainNode!.Number);
                _protocol.Add(
                    ProtocolTag.Warning,
                    "Выход за границы графика!",
                    "час " + hour + ": " + DescribeVoltage(voltage, bounds),
                    controlPointEntry);
                _controlPointEntries[controlPoint.Id] = controlPointEntry;
            }

            hourEntry = _protocol.Add(
                ProtocolTag.Hour,
                "Час " + hour,
                DescribeVoltage(voltage, bounds),
                controlPointEntry);
            _hourEntries[(controlPoint.Id, hour)] = hourEntry;
            return hourEntry;
        }

        /// <summary>
        /// Описывает положение напряжения относительно границ графика.
        /// </summary>
        /// <param name="voltage">Напряжение, кВ.</param>
        /// <param name="bounds">Границы графика.</param>
        /// <returns>Строка вида «U = 531,00 кВ, выше верхней границы 525,00 кВ».</returns>
        public static string DescribeVoltage(double voltage, VoltageBounds bounds)
        {
            string text = "U = " + ValueFormat.Voltage(voltage) + " кВ, ";
            if (bounds.IsAbove(voltage))
            {
                return text + "выше верхней границы " + ValueFormat.Voltage(bounds.UpperBound) + " кВ";
            }

            if (bounds.IsBelow(voltage))
            {
                return text + "ниже нижней границы " + ValueFormat.Voltage(bounds.LowerBound) + " кВ";
            }

            return text + "в допустимой области";
        }
    }
}

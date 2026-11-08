using System;

namespace VrdoPdg.Model.Entities
{
    /// <summary>
    /// Наименования типов и видов СКРМ, принятые в БД и пользовательском интерфейсе.
    /// </summary>
    public static class DeviceNames
    {
        /// <summary>
        /// Наименование типа «Управляемые».
        /// </summary>
        public const string Controlled = "Управляемые";

        /// <summary>
        /// Наименование типа «Коммутируемые».
        /// </summary>
        public const string Switched = "Коммутируемые";

        /// <summary>
        /// Наименование типа «Генераторы».
        /// </summary>
        public const string Generator = "Генераторы";

        /// <summary>
        /// Наименование вида коммутируемого СКРМ «Реактор».
        /// </summary>
        public const string Reactor = "Реактор";

        /// <summary>
        /// Наименование вида коммутируемого СКРМ «БСК».
        /// </summary>
        public const string CapacitorBank = "БСК";

        /// <summary>
        /// Возвращает наименование типа СКРМ.
        /// </summary>
        /// <param name="type">Тип СКРМ.</param>
        /// <returns>Наименование типа СКРМ.</returns>
        public static string GetTypeName(DeviceType type)
        {
            switch (type)
            {
                case DeviceType.Controlled:
                    return Controlled;
                case DeviceType.Switched:
                    return Switched;
                case DeviceType.Generator:
                    return Generator;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Неизвестный тип СКРМ");
            }
        }

        /// <summary>
        /// Определяет тип СКРМ по наименованию.
        /// </summary>
        /// <param name="name">Наименование типа СКРМ.</param>
        /// <param name="type">Тип СКРМ.</param>
        /// <returns><c>true</c>, если наименование распознано.</returns>
        public static bool TryParseType(string? name, out DeviceType type)
        {
            switch (name)
            {
                case Controlled:
                    type = DeviceType.Controlled;
                    return true;
                case Switched:
                    type = DeviceType.Switched;
                    return true;
                case Generator:
                    type = DeviceType.Generator;
                    return true;
                default:
                    type = DeviceType.Controlled;
                    return false;
            }
        }

        /// <summary>
        /// Возвращает наименование вида коммутируемого СКРМ.
        /// </summary>
        /// <param name="kind">Вид коммутируемого СКРМ.</param>
        /// <returns>Наименование вида.</returns>
        public static string GetKindName(SwitchedDeviceKind kind)
        {
            return kind == SwitchedDeviceKind.CapacitorBank
                ? CapacitorBank
                : Reactor;
        }

        /// <summary>
        /// Определяет вид коммутируемого СКРМ по наименованию.
        /// </summary>
        /// <param name="name">Наименование вида.</param>
        /// <param name="kind">Вид коммутируемого СКРМ.</param>
        /// <returns><c>true</c>, если наименование распознано.</returns>
        public static bool TryParseKind(string? name, out SwitchedDeviceKind kind)
        {
            switch (name)
            {
                case Reactor:
                    kind = SwitchedDeviceKind.Reactor;
                    return true;
                case CapacitorBank:
                    kind = SwitchedDeviceKind.CapacitorBank;
                    return true;
                default:
                    kind = SwitchedDeviceKind.Reactor;
                    return false;
            }
        }

        /// <summary>
        /// Возвращает подпись номера СКРМ в МТ: «узел», «реактор» или «генератор».
        /// </summary>
        /// <param name="type">Тип СКРМ.</param>
        /// <returns>Подпись номера.</returns>
        public static string GetNumberCaption(DeviceType type)
        {
            switch (type)
            {
                case DeviceType.Controlled:
                    return "узел";
                case DeviceType.Switched:
                    return "реактор";
                default:
                    return "генератор";
            }
        }
    }
}

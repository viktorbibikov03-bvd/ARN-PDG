using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using VrdoPdg.Model.Regime;

namespace VrdoPdg.View.Layout
{
    /// <summary>
    /// Схема таблиц форм «Напряжение по часам» и «Сравнение расчетов»: энергосистемы, блоки КП,
    /// разделы и столбцы в том порядке, в котором они расположены на листах «U-500» файла
    /// «Анализ.xlsx». Хранится в файле hourly-layout.json рядом с исполняемым файлом.
    /// </summary>
    internal sealed class HourlyLayout
    {
        /// <summary>
        /// Создает схему таблиц.
        /// </summary>
        /// <param name="lowerVoltageBound">Нижняя граница напряжения на шинах, кВ.</param>
        /// <param name="upperVoltageBound">Верхняя граница напряжения на шинах, кВ.</param>
        /// <param name="powerSystems">Энергосистемы.</param>
        public HourlyLayout(
            double lowerVoltageBound,
            double upperVoltageBound,
            IReadOnlyList<HourlyLayoutPowerSystem> powerSystems)
        {
            LowerVoltageBound = lowerVoltageBound;
            UpperVoltageBound = upperVoltageBound;
            PowerSystems = powerSystems;
        }

        /// <summary>
        /// Пустая схема: таблицы не содержат объектов
        /// </summary>
        public static HourlyLayout Empty { get; } = new HourlyLayout(500, 525, new HourlyLayoutPowerSystem[0]);

        /// <summary>
        /// Нижняя граница напряжения на шинах КП, не заданных в БД, кВ.
        /// </summary>
        public double LowerVoltageBound { get; }

        /// <summary>
        /// Верхняя граница напряжения на шинах КП, не заданных в БД, кВ.
        /// </summary>
        public double UpperVoltageBound { get; }

        /// <summary>
        /// Энергосистемы.
        /// </summary>
        public IReadOnlyList<HourlyLayoutPowerSystem> PowerSystems { get; }

        /// <summary>
        /// Загружает схему таблиц из файла.
        /// </summary>
        /// <param name="path">Путь к файлу hourly-layout.json.</param>
        /// <returns>Схема таблиц.</returns>
        /// <exception cref="InvalidDataException">Файл не найден или заполнен неправильно.</exception>
        public static HourlyLayout Load(string path)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                RespectNullableAnnotations = true,
                RespectRequiredConstructorParameters = true
            };
            options.Converters.Add(new JsonStringEnumConverter());

            HourlyLayout? layout;
            try
            {
                layout = JsonSerializer.Deserialize<HourlyLayout>(File.ReadAllText(path), options);
            }
            catch (Exception exception)
                when (exception is IOException
                    || exception is UnauthorizedAccessException
                    || exception is JsonException)
            {
                throw new InvalidDataException(exception.Message, exception);
            }

            if (layout == null)
            {
                throw new InvalidDataException("Файл не содержит схемы таблиц");
            }

            if (layout.LowerVoltageBound >= layout.UpperVoltageBound)
            {
                throw new InvalidDataException("Нижняя граница напряжения должна быть меньше верхней");
            }

            return layout;
        }

        /// <summary>
        /// Возвращает объекты МТ, значения которых нужны для таблиц: узлы и реакторы всех столбцов.
        /// </summary>
        /// <returns>Наблюдаемые объекты.</returns>
        public ObservedObjects GetObservedObjects()
        {
            List<HourlyLayoutColumn> columns = PowerSystems
                .SelectMany(powerSystem => powerSystem.Blocks)
                .SelectMany(block => block.ControlPoints)
                .SelectMany(controlPoint => controlPoint.Groups)
                .SelectMany(group => group.Columns)
                .ToList();
            return new ObservedObjects(
                columns.Where(column => !column.IsReactor).Select(column => column.Number),
                columns.Where(column => column.IsReactor).Select(column => column.Number));
        }
    }
}

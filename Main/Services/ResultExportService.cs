using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Protocol;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.SmpSmzu;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Сервис сохранения результатов: формирует записи об изменении состояния и загрузки СКРМ
    /// согласно инструкции и дополняет ими файл smp_smzu.csv.
    /// </summary>
    public sealed class ResultExportService
    {
        /// <summary>
        /// Точность сравнения значений режима до и после расчета.
        /// </summary>
        private const int ComparisonDigits = 3;

        /// <summary>
        /// Модуль формирования файла smp_smzu.csv.
        /// </summary>
        private readonly ISmpSmzuCsvWriter _writer;

        /// <summary>
        /// Протокол расчета.
        /// </summary>
        private readonly CalculationProtocol _protocol;

        /// <summary>
        /// Создает сервис сохранения результатов.
        /// </summary>
        /// <param name="writer">Модуль формирования файла smp_smzu.csv.</param>
        /// <param name="protocol">Протокол расчета.</param>
        public ResultExportService(ISmpSmzuCsvWriter writer, CalculationProtocol protocol)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        }

        /// <summary>
        /// Возвращает путь сохранения результатов по умолчанию:
        /// путь к загруженному файлу smp_smzu.csv
        /// (D:\BarsMDP\Results\&lt;ДДММГГ&gt;\BarsCSV\smp_smzu.csv).
        /// </summary>
        /// <param name="result">Расчет.</param>
        /// <returns>Путь к файлу.</returns>
        public static string GetDefaultPath(CalculationResult result)
        {
            return result.Session.SmpSmzuPath;
        }

        /// <summary>
        /// Сохраняет результаты расчета: строки загруженного файла smp_smzu.csv сохраняются
        /// без изменений, во второй блок добавляются записи об изменении СКРМ.
        /// </summary>
        /// <param name="result">Расчет.</param>
        /// <param name="path">Путь к сохраняемому файлу.</param>
        /// <returns>Количество добавленных записей.</returns>
        /// <exception cref="DataFileException">Нет доступа к файлу.</exception>
        public int Export(CalculationResult result, string path)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            IReadOnlyList<SmpSmzuRecord> records = CreateRecords(result);
            try
            {
                _writer.Write(path, result.Session.SmpSmzuDocument, records);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                DataFileException error = new DataFileException(
                    DataFileError.AccessDenied,
                    path,
                    exception.Message,
                    exception);
                _protocol.Add(ProtocolTag.Error, error.Message, error.Details);
                throw error;
            }

            _protocol.Add(
                ProtocolTag.Info,
                "Результаты расчета №" + result.Number + " сохранены",
                path + "; добавлено записей: " + records.Count);
            return records.Count;
        }

        /// <summary>
        /// Формирует записи файла smp_smzu.csv по различиям режимов СКРМ до и после расчета.
        /// Одинаковые изменения в соседних часах объединяются в одну запись.
        /// </summary>
        /// <param name="result">Расчет.</param>
        /// <returns>Записи об изменении управляемых СКРМ (код 03), коммутируемых СКРМ (код 20)
        /// и заданного напряжения генераторов (код 01).</returns>
        public IReadOnlyList<SmpSmzuRecord> CreateRecords(CalculationResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            List<SmpSmzuRecord> records = new List<SmpSmzuRecord>();
            IEnumerable<Device> devices = result.Session.Devices
                .OrderBy(device => device.Type)
                .ThenBy(device => device.Number);
            foreach (Device device in devices)
            {
                int? address = GetAddress(result.Session, device);
                if (!address.HasValue)
                {
                    continue;
                }

                double?[] values = new double?[RegimeSnapshot.HourCount + 1];
                double?[] secondValues = new double?[RegimeSnapshot.HourCount + 1];
                for (int hour = 1; hour <= RegimeSnapshot.HourCount; hour++)
                {
                    DeviceMode? before = result.Before.GetMode(device.Id, hour);
                    DeviceMode? after = result.After.GetMode(device.Id, hour);
                    if (before == null || after == null)
                    {
                        continue;
                    }

                    SetChangedValues(device.Type, before, after, hour, values, secondValues);
                }

                AddRecords(records, GetRecordType(device.Type), address.Value, values, secondValues);
            }

            return records
                .GroupBy(record => new
                {
                    record.TypeCode,
                    record.StartHour,
                    record.EndHour,
                    record.Address1,
                    record.Value1,
                    record.Value2
                })
                .Select(group => group.First())
                .ToList();
        }

        /// <summary>
        /// Определяет адрес записи СКРМ: номер узла или номер реактора.
        /// </summary>
        /// <param name="session">Исходные данные сеанса.</param>
        /// <param name="device">СКРМ.</param>
        /// <returns>Адрес записи или <c>null</c>, если генератор не найден в МТ.</returns>
        private static int? GetAddress(SessionData session, Device device)
        {
            return device.Type == DeviceType.Generator
                ? session.GetGeneratorNode(device.Id)
                : device.Number;
        }

        /// <summary>
        /// Возвращает тип записи файла smp_smzu.csv для типа СКРМ.
        /// </summary>
        /// <param name="type">Тип СКРМ.</param>
        /// <returns>Тип записи.</returns>
        private static SmpSmzuRecordType GetRecordType(DeviceType type)
        {
            switch (type)
            {
                case DeviceType.Controlled:
                    return SmpSmzuRecordType.NodeReactivePowerLimits;
                case DeviceType.Switched:
                    return SmpSmzuRecordType.Reactor;
                default:
                    return SmpSmzuRecordType.NodeVoltage;
            }
        }

        /// <summary>
        /// Записывает значения измененного в часе параметра СКРМ.
        /// </summary>
        /// <param name="type">Тип СКРМ.</param>
        /// <param name="before">Режим до расчета.</param>
        /// <param name="after">Режим после расчета.</param>
        /// <param name="hour">Час суток.</param>
        /// <param name="values">Значения 1 по часам.</param>
        /// <param name="secondValues">Значения 2 по часам.</param>
        private static void SetChangedValues(
            DeviceType type,
            DeviceMode before,
            DeviceMode after,
            int hour,
            double?[] values,
            double?[] secondValues)
        {
            switch (type)
            {
                case DeviceType.Controlled:
                    if (!AreEqual(before.MaxReactivePower, after.MaxReactivePower)
                        || !AreEqual(before.MinReactivePower, after.MinReactivePower))
                    {
                        values[hour] = Round(after.MaxReactivePower);
                        secondValues[hour] = Round(after.MinReactivePower);
                    }

                    break;

                case DeviceType.Switched:
                    if (before.IsOn != after.IsOn)
                    {
                        values[hour] = after.IsOn
                            ? 1
                            : 0;
                        secondValues[hour] = 0;
                    }

                    break;

                default:
                    if (!AreEqual(before.SetVoltage, after.SetVoltage))
                    {
                        values[hour] = Round(after.SetVoltage);
                        secondValues[hour] = 0;
                    }

                    break;
            }
        }

        /// <summary>
        /// Добавляет записи, объединяя соседние часы с одинаковыми значениями.
        /// </summary>
        /// <param name="records">Список записей.</param>
        /// <param name="type">Тип записи.</param>
        /// <param name="address">Адрес записи.</param>
        /// <param name="values">Значения 1 по часам.</param>
        /// <param name="secondValues">Значения 2 по часам.</param>
        private static void AddRecords(
            List<SmpSmzuRecord> records,
            SmpSmzuRecordType type,
            int address,
            double?[] values,
            double?[] secondValues)
        {
            int hour = 1;
            while (hour <= RegimeSnapshot.HourCount)
            {
                if (!values[hour].HasValue)
                {
                    hour++;
                    continue;
                }

                int startHour = hour;
                while (hour + 1 <= RegimeSnapshot.HourCount
                    && values[hour + 1].HasValue
                    && AreEqual(values[hour + 1], values[startHour])
                    && AreEqual(secondValues[hour + 1], secondValues[startHour]))
                {
                    hour++;
                }

                records.Add(new SmpSmzuRecord(
                    (int)type,
                    startHour,
                    hour,
                    address,
                    0,
                    0,
                    values[startHour],
                    secondValues[startHour],
                    0,
                    0));
                hour++;
            }
        }

        /// <summary>
        /// Сравнивает значения режима с заданной точностью.
        /// </summary>
        /// <param name="first">Первое значение.</param>
        /// <param name="second">Второе значение.</param>
        /// <returns><c>true</c>, если значения равны.</returns>
        private static bool AreEqual(double? first, double? second)
        {
            if (!first.HasValue || !second.HasValue)
            {
                return first.HasValue == second.HasValue;
            }

            return Math.Round(first.Value, ComparisonDigits) == Math.Round(second.Value, ComparisonDigits);
        }

        /// <summary>
        /// Округляет значение режима до заданной точности.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <returns>Округленное значение.</returns>
        private static double? Round(double? value)
        {
            return value.HasValue
                ? Math.Round(value.Value, ComparisonDigits)
                : (double?)null;
        }
    }
}

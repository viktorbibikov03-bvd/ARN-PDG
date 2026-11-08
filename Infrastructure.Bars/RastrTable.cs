using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

using ASTRALib;

namespace VrdoPdg.Infrastructure.Bars
{
    /// <summary>
    /// Таблица МТ с индексом строк по ключевому столбцу.
    /// </summary>
    internal sealed class RastrTable
    {
        /// <summary>
        /// Таблица ASTRALib.
        /// </summary>
        private readonly ITable _table;

        /// <summary>
        /// Столбцы таблицы по имени.
        /// </summary>
        private readonly Dictionary<string, ICol> _columns = new Dictionary<string, ICol>(StringComparer.Ordinal);

        /// <summary>
        /// Индексы строк по значению ключевого столбца.
        /// </summary>
        private readonly Dictionary<int, int> _rows = new Dictionary<int, int>();

        /// <summary>
        /// Создает таблицу и строит индекс строк.
        /// </summary>
        /// <param name="table">Таблица ASTRALib.</param>
        /// <param name="keyColumn">Имя ключевого столбца.</param>
        private RastrTable(ITable table, string keyColumn)
        {
            _table = table;
            ICol key = GetColumn(keyColumn);
            int count = table.Count;
            for (int row = 0; row < count; row++)
            {
                int number = ReadInteger(key, row);
                if (!_rows.ContainsKey(number))
                {
                    _rows[number] = row;
                }
            }
        }

        /// <summary>
        /// Количество строк таблицы.
        /// </summary>
        public int Count => _table.Count;

        /// <summary>
        /// Открывает таблицу МТ.
        /// </summary>
        /// <param name="rastr">Объект ASTRALib.</param>
        /// <param name="tableName">Имя таблицы.</param>
        /// <param name="keyColumn">Имя ключевого столбца.</param>
        /// <returns>Таблица или <c>null</c>, если таблицы нет в МТ.</returns>
        public static RastrTable? TryOpen(IRastr rastr, string tableName, string keyColumn)
        {
            ITable? table;
            try
            {
                table = rastr.Tables.Item(tableName) as ITable;
            }
            catch (COMException)
            {
                return null;
            }

            return table == null
                ? null
                : new RastrTable(table, keyColumn);
        }

        /// <summary>
        /// Ищет строку по значению ключевого столбца.
        /// </summary>
        /// <param name="key">Значение ключа.</param>
        /// <param name="row">Индекс строки.</param>
        /// <returns><c>true</c>, если строка найдена.</returns>
        public bool TryFindRow(int key, out int row)
        {
            return _rows.TryGetValue(key, out row);
        }

        /// <summary>
        /// Возвращает столбец таблицы.
        /// </summary>
        /// <param name="name">Имя столбца.</param>
        /// <returns>Столбец.</returns>
        public ICol GetColumn(string name)
        {
            ICol? column;
            if (!_columns.TryGetValue(name, out column))
            {
                column = (ICol)_table.Cols.Item(name);
                _columns[name] = column;
            }

            return column;
        }

        /// <summary>
        /// Читает вещественное значение.
        /// </summary>
        /// <param name="name">Имя столбца.</param>
        /// <param name="row">Индекс строки.</param>
        /// <returns>Значение.</returns>
        public double ReadDouble(string name, int row)
        {
            object? value = Read(GetColumn(name), row);
            return value == null || value is DBNull
                ? 0
                : Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Читает целое значение.
        /// </summary>
        /// <param name="name">Имя столбца.</param>
        /// <param name="row">Индекс строки.</param>
        /// <returns>Значение.</returns>
        public int ReadInteger(string name, int row)
        {
            return ReadInteger(GetColumn(name), row);
        }

        /// <summary>
        /// Читает логическое значение (например, признак отключения «sta»).
        /// </summary>
        /// <param name="name">Имя столбца.</param>
        /// <param name="row">Индекс строки.</param>
        /// <returns>Значение.</returns>
        public bool ReadBoolean(string name, int row)
        {
            object? value = Read(GetColumn(name), row);
            return value != null
                && !(value is DBNull)
                && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Читает значение ячейки в исходном виде.
        /// </summary>
        /// <param name="column">Столбец.</param>
        /// <param name="row">Индекс строки.</param>
        /// <returns>Значение ячейки.</returns>
        public static object? Read(ICol column, int row)
        {
            return column.get_ZN(row);
        }

        /// <summary>
        /// Записывает значение ячейки.
        /// </summary>
        /// <param name="column">Столбец.</param>
        /// <param name="row">Индекс строки.</param>
        /// <param name="value">Значение.</param>
        public static void Write(ICol column, int row, object value)
        {
            column.set_Z(row, value);
        }

        /// <summary>
        /// Читает целое значение ячейки.
        /// </summary>
        /// <param name="column">Столбец.</param>
        /// <param name="row">Индекс строки.</param>
        /// <returns>Значение.</returns>
        private static int ReadInteger(ICol column, int row)
        {
            object? value = Read(column, row);
            return value == null || value is DBNull
                ? 0
                : Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }
    }
}

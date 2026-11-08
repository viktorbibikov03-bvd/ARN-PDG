using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Microsoft.Data.Sqlite;

using VrdoPdg.Model;
using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Storage;

namespace VrdoPdg.Infrastructure
{
    /// <summary>
    /// Репозиторий базы данных SQLite: доступ к данным системы с помощью
    /// параметризованных SQL-запросов через библиотеку Microsoft.Data.Sqlite.
    /// Работает с существующим файлом БД и не создает его. При открытии каждого соединения
    /// включается проверка внешних ключей.
    /// </summary>
    public sealed class ModelDataRepository : IModelDataRepository
    {
        /// <summary>
        /// Сообщение об отсутствии файла БД.
        /// </summary>
        private const string MissingDatabaseMessage = "Не найден файл базы данных";

        /// <summary>
        /// Сообщение об отсутствии таблиц системы в БД.
        /// </summary>
        private const string MissingTablesMessage = "База данных не содержит таблиц системы";

        /// <summary>
        /// Формат хранения даты и времени (ISO 8601).
        /// </summary>
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Таблицы БД, с которыми работает система.
        /// </summary>
        private static readonly string[] _requiredTables =
        {
            "Power_system", "Control_point", "Node", "Device", "Controlled_device", "Switched_device",
            "Generator", "PQ_point", "Efficiency_table", "Voltage_schedule", "Node_voltage", "Device_mode"
        };

        /// <summary>
        /// Строка подключения к БД.
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Создает репозиторий.
        /// </summary>
        /// <param name="databasePath">Путь к файлу БД.</param>
        public ModelDataRepository(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("Не задан путь к файлу базы данных", nameof(databasePath));
            }

            DatabasePath = Path.GetFullPath(databasePath);
            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder
            {
                DataSource = DatabasePath,
                Mode = SqliteOpenMode.ReadWrite
            };
            _connectionString = builder.ToString();
        }

        /// <summary>
        /// Полный путь к файлу БД.
        /// </summary>
        public string DatabasePath { get; }

        /// <inheritdoc/>
        public void CheckDatabase()
        {
            if (!File.Exists(DatabasePath))
            {
                throw new VrdoException(MissingDatabaseMessage, DatabasePath);
            }

            HashSet<string> tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (SqliteConnection connection = OpenConnection())
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tables.Add(reader.GetString(0));
                    }
                }
            }

            List<string> missing = _requiredTables
                .Where(table => !tables.Contains(table))
                .ToList();
            if (missing.Count > 0)
            {
                throw new VrdoException(
                    MissingTablesMessage,
                    DatabasePath + Environment.NewLine + "Нет таблиц: " + string.Join(", ", missing));
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<PowerSystem> GetPowerSystems()
        {
            List<PowerSystem> result = new List<PowerSystem>();
            using (SqliteConnection connection = OpenConnection())
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT id_ps, name_ps FROM Power_system ORDER BY id_ps";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new PowerSystem(reader.GetInt32(0), reader.GetString(1)));
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public IReadOnlyList<ControlPoint> GetControlPoints()
        {
            List<(int Id, int PowerSystemId, string Name)> controlPoints =
                new List<(int Id, int PowerSystemId, string Name)>();
            Dictionary<int, List<ControlPointNode>> nodes = new Dictionary<int, List<ControlPointNode>>();
            using (SqliteConnection connection = OpenConnection())
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT cp.id_cp, cp.id_ps, cp.name_cp, n.id_node, n.is_main "
                    + "FROM Control_point cp LEFT JOIN Node n ON n.id_cp = cp.id_cp "
                    + "ORDER BY cp.id_cp, n.id_node";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        List<ControlPointNode>? list;
                        if (!nodes.TryGetValue(id, out list))
                        {
                            list = new List<ControlPointNode>();
                            nodes[id] = list;
                            controlPoints.Add((id, reader.GetInt32(1), reader.GetString(2)));
                        }

                        if (!reader.IsDBNull(3))
                        {
                            list.Add(new ControlPointNode(reader.GetInt32(3), reader.GetInt32(4) == 1));
                        }
                    }
                }
            }

            return controlPoints
                .Select(item => new ControlPoint(item.Id, item.PowerSystemId, item.Name, nodes[item.Id]))
                .ToList();
        }

        /// <inheritdoc/>
        public IReadOnlyList<Device> GetDevices()
        {
            List<Device> result = new List<Device>();
            using (SqliteConnection connection = OpenConnection())
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT d.id_dev, d.type_dev, d.name_dev, "
                    + "c.num_node, s.num_reactor, s.kind_sd, g.num_node "
                    + "FROM Device d "
                    + "LEFT JOIN Controlled_device c ON c.id_dev = d.id_dev "
                    + "LEFT JOIN Switched_device s ON s.id_dev = d.id_dev "
                    + "LEFT JOIN Generator g ON g.id_dev = d.id_dev "
                    + "ORDER BY d.id_dev";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Device? device = ReadDevice(reader);
                        if (device != null)
                        {
                            result.Add(device);
                        }
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public IReadOnlyList<EfficiencyEntry> GetEfficiencyEntries()
        {
            List<EfficiencyEntry> result = new List<EfficiencyEntry>();
            using (SqliteConnection connection = OpenConnection())
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT id_cp, id_dev, priority, priority_pur FROM Efficiency_table "
                    + "ORDER BY id_cp, priority, priority_pur";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new EfficiencyEntry(
                            reader.GetInt32(0),
                            reader.GetInt32(1),
                            reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                            reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3)));
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public void SaveEfficiencyTable(int controlPointId, IReadOnlyList<DevicePriority> priorities)
        {
            if (priorities == null)
            {
                throw new ArgumentNullException(nameof(priorities));
            }

            using (SqliteConnection connection = OpenConnection())
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                Dictionary<int, bool> existing = new Dictionary<int, bool>();
                using (SqliteCommand select = CreateCommand(
                    connection,
                    transaction,
                    "SELECT id_dev, priority_pur FROM Efficiency_table WHERE id_cp = $cp"))
                {
                    select.Parameters.AddWithValue("$cp", controlPointId);
                    using (SqliteDataReader reader = select.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            existing[reader.GetInt32(0)] = !reader.IsDBNull(1);
                        }
                    }
                }

                // Временные отрицательные приоритеты исключают нарушение уникальности при перестановке.
                using (SqliteCommand shift = CreateCommand(
                    connection,
                    transaction,
                    "UPDATE Efficiency_table SET priority = -priority WHERE id_cp = $cp AND priority IS NOT NULL"))
                {
                    shift.Parameters.AddWithValue("$cp", controlPointId);
                    shift.ExecuteNonQuery();
                }

                HashSet<int> kept = new HashSet<int>(priorities.Select(item => item.DeviceId));
                foreach (KeyValuePair<int, bool> pair in existing.Where(item => !kept.Contains(item.Key)))
                {
                    string sql = pair.Value
                        ? "UPDATE Efficiency_table SET priority = NULL WHERE id_cp = $cp AND id_dev = $dev"
                        : "DELETE FROM Efficiency_table WHERE id_cp = $cp AND id_dev = $dev";
                    using (SqliteCommand remove = CreateCommand(connection, transaction, sql))
                    {
                        remove.Parameters.AddWithValue("$cp", controlPointId);
                        remove.Parameters.AddWithValue("$dev", pair.Key);
                        remove.ExecuteNonQuery();
                    }
                }

                using (SqliteCommand upsert = CreateCommand(
                    connection,
                    transaction,
                    "INSERT INTO Efficiency_table (id_cp, id_dev, priority, priority_pur) "
                        + "VALUES ($cp, $dev, $priority, NULL) "
                        + "ON CONFLICT (id_cp, id_dev) DO UPDATE SET priority = excluded.priority"))
                {
                    SqliteParameter cp = upsert.Parameters.Add("$cp", SqliteType.Integer);
                    SqliteParameter dev = upsert.Parameters.Add("$dev", SqliteType.Integer);
                    SqliteParameter priority = upsert.Parameters.Add("$priority", SqliteType.Integer);
                    foreach (DevicePriority item in priorities)
                    {
                        cp.Value = controlPointId;
                        dev.Value = item.DeviceId;
                        priority.Value = item.Priority;
                        upsert.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<VoltageBounds> GetVoltageSchedule()
        {
            List<VoltageBounds> result = new List<VoltageBounds>();
            using (SqliteConnection connection = OpenConnection())
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT id_cp, hour, u_low, u_high, received_at FROM Voltage_schedule "
                    + "ORDER BY id_cp, hour";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new VoltageBounds(
                            reader.GetInt32(0),
                            reader.GetInt32(1),
                            reader.GetDouble(2),
                            reader.GetDouble(3),
                            ParseDateTime(reader.GetString(4))));
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public void SaveVoltageSchedule(IReadOnlyList<VoltageBounds> bounds)
        {
            if (bounds == null)
            {
                throw new ArgumentNullException(nameof(bounds));
            }

            using (SqliteConnection connection = OpenConnection())
            using (SqliteTransaction transaction = connection.BeginTransaction())
            using (SqliteCommand command = CreateCommand(
                connection,
                transaction,
                "INSERT INTO Voltage_schedule (id_cp, hour, u_low, u_high, received_at) "
                    + "VALUES ($cp, $hour, $low, $high, $received) "
                    + "ON CONFLICT (id_cp, hour) DO UPDATE SET u_low = excluded.u_low, "
                    + "u_high = excluded.u_high, received_at = excluded.received_at"))
            {
                SqliteParameter cp = command.Parameters.Add("$cp", SqliteType.Integer);
                SqliteParameter hour = command.Parameters.Add("$hour", SqliteType.Integer);
                SqliteParameter low = command.Parameters.Add("$low", SqliteType.Real);
                SqliteParameter high = command.Parameters.Add("$high", SqliteType.Real);
                SqliteParameter received = command.Parameters.Add("$received", SqliteType.Text);
                foreach (VoltageBounds item in bounds)
                {
                    cp.Value = item.ControlPointId;
                    hour.Value = item.Hour;
                    low.Value = item.LowerBound;
                    high.Value = item.UpperBound;
                    received.Value = item.ReceivedAt.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<int, PQDiagram> GetPQDiagrams()
        {
            Dictionary<int, List<PQPoint>> points = new Dictionary<int, List<PQPoint>>();
            using (SqliteConnection connection = OpenConnection())
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT id_dev, p_pq, q_min_pq, q_max_pq FROM PQ_point "
                    + "ORDER BY id_dev, num_point";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int deviceId = reader.GetInt32(0);
                        List<PQPoint>? list;
                        if (!points.TryGetValue(deviceId, out list))
                        {
                            list = new List<PQPoint>();
                            points[deviceId] = list;
                        }

                        list.Add(new PQPoint(reader.GetDouble(1), reader.GetDouble(2), reader.GetDouble(3)));
                    }
                }
            }

            return points.ToDictionary(pair => pair.Key, pair => new PQDiagram(pair.Value));
        }

        /// <inheritdoc/>
        public void SavePQDiagram(int deviceId, IReadOnlyList<PQPoint> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            using (SqliteConnection connection = OpenConnection())
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                using (SqliteCommand delete = CreateCommand(
                    connection,
                    transaction,
                    "DELETE FROM PQ_point WHERE id_dev = $dev"))
                {
                    delete.Parameters.AddWithValue("$dev", deviceId);
                    delete.ExecuteNonQuery();
                }

                using (SqliteCommand insert = CreateCommand(
                    connection,
                    transaction,
                    "INSERT INTO PQ_point (id_dev, num_point, p_pq, q_min_pq, q_max_pq) "
                        + "VALUES ($dev, $number, $p, $qmin, $qmax)"))
                {
                    SqliteParameter dev = insert.Parameters.Add("$dev", SqliteType.Integer);
                    SqliteParameter number = insert.Parameters.Add("$number", SqliteType.Integer);
                    SqliteParameter p = insert.Parameters.Add("$p", SqliteType.Real);
                    SqliteParameter qmin = insert.Parameters.Add("$qmin", SqliteType.Real);
                    SqliteParameter qmax = insert.Parameters.Add("$qmax", SqliteType.Real);
                    for (int index = 0; index < points.Count; index++)
                    {
                        dev.Value = deviceId;
                        number.Value = index + 1;
                        p.Value = points[index].ActivePower;
                        qmin.Value = points[index].MinReactivePower;
                        qmax.Value = points[index].MaxReactivePower;
                        insert.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }

        /// <inheritdoc/>
        public void SaveRegime(IReadOnlyList<NodeVoltage> voltages, IReadOnlyList<DeviceMode> modes)
        {
            if (voltages == null)
            {
                throw new ArgumentNullException(nameof(voltages));
            }

            if (modes == null)
            {
                throw new ArgumentNullException(nameof(modes));
            }

            using (SqliteConnection connection = OpenConnection())
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                using (SqliteCommand delete = CreateCommand(
                    connection,
                    transaction,
                    "DELETE FROM Node_voltage; DELETE FROM Device_mode;"))
                {
                    delete.ExecuteNonQuery();
                }

                using (SqliteCommand insert = CreateCommand(
                    connection,
                    transaction,
                    "INSERT INTO Node_voltage (id_node, hour, u_node) VALUES ($node, $hour, $voltage)"))
                {
                    SqliteParameter node = insert.Parameters.Add("$node", SqliteType.Integer);
                    SqliteParameter hour = insert.Parameters.Add("$hour", SqliteType.Integer);
                    SqliteParameter voltage = insert.Parameters.Add("$voltage", SqliteType.Real);
                    foreach (NodeVoltage item in voltages)
                    {
                        node.Value = item.NodeNumber;
                        hour.Value = item.Hour;
                        voltage.Value = item.Voltage;
                        insert.ExecuteNonQuery();
                    }
                }

                using (SqliteCommand insert = CreateCommand(
                    connection,
                    transaction,
                    "INSERT INTO Device_mode (id_dev, hour, is_on, q_dev, q_min, q_max, u_set, p_gen, is_fixed) "
                        + "VALUES ($dev, $hour, $on, $q, $qmin, $qmax, $u, $p, $fixed)"))
                {
                    SqliteParameter dev = insert.Parameters.Add("$dev", SqliteType.Integer);
                    SqliteParameter hour = insert.Parameters.Add("$hour", SqliteType.Integer);
                    SqliteParameter isOn = insert.Parameters.Add("$on", SqliteType.Integer);
                    SqliteParameter q = insert.Parameters.Add("$q", SqliteType.Real);
                    SqliteParameter qmin = insert.Parameters.Add("$qmin", SqliteType.Real);
                    SqliteParameter qmax = insert.Parameters.Add("$qmax", SqliteType.Real);
                    SqliteParameter u = insert.Parameters.Add("$u", SqliteType.Real);
                    SqliteParameter p = insert.Parameters.Add("$p", SqliteType.Real);
                    SqliteParameter isFixed = insert.Parameters.Add("$fixed", SqliteType.Integer);
                    foreach (DeviceMode item in modes)
                    {
                        dev.Value = item.DeviceId;
                        hour.Value = item.Hour;
                        isOn.Value = item.IsOn ? 1 : 0;
                        q.Value = ToDbValue(item.ReactivePower);
                        qmin.Value = ToDbValue(item.MinReactivePower);
                        qmax.Value = ToDbValue(item.MaxReactivePower);
                        u.Value = ToDbValue(item.SetVoltage);
                        p.Value = ToDbValue(item.ActivePower);
                        isFixed.Value = item.IsFixed ? 1 : 0;
                        insert.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }

        /// <summary>
        /// Создает СКРМ по строке результата запроса.
        /// </summary>
        /// <param name="reader">Строка результата запроса справочника СКРМ.</param>
        /// <returns>СКРМ или <c>null</c>, если для СКРМ не задан подтип.</returns>
        private static Device? ReadDevice(SqliteDataReader reader)
        {
            DeviceType type;
            if (!DeviceNames.TryParseType(reader.GetString(1), out type))
            {
                return null;
            }

            int id = reader.GetInt32(0);
            string name = reader.GetString(2);
            switch (type)
            {
                case DeviceType.Controlled:
                    return reader.IsDBNull(3)
                        ? null
                        : new Device(id, type, name, reader.GetInt32(3), null);

                case DeviceType.Switched:
                    SwitchedDeviceKind kind;
                    if (reader.IsDBNull(4) || !DeviceNames.TryParseKind(reader.GetString(5), out kind))
                    {
                        return null;
                    }

                    return new Device(id, type, name, reader.GetInt32(4), kind);

                default:
                    return reader.IsDBNull(6)
                        ? null
                        : new Device(id, type, name, reader.GetInt32(6), null);
            }
        }

        /// <summary>
        /// Разбирает дату и время, сохраненные в БД.
        /// </summary>
        /// <param name="text">Дата и время в формате ISO 8601.</param>
        /// <returns>Дата и время.</returns>
        private static DateTime ParseDateTime(string text)
        {
            DateTime value;
            if (DateTime.TryParseExact(
                    text,
                    DateTimeFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out value)
                || DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
            {
                return value;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Преобразует необязательное значение в значение параметра запроса.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <returns>Значение или <see cref="DBNull.Value"/>.</returns>
        private static object ToDbValue(double? value)
        {
            return value.HasValue
                ? (object)value.Value
                : DBNull.Value;
        }

        /// <summary>
        /// Создает команду в транзакции.
        /// </summary>
        /// <param name="connection">Соединение.</param>
        /// <param name="transaction">Транзакция.</param>
        /// <param name="sql">Текст запроса.</param>
        /// <returns>Команда.</returns>
        private static SqliteCommand CreateCommand(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string sql)
        {
            SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = sql;
            return command;
        }

        /// <summary>
        /// Открывает соединение с БД и включает проверку внешних ключей.
        /// </summary>
        /// <returns>Открытое соединение.</returns>
        private SqliteConnection OpenConnection()
        {
            SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys = ON";
                command.ExecuteNonQuery();
            }

            return connection;
        }
    }
}

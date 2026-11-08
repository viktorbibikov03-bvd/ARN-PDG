-- Физическая модель базы данных АС «ВРДО ПДГ» (СУБД SQLite)
-- Внешние ключи проверяются только при PRAGMA foreign_keys = ON,
-- которую ModelDataRepository выполняет при открытии каждого соединения.
PRAGMA foreign_keys = ON;

CREATE TABLE Power_system (
    id_ps    INTEGER PRIMARY KEY,
    name_ps  TEXT    NOT NULL UNIQUE
);

CREATE TABLE Control_point (
    id_cp    INTEGER PRIMARY KEY,
    id_ps    INTEGER NOT NULL,
    name_cp  TEXT    NOT NULL UNIQUE,
    CONSTRAINT FK_Control_point_Power_system FOREIGN KEY (id_ps)
        REFERENCES Power_system (id_ps) ON DELETE RESTRICT
);

CREATE TABLE Node (
    id_node  INTEGER PRIMARY KEY,
    id_cp    INTEGER NOT NULL,
    is_main  INTEGER NOT NULL CHECK (is_main IN (0, 1)),
    CONSTRAINT FK_Node_Control_point FOREIGN KEY (id_cp)
        REFERENCES Control_point (id_cp) ON DELETE RESTRICT
);

CREATE TABLE Device (
    id_dev    INTEGER PRIMARY KEY,
    type_dev  TEXT    NOT NULL CHECK (type_dev IN ('Управляемые', 'Коммутируемые', 'Генераторы')),
    name_dev  TEXT    NOT NULL
);

CREATE TABLE Controlled_device (
    id_dev    INTEGER PRIMARY KEY,
    num_node  INTEGER NOT NULL UNIQUE,
    CONSTRAINT FK_Controlled_device_Device FOREIGN KEY (id_dev)
        REFERENCES Device (id_dev) ON DELETE CASCADE
);

CREATE TABLE Switched_device (
    id_dev       INTEGER PRIMARY KEY,
    num_reactor  INTEGER NOT NULL UNIQUE,
    kind_sd      TEXT    NOT NULL CHECK (kind_sd IN ('Реактор', 'БСК')),
    CONSTRAINT FK_Switched_device_Device FOREIGN KEY (id_dev)
        REFERENCES Device (id_dev) ON DELETE CASCADE
);

CREATE TABLE Generator (
    id_dev    INTEGER PRIMARY KEY,
    num_node  INTEGER NOT NULL UNIQUE,
    CONSTRAINT FK_Generator_Device FOREIGN KEY (id_dev)
        REFERENCES Device (id_dev) ON DELETE CASCADE
);

CREATE TABLE PQ_point (
    id_dev     INTEGER NOT NULL,
    num_point  INTEGER NOT NULL,
    p_pq       REAL    NOT NULL,
    q_min_pq   REAL    NOT NULL,
    q_max_pq   REAL    NOT NULL,
    CONSTRAINT PK_PQ_point PRIMARY KEY (id_dev, num_point),
    CONSTRAINT FK_PQ_point_Generator FOREIGN KEY (id_dev)
        REFERENCES Generator (id_dev) ON DELETE CASCADE
);

CREATE TABLE Efficiency_table (
    id_cp         INTEGER NOT NULL,
    id_dev        INTEGER NOT NULL,
    priority      INTEGER,
    priority_pur  INTEGER,
    CONSTRAINT PK_Efficiency_table PRIMARY KEY (id_cp, id_dev),
    CONSTRAINT UQ_Efficiency_table_priority UNIQUE (id_cp, priority),
    CONSTRAINT UQ_Efficiency_table_priority_pur UNIQUE (id_cp, priority_pur),
    CONSTRAINT CK_Efficiency_table_priority CHECK (priority IS NOT NULL OR priority_pur IS NOT NULL),
    CONSTRAINT FK_Efficiency_table_Control_point FOREIGN KEY (id_cp)
        REFERENCES Control_point (id_cp) ON DELETE CASCADE,
    CONSTRAINT FK_Efficiency_table_Device FOREIGN KEY (id_dev)
        REFERENCES Device (id_dev) ON DELETE CASCADE
);

CREATE TABLE Voltage_schedule (
    id_cp        INTEGER NOT NULL,
    hour         INTEGER NOT NULL CHECK (hour BETWEEN 1 AND 24),
    u_low        REAL    NOT NULL,
    u_high       REAL    NOT NULL,
    received_at  TEXT    NOT NULL,
    CONSTRAINT PK_Voltage_schedule PRIMARY KEY (id_cp, hour),
    CONSTRAINT FK_Voltage_schedule_Control_point FOREIGN KEY (id_cp)
        REFERENCES Control_point (id_cp) ON DELETE CASCADE
);

CREATE TABLE Node_voltage (
    id_node  INTEGER NOT NULL,
    hour     INTEGER NOT NULL CHECK (hour BETWEEN 1 AND 24),
    u_node   REAL    NOT NULL,
    CONSTRAINT PK_Node_voltage PRIMARY KEY (id_node, hour),
    CONSTRAINT FK_Node_voltage_Node FOREIGN KEY (id_node)
        REFERENCES Node (id_node) ON DELETE CASCADE
);

CREATE TABLE Device_mode (
    id_dev    INTEGER NOT NULL,
    hour      INTEGER NOT NULL CHECK (hour BETWEEN 1 AND 24),
    is_on     INTEGER NOT NULL CHECK (is_on IN (0, 1)),
    q_dev     REAL,
    q_min     REAL,
    q_max     REAL,
    u_set     REAL,
    p_gen     REAL,
    is_fixed  INTEGER NOT NULL DEFAULT 0 CHECK (is_fixed IN (0, 1)),
    CONSTRAINT PK_Device_mode PRIMARY KEY (id_dev, hour),
    CONSTRAINT FK_Device_mode_Device FOREIGN KEY (id_dev)
        REFERENCES Device (id_dev) ON DELETE CASCADE
);

-- Индексы по внешним ключам, которые не являются первым столбцом первичного ключа
CREATE INDEX IX_Control_point_id_ps ON Control_point (id_ps);
CREATE INDEX IX_Node_id_cp ON Node (id_cp);
CREATE INDEX IX_Efficiency_table_id_dev ON Efficiency_table (id_dev);

-- Не более одного основного узла у КП
CREATE UNIQUE INDEX UX_Node_main ON Node (id_cp) WHERE is_main = 1;

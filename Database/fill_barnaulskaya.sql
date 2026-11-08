-- Заполнение БД АС «ВРДО ПДГ» по КП «Барнаульская»
-- СУБД SQLite. Скрипт можно запускать повторно: существующие записи не дублируются,
-- таблица эффективности КП перезаписывается целиком.
-- Коды (id_ps, id_cp, id_dev) не задаются вручную, а определяются по наименованиям
-- и номерам в МТ, поэтому скрипт работает и на пустой, и на уже заполненной БД.
--
-- Скрипт рассчитан на выполнение в DB Browser for SQLite (вкладка «Выполнить SQL»):
-- программа сама выполняет его в транзакции, поэтому BEGIN/COMMIT в скрипте нет.
-- После выполнения нажмите «Записать изменения» (Ctrl+S), чтобы сохранить данные в файл БД.
-- Проверка внешних ключей в DB Browser включена по умолчанию
-- (Правка → Настройки → База данных → «Внешние ключи»).

-- 1. Энергосистема и контрольный пункт
INSERT INTO Power_system (name_ps)
VALUES ('ОЭС Сибири')
ON CONFLICT (name_ps) DO NOTHING;

INSERT INTO Control_point (id_ps, name_cp)
SELECT id_ps, 'Барнаульская' FROM Power_system WHERE name_ps = 'ОЭС Сибири'
ON CONFLICT (name_cp) DO NOTHING;

-- 2. Узлы КП: 564 – основной (в нем выполняется ВРДО), 563 – дополнительный
INSERT INTO Node (id_node, id_cp, is_main)
SELECT 563, id_cp, 0 FROM Control_point WHERE name_cp = 'Барнаульская'
ON CONFLICT (id_node) DO UPDATE SET id_cp = excluded.id_cp, is_main = excluded.is_main;

INSERT INTO Node (id_node, id_cp, is_main)
SELECT 564, id_cp, 1 FROM Control_point WHERE name_cp = 'Барнаульская'
ON CONFLICT (id_node) DO UPDATE SET id_cp = excluded.id_cp, is_main = excluded.is_main;

-- 3. Исходный перечень СКРМ КП в порядке приоритета
--    num – номер узла (управляемые), номер реактора (коммутируемые) или номер генератора
CREATE TEMP TABLE src (
    priority  INTEGER PRIMARY KEY,
    type_dev  TEXT    NOT NULL,
    name_dev  TEXT    NOT NULL,
    num       INTEGER NOT NULL,
    kind_sd   TEXT,
    id_dev    INTEGER
);

INSERT INTO src (priority, type_dev, name_dev, num, kind_sd) VALUES
 ( 1, 'Управляемые',   'ПС 220 кВ Светлая СШ 220 кВ',                 520, NULL),
 ( 2, 'Управляемые',   'ЗаряСТК1 (выведено из эксплуатации)',          633, NULL),
 ( 3, 'Управляемые',   'ПС 500 кВ Томская УШР-500',                  6318, NULL),
 ( 4, 'Управляемые',   'ПС 500 кВ Барабинская ВЛ-532',               6273, NULL),
 ( 5, 'Коммутируемые', 'Р-1 ПС 500 кВ Барнаульская',                    9, 'Реактор'),
 ( 6, 'Коммутируемые', 'Р-2 ПС 500 кВ Барнаульская',                   10, 'Реактор'),
 ( 7, 'Коммутируемые', 'Р-3 ПС 500 кВ Барнаульская',                   11, 'Реактор'),
 ( 8, 'Коммутируемые', 'Р3-1104 ПС 1150 кВ Алтай',                     12, 'Реактор'),
 ( 9, 'Коммутируемые', 'Р4-1104 ПС 1150 кВ Алтай',                     13, 'Реактор'),
 (10, 'Коммутируемые', 'Р1-1106 ПС 1150 кВ Алтай',                     14, 'Реактор'),
 (11, 'Коммутируемые', 'Р2-1106 ПС 1150 кВ Алтай',                     15, 'Реактор'),
 (12, 'Коммутируемые', 'Р-532 ПС 500 кВ Заря',                          6, 'Реактор'),
 (13, 'Коммутируемые', 'Р-1 ПС 500 кВ Рубцовская',                      7, 'Реактор'),
 (14, 'Коммутируемые', 'Р-2 ПС 500 кВ Рубцовская',                      8, 'Реактор'),
 (15, 'Коммутируемые', 'Р-541 ПС 500 кВ Новокузнецкая',                19, 'Реактор'),
 (16, 'Коммутируемые', 'Р-542 ПС 500 кВ Новокузнецкая',                20, 'Реактор'),
 (17, 'Коммутируемые', 'БСК-1-220 ПС 220 кВ Светлая',                 125, 'БСК'),
 (18, 'Коммутируемые', 'БСК-2-220 ПС 220 кВ Светлая',                 126, 'БСК'),
 (19, 'Коммутируемые', 'ПС 1150 кВ Итатская 1РШ',                      23, 'Реактор'),
 (20, 'Коммутируемые', 'ПС 1150 кВ Итатская 2РШ',                      24, 'Реактор'),
 (21, 'Коммутируемые', 'ПС 1150 кВ Итатская 3РШ',                      25, 'Реактор'),
 (22, 'Коммутируемые', 'ПС 1150 кВ Итатская 4РШ',                      26, 'Реактор'),
 (23, 'Коммутируемые', 'ПС 1150 кВ Итатская 5РШ',                      27, 'Реактор'),
 (24, 'Коммутируемые', 'Р-1 ПС 500 кВ Ново-Анжерская',                 16, 'Реактор'),
 (25, 'Коммутируемые', 'Р-2 ПС 500 кВ Ново-Анжерская',                 17, 'Реактор'),
 (26, 'Коммутируемые', 'Р-534 ПС 500 кВ Барабинская',                   5, 'Реактор'),
 (27, 'Коммутируемые', 'ШР-500 ПС 500 кВ Томская',                     18, 'Реактор'),
 (28, 'Генераторы',    'Саяно-Шушенская ГЭС, Г1',                     291, NULL),
 (29, 'Генераторы',    'Саяно-Шушенская ГЭС, Г2',                     292, NULL),
 (30, 'Генераторы',    'Саяно-Шушенская ГЭС, Г3',                     293, NULL),
 (31, 'Генераторы',    'Саяно-Шушенская ГЭС, Г4',                     294, NULL),
 (32, 'Генераторы',    'Саяно-Шушенская ГЭС, Г5',                     295, NULL),
 (33, 'Генераторы',    'Саяно-Шушенская ГЭС, Г6',                     296, NULL),
 (34, 'Генераторы',    'Саяно-Шушенская ГЭС, Г7',                     297, NULL),
 (35, 'Генераторы',    'Саяно-Шушенская ГЭС, Г8',                     298, NULL),
 (36, 'Генераторы',    'Саяно-Шушенская ГЭС, Г9',                     299, NULL),
 (37, 'Генераторы',    'Саяно-Шушенская ГЭС, Г10',                    300, NULL),
 (38, 'Генераторы',    'Березовская ГРЭС, Блок 1',                     27, NULL),
 (39, 'Генераторы',    'Березовская ГРЭС, Блок 2',                     28, NULL),
 (40, 'Генераторы',    'Березовская ГРЭС, Блок 3',                     29, NULL),
 (41, 'Генераторы',    'Барнаульская ТЭЦ-2, ТГ-5',                     13, NULL),
 (42, 'Генераторы',    'Барнаульская ТЭЦ-2, ТГ-6',                     14, NULL),
 (43, 'Генераторы',    'Барнаульская ТЭЦ-2, ТГ-7',                     15, NULL),
 (44, 'Генераторы',    'Барнаульская ТЭЦ-2, ТГ-8',                     16, NULL),
 (45, 'Генераторы',    'Барнаульская ТЭЦ-2, ТГ-9',                     17, NULL),
 (46, 'Генераторы',    'Барнаульская ТЭЦ-3, ТГ-1',                     18, NULL),
 (47, 'Генераторы',    'Барнаульская ТЭЦ-3, ТГ-2',                     19, NULL),
 (48, 'Генераторы',    'Барнаульская ТЭЦ-3, ТГ-3',                     20, NULL),
 (49, 'Генераторы',    'Бийская ТЭЦ, ТГ-3',                            31, NULL),
 (50, 'Генераторы',    'Бийская ТЭЦ, ТГ-4',                            32, NULL),
 (51, 'Генераторы',    'Бийская ТЭЦ, ТГ-5',                            33, NULL),
 (52, 'Генераторы',    'Бийская ТЭЦ, ТГ-6',                            34, NULL),
 (53, 'Генераторы',    'Бийская ТЭЦ, ТГ-7',                            35, NULL),
 (54, 'Генераторы',    'Бийская ТЭЦ, ТГ-8',                            36, NULL),
 (55, 'Генераторы',    'ТЭЦ АКХЗ, ТГ-1',                              336, NULL),
 (56, 'Генераторы',    'ТЭЦ АКХЗ, ТГ-2',                              337, NULL),
 (57, 'Генераторы',    'ТЭЦ АКХЗ, ТГ-3',                              338, NULL);

-- 4. СКРМ, уже имеющиеся в БД, находятся по номеру в МТ
UPDATE src SET id_dev = (SELECT id_dev FROM Controlled_device c WHERE c.num_node = src.num)
 WHERE type_dev = 'Управляемые';
UPDATE src SET id_dev = (SELECT id_dev FROM Switched_device s WHERE s.num_reactor = src.num)
 WHERE type_dev = 'Коммутируемые';
UPDATE src SET id_dev = (SELECT id_dev FROM Generator g WHERE g.num_node = src.num)
 WHERE type_dev = 'Генераторы';

-- 5. Новым СКРМ назначаются коды после максимального существующего
WITH n AS (
    SELECT priority, ROW_NUMBER() OVER (ORDER BY priority) AS rn
      FROM src WHERE id_dev IS NULL
)
UPDATE src
   SET id_dev = (SELECT COALESCE(MAX(id_dev), 0) FROM Device)
              + (SELECT rn FROM n WHERE n.priority = src.priority)
 WHERE id_dev IS NULL;

-- 6. Справочник СКРМ и его подтипы
INSERT INTO Device (id_dev, type_dev, name_dev)
SELECT id_dev, type_dev, name_dev FROM src
 WHERE id_dev NOT IN (SELECT id_dev FROM Device);

INSERT INTO Controlled_device (id_dev, num_node)
SELECT id_dev, num FROM src
 WHERE type_dev = 'Управляемые' AND id_dev NOT IN (SELECT id_dev FROM Controlled_device);

INSERT INTO Switched_device (id_dev, num_reactor, kind_sd)
SELECT id_dev, num, kind_sd FROM src
 WHERE type_dev = 'Коммутируемые' AND id_dev NOT IN (SELECT id_dev FROM Switched_device);

INSERT INTO Generator (id_dev, num_node)
SELECT id_dev, num FROM src
 WHERE type_dev = 'Генераторы' AND id_dev NOT IN (SELECT id_dev FROM Generator);

-- 7. Таблица эффективности КП: действующий приоритет = приоритет по умолчанию (ПУР)
DELETE FROM Efficiency_table
 WHERE id_cp = (SELECT id_cp FROM Control_point WHERE name_cp = 'Барнаульская');

INSERT INTO Efficiency_table (id_cp, id_dev, priority, priority_pur)
SELECT cp.id_cp, s.id_dev, s.priority, s.priority
  FROM src s, Control_point cp
 WHERE cp.name_cp = 'Барнаульская';

DROP TABLE src;

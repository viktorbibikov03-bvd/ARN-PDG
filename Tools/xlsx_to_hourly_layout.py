"""Формирование схемы таблиц по часам (hourly-layout.json) из файла «Анализ.xlsx».

Схема описывает формы «Напряжение по часам» и «Сравнение расчетов» АС «ВРДО ПДГ»: энергосистемы
(листы «U-500 …»), блоки КП (строки «Час» на листе), разделы КП («U сш», «СКРМ», «U ген») и столбцы
в том порядке, в котором они расположены на листе. Вид столбца определяется по формуле RTD первой
строки данных, номер узла или реактора — по строке номеров блока. Скрытые столбцы пропускаются.

Запуск:
    python xlsx_to_hourly_layout.py Анализ.xlsx ../View/hourly-layout.json

Требуется пакет openpyxl.
"""

import json
import re
import sys

import openpyxl
from openpyxl.utils import column_index_from_string

# Листы файла и наименования энергосистем.
SHEETS = {
    "U-500 ОЭС Сибири": "ОЭС Сибири",
    "U-500 ЕЭС Казахстана": "ЕЭС Казахстана",
    "U-500 Внешние ЭС": "Внешние ЭС",
}

# Границы напряжения на шинах из условного форматирования листов, кВ.
LOWER_VOLTAGE_BOUND = 500
UPPER_VOLTAGE_BOUND = 525

# Диапазон напряжений, выделяемых цветом при выходе за границы графика, кВ: узлы другого
# класса напряжения (не 500 кВ) не выделяются.
HIGHLIGHT_MIN_VOLTAGE = 400
HIGHLIGHT_MAX_VOLTAGE = 600


def bus_voltage(name, nodes, lower=None, upper=None):
    """Возвращает КП с разделом «U сш» по узлам и, при необходимости, своими границами напряжения."""
    control_point = {"name": name}
    if lower is not None:
        control_point["lowerVoltageBound"] = lower
        control_point["upperVoltageBound"] = upper
    control_point["groups"] = [{"caption": "U сш",
                                "columns": [{"kind": "BusVoltage", "number": node} for node in nodes]}]
    return control_point


# Исправленные номера узлов: (КП, номер в файле «Анализ.xlsx») -> номер узла МТ.
NODE_CORRECTIONS = {
    ("Ключи", 110): 6483,
    ("Иркутская", 133): 6490,
}

# КП, которых нет в файле «Анализ.xlsx»: добавляются справа от указанного КП.
EXTRA_CONTROL_POINTS = {
    "Томская": [bus_voltage("Юрга", [6230, 6231])],
}

# Блоки КП, которых нет в файле: добавляются под блоком, содержащим указанный КП.
# Для КП 220 кВ заданы свои границы напряжения на шинах.
EXTRA_BLOCKS = {
    "Усть-Илимская ГЭС": [{"controlPoints": [
        bus_voltage("БЦБК", [197], 220, 252),
        bus_voltage("Петровск-Забайкальская", [6240, 6241, 6253, 6254], 220, 252),
    ]}],
}

# Ссылка формулы RTD на номер узла: "ny="&D$4.
NODE_REFERENCE = re.compile(r'ny="&\$?([A-Z]+)\$?\d+')

# Напряжение генератора: =IF(vras<>0,vzd,0) по вспомогательным столбцам.
GENERATOR_VOLTAGE = re.compile(r"^=IF\(\$?[A-Z]+\$?\d+<>0,\$?[A-Z]+\$?\d+,0\)$")


def get_spans(sheet, row):
    """Возвращает диапазоны столбцов объединенных и одиночных заполненных ячеек строки."""
    spans = [(cells.min_col, cells.max_col) for cells in sheet.merged_cells.ranges
             if cells.min_row == row == cells.max_row]
    for column in range(1, sheet.max_column + 1):
        if sheet.cell(row, column).value is not None and not any(a <= column <= b for a, b in spans):
            spans.append((column, column))
    return sorted(spans)


def get_number(sheet, row, column):
    """Возвращает номер узла или реактора из ячейки строки номеров или None."""
    value = sheet.cell(row, column).value
    if value is None or str(value).strip() == "":
        return None
    return int(float(str(value).strip()))


def get_hidden_columns(sheet):
    """Возвращает номера скрытых столбцов листа."""
    hidden = set()
    for dimension in sheet.column_dimensions.values():
        if dimension.hidden and dimension.min and dimension.max:
            hidden.update(range(dimension.min, dimension.max + 1))
    return hidden


def read_column(sheet, number_row, column, formula, warnings):
    """Определяет вид и номер столбца по формуле первой строки данных."""
    own_number = get_number(sheet, number_row, column)
    if GENERATOR_VOLTAGE.match(formula.replace(" ", "")):
        return {"kind": "GeneratorVoltage", "number": own_number}

    reference = NODE_REFERENCE.search(formula)
    reference_number = get_number(sheet, number_row, column_index_from_string(reference.group(1))) \
        if reference else None
    number = own_number if own_number is not None else reference_number
    if '"Reactors","sta"' in formula:
        return {"kind": "Reactor", "number": own_number}
    if '"node","vras"' in formula:
        return {"kind": "BusVoltage", "number": number}
    if '"node","vzd"' in formula:
        if own_number is not None and reference_number is not None and own_number != reference_number:
            warnings.append(f"{sheet.title}: формула ссылается на узел {reference_number}, "
                            f"в строке номеров {own_number}; взят {own_number}")
        return {"kind": "SetVoltage", "number": number}
    if '"node","qmin"' in formula:
        return {"kind": "ReactivePower", "number": number}

    warnings.append(f"{sheet.title}: не распознана формула {formula[:60]}")
    return None


def read_sheet(sheet, warnings):
    """Считывает блоки КП листа."""
    hidden = get_hidden_columns(sheet)
    header_rows = [row for row in range(1, sheet.max_row + 1)
                   if str(sheet.cell(row, 1).value or "").strip().lower().startswith("час")]
    blocks = []
    for header_row in header_rows:
        section_row, number_row, data_row = header_row + 1, header_row + 2, header_row + 3
        control_points = []
        for first, last in get_spans(sheet, header_row):
            if first == 1:
                continue
            groups = []
            for section_first, section_last in get_spans(sheet, section_row):
                if section_last < first or section_first > last:
                    continue
                columns = []
                for column in range(max(section_first, first), min(section_last, last) + 1):
                    formula = sheet.cell(data_row, column).value
                    if column in hidden or not isinstance(formula, str) or not formula.startswith("="):
                        continue
                    item = read_column(sheet, number_row, column, formula, warnings)
                    if item is not None and item["number"] is not None:
                        columns.append(item)
                if columns:
                    caption = str(sheet.cell(section_row, section_first).value).strip()
                    groups.append({"caption": caption, "columns": columns})
            if groups:
                name = str(sheet.cell(header_row, first).value).strip()
                control_points.append({"name": name, "groups": groups})
        if control_points:
            blocks.append({"controlPoints": control_points})
    return blocks


def add_extra_objects(blocks):
    """Исправляет номера узлов и добавляет в блоки листа КП и блоки, которых нет в файле «Анализ.xlsx»."""
    result = []
    for block in blocks:
        control_points = []
        for control_point in block["controlPoints"]:
            for group in control_point["groups"]:
                for column in group["columns"]:
                    key = (control_point["name"], column["number"])
                    column["number"] = NODE_CORRECTIONS.get(key, column["number"])
            control_points.append(control_point)
            control_points.extend(EXTRA_CONTROL_POINTS.get(control_point["name"], []))
        result.append({"controlPoints": control_points})
        for control_point in block["controlPoints"]:
            result.extend(EXTRA_BLOCKS.get(control_point["name"], []))
    return result


def main():
    if len(sys.argv) != 3:
        print(__doc__)
        sys.exit(1)

    workbook = openpyxl.load_workbook(sys.argv[1])
    warnings = []
    layout = {
        "lowerVoltageBound": LOWER_VOLTAGE_BOUND,
        "upperVoltageBound": UPPER_VOLTAGE_BOUND,
        "highlightMinVoltage": HIGHLIGHT_MIN_VOLTAGE,
        "highlightMaxVoltage": HIGHLIGHT_MAX_VOLTAGE,
        "powerSystems": [{"name": name, "blocks": add_extra_objects(read_sheet(workbook[sheet], warnings))}
                         for sheet, name in SHEETS.items()],
    }
    text = json.dumps(layout, ensure_ascii=False, indent=2)
    text = re.sub(r'\{\s*"kind": "(\w+)",\s*"number": (\d+)\s*\}', r'{ "kind": "\1", "number": \2 }', text)
    with open(sys.argv[2], "w", encoding="utf-8", newline="\n") as output:
        output.write(text + "\n")

    for warning in warnings:
        print("Предупреждение:", warning)
    for power_system in layout["powerSystems"]:
        print(power_system["name"] + ": блоков " + str(len(power_system["blocks"])))


if __name__ == "__main__":
    main()

import csv
from dataclasses import dataclass
from collections import Counter

@dataclass
class LocationData:
    pretty_name: str
    flag_name: str
    item_name: str

if __name__ == "__main__":
    accum: list[str] = []
    location_datas: dict[str, list[LocationData]] = {}
    with open("location_names.csv", "r") as csv_file:
        reader = csv.reader(csv_file)
        current_category: str | None = None
        for row in reader:
            if len(row) == 0:
                continue
            if len(row) == 1:
                current_category = row[0]
                location_datas[current_category] = []
                continue

            location_datas[str(current_category)].append(LocationData(row[1], row[2], row[0]))

    accum.append("// Generated using prebuild.py")

    b: Counter[str] = Counter()
    for a in location_datas.values():
        b.update(map(lambda x: x.pretty_name, a))

    for a, c in b.items():
        if c > 1:
            print(f"{a}: {c}")

    accum.append("using System.Collections.Generic;")
    accum.append("namespace CoDArchipelago{")
    accum.append("static class Data{")

    for (category, datas) in location_datas.items():
        accum.append(f"public static readonly Dictionary<string, string> {category}Locations = new()" "{")
        for data in datas:
            accum.append('{' + f'"LOCATION_{data.flag_name}", "{data.pretty_name}"' + '},')
        accum.append("};")

    for (category, datas) in location_datas.items():
        accum.append(f"public static readonly Dictionary<string, string> {category}Items = new()" "{")
        for data in datas:
            accum.append('{' + f'"{data.flag_name}", "{data.item_name}"' + '},')
        accum.append("};")

    accum.append("}}")

    with open("Data.cs", "w") as out_cs:
        out_cs.write("\n".join(accum))
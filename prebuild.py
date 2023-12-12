import csv
from dataclasses import dataclass
from collections import Counter
from typing import cast

@dataclass
class LocationData:
    location_name: str | None
    flag_name: str
    item_name: str

class SerializedLocationData:
    has_locations: bool
    locations: dict[str, str]
    items: dict[str, str]

    def __init__(self, datas: list[LocationData]):
        self.locations = {}
        self.items = {}
        self.has_locations = datas[0].location_name is not None
        for data in datas:
            if self.has_locations:
                self.locations[f"LOCATION_{data.flag_name}"] = cast(str, data.location_name)
            self.items[data.flag_name] = data.item_name

def serialize_dictionary(d: dict[str, str], name: str) -> list[str]:
    ret: list[str] = []
    ret.append(f"public static readonly Dictionary<string, string> {name} = new()" "{")

    for k, v in d.items():
        ret.append('{' + f'"{k}", "{v}"' + '},')

    ret.append("};")
    return ret

def get_items(location_datas: dict[str, list[LocationData]]) -> list[str]:
    accum: list[str] = []

    all_locations: dict[str, str] = {}
    all_items: dict[str, str] = {}

    cats = {k: SerializedLocationData(v) for k, v in location_datas.items()}
    for cat, serialized in cats.items():
        if serialized.has_locations:
            all_locations.update(serialized.locations)
            accum += serialize_dictionary(serialized.locations, f"{cat}Locations")
            accum += serialize_dictionary({v: k for k, v in serialized.locations.items()}, f"{cat}LocationsByName")

        all_items.update(serialized.items)
        accum += serialize_dictionary(serialized.items, f"{cat}Items")
        accum += serialize_dictionary({v: k for k, v in serialized.items.items()}, f"{cat}ItemsByName")

    accum += serialize_dictionary(all_locations, "allLocations")
    accum += serialize_dictionary({v: k for k, v in all_locations.items()}, "allLocationsByName")
    accum += serialize_dictionary(all_items, "allItems")
    accum += serialize_dictionary({v: k for k, v in all_items.items()}, "allItemsByName")

    return accum

if __name__ == "__main__":
    accum: list[str] = []
    location_datas: dict[str, list[LocationData]] = {}
    with open("cavern_of_dreams_ap_logic/location_names.csv", "r") as csv_file:
        reader = csv.reader(csv_file)
        current_category: str | None = None
        for row in reader:
            if len(row) == 0:
                continue
            if len(row) == 1:
                current_category = row[0]
                location_datas[current_category] = []
                continue
            if len(row) == 2:
                location_datas[str(current_category)].append(LocationData(None, row[1], row[0]))
            if len(row) == 3:
                location_datas[str(current_category)].append(LocationData(row[1], row[2], row[0]))

    accum.append("// Generated using prebuild.py")

    b: Counter[str] = Counter()
    for a in location_datas.values():
        if a[0].location_name is not None:
            b.update(map(lambda x: cast(str, x.location_name), a))

    for a, c in b.items():
        if c > 1:
            print(f"{a}: {c}")

    accum.append("using System.Collections.Generic;")
    accum.append("namespace CoDArchipelago{")
    accum.append("static class Data{")

    accum += get_items(location_datas)

    accum.append("}}")

    with open("Generated.cs", "w") as out_cs:
        out_cs.write("\n".join(accum))
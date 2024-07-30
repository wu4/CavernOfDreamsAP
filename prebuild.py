from collections.abc import Iterable
from cavern_of_dreams_ap_logic import all_locations

def start(name: str):
    return f"public static readonly Dictionary<string,string> {name}=new()" + "{"

def categories_as_code(type: str, container: Iterable[tuple[str, Iterable[tuple[str, str]]]]) -> list[str]:
    all: list[str] = []
    normal: list[str] = []
    all.append(start(f"all{type}"))
    for category, rows in container:
        normal.append(start(f"{category}{type}"))
        for a, b in rows:
            entry = '{' + f'"{a}","{b}"' + '},'
            normal.append(entry)
            all.append(entry)
        normal.append("};")
    all.append("};")
    normal.extend(all)
    return normal

if __name__ == "__main__":
    accum: list[str] = []

    accum.append("// Generated using prebuild.py")

    accum.append("using System.Collections.Generic;")
    accum.append("namespace CoDArchipelago{")
    accum.append("static class Data{")

    types = {
        "Items": all_locations.by_flag(all_locations.all_items_with_flags()),
        "ItemsByName": all_locations.all_items_with_flags(),
        "Locations": all_locations.by_flag(all_locations.all_locations_with_flags()),
        "LocationsByName": all_locations.all_locations_with_flags(),
    }

    for type, container in types.items():
        accum.extend(categories_as_code(type, container))

    accum.append("}}")

    with open("Generated.cs", "w") as out_cs:
        _ = out_cs.write("\n".join(accum))
    print("Done")

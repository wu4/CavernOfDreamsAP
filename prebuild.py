from cavern_of_dreams_ap_logic.csv_parsing import read_locations_csv, parse, FlagListIteration

def serialize(item: FlagListIteration) -> list[str]:
    ret: list[str] = []
    type = ""
    if item.type == "Item": type = "Items"
    elif item.type == "ItemByName": type = "ItemsByName"
    elif item.type == "Location": type = "Locations"
    elif item.type == "LocationByName": type = "LocationsByName"

    name = f"{item.category}{type}" if item.category is not None else f"all{type}"
    ret.append(f"public static readonly Dictionary<string,string> {name}=new()" "{")

    for k, v in item.flag_list.items():
        ret.append('{' + f'"{k}","{v}"' + '},')

    ret.append("};")
    return ret

if __name__ == "__main__":
    accum: list[str] = []
    location_datas = read_locations_csv("../cavern_of_dreams_ap_logic/location_names.csv")

    accum.append("// Generated using prebuild.py")

    accum.append("using System.Collections.Generic;")
    accum.append("namespace CoDArchipelago{")
    accum.append("static class Data{")

    accum += parse(location_datas, serialize, include_by_name = True)

    accum.append("}}")

    with open("Generated.cs", "w") as out_cs:
        out_cs.write("\n".join(accum))
    print("Done")
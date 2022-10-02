import os
import pathlib
import json

data_dir = "src/GrillBot/GrillBot.Data/Resources"


def get_json_files() -> list:
    result = []
    for root, _, filenames in os.walk(data_dir):
        for file in filter(lambda x: pathlib.Path(x).suffix == ".json", filenames):
            result.append(os.path.join(root.replace("\\", "/"), file))
    return result


def group_files(files: list) -> dict:
    result = {}
    for file in files:
        filename_parts = os.path.basename(file).split(".")
        if len(filename_parts) < 3:
            continue
        if filename_parts[0] not in result:
            result[filename_parts[0]] = []
        result[filename_parts[0]].append(file)
    return result


def get_and_check_json(filename: str) -> dict:
    print(f"Reading file {os.path.basename(filename)}")
    with open(filename, encoding="utf-8") as json_file:
        json_data = json_file.read().encode().decode("utf-8-sig")
        return json.loads(json_data)


def read_json_keys(json: dict, id: str = "") -> list:
    result = []

    for item in json:
        next_id = f"{id}/{item}"
        if type(json[item]) == dict:
            result += read_json_keys(json[item], next_id)
        else:
            result.append(next_id)

    return result


def cross_check_json(first_json: dict, second_json: dict):
    first_json_basename = os.path.basename(str(first_json["filename"]))
    second_json_basename = os.path.basename(str(second_json["filename"]))
    print(f"Cross-checking files {first_json_basename} and {second_json_basename}")

    first_json_keys = read_json_keys(first_json["content"])
    second_json_keys = read_json_keys(second_json["content"])

    for value in first_json_keys:
        if value not in second_json_keys:
            raise ValueError(
                f'Key "{value}" was found in {first_json_basename}, but wasn\'t found in {second_json_basename}'
            )

    for value in second_json_keys:
        if value not in first_json_keys:
            raise ValueError(
                f'Key "{value}" was found in {second_json_basename}, but wasn\'t found in {first_json_basename}'
            )

    print(
        f"Cross-check of {first_json_basename} and {second_json_basename} - Success"
    )


def check_group(group: str, files: list):
    print(f"Checking group {group} (Files: {len(files)})")
    json_files = list(
        map(lambda x: {"filename": x, "content": get_and_check_json(x)}, files)
    )
    checked_groups = []
    for file in json_files:
        for another_file in json_files:
            key1 = f'{file["filename"]}-{another_file["filename"]}'
            key2 = f'{another_file["filename"]}-{file["filename"]}'

            if (
                file["filename"] == another_file["filename"]
                or key1 in checked_groups
                or key2 in checked_groups
            ):
                continue
            cross_check_json(file, another_file)
            checked_groups.append(key1)
            checked_groups.append(key2)


try:
    files = get_json_files()
    grouped = group_files(files)
    for group in grouped:
        check_group(group, grouped[group])
except Exception as e:
    print(e)
    exit(1)

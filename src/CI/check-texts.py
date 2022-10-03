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
        return json.loads(json_file.read().encode().decode("utf-8-sig"))


def read_json_keys(json: dict, id: str = "") -> list:
    result = []

    for item in json:
        next_id = f"{id}/{item}"
        if type(json[item]) == dict:
            result += read_json_keys(json[item], next_id)
        else:
            result.append(next_id)

    return result


def flattern(json: dict, id: str = "") -> dict:
    result = {}
    for item in json:
        next_id = f"{id}/{item}"
        if type(json[item]) == dict:
            result.update(flattern(json[item], next_id))
        else:
            result[next_id] = json[item]
    return result


def cross_check_keys(
    first_json_filename: str,
    first_json: dict,
    second_json_filename: str,
    second_json: dict,
):
    print(
        f"Cross-checking file structure of files {first_json_filename} and {second_json_filename}"
    )

    def raise_err(val: str, swapped: bool):
        file1 = second_json_filename if swapped else first_json_filename
        file2 = first_json_filename if swapped else second_json_filename
        raise ValueError(
            f'Key "{val}" was found in {file1}, but wasn\'t found in {file2}'
        )

    for val in first_json:
        if val not in second_json:
            raise_err(val, False)
    for val in second_json:
        if val not in first_json:
            raise_err(val, True)

def check_group(group: str, files: list):
    print(f"Checking group {group} (Files: {len(files)})")
    json_files = list(
        map(lambda x: {"filename": x, "content": get_and_check_json(x)}, files)
    )
    checked_groups = []
    for file in json_files:
        first_value_data = flattern(file)
        first_filename = os.path.basename(file["filename"])

        for another_file in json_files:
            key1 = f'{file["filename"]}-{another_file["filename"]}'
            key2 = f'{another_file["filename"]}-{file["filename"]}'
            second_value_data = flattern(another_file)
            second_filename = os.path.basename(another_file["filename"])

            if (
                file["filename"] == another_file["filename"]
                or key1 in checked_groups
                or key2 in checked_groups
            ):
                continue
            cross_check_keys(
                first_filename, first_value_data, second_filename, second_value_data
            )
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

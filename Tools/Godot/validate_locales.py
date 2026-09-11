"""Validate the bounded world-interface catalogs without launching Godot or touching saves.

The legacy catalog and the new UI catalog are one runtime catalog. New UI entries are
kept in a small pair here so translators can deliver this slice without pretending
that all original dialogue or management screens have already been translated.
"""
from __future__ import annotations

import json
import re
import string
from pathlib import Path

MAX_BYTES = 512 * 1024
GAME = Path(__file__).resolve().parents[2] / "OpenMakaiRanchGame"


def _unique_object(pairs: list[tuple[str, object]]) -> dict[str, object]:
    result: dict[str, object] = {}
    for key, value in pairs:
        if key in result:
            raise ValueError(f"Duplicate translation key: {key}")
        result[key] = value
    return result


def read_catalog(path: Path) -> dict[str, str]:
    raw = path.read_bytes()
    if len(raw) > MAX_BYTES:
        raise ValueError(f"Catalog exceeds runtime size limit: {path}")
    data = json.loads(raw.decode("utf-8"), object_pairs_hook=_unique_object)
    if not isinstance(data, dict):
        raise ValueError(f"Catalog must be an object: {path}")
    for key, value in data.items():
        if not isinstance(key, str) or not key.strip() or not isinstance(value, str) or not value.strip():
            raise ValueError(f"Blank/non-text translation: {path}: {key}")
    return data


def format_slots(text: str) -> frozenset[int]:
    """Parse only numbered .NET composite slots, never evaluate translation strings.

    Python's formatter parser handles escaped {{braces}} and the format-spec separator.
    Alignment belongs to the .NET field name (e.g. {0,-12}); formats such as 0.0 or
    +0;-0;0 remain opaque strings, not Python numeric format instructions.
    """
    slots: set[int] = set()
    for _, field, spec, conversion in string.Formatter().parse(text):
        if field is None:
            continue
        if conversion is not None or not re.fullmatch(r"[0-9]+(?:,[+-]?[0-9]+)?", field):
            raise ValueError(f"Expected numbered .NET placeholder, got {field!r}")
        if "{" in spec or "}" in spec:
            raise ValueError("Nested format fields are not part of this catalog's contract")
        slots.add(int(field.split(",", 1)[0]))
    return frozenset(slots)


def validate_pair(english: dict[str, str], translated: dict[str, str]) -> None:
    missing, extra = english.keys() - translated.keys(), translated.keys() - english.keys()
    if missing or extra:
        raise ValueError(f"Mismatched catalog keys: missing={sorted(missing)}, extra={sorted(extra)}")
    for key, text in english.items():
        if format_slots(text) != format_slots(translated[key]):
            raise ValueError(f"Missing/extra format arguments at {key}")


def validate_repository(game: Path = GAME) -> int:
    english = read_catalog(game / "locale/ui/en.json")
    german = read_catalog(game / "locale/ui/de.json")
    validate_pair(english, german)
    # Every literal key newly used in the world UI must have an editable source entry.
    # Data names, enums and other computed keys deliberately retain runtime English fallback.
    for path in (game / "src").rglob("*.cs"):
        if "Tests" in path.parts:
            continue
        for key in re.findall(r'\bT\("(world\.[^"\n]+)"\s*,', path.read_text(encoding="utf-8")):
            if key not in english:
                raise ValueError(f"Missing source translation {key} referenced by {path}")
    filters = re.findall(r'^include_filter="([^"]*)"', (game / "export_presets.cfg").read_text(), re.M)
    if not filters or any(not {"data/*.json", "locale/*.json", "locale/ui/*.json"}.issubset(set(value.split(","))) for value in filters):
        raise ValueError("Export presets must include the raw JSON data and language catalogs")
    return len(english)


if __name__ == "__main__":
    print(f"Locale catalogs valid: {validate_repository()} English/German UI keys")

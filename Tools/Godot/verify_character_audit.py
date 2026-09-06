"""Recheck audit citations and hashes without executing the embedded extractor."""
from pathlib import Path
import argparse
import csv
import hashlib
import json
import re
import sys

ROOT = Path(__file__).resolve().parents[2]
REPORT = ROOT / 'OpenMakaiRanchGame/docs/audit/character-metadata.json'
DISCLAIMER = 'This verifies evidence only; it does not certify age, visual design or runtime safety.'

# Schema-1 neutral extractor vocabulary. A real citation cannot prove an unrelated field.
SOURCE_KEYS = {
    'source_id': '番号',
    'full_name': '名前',
    'display_name': '呼び名',
    'race': 'CSTR,種族',
    'race_category': 'CSTR,カテゴリ／種族',
    'occupation': 'CSTR,ジョブ',
    'affiliation': 'CSTR,所属身分',
    'height_label': 'CSTR,身長',
    'hair_color': 'CSTR,髪色',
    'hair_style': 'CSTR,髪型',
    'hair_feature': 'CSTR,髪特徴',
    'right_eye_color': 'CSTR,右目の色',
    'left_eye_color': 'CSTR,左目の色',
    'eye_feature': 'CSTR,目の特徴',
    'eye_shape': 'CSTR,瞳の形',
    'identity_name': 'CSTR,識別名',
    'origin_work': 'CSTR,出典作品名',
    'apparent_age': 'フラグ,外見年齢',
    'height_mm': 'フラグ,身長',
    'hair_color_code': 'フラグ,カラーコード／髪',
    'right_eye_color_code': 'フラグ,カラーコード／右目',
}


class AuditError(ValueError):
    """Evidence is missing, malformed or no longer matches the snapshot."""


def require(condition, message):
    # Validation must remain active under python -O / PYTHONOPTIMIZE.
    if not condition:
        raise AuditError(str(message))


def evidence_path(root, value):
    require(isinstance(value, str) and value, 'Evidence path must be a nonempty string')
    # Keep identifiers literal; do not silently repair traversal or Windows paths.
    require('\\' not in value and ':' not in value and
            all(part not in ('', '.', '..') for part in value.split('/')),
            f'Invalid repository-relative evidence path: {value!r}')
    path = root / value
    require(path.resolve().is_relative_to(root.resolve()), f'Evidence path escapes repository: {value!r}')
    require(path.is_file(), f'Evidence file missing: {value}')
    return path


def read_evidence(path):
    # Bound report-controlled reads; decoding and hashing use the same snapshot.
    with path.open('rb') as stream:
        content = stream.read(16 * 1024 * 1024 + 1)
    require(len(content) <= 16 * 1024 * 1024, f'Evidence file exceeds 16 MiB: {path.name}')
    return content


def hash_matches(content, expected):
    require(isinstance(expected, str) and re.fullmatch(r'[0-9a-f]{64}', expected),
            'Expected SHA-256 must be exactly 64 lowercase hex characters')
    return hashlib.sha256(content).hexdigest() == expected


def unique_json_object(pairs):
    result = {}
    for key, value in pairs:
        require(key not in result, f'Duplicate JSON field: {key}')
        result[key] = value
    return result


def verify(root=None, report=None, *, sources_only=False):
    root = Path(root) if root is not None else ROOT
    report = Path(report) if report is not None else REPORT
    try:
        data = json.loads(read_evidence(report).decode('utf-8-sig'), object_pairs_hook=unique_json_object)
    except (ValueError, RecursionError) as error:
        raise AuditError(f'Invalid audit JSON: {error}') from error
    require(isinstance(data, dict) and type(data.get('schema_version')) is int and
            data['schema_version'] == 1, 'Unsupported audit schema_version (expected 1)')
    rows = data['source_characters']
    require(isinstance(rows, list) and rows, 'Source evidence must be a nonempty list')
    source_root = root / 'eraMakaiRanch-game-eng-translation'
    files = [p for p in source_root.rglob('*') if p.is_file() and
             p.name.casefold().startswith('chara') and p.suffix.casefold() == '.csv']
    count = data['counts']['source_files']
    require(type(count) is int and len(rows) == len(files) == count, 'Source count mismatch')
    ids, paths = set(), set()
    citations = 0
    for row in rows:
        require(isinstance(row, dict), 'Source row must be an object')
        source_id, source_file = row['source_id'], row['source_file']
        require(type(source_id) is int and source_id >= 0, 'Source ID must be a nonnegative integer')
        require(source_id not in ids, f'Duplicate source ID: {source_id}')
        ids.add(source_id)
        path = evidence_path(root, source_file)
        require(path.resolve().is_relative_to(source_root.resolve()), 'Source file outside original reference')
        require(source_file.casefold() not in paths, f'Duplicate source file: {source_file}')
        paths.add(source_file.casefold())
        content = read_evidence(path)
        require(hash_matches(content, row['source_sha256']), f'Source hash mismatch: {source_file}')
        lines = content.decode('utf-8-sig').splitlines()
        metadata = row['metadata']
        require(isinstance(metadata, dict) and metadata.get('source_id'), f'Missing source ID citation: {source_file}')
        for name, values in metadata.items():
            require(isinstance(values, list) and values, f'Missing citations: {source_file} / {name}')
            for field in values:
                require(isinstance(field, dict), f'Citation must be an object: {source_file}')
                line = field['line']
                require(type(line) is int and 1 <= line <= len(lines), f'Invalid citation line: {source_file}')
                label = f'{source_file}:{line}'
                require(field['source_file'] == source_file, f'Citation file mismatch: {label}')
                key_text = field['source_key']
                require(isinstance(key_text, str) and key_text, f'Missing citation key: {label}')
                require(name in SOURCE_KEYS and key_text == SOURCE_KEYS[name],
                        f'Metadata field/key mismatch: {label} / {name}')
                key = key_text.split(',')
                parts = [x.strip() for x in next(csv.reader([lines[line - 1]], strict=True))]
                require(len(parts) > len(key) and parts[:len(key)] == key, f'Citation key mismatch: {label}')
                require(type(field['value']) in (str, int, float) and
                        parts[len(key)] == str(field['value']), f'Citation value mismatch: {label}')
                if name == 'source_id':
                    require(key == ['番号'] and type(field['value']) is int and field['value'] == source_id,
                            f'Source ID citation mismatch: {label}')
                citations += 1
    require(paths == {p.relative_to(root).as_posix().casefold() for p in files}, 'Source inventory mismatch')
    if sources_only:
        print(f'AUDIT_SOURCE_EVIDENCE_PASS {len(rows)} sources, {citations} field citations')
        print('Code/data snapshot NOT CHECKED (--sources-only).')
        print(DISCLAIMER)
        return
    manifest = data['input_manifest']
    require(isinstance(manifest, list) and manifest, 'Input manifest must be a nonempty list')
    input_paths, stale = set(), []
    for item in manifest:
        require(isinstance(item, dict), 'Input manifest entry must be an object')
        path = evidence_path(root, item['file'])
        key = item['file'].casefold()
        require(key not in input_paths, f'Duplicate input manifest path: {item["file"]}')
        input_paths.add(key)
        if not hash_matches(read_evidence(path), item['sha256']):
            stale.append(item['file'])
    require(not stale, 'Code/data snapshot stale; re-audit changed inputs:\n' + '\n'.join(stale))
    print(f'AUDIT_EVIDENCE_PASS {len(rows)} sources, {citations} field citations, '
          f'{len(data["input_manifest"])} code/data hashes')
    print(DISCLAIMER)


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', type=Path, default=ROOT, help='Repository root; defaults to script location')
    parser.add_argument('--report', type=Path, help='Report path, relative to --root or absolute')
    parser.add_argument('--sources-only', action='store_true',
                        help='Verify original CSV inventory/citations only; skip code/data snapshot explicitly')
    args = parser.parse_args(argv)
    root = args.root.resolve()
    report = args.report or Path('OpenMakaiRanchGame/docs/audit/character-metadata.json')
    if not report.is_absolute():
        report = root / report
    try:
        verify(root, report, sources_only=args.sources_only)
    except (AuditError, OSError, UnicodeError, json.JSONDecodeError, csv.Error, KeyError, TypeError) as error:
        print(f'AUDIT_EVIDENCE_FAIL {error}', file=sys.stderr)
        return 1
    return 0


if __name__ == '__main__':
    sys.exit(main())

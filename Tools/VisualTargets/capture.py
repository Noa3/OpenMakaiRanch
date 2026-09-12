"""Capture the current ranch with P01 in an isolated profile; never approves artwork."""
from __future__ import annotations
import json
import math
import hashlib
import os
from pathlib import Path
import subprocess
import sys
import tempfile

ROOT = Path(__file__).resolve().parents[2]
TOWN_WALK_REQUIRED = (
    'entering building opens no menu', 'escort entered actual town_hall room',
    'escort entered actual planning_board room', 'escort reached market counter on foot',
    'escort reached market bypass exit on foot',
    'actual reception interaction opens visible existing milestones',
    'market counter opens existing visible shop',
    'walk speed, gold and delivery count unchanged; no transaction performed',
)
sys.path.insert(0, str(ROOT / 'Tools/Godot'))
import launch
from ui_acceptance import isolated_environment


SOURCE_SCOPE = (
    'OpenMakaiRanchGame/src', 'OpenMakaiRanchGame/scenes', 'OpenMakaiRanchGame/assets',
    'OpenMakaiRanchGame/data', 'OpenMakaiRanchGame/locale', 'OpenMakaiRanchGame/shaders',
    'OpenMakaiRanchGame/addons', 'OpenMakaiRanchGame/Tools', 'OpenMakaiRanchGame/project.godot',
    'OpenMakaiRanchGame/OpenMakaiRanchGame.csproj', 'Directory.Build.props',
    'Directory.Build.targets', 'global.json', 'Tools/Godot', 'Tools/VisualTargets',
)


def file_sha(path: Path) -> str:
    if path.is_symlink() or not path.is_file():
        raise RuntimeError(f'Expected a regular evidence input: {path}')
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def source_snapshot(root: Path) -> dict:
    # Include local edits and new source/assets; exclude ignored caches and generated build output.
    raw = subprocess.check_output(['git', 'ls-files', '--cached', '--others', '--exclude-standard',
                                   '-z', '--', *SOURCE_SCOPE], cwd=root)
    records = []
    for relative in sorted(set(raw.decode('utf-8').split('\0')) - {''}):
        path = root / relative
        if (path.name.lower().startswith('.env') or path.suffix.lower() in ('.pem', '.key', '.pfx', '.p12')
                or path.stem.lower() in ('credentials', 'secrets')):
            raise RuntimeError('Secret-like file found in source scope; refusing to fingerprint it')
        if not path.resolve().is_relative_to(root.resolve()) or any(
            p.is_symlink() for p in (path, *path.parents) if p != root and root in p.parents
        ):
            raise RuntimeError('Linked or external source input is not supported')
        records.append({'path': relative, 'sha256': file_sha(path) if path.exists() else None})
    if not records:
        raise RuntimeError('No source files found for capture provenance')
    digest = hashlib.sha256(json.dumps(records, sort_keys=True, separators=(',', ':')).encode('utf-8')).hexdigest()
    return {'sha256': digest, 'scope': list(SOURCE_SCOPE), 'files': records}


def main():
    engine, version = launch.resolve_godot(ROOT, dict(os.environ))
    launch.assert_port_available(9501)
    working = ROOT / '.dream-loop'
    working.mkdir(exist_ok=True)
    run = Path(tempfile.mkdtemp(prefix='capture-', dir=working))
    env = isolated_environment(run, dict(os.environ), sys.platform)
    env['OMR_VISUAL_TARGET_OUTPUT'] = str(run / 'evidence')
    env['OMR_VISUAL_SOURCE_COMMIT'] = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT, text=True).strip()
    code, text = launch.invoke([engine, '--headless', '--path', launch.PROJECT,
                               '--script', ROOT / 'Tools/Godot/check_user_data.gd'],
                              env, 30, run / 'profile-check.log')
    if code or 'USER_DATA_ISOLATION_PASS' not in text.splitlines():
        raise RuntimeError('Profile isolation failed; capture not started')
    source = source_snapshot(ROOT)
    (run / 'source-state.json').write_text(json.dumps(source, indent=2), encoding='utf-8')
    code, _ = launch.invoke(['dotnet', 'build', launch.PROJECT / 'OpenMakaiRanchGame.csproj',
                             '--configuration', 'Debug'], dict(os.environ), 300, run / 'build.log')
    if code:
        raise RuntimeError(f'Build failed; capture not started; inspect {run / "build.log"}')
    assembly = launch.PROJECT / '.godot/mono/temp/bin/Debug/OpenMakaiRanch.dll'
    assembly_sha = file_sha(assembly)
    (run / 'visual-target.marker').write_text('isolated-visual-target-v1\n', encoding='utf-8')
    regional = env.get('OMR_REGIONAL_TRAVERSAL') == '1'
    scene = 'res://scenes/dev/RegionalTraversalCapture.tscn' if regional else 'res://scenes/dev/VisualTargetCapture.tscn'
    code, text = launch.invoke([engine, '--path', launch.PROJECT, '--rendering-method', 'forward_plus',
                               '--audio-driver', 'Dummy', '--resolution', '1600x900',
                               '--log-file', run / 'engine.log', scene,
                               '--', '--visual-target-capture'], env, 240 if regional else 120, run / 'console.log')
    if regional:
        from regional_traversal import verify_recording
        return verify_recording(run, code, text, env, source, source_snapshot(ROOT), assembly_sha, file_sha(assembly))
    if code or text.splitlines().count('VISUAL CAPTURE PASS') != 1 or any(
        line.lstrip().startswith(('ERROR:', 'SCRIPT ERROR:', 'VISUAL CAPTURE FAIL')) for line in text.splitlines()
    ):
        raise RuntimeError(f'Render failed; inspect {run}')
    from atlas import png_size
    image = run / 'evidence/W01-current.png'
    if png_size(image) != (1600, 900):
        raise RuntimeError('Incorrect capture dimensions')
    context = json.loads((run / 'evidence/W01-context.json').read_text(encoding='utf-8'))
    if context['source_commit'] != env['OMR_VISUAL_SOURCE_COMMIT'] or context['renderer'] != 'forward_plus':
        raise RuntimeError('Capture source or renderer mismatch')
    if source_snapshot(ROOT) != source:
        raise RuntimeError('Source changed during build/capture; evidence is not verified')
    if file_sha(assembly) != assembly_sha:
        raise RuntimeError('Assembly changed during capture; evidence is not verified')
    context['source_state'] = source
    context['build'] = {'configuration': 'Debug', 'exit_code': 0, 'assembly_sha256': assembly_sha,
                        'assembly': '.godot/mono/temp/bin/Debug/OpenMakaiRanch.dll'}
    if env.get('OMR_VISUAL_WORLD_REVIEW') == '1':
        report = run / 'evidence/organic-world-review.json'
        review = json.loads(report.read_text(encoding='utf-8'))
        expected = {'ranch-overview', 'ranch-lane', 'town-overview', 'town-market-lane'}
        if not review.get('passed') or {shot['name'] for shot in review['shots']} != expected:
            raise RuntimeError('Organic world review is incomplete or failed')
        outputs = {}
        for name in sorted(expected):
            shot = run / 'evidence' / (name + '.png')
            if png_size(shot) != (1600, 900):
                raise RuntimeError(f'Invalid world-review capture: {name}')
            outputs[name] = file_sha(shot)
        context['world_review'] = {'report_sha256': file_sha(report), 'images': outputs}
    if env.get('OMR_RANCH_SCALE_REVIEW') == '1':
        report = run / 'evidence/ranch-scale.json'
        measurements = json.loads(report.read_text(encoding='utf-8'))
        if measurements['renderer'] != 'forward_plus' or not measurements['actors']:
            raise RuntimeError('Scale measurements missing or wrong renderer')
        images = {}
        for name in ('scale-front', 'scale-player-eye'):
            shot = run / 'evidence' / (name + '.png')
            if png_size(shot) != (1600, 900):
                raise RuntimeError('Scale screenshot is missing or invalid')
            images[name] = file_sha(shot)
        context['scale_review'] = {'report_sha256': file_sha(report), 'images': images}
    if env.get('OMR_RANCH_ASSET_REVIEW') == '1':
        report = run / 'evidence/ranch-assets.json'
        review = json.loads(report.read_text(encoding='utf-8'))
        expected = {'home-exterior', 'home-furnished-cutaway', 'home-office',
                    'home-kitchen', 'home-resident-room', 'home-common-scale'}
        if (review.get('renderer') != 'forward_plus'
                or review.get('verified_unique_asset_count') != 18
                or len(set(review.get('sources', []))) != 18
                or {shot['name'] for shot in review['shots']} != expected):
            raise RuntimeError('Ranch asset review is incomplete or wrong renderer')
        images = {}
        for name in sorted(expected):
            shot = run / 'evidence' / (name + '.png')
            if png_size(shot) != (1600, 900):
                raise RuntimeError('Ranch asset screenshot is missing or invalid')
            images[name] = file_sha(shot)
        context['ranch_asset_review'] = {'report_sha256': file_sha(report), 'images': images}
    if env.get('OMR_OKACHI_ASSET_REVIEW') == '1':
        report = run / 'evidence/okachi-assets.json'
        review = json.loads(report.read_text(encoding='utf-8'))
        expected = {f'okachi-{state}-{view}' for state in ('base', 'expanded')
                    for view in ('exterior', 'cutaway', 'reception')}
        variants = review.get('variants', [])
        if (review.get('passed') is not True or review.get('renderer') != 'forward_plus'
                or len(set(review.get('unique_architecture', []))) != 3
                or len(review.get('shots', [])) != 6
                or {s['name'] for s in review.get('shots', [])} != expected
                or len(variants) != 2 or {v['state'] for v in variants} != {'base', 'expanded'}
                or any(v.get('front_clear_ray') is not True or v.get('four_room_clear_rays') is not True
                       or v.get('rear_blocked_ray') is not (v['state'] == 'base') for v in variants)):
            raise RuntimeError('Okachi asset review is incomplete or failed')
        images = {}
        for name in sorted(expected):
            shot = run / 'evidence' / (name + '.png')
            if png_size(shot) != (1600, 900):
                raise RuntimeError('Okachi asset screenshot is missing or invalid')
            images[name] = file_sha(shot)
        context['okachi_asset_review'] = {'report_sha256': file_sha(report), 'images': images}
    if env.get('OMR_MARKET_ASSET_REVIEW') == '1':
        report = run / 'evidence/market-assets.json'
        review = json.loads(report.read_text(encoding='utf-8'))
        expected_assets = {'canopy.glb', 'counter.glb', 'scaffold_bay.glb', 'material_stack.glb', 'barrier.glb'}
        variants = review.get('variants', [])
        if (review.get('passed') is not True or review.get('renderer') != 'forward_plus'
                or set(review.get('unique_assets', [])) != expected_assets or len(review['unique_assets']) != 5
                or len(variants) != 3 or {v.get('state') for v in variants} != {'base', 'work', 'finished'}):
            raise RuntimeError('Market kit/stage evidence incomplete')
        for v in variants:
            if (v.get('bypass_clear_volume') is not True or v.get('central_clear_volume') is not True
                    or v.get('front_work_barrier_ray') is not (v['state'] == 'work')
                    or v.get('visible_roof_meshes') != (17 if v['state'] == 'finished' else 0)):
                raise RuntimeError('Market collision/roof-state evidence failed')
        expected = {f'market-{state}-{view}' for state in ('base', 'work', 'finished') for view in ('overview', 'eye')}
        names = [s.get('name') for s in review.get('shots', [])]
        if len(names) != len(expected) or set(names) != expected:
            raise RuntimeError('Market screenshot coverage incomplete')
        images = {}
        for name in sorted(expected):
            shot = run / 'evidence' / (name + '.png')
            if png_size(shot) != (1600, 900):
                raise RuntimeError('Market screenshot dimensions changed')
            images[name] = file_sha(shot)
        context['market_asset_review'] = {'report_sha256': file_sha(report), 'images': images}
    if os.environ.get('OMR_TOWN_CORE_REVIEW') == '1':
        report = run / 'evidence/town-core-walk.json'
        review = json.loads(report.read_text(encoding='utf-8'))
        checks = review.get('checks', [])
        if (review.get('passed') is not True or review.get('error') is not None
                or not checks or any(c.get('ok') is not True for c in checks)
                or not set(TOWN_WALK_REQUIRED).issubset({c.get('label') for c in checks})):
            raise RuntimeError('Town walk checks are incomplete or failed')
        trace = review.get('trace', [])
        if len(trace) < 2:
            raise RuntimeError('Town walk trajectory is missing')
        for index, sample in enumerate(trace):
            if sample.get('frame') != index + 1:
                raise RuntimeError('Town walk trajectory frames are inconsistent')
            for actor in ('player', 'escort'):
                p = sample.get(actor, [])
                if len(p) != 3 or any(not math.isfinite(v) for v in p):
                    raise RuntimeError('Town walk trajectory has invalid coordinates')
                if index and math.dist(trace[index-1][actor], p) > 0.65:
                    raise RuntimeError('Town walk trajectory contains a jump')
        names = {f'town-core-{s}' for s in ('overview', 'reception', 'market', 'milestones', 'shop')}
        if {s.get('name') for s in review.get('shots', [])} != names or len(review['shots']) != len(names):
            raise RuntimeError('Town walk image set incomplete')
        images = {}
        for name in sorted(names):
            shot = run / 'evidence' / (name + '.png')
            if png_size(shot) != (1600, 900):
                raise RuntimeError('Town walk image dimensions changed')
            images[name] = file_sha(shot)
        context['town_core_walk'] = {'checks': len(checks), 'frames': len(trace), 'images': images, 'report_sha256': file_sha(report)}
    (run / 'evidence/W01-context.json').write_text(json.dumps(context, indent=2), encoding='utf-8')
    print(f'CAPTURE VERIFIED: {image}; Godot {version}')
    return 0


if __name__ == '__main__':
    try:
        raise SystemExit(main())
    except (OSError, ValueError, RuntimeError, subprocess.SubprocessError) as error:
        print(f'CAPTURE ERROR: {error}', file=sys.stderr)
        raise SystemExit(1)

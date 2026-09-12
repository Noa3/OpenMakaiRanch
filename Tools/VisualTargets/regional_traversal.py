"""Validate real regional capture artifacts; failed walks remain failed evidence."""
import hashlib
import json
import math
from atlas import png_size


def verify_recording(run, code, text, env, source, after, assembly_sha, assembly_after):
    report = run / 'evidence/regional-traversal.json'
    if not report.is_file():
        raise RuntimeError(f'Regional report missing; inspect {run}')
    data = json.loads(report.read_text(encoding='utf-8'))
    trace_path = run / 'evidence/regional-trace.jsonl'
    trace = [json.loads(line) for line in trace_path.read_text(encoding='utf-8').splitlines()]
    issues = []
    if code or text.splitlines().count('REGIONAL TRAVERSAL RECORDED') != 1 or any(
            line.lstrip().startswith(('ERROR:', 'SCRIPT ERROR:', 'VISUAL CAPTURE FAIL')) for line in text.splitlines()):
        issues.append('engine error or incomplete recording')
    if source != after or assembly_sha != assembly_after:
        issues.append('source/assembly changed during capture')
    if data.get('source_commit') != env['OMR_VISUAL_SOURCE_COMMIT'] or data.get('renderer') != 'forward_plus':
        issues.append('source/renderer mismatch')
    images = {}
    for shot in data.get('shots', []):
        name = shot['name']
        path = run / 'evidence' / (name + '.png')
        if png_size(path) != (1600, 900):
            issues.append('invalid image dimensions: ' + name)
        images[name] = hashlib.sha256(path.read_bytes()).hexdigest()
    if not images or len(trace) != data.get('frames'):
        issues.append('image or frame inventory incomplete')
    max_steps = {'player': 0, 'escort': 0}
    for i, sample in enumerate(trace):
        if sample['frame'] != i + 1 or (i and sample['physics_frame'] != trace[i-1]['physics_frame'] + 1):
            issues.append('nonconsecutive physics trace')
        for actor in max_steps:
            p = sample[actor]
            if len(p) != 3 or not all(math.isfinite(v) for v in p):
                issues.append('invalid coordinates')
            if i:
                max_steps[actor] = max(max_steps[actor], math.dist(p, trace[i-1][actor]))
    route_pass = data.get('passed') is True and not data.get('error')
    if route_pass and (not trace or max(max_steps.values()) > .65 or any(s['collisions'] for s in trace)
                       or trace[-1]['area'] != 'town' or trace[-1]['transitions'] != 1
                       or not {'regional-home', 'regional-meadow', 'regional-valley-bridge', 'regional-seam',
                               'regional-civic', 'regional-market'}.issubset(images)):
        issues.append('claimed pass lacks continuous regional coverage')
    summary = dict(recording_verified=not issues, route_passed=route_pass and not issues, accepted=False,
                   issues=issues, frames=len(trace), max_frame_displacement=max_steps, images=images,
                   report_sha256=hashlib.sha256(report.read_bytes()).hexdigest(),
                   trace_sha256=hashlib.sha256(trace_path.read_bytes()).hexdigest(),
                   source_sha256=source['sha256'], assembly_sha256=assembly_sha,
                   first_blocker=data.get('error'), segment=data.get('segment'))
    (run / 'evidence/regional-verification.json').write_text(json.dumps(summary, indent=2), encoding='utf-8')
    print(f'REGIONAL RECORDING: {run}; verified={not issues}; route_passed={summary["route_passed"]}; blocker={data.get("error")}')
    return 0 if summary['route_passed'] else 1

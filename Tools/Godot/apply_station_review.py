"""One-use, exact-parent and exact-payload guarded station implementation."""
from pathlib import Path
import hashlib
import subprocess
import zlib

branch = 'feature/world-stations-and-interiors-20260911'
base = 'bae6ad1d5025550cea44358bfeeedd3dc7952ab0'
def git(*args):
    return subprocess.check_output(['git', *args], text=True).strip()
parents = [line[7:] for line in git('cat-file', '-p', 'HEAD').splitlines() if line.startswith('parent ')]
if git('branch', '--show-current') != branch or parents != [base]:
    raise SystemExit('Concurrent source change: refusing to apply reviewed patch')
patch = zlib.decompress(b''.join(Path(f'Tools/Godot/station-review.{i}').read_bytes() for i in range(2)))
if hashlib.sha256(patch).hexdigest() != 'fb0c86d56508cf4ae37045e69f75a269dba4e41f66a80ce62b8dccace0541c9c':
    raise SystemExit('Reviewed patch hash mismatch')
subprocess.run(['git', 'apply', '--check', '--index', '-'], input=patch, check=True)
subprocess.run(['git', 'apply', '--index', '-'], input=patch, check=True)
helpers = ['Tools/Godot/station-review.0', 'Tools/Godot/station-review.1', 'Tools/Godot/apply_station_review.py', '.github/workflows/apply-station-review.yml']
for path in helpers:
    Path(path).unlink()
subprocess.run(['git', 'add', '--', *helpers], check=True)
subprocess.run(['git', 'commit', '-m', 'feat(world): add physical station panels, bounded wayfinding and walk-in ranch buildings'], check=True)
subprocess.run(['git', 'push', 'origin', 'HEAD:refs/heads/' + branch], check=True)

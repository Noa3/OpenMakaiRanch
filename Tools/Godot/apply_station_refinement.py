"""One-use exact-parent/payload guarded station refinement."""
from pathlib import Path
import hashlib, subprocess, zlib
branch = 'feature/world-stations-and-interiors-20260911'
base = '9b8f9a4f5dfa7a978800773f496730783049df5f'
def git(*a): return subprocess.check_output(['git', *a], text=True).strip()
parents = [l[7:] for l in git('cat-file', '-p', 'HEAD').splitlines() if l.startswith('parent ')]
if git('branch', '--show-current') != branch or parents != [base]: raise SystemExit('Concurrent source change')
patch = zlib.decompress(Path('Tools/Godot/station-refinement.zlib').read_bytes())
if hashlib.sha256(patch).hexdigest() != '601fe5be5007627cd62fa9c68e3e177aae01586acf32043e051b0f6b40984618': raise SystemExit('Payload mismatch')
subprocess.run(['git', 'apply', '--check', '--index', '-'], input=patch, check=True)
subprocess.run(['git', 'apply', '--index', '-'], input=patch, check=True)
helpers = ['Tools/Godot/station-refinement.zlib', 'Tools/Godot/apply_station_refinement.py', '.github/workflows/station-refinement.yml']
for p in helpers: Path(p).unlink()
subprocess.run(['git', 'add', '--', *helpers], check=True)
subprocess.run(['git', 'commit', '-m', 'fix(world): restrict physical service routes and exercise the new workstation contracts'], check=True)
subprocess.run(['git', 'push', 'origin', 'HEAD:refs/heads/' + branch], check=True)

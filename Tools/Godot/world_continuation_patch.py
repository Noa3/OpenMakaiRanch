"""One-use reviewed opening repair, confined to the requested new work branch."""
import hashlib
import subprocess
import zlib
from pathlib import Path

branch = "feature/world-stations-and-interiors-20260911"
base = "3c0b0198b900ea4b8579df343c8d3fef7e34a9f4"
def git(*args):
    return subprocess.check_output(["git", *args], text=True).strip()
if git("branch", "--show-current") != branch or git("rev-parse", "HEAD^") != base:
    raise SystemExit("Unexpected branch/source; refusing to replace concurrent work")
patch = zlib.decompress(Path("Tools/Godot/world-continuation.zlib").read_bytes())
if hashlib.sha256(patch).hexdigest() != "a45f1ab03dae16e95b009f8faa8b448fe80ab169b1cddc0e00e32db793e29f15":
    raise SystemExit("Reviewed patch hash mismatch")
subprocess.run(["git", "apply", "--check", "--index", "-"], input=patch, check=True)
subprocess.run(["git", "apply", "--index", "-"], input=patch, check=True)
helpers = ["Tools/Godot/world-continuation.zlib", "Tools/Godot/world_continuation_patch.py", ".github/workflows/world-continuation.yml"]
for name in helpers:
    Path(name).unlink()
subprocess.run(["git", "add", "--", *helpers], check=True)
subprocess.run(["git", "commit", "-m", "fix(opening): integrate reviewed text, utility-return, first-day input and movement repairs"], check=True)
subprocess.run(["git", "push", "origin", "HEAD:refs/heads/" + branch], check=True)

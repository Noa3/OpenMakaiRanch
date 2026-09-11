"""One-use reviewed opening repair, confined to the requested new work branch."""
import hashlib
import subprocess
import zlib
from pathlib import Path

branch = "feature/world-stations-and-interiors-20260911"
base = "d0f02154e7accec2c62042c32026aa7931d39a0e"
def git(*args):
    return subprocess.check_output(["git", *args], text=True).strip()
# Read the commit object directly: a shallow checkout deliberately has no HEAD^ object.
parents = [line[7:] for line in git("cat-file", "-p", "HEAD").splitlines() if line.startswith("parent ")]
if git("branch", "--show-current") != branch or parents != [base]:
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

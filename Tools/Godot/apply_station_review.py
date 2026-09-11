"""One-use exact-base patch application for the explicitly requested work branch."""
import hashlib
from pathlib import Path
import subprocess
import zlib

BRANCH = "feature/world-stations-and-interiors-20260911"
BASE = "fe69547d3762b3ee14ad4a33525f01a02f6d9f15"
FILES = ["OpenMakaiRanchGame/src/App/GameRoot.cs", "OpenMakaiRanchGame/src/Tests/RanchLeisureFrameTests.cs", "OpenMakaiRanchGame/src/World/WorldInteractionPresentation.cs"]
HELPERS = ["Tools/Godot/station-reviewed.zpatch", "Tools/Godot/apply_station_review.py", ".github/workflows/station-review.yml"]
def git(*args):
    return subprocess.check_output(["git", *args], text=True).strip()
if git("branch", "--show-current") != BRANCH or git("rev-parse", "HEAD^") != BASE:
    raise SystemExit("Source branch advanced; inspect before reapplying")
patch = zlib.decompress(Path(HELPERS[0]).read_bytes())
if hashlib.sha256(patch).hexdigest() != "6532577503be03ca70353663816b07f4e2549948ee15a32ddb5c15cbc7ba0959":
    raise SystemExit("Patch bytes differ from the reviewed diff")
subprocess.run(["git", "apply", "--check", "-"], input=patch, check=True)
subprocess.run(["git", "apply", "-"], input=patch, check=True)
subprocess.run(["git", "diff", "--check"], check=True)
for file in HELPERS:
    Path(file).unlink()
subprocess.run(["git", "add", "--", *FILES, *HELPERS], check=True)
if set(git("diff", "--cached", "--name-only").splitlines()) != set(FILES + HELPERS):
    raise SystemExit("Unexpected staged changes; refusing to publish")
subprocess.run(["git", "commit", "-m", "fix(stations): retain construction warnings and exercise local supply and sleep routes"], check=True)
subprocess.run(["git", "push", "origin", "HEAD:refs/heads/" + BRANCH], check=True)

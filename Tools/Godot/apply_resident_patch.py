"""One-use exact-preimage patch for the authorized resident interaction continuation."""
import base64
import hashlib
from pathlib import Path
import subprocess
import tempfile
import zlib

BRANCH = "feature/world-stations-and-interiors-20260911"
GUARDS = {
 "OpenMakaiRanchGame/docs/NSFW_CONTENT_HANDOFF.md": None,
 "OpenMakaiRanchGame/docs/RESIDENT_INTERACTIONS.md": None,
 "OpenMakaiRanchGame/locale/ui/de.json": "f6ccd9994e24c291b9b891e3d28338f7e6eeb262",
 "OpenMakaiRanchGame/locale/ui/en.json": "74547edbb3b00972b1961e2f33cdc1cf385cf56b",
 "OpenMakaiRanchGame/src/App/GameRoot.ResidentInteractions.cs": None,
 "OpenMakaiRanchGame/src/Gameplay/ManagementServices.cs": "ec218b1692cc1384875aa784c683e77517bc99cb",
 "OpenMakaiRanchGame/src/Gameplay/ResidentInteractionService.cs": None,
 "OpenMakaiRanchGame/src/Tests/DaySettlementRegressionTests.cs": "05df87257a902dc29acfe5de698fe2f3b8b21546",
 "OpenMakaiRanchGame/src/Tests/ResidentInteractionRegressionTests.cs": None,
 "OpenMakaiRanchGame/src/Tests/SmokeTestRunner.cs": "feb9bf66da5dcd7d9ee04e279b7331cb027e8f65",
 "OpenMakaiRanchGame/src/Tests/UiLayoutAcceptance.Night.cs": "38d51468269ef1bfbbd6f9715a06697849545b85",
 "OpenMakaiRanchGame/src/Tests/UiLayoutAcceptance.Residents.cs": None,
 "OpenMakaiRanchGame/src/Ui/WorldStationPanel.Residents.cs": None,
 "OpenMakaiRanchGame/src/Ui/WorldStationPanel.cs": "5a29f8f57c908bb97bf1b0d8bceac3c96ed28cb7",
 "OpenMakaiRanchGame/src/World/RosterRig.cs": "588851bbe1bc1aa83f2c9394f952e614536eef93",
 "OpenMakaiRanchGame/src/World/WorldGameController.ResidentInteractions.cs": None,
 "OpenMakaiRanchGame/src/World/WorldGameController.Stations.cs": "ce240e06f99634ce23179a10ea85e96e98d078f9",
 "OpenMakaiRanchGame/src/World/WorldGameController.cs": "a43cbe070a1552b949fd9d97a97ba9e22cd40eea"
}

def git(*args):
    return subprocess.check_output(["git", *args], text=True).strip()

if git("branch", "--show-current") != BRANCH or git("status", "--porcelain"):
    raise SystemExit("Refusing an unexpected branch or dirty checkout")
for path, expected in GUARDS.items():
    file = Path(path)
    if expected is None:
        if file.exists(): raise SystemExit("New file already exists: " + path)
    else:
        raw = file.read_bytes()
        actual = hashlib.sha1(b"blob " + str(len(raw)).encode() + b"\0" + raw).hexdigest()
        if actual != expected: raise SystemExit("Changed source; review before applying: " + path)
parts = [Path(f"Tools/Godot/resident_patch_part{i}.txt") for i in range(1, 4)]
patch = zlib.decompress(base64.b64decode("".join(p.read_text() for p in parts), validate=True))
if hashlib.sha256(patch).hexdigest() != "9cd246be48c3e20d6e8d143a09f3bfd7ed58674498277c734985cbb40bee30b4":
    raise SystemExit("Patch checksum mismatch")
with tempfile.TemporaryDirectory() as temp:
    target = Path(temp) / "reviewed.diff"; target.write_bytes(patch)
    subprocess.run(["git", "apply", "--check", str(target)], check=True)
    subprocess.run(["git", "apply", str(target)], check=True)
helpers = [*parts, Path(__file__), Path(".github/workflows/apply-resident-patch.yml")]
for helper in helpers: helper.unlink()
subprocess.run(["git", "add", "--", *GUARDS, *map(str, helpers)], check=True)
subprocess.run(["git", "diff", "--cached", "--check"], check=True)
subprocess.run(["git", "commit", "-m", "feat(residents): connect practical lessons and daily care to localized world interactions"], check=True)
subprocess.run(["git", "push", "origin", "HEAD:refs/heads/" + BRANCH], check=True)

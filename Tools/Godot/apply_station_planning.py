"""One-use exact-preimage, checksum-guarded work-station continuation."""
import base64
import hashlib
from pathlib import Path
import subprocess
import tempfile
import zlib

BRANCH = "feature/world-stations-and-interiors-20260911"
GUARDS = {
 "OpenMakaiRanchGame/locale/ui/de.json": "437b27de98a10a784778a68384bd6670a6db4de1",
 "OpenMakaiRanchGame/locale/ui/en.json": "7aef67a73ec9482299b87c65aad89639489a627b",
 "OpenMakaiRanchGame/src/App/GameRoot.DayLoop.cs": "9141eaa7f338024651ad4188207b4d9ae3e8591e",
 "OpenMakaiRanchGame/src/App/GameRoot.ResidentInteractions.cs": "2aca709c117f087bc931a5d3959e4947b76f649f",
 "OpenMakaiRanchGame/src/App/GameRoot.StationPlanning.cs": None,
 "OpenMakaiRanchGame/src/App/GameRoot.cs": "e1cc7b6b83b1d73e37b83ccb5ade825e77e1d6c9",
 "OpenMakaiRanchGame/src/Gameplay/DailySettlementService.cs": "85d8d538a6d63c2ba510ec1f0adf52daee9942aa",
 "OpenMakaiRanchGame/src/Gameplay/RanchService.Facilities.cs": None,
 "OpenMakaiRanchGame/src/Gameplay/RanchService.cs": "5b0188f48831dfda3feb291e7667d1b7ac435011",
 "OpenMakaiRanchGame/src/Tests/DaySettlementRegressionTests.cs": "c038c416369ce2ceb2a0dbffc9d47f12dd4e7090",
 "OpenMakaiRanchGame/src/Tests/RanchLeisureFrameTests.cs": "3b49e7550503257015b77c7f42359e767aca4432",
 "OpenMakaiRanchGame/src/Tests/StationPlanningRegressionTests.cs": None,
 "OpenMakaiRanchGame/src/Tests/UiLayoutAcceptance.Localization.cs": "b6ca2b9c33c64661d4dc3adff0d7f1de8272d9b8",
 "OpenMakaiRanchGame/src/Tests/UiLayoutAcceptance.Night.cs": "46dbf764514eacfaea4744e917aabf457d06d97f",
 "OpenMakaiRanchGame/src/Tests/UiLayoutAcceptance.Stations.cs": "2bc1c0d80d6299375d42b87e8c165d1dfa2af7a3",
 "OpenMakaiRanchGame/src/Tests/UiLayoutAcceptance.WorkPlanning.cs": None,
 "OpenMakaiRanchGame/src/Ui/WorldStationPanel.Work.cs": None,
 "OpenMakaiRanchGame/src/Ui/WorldStationPanel.cs": "0f105edcc54ca733bf571587ae2115e3d81f738a",
 "OpenMakaiRanchGame/src/World/WorldGameController.StationPlanning.cs": None
}

def git(*args):
    return subprocess.check_output(["git", *args], text=True).strip()

if git("branch", "--show-current") != BRANCH or git("status", "--porcelain"):
    raise SystemExit("Refusing unexpected branch or dirty checkout")
for path, expected in GUARDS.items():
    file = Path(path)
    if expected is None:
        if file.exists(): raise SystemExit("New path already exists: " + path)
    else:
        raw = file.read_bytes()
        actual = hashlib.sha1(b"blob " + str(len(raw)).encode() + b"\0" + raw).hexdigest()
        if actual != expected: raise SystemExit("Changed source; inspect before applying: " + path)
parts = [Path(f"Tools/Godot/station_planning_part{i}.txt") for i in range(1, 4)]
encoded = "".join(p.read_text(encoding="utf-8") for p in parts)
# Correct one known transport transcription; the complete decoded patch is independently pinned.
encoded = encoded.replace("kdSHPAVAVH3h2", "kdSHPAVavH3h2")
patch = zlib.decompress(base64.b64decode(encoded, validate=True))
if hashlib.sha256(patch).hexdigest() != "837ea8cb5cf135440a0b2e0e8b8fbf0de59577f69c98e7a7917e8159fe508491":
    raise SystemExit("Patch checksum mismatch")
with tempfile.TemporaryDirectory() as temp:
    target = Path(temp) / "reviewed.diff"; target.write_bytes(patch)
    subprocess.run(["git", "apply", "--check", str(target)], check=True)
    subprocess.run(["git", "apply", str(target)], check=True)
helpers = [*parts, Path(__file__), Path(".github/workflows/apply-station-planning.yml")]
for helper in helpers: helper.unlink()
subprocess.run(["git", "add", "--", *GUARDS, *map(str, helpers)], check=True)
subprocess.run(["git", "diff", "--cached", "--check"], check=True)
subprocess.run(["git", "commit", "-m", "feat(stations): add scoped work planning, honest upgrade quotes and guarded upkeep transactions"], check=True)
subprocess.run(["git", "push", "origin", "HEAD:refs/heads/" + BRANCH], check=True)

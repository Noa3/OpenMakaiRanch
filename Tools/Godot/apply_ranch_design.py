"""One-use reviewed patch; only the named work branch and exact file preimages."""
import base64
import hashlib
from pathlib import Path
import subprocess
import tempfile
import zlib

BRANCH = "feature/world-stations-and-interiors-20260911"
# Preserve the newer, concurrent engine-input fix rather than applying our older popup attempt.
EXCLUDE = "OpenMakaiRanchGame/src/Tests/UiLayoutAcceptance.Localization.cs"
GUARDS = {
 "OpenMakaiRanchGame/docs/AUTHOR_CONTENT_HANDOFF.md": None,
 "OpenMakaiRanchGame/docs/BUILDING_PLOTS.md": None,
 "OpenMakaiRanchGame/docs/GAME_DESIGN_DIRECTION.md": None,
 "OpenMakaiRanchGame/docs/LOCALIZATION_ROADMAP.md": None,
 "OpenMakaiRanchGame/locale/ui/de.json": "5997e330a3cf28563bcb926ef5cc0d32524d95f4",
 "OpenMakaiRanchGame/locale/ui/en.json": "8e8e85ced3b861c137bd400d21df2c5cb7750a8c",
 "OpenMakaiRanchGame/src/App/GameRoot.DayLoop.cs": "32f83d8232c05df7137e2e9fa415eb34406b49da",
 "OpenMakaiRanchGame/src/App/GameRoot.SharedEvening.cs": None,
 "OpenMakaiRanchGame/src/Gameplay/DatingService.cs": "0c996b3340bde309cb9429b160d84ab3684400b8",
 "OpenMakaiRanchGame/src/Gameplay/RanchProjectService.cs": None,
 "OpenMakaiRanchGame/src/Gameplay/RanchService.cs": "cf3e4ded9b32e0595848533ca984113b4fa1f1f7",
 "OpenMakaiRanchGame/src/Gameplay/SharedEveningService.cs": None,
 "OpenMakaiRanchGame/src/Tests/DaySettlementRegressionTests.cs": "115e7055493851d8b9223f08c50702935fe680b3",
 "OpenMakaiRanchGame/src/Tests/RanchDesignRegressionTests.cs": None,
 "OpenMakaiRanchGame/src/Tests/UiLayoutAcceptance.Night.cs": "9998a88077f722581c87f103189f77c2984bfa5e",
 "OpenMakaiRanchGame/src/Tests/UiLayoutAcceptance.Projects.cs": None,
 "OpenMakaiRanchGame/src/Tests/UiLayoutAcceptance.SharedEvening.cs": None,
 "OpenMakaiRanchGame/src/Ui/WorldStationPanel.Projects.cs": None,
 "OpenMakaiRanchGame/src/Ui/WorldStationPanel.cs": "0600da779ad909d169ba87dfc17b1421ede87353",
 "OpenMakaiRanchGame/src/World/RanchBuildingPlots.cs": None,
 "OpenMakaiRanchGame/src/World/RanchPresentationBuilder.cs": "63acf357075b08fc9f091f3ea37938657a84a76d",
 "OpenMakaiRanchGame/src/World/WalkInBuilding.cs": "2659c6be16dcd6dc12e7370b9b44ef4ce6e216d9",
 "OpenMakaiRanchGame/src/World/WorldGameController.SharedEvening.cs": None,
 "Tools/Godot/validate_locales.py": "569c30cde72ee15a8bcd956bbfb926d1c130a3f8"
}

def git(*args):
    return subprocess.check_output(["git", *args], text=True).strip()

if git("branch", "--show-current") != BRANCH or git("status", "--porcelain"):
    raise SystemExit("Refusing an unexpected branch or dirty checkout")
for path, expected in GUARDS.items():
    file = Path(path)
    if expected is None:
        if file.exists(): raise SystemExit("New path already exists: " + path)
    else:
        raw = file.read_bytes()
        actual = hashlib.sha1(b"blob " + str(len(raw)).encode() + b"\0" + raw).hexdigest()
        if actual != expected: raise SystemExit("Changed source; review before applying: " + path)
parts = [Path(f"Tools/Godot/ranch_design_part{i}.txt") for i in range(1, 4)]
patch = zlib.decompress(base64.b64decode("".join(p.read_text() for p in parts), validate=True))
if hashlib.sha256(patch).hexdigest() != "f3cca1683af9522d900c7ea8f75b11dc2ac193813749e22441401c31fbb668fd":
    raise SystemExit("Patch checksum mismatch")
with tempfile.TemporaryDirectory() as temp:
    target = Path(temp) / "reviewed.diff"; target.write_bytes(patch)
    subprocess.run(["git", "apply", "--exclude=" + EXCLUDE, "--check", str(target)], check=True)
    subprocess.run(["git", "apply", "--exclude=" + EXCLUDE, str(target)], check=True)
helpers = [*parts, Path(__file__), Path(".github/workflows/apply-ranch-design.yml")]
for helper in helpers: helper.unlink()
subprocess.run(["git", "add", "--", *GUARDS, *map(str, helpers)], check=True)
subprocess.run(["git", "diff", "--cached", "--check"], check=True)
subprocess.run(["git", "commit", "-m", "feat(design): add optional ranch projects, voluntary shared nights and reserved building plots"], check=True)
subprocess.run(["git", "push", "origin", "HEAD:refs/heads/" + BRANCH], check=True)

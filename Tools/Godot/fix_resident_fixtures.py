"""One-use correction of two reproduced fixture prerequisites; assertions remain unchanged."""
from pathlib import Path
import hashlib
import subprocess

branch = "feature/world-stations-and-interiors-20260911"
def git(*args): return subprocess.check_output(["git", *args], text=True).strip()
if git("branch", "--show-current") != branch or git("status", "--porcelain"):
    raise SystemExit("Unexpected branch or dirty checkout")
edits = {
 "OpenMakaiRanchGame/src/Tests/SmokeTestRunner.cs": (
  "ac08459d8cd5ef69fe9ac627a067a56b6ddf9175",
  '''        traineeA.Fatigue = 0;
        traineeB.Fatigue = 0;
        Assert(result, training.Train(traineeA.Id, "ranch"), "first training slot succeeds");''',
  '''        traineeA.Fatigue = 0;
        traineeB.Fatigue = 0;
        // This tests the two-slot budget, not injuries left by the preceding adventure.
        // Explicitly restore living, recovered trainees and uncapped lesson skills.
        traineeA.Hp = 100; traineeB.Hp = 100;
        traineeA.Mature.IsCollapsed = false; traineeB.Mature.IsCollapsed = false;
        traineeA.Mature.FallState = FallState.Normal; traineeB.Mature.FallState = FallState.Normal;
        traineeA.RanchSkill = 2; traineeB.CombatSkill = 2;
        state.Calendar.TrainedToday = 0;
        Assert(result, training.Train(traineeA.Id, "ranch"), "first training slot succeeds");'''),
 "OpenMakaiRanchGame/src/Tests/ResidentInteractionRegressionTests.cs": (
  "5a667b00d4c0b0106ffbd2d70b377ab9e43e2512",
  '''        state.Roster.Characters.RemoveAt(state.Roster.Characters.Count - 1);
        before = Snapshot();
        Check(!service.Execute(resident.Id, ResidentAction.Meal).Success && before == Snapshot(), "missing meal does not consume player energy");''',
  '''        state.Roster.Characters.RemoveAt(state.Roster.Characters.Count - 1);
        // A new game includes meals. Remove only this isolated fixture's starting stock so
        // the missing-item assertion actually exercises a missing item.
        state.Inventory.Items.Remove("meal_box");
        before = Snapshot();
        Check(!service.Execute(resident.Id, ResidentAction.Meal).Success && before == Snapshot(), "missing meal does not consume player energy");''')
}
for path, (expected, old, new) in edits.items():
    raw = Path(path).read_bytes()
    if hashlib.sha1(b"blob " + str(len(raw)).encode() + b"\0" + raw).hexdigest() != expected:
        raise SystemExit("Source changed; review " + path)
    if raw.decode().count(old) != 1: raise SystemExit("Missing unique edit boundary: " + path)
for path, (_, old, new) in edits.items():
    file = Path(path); file.write_text(file.read_text().replace(old, new), encoding="utf-8", newline="\n")
helpers = [Path(__file__), Path(".github/workflows/fix-resident-fixtures.yml")]
for path in helpers: path.unlink()
subprocess.run(["git", "add", "--", *edits, *map(str, helpers)], check=True)
subprocess.run(["git", "diff", "--cached", "--check"], check=True)
subprocess.run(["git", "commit", "-m", "test(residents): explicitly establish living trainees and a genuinely missing meal"], check=True)
subprocess.run(["git", "push", "origin", "HEAD:refs/heads/" + branch], check=True)

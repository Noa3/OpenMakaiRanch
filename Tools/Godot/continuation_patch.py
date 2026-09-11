"""One-use, exact-source guarded patch for the dedicated continuation branch."""
import hashlib
from pathlib import Path
import subprocess

branch = "fix/intro-navigation-and-presentation-20260911"
if subprocess.check_output(["git", "branch", "--show-current"], text=True).strip() != branch:
    raise SystemExit("Refusing to patch any other branch")
path = Path("OpenMakaiRanchGame/src/Tests/UiLayoutAcceptance.cs")
raw = path.read_bytes()
sha = hashlib.sha1(b"blob " + str(len(raw)).encode() + b"\0" + raw).hexdigest()
if sha != "d8beff82a609f883f5d05db96d69a15ff553f414":
    raise SystemExit("Source changed; inspect before applying the opening regression hook")
text = raw.decode("utf-8")
old = "        await CheckCreationLayouts(shell);\n"
if text.count(old) != 1:
    raise SystemExit("Expected one creation acceptance boundary")
path.write_text(text.replace(old, old + "        await CheckOpeningJourney(world);\n"), encoding="utf-8", newline="\n")
Path("Tools/Godot/continuation_patch.py").unlink()
Path(".github/workflows/continuation-patch.yml").unlink()
subprocess.run(["git", "add", "--", str(path), "Tools/Godot/continuation_patch.py", ".github/workflows/continuation-patch.yml"], check=True)
subprocess.run(["git", "commit", "-m", "test(opening): reproduce erased narrative and bedroom utility return before repairs"], check=True)
subprocess.run(["git", "push", "origin", "HEAD:refs/heads/" + branch], check=True)

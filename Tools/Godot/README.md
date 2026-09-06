# OpenMakaiRanch Godot baseline tools

Run from repository root. Requires Python 3.10+, .NET SDK compatible with `global.json`, and stable Godot 4.7 **.NET**. No engine download or upgrade is performed.

## Launch / build / tests

```bash
python Tools/Godot/launch.py --mode check
python Tools/Godot/launch.py --mode editor
python Tools/Godot/launch.py --mode runtime --isolated
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python -m unittest discover -s Tools/Godot -p 'test_*.py' -v
python Tools/Godot/launch.py --mode smoke
```

`OpenMakaiRanchGame-Editor.bat` and `start-godot-mcp.bat` use this same launcher. `build-and-verify.bat` builds, runs launcher tests and executes isolated Godot smoke tests; it no longer claims verification after compilation alone.

Discovery order: `--godot` → `GODOT_BIN` / `GODOT_PATH` → `GODOT_HOME` → repository / repository tools / parent → bounded installed Godot directories → PATH. An invalid explicit executable fails closed. Stable Mono 4.7 is preferred before maintenance candidates; 4.6, development and non-Mono builds are rejected. Selected path, version and project are printed.

Smoke always uses a new profile under `.artifacts/godot/`. On Windows, both APPDATA and LOCALAPPDATA are redirected and a separate Godot preflight checks engine-resolved `user://` before the smoke flag is ever passed. Other platforms currently fail closed for isolation. Do not invoke `--run-smoke-tests` directly against personal saves: existing tests overwrite/delete slot 99. Preflight still constructs autoload services; it is a storage check, not a stripped boot.

The launcher refuses a conflicting bridge listener. Stop the exact owned runtime before smoke; stop the exact owned editor before import. Evidence directories retain engine logs, console output and disposable settings/saves. Do not delete normal user data to fix a test. Import/smoke timeouts return failure with captured output; editor/runtime remain open until closed normally.

## Character audit evidence

```bash
python Tools/Godot/verify_character_audit.py
python Tools/Godot/verify_character_audit.py --sources-only
python -m unittest discover -s Tools/Godot -p 'test_character_audit.py' -v
```

The default checks the original Chara CSV inventory, source hashes, listed field citations and declared code/data manifest. Metadata names must match the schema-1 neutral source-key vocabulary; a valid ID citation cannot be relabeled as an age or display-name citation. Duplicate JSON keys and decoder depth/integer-limit errors fail explicitly. A stale snapshot exits 1 and lists changed hashed inputs; never refresh hashes without re-auditing the affected conclusions. Validation stays active under `python -O` and `PYTHONOPTIMIZE`.

`--sources-only` intentionally skips the code/data snapshot and prints a distinct `AUDIT_SOURCE_EVIDENCE_PASS` marker plus `Code/data snapshot NOT CHECKED`. It is not a substitute for the full check. `--root` selects a relocated repository; `--report` is relative to that root unless absolute. Report-embedded Python is never executed. Evidence paths must remain inside the repository; reads are bounded to 16 MiB per file.

This checks the declared evidence, not the completeness of the human analysis, inferred ages, visual/context approval or runtime enforcement. No command certifies characters for adult presentation. Source files, report hashes, gameplay and saves are never changed.

## Project-local MCP adapter

The installed Coding-Solo tool server and another local GodotMCP checkout do **not** automatically speak this project's custom newline-JSON, dotted-tool-name addon protocol. A detached `npx ...` console is not a usable stdio connection. MCP clients must spawn the server and own its stdin/stdout.

`bridge_server.mjs` is a narrow adapter over the existing addon. It reuses the MCP SDK in the existing `mcp-server` package; it does not launch that package's unrelated workflow server. Install dependencies if absent:

```bash
npm --prefix mcp-server ci
node --test Tools/Godot/test_bridge.mjs
```

Configure a client with command `node` and one absolute argument pointing to `Tools/Godot/bridge_server.mjs`. No Hermes profile or other client settings are edited by this tooling.

After launching the editor, verify through a real stdio MCP handshake:

```bash
node Tools/Godot/verify_mcp.mjs editor_get_status
node Tools/Godot/verify_mcp.mjs editor_get_scene_tree
# Bash syntax; Windows cmd equivalent: set OMR_MCP_ALLOW_CONTROL=1
OMR_MCP_ALLOW_CONTROL=1 node Tools/Godot/verify_mcp.mjs editor_run_project
node Tools/Godot/verify_mcp.mjs runtime_get_status
node Tools/Godot/verify_mcp.mjs runtime_get_tree
node Tools/Godot/verify_mcp.mjs runtime_screenshot
OMR_MCP_ALLOW_CONTROL=1 node Tools/Godot/verify_mcp.mjs editor_stop_project
```

Read tools are enabled by default; editor open/run/stop require explicit `OMR_MCP_ALLOW_CONTROL=1`. `editor_open_scene` checks file existence and reads back the active scene before reporting success. Each editor/runtime request carries the expected project path; the addon rejects a mismatch before dispatch, and the adapter checks the responding connection's project identity before returning any data. Older addon copies without identity fail closed. Results are saved under `.artifacts/mcp/`; screenshots need a graphical runtime, not `--headless`.

Limits: ports 9500/9501 are this project's current defaults. Project-path identity prevents accidental cross-project access but is not authentication or a unique runtime-session token. The addon lacks authentication, robust raw-input limits and real log capture. The adapter's read-only policy is **not** a security boundary around direct TCP access. Do not port-forward these listeners; use only in a trusted local development session. Runtime tree depth is 4. `runtime.get_perf` is FPS/static memory, not a GPU profiler. Engine logs remain authoritative. These are development tools, not release/export approval.

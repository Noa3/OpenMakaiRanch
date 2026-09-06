# Decisions

## D-001 — One authoritative simulation (2026-09-05)

Preserve GameRoot, existing services, JSON registry and management UI. The world is an additive view with physical interaction entry points, not an independent economy/calendar/relationship model. Work animations and navigation do not pay rewards; existing day settlement remains sole authority initially.

## D-002 — Stable engine baseline (2026-09-05)

Use actual discovered stable Godot 4.7 .NET. Keep Godot.NET.Sdk/4.7.0 and net8.0 unchanged. Launcher supports configured/repository/installed/PATH discovery and verifies version. No 4.8 development builds; maintenance upgrade is separate work.

## D-003 — Test storage is disposable (2026-09-05)

Smoke overwrites/deletes slot 99. Always isolate Windows APPDATA/LOCALAPPDATA and verify engine-resolved user:// before passing the test flag. Keep evidence under .artifacts. Do not assume a high slot number is safe. Other platforms fail closed until equivalent isolation is validated.

## D-004 — Current save schema is 14 (2026-09-05)

Code is authoritative over stale schema-11 docs. Derived world presentation needs no new saved mesh/node state. Future authoritative location/work progress needs explicit versioned schema and fixtures. Repair existing flag/null migration problems before depending on them.

## D-005 — Explicit mutation and rebinding contracts (2026-09-05)

GameRoot replaces State/rebuilds services on NewGame and LoadSlot. World stores stable IDs, resolves current services, and cancels asynchronous work/reservations on state replacement. Add shared notification wrappers for direct UI mutations before dual views coexist. Do not introduce a generic event bus without a concrete need.

## D-006 — MCP implementation identity matters (2026-09-05)

Coding-Solo's installed server, external GodotMCP checkouts and this project's dotted-tool newline-TCP addon are not interchangeable. Use the narrow repository adapter and real MCP SDK roundtrip. Client owns stdio. Editor mutations opt in; each responding connection must match the project path. This prevents accidental cross-project use, not malicious local spoofing. Raw endpoint hardening remains open.

## D-007 — Adult validation is fail-closed, not relabeling (2026-09-05)

Confirmed adult identity and unambiguous adult visual/context review are distinct requirements. Source apparent age is not chronological age. Minor/minor-coded, ambiguous or unknown designs have no adult presentation clearance. No asset was visually certified; metadata audit is not runtime protection. Preserve original source read-only. Initial 3D slice is non-explicit; do not create adult-specific imagery or use numeric aging as clearance.

## D-008 — Art direction remains draft (2026-09-05)

One master character and one ranch section prove the pipeline before expansion. References precede detailed modeling, editable Blender is source, GLB is runtime export, Godot owns final shading. Significant identity/style choices require selection rather than unilateral final approval. Draft visual bible and manifest are planning artifacts, not shipped art.

## D-009 — Canonical continuity directory (2026-09-05)

Use existing OpenMakaiRanchGame/docs for handoff, kanban, decisions, issues, project state, migration plan, parity and art documentation. Root AGENTS/README link to it. Keep statuses honest and record exact executed checks. Preserve unrelated working-tree data; no automatic broad commit or cleanup.

## D-010 — Importer recovery before regeneration (2026-09-05)

The present importer csproj fails with missing Core project and Main. Treat existing JSON as valuable data, not disposable generated output. Recover provenance/implementation or develop a fixture-tested bounded importer before writing new runtime data. No fabricated original formulas.

## D-011 — No pre-release legacy-save support requirement

User clarified that nothing is publicly released and games start fresh. Old-save migrations and backwards compatibility are not development goals or release gates. Keep current-version save/load and live-state correctness; focus effort on playable features. Existing migration code/tests are not being removed as unrelated cleanup, but require no further expansion. Preserve user files. This supersedes earlier migration/old-save commitments in planning documents.

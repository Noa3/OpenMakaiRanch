# Decisions

Current living-world direction is recorded in D-013 to D-017 (2026-09-12), [LIVING_WORLD_PLAN.md](LIVING_WORLD_PLAN.md) and the task board. Earlier decisions retain their historical scope; preserving simulation is not a permanent one-building-per-menu layout requirement. No new financial rates or production implementation are approved merely by this documentation update.

## D-001 — One authoritative simulation (2026-09-05)

Preserve GameRoot, existing services, JSON registry and management UI. The world is an additive view with physical interaction entry points, not an independent economy/calendar/relationship model. Work animations and navigation do not pay rewards; existing day settlement remains sole authority initially.

## D-002 — Stable engine baseline (2026-09-05)

Use actual discovered stable Godot 4.7 .NET. Keep Godot.NET.Sdk/4.7.0 and net8.0 unchanged. Launcher supports configured/repository/installed/PATH discovery and verifies version. No 4.8 development builds; maintenance upgrade is separate work.

## D-003 — Test storage is disposable (2026-09-05)

Smoke overwrites/deletes slot 99. Always isolate Windows APPDATA/LOCALAPPDATA and verify engine-resolved user:// before passing the test flag. Keep evidence under .artifacts. Do not assume a high slot number is safe. Other platforms fail closed until equivalent isolation is validated.

## D-004 — Historical save-schema checkpoint: 14 (2026-09-05)

At this historical checkpoint, code was authoritative over stale schema-11 docs. Derived world presentation needs no new saved mesh/node state. Future authoritative location/work progress needs explicit versioned schema and fixtures. Repair existing flag/null migration problems before depending on them.

The numeral 14 is not a current-version pin. Later handoffs report schema 16; use actual current code and D-011 rather than downgrading it to match this historical entry.

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

## D-012 — C# 12 is the source-language contract (2026-09-10)

Explicit user request: use C# 12 throughout this project. Pin LangVersion to 12.0 in the game and repository defaults, and reject accidental latest/preview overrides at CoreCompile. A newer installed .NET SDK is not permission to use newer syntax. Preserve the existing target framework and Godot versions; the source-language requirement is independent of those versions. New changes and test fixtures must compile under this contract.

## D-013 — Useful places instead of one building per station (2026-09-12)

User direction: own housing and important public buildings should have plausible usable interiors. Kitchen and ranch office belong inside the main house, while genuinely different work functions may have separate buildings. Gameplay services/IDs and rooms/plots are separate concepts. Several actions can share a room or building without a second economic service. Ordinary private town houses need not all be open. Short everyday routes, persistent work plans and accessible information take precedence over new compulsory daily clicks.

This supersedes the old production-ring/fixed radial plan and the assumption that preserving an existing service ID requires preserving its separate facade or universal menu. Preserve current gameplay until the replacement is verified. See LW-03/LW-05 and LIVING_WORLD_PLAN sections 3-5.

## D-014 — Coherent expandable coastal region at consistent scale (2026-09-12)

User direction: a larger ranch/town setting with a sea-facing coast, believable drainage/terrain and room to grow and breathe. Design the shared terrain, routes and complete growth footprints first, while preserving a compact daily core and permanent green/shore space. Curves and asymmetry need a use/terrain reason. Metre-scale bodies, houses, fences, furniture, collision and camera must agree. Building upgrades add usable rooms/equipment rather than scaling all existing geometry.

A seamless experience can use separate authored scenes; no giant map, streaming rewrite or fixed metre dimensions are required before a real route works. The existing rejected coastal candidate remains rejected until corrected and walked. No mandatory extra movement costs or artificial day delays to compensate for layout. See LW-02/LW-04/LW-08/LW-17.

## D-015 — Approximately 300 inhabitants is a town impression, not an agent quota (2026-09-12)

The user's latest clarification explicitly permits background townspeople not to be rendered or individually exist. Preserve important merchant/story/companion identities and state; use bounded time/weather/stage-sensitive background presence for the rest. Architecture and activity should suggest a believable populated place. No requirement for 300 persistent NPC records, full agents or player-managed town beds.

Actual permanent ranch residents are a separate habitation problem: resolve identity, uncapped recruitment, finite beds and expansion explicitly. Ambient culling does not house real ranch residents. This supersedes earlier suggested mandatory 300-person household simulation. See LW-06/LW-07/LW-14.

## D-016 — Town development as visible ranch feedback, financial details unapproved (2026-09-12)

User direction: a modest civic/administrative or appropriately justified landlord contact receives understandable support; later construction and premises visibly develop as the ranch helps the town. Plan for new usable options and town participation, not endless player chores. Contributions and milestones may accelerate development; elapsed time alone and arbitrary minimum-day gates are not the intended progression. Completed improvements do not decay for missing optional contributions.

Still unresolved: official/ownership role, exact tax obligations, mana/energy pool, distribution, thresholds, project benefit and financial binding to original/remake rules. LW-09 must audit before LW-12 activation. No new taxes, rates, conversions, automatic deductions or second treasury are authorized by this design record. Reusing paid tax for civic development must not charge twice; sales and rewarded deliveries cannot be consumed/credited twice. Proposed four-stage visuals are planning, not fixed balance or source canon. See LW-09..LW-13.

## D-017 — Local asset-first delivery, scoped cleanup and honest evidence (2026-09-12)

User direction: local Astra should actually model, texture, furnish and create appropriate neutral usage assets with the available tools, then integrate and inspect them in the ordinary game. General economic/trait/dialogue/engine rewrites are not the local art assignment. Reuse recovered sources, finish one connected house/civic/market slice and one actual development before mass expansion. No paid services or cloud uploads without separate authorization.

Unused/replaced scenes may be removed after full reference/dynamic-path/UID/data/tool/test/export and successor checks. Names alone never justify deletion; retain sources, licenses, personal files, unrelated work and historic evidence. Keep fixed-condition before/after, actual walking, exact-head tests, Forward+ measurements and audio-review limits distinct. Passing tests do not equal human design approval. Implementation status lives in KANBAN, direction here and in LIVING_WORLD_PLAN, next action in ASTRA_HANDOFF.

# ASTRA Handoff

Checkpoint: **2026-09-10**. Resume from repository and CI evidence, not an older chat or the historical September 5 baseline. Canonical documentation lives in `OpenMakaiRanchGame/docs/`. The remake is not complete or release-ready.

## Current objective

The user requested actual continuation toward an enjoyable game. This slice gives existing ranch production a concrete optional purpose, repairs current save rejection, and fixes conflicting pause/Back routing. Prioritize understandable choices and visible outcomes over more disconnected systems. See `PLAYABILITY.md` for the implemented loop and remaining manual acceptance.

D-011 still applies: fresh games are expected and legacy-save support is not a development goal. The save change in this slice repairs unsafe rejection of unsupported data; it adds no schema or new backwards-compatibility feature.

## Git checkpoint

- Repository: `Noa3/OpenMakaiRanch`.
- Working branch: `integration/consolidate-open-work`.
- Existing PR: **#8**, base `main`; left open and unmerged because the full smoke suite still has failures.
- Last code commit: **`b7cf272d9b1af4a18da0753c992f1eafdf8591bc`**, `fix(input): give pause and nested Back a single event-owned route`.
- Documentation may be committed after that code checkpoint. Inspect the branch before any write and never force-reset concurrent changes.
- Current save schema is **16**, not the historical schema 14 mentioned in older documentation. CI uses pinned .NET SDK **10.0.401** and verified **Godot 4.7.2 Mono**. This slice did not change the engine or CI configuration.

## Completed in this continuation

**Save rejection** — commit `a1a907b99013e65197cf8da96372a88097e7b08a` rejects future schemas and null roster entries before normalization mutates the supplied state. Removed the unconditional current-schema stamp. Existing root rejection assertions now pass; failed loads preserve live state/services and source bytes.

**Optional courier orders** — commit `2a8dabecf7bc119d69865273641e12eef6b4495f` adds CommunityRequestService, a root command partial, CommunityBoardPanel and pause-menu access. Day 1 previews; deliveries open Day 2. Three choices consume existing farm goods, meals or supplies and pay 30-60 G through EconomyService. One delivery total per in-game day, no streaks or penalties, no mandatory town commute, no new stamina/time charge. The board explains production and links to the existing schedule screen.

The receipt uses four reserved live FlagService IDs, `1230000` through `1230003`, synchronized through the real root save boundary. Captured generation/day, stock validation, combat rejection, wallet bounds, completed receipts before notifications and per-day limits prevent stale/repeated callbacks from paying twice. No second production, calendar or economy model was introduced.

**Pause/Back ownership** — parent and child controllers now share `PauseMenuController.GoBack()`. Back closes the board before pause. World `_Process` no longer polls ui_cancel after its input event may already have been consumed. Both standard Back and remappable pause actions use the event route; echoes are ignored.

## Verification actually performed

Code checkpoint: `b7cf272d9b1af4a18da0753c992f1eafdf8591bc`.

- Build Smoke Check **#523**, run `34509942193`: success.
- Godot 4.7 Mono CI **#515**, run `34509942143`: restore, C# compilation, launcher regressions, engine resolution and project import succeeded.
- Isolated engine smoke: **1486 passing assertions, 3 failing assertions**. Overall smoke/CI remains **FAIL**.
- All **50 new community checks and 8 pause-input checks PASS**.
- Community checks include actual first-day pasture settlement -> Day 2 delivery, exact inventory/payment, bounds, stale day/generation, repeat/reentrant callbacks, serialized receipts, real root save/load, next-day replay protection and pause/schedule routing.
- Logs still contain **20 pre-existing `!is_inside_tree()` Transform3D errors**, also present in the earlier integration baseline. These were not repaired or newly attributed by this slice. Malformed-save fixture errors are separate expected rejection diagnostics.

Final evidence: artifact **`10165424061`**, `godot-4.7-verification`, SHA-256 **`82f1cf4bb465c63f2115d8d52b7b69f80cb3b8ede4bd2cc0e346a14a185f83be`**. Smoke log `.artifacts/godot/smoke-xo9wgypj/console.log`.

Negative control: code `c56d5358f76aab33d84ca63f37fd87b9c525e8ee`, run `34509481848`, artifact `10165309142`, failed the no-polling source guard and parent Back routing assertions before the code correction. The earlier `f724e85...` test attempt used an invalid ActionPress/just-pressed timing assumption inside the synchronous deferred smoke harness; it was replaced, not used as proof of a reproduced frame bug. Current tests exercise handler behavior and source ownership, not physical input timing.

## Three remaining smoke failures

All are in the existing `src/Tests/SmokeTestRunner.cs`, unchanged by this slice:

1. `constitution enables milk production`: the positive fixture uses the unreviewed starting rancher, so the existing eligibility gate correctly denies it. Separate denial coverage from an explicitly synthetic, unambiguously adult positive test fixture. Do not grant production characters approval or weaken the gate merely to pass a test.
2. `mana: Magic Supply Device refills personal MP from stored mana without increasing Max MP`: old assertion assumes 1:1 transfer. Existing production charges 2 stored MP per personal MP. Starting at 20 personal / 45 stored, request 30 restores **22**, leaving **42 personal / 1 stored**.
3. `mana: spell spending affects personal MP and applies its gameplay effect`: the subsequent 10 MP spell should leave **32** personal MP, not 40. The effect assertion must remain.

A dedicated new mana contract check confirms the current 2:1 behavior. It does not replace or silently skip these failing old assertions. Do not change production back to 1:1, disable smoke failures, or claim the build-only success means runtime acceptance.

## Next exact action

Correct those three fixtures/expectations in a local working copy with narrowly scoped edits, retain all relevant rejection/effect assertions, then rerun the isolated full suite. Do not re-consolidate old branches or repeat save/flag groundwork. Investigate the existing out-of-tree errors separately with actual call-site evidence.

Then perform a rendered Day 1 -> Day 2 playthrough from the real main menu: schedule work, use the established day/night flow, inspect production, complete a courier order, save/load, return to exploration. Check physical keyboard/controller navigation and narrow-window reachability. No manual rendered playtest, new final art, physical-controller test or enjoyment study was performed in this continuation.

For the next gameplay slice, favor optional world encounters, companion reactions and visible ranch improvements rather than expanding this small courier board into mandatory daily chores. The 60 G reward ceiling is an initial bound, not certified long-term balance.

## Guardrails and commands

Preserve GameRoot and its existing economy/calendar/roster/management authority. UI mutations use generation-checked root commands; observers resolve the current state after load. Original `eraMakaiRanch-game-eng-translation/` remains read-only. Existing eligibility gates and data approvals were not altered. Do not mass-generate assets, approve identities, delete unrelated files, rewrite branch history or implicitly merge PR #8.

From repository root:

```bash
git status --short
git diff --stat
git diff --cached --stat
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python Tools/Godot/launch.py --mode smoke
```

Smoke overwrites/deletes disposable slot 99. Always use the isolated launcher/profile validation, never a raw smoke flag against personal saves. Earlier document counts, local drive paths, importer reports and art approvals are historical unless reverified. Git history retains the superseded handoff.

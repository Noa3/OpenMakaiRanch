# Known Issues

Updated **2026-09-10** against code **`40daf7560a53b66d8210989f695bfb3e3a8ec692`**, PR #11. Distinguish executed checks, repaired defects, earlier audit leads and untested acceptance. See `ASTRA_HANDOFF.md` for reproducible evidence and the next work.

## Current verified status

**Godot 4.7 Mono CI #527**, run `34517743009`, and **Build Smoke Check #535**, run `34517742952`, succeeded. Effective C# 12 is verified for both Godot project files; a preview override is deliberately rejected. Restore, primary-game compilation, 34 launcher regressions and Godot 4.7.2 Mono import succeeded. The full isolated suite produced **1,547 OK / zero failed assertions / SMOKE PASS**.

The final import has no engine errors. The smoke log has **zero out-of-tree Transform3D errors** and five intentional malformed/unsupported-save rejection diagnostics. Those save errors are expected negative-fixture evidence, not newly observed player save failures. Do not hide them to make logs appear clean.

Artifact `10168420601`, SHA-256 `29d9079234d73ec4c1007a1f783e9b0231d6541f34be1e3b41f06f1b95043402`, smoke path `.artifacts/godot/smoke-ozygihzg/console.log`. Documentation may follow the verified code checkpoint without changing code.

## Open acceptance and risks

### UI-PLAY-001 — rendered and physical-device acceptance remains open

The new frame walkthrough now executes the complete first-day world/story flow, a real production-backed Day 2 delivery, nested Escape/Back and root save/load. It injects keyboard events into Godot and advances real process/physics frames. It stages positions near interactables and activates live buttons by signal, so it does not establish rendered hit targets, accessible routes around obstacles, or physical keyboard/gamepad behavior.

Run the real main-menu -> character creation -> first day and skip-to-night paths in a rendered isolated session. Check mouse capture and Alt-Tab/pause/travel, controller deadzones and look speed, camera collision/recenter feel, narrow-screen scrolling/text clipping, focus visibility and returning from all management screens. Headless mouse ownership checks are not a real OS cursor test. Camera feel and representative-hardware performance remain unmeasured by this continuation.

### GAMEPLAY-BALANCE-001 — optional orders are not long-term progression validation

The courier board remains in Pause -> Community Board and delegates planning to Schedule. It adds no physical noticeboard or new exploration encounters. Its 30-60 G rewards and one-delivery-per-day limit are preliminary bounds, not a validated long-term economy. No injected resources were needed for the tested Day 2 market basket, but that does not prove all upgrade/win paths are balanced. Optional encounters, companion-specific world reactions and visible ranch improvements remain future work.

### WORLD-NAV-001 — final navigation and visual production remain incomplete

Existing simple navigation/stand-ins are not final obstacle-aware authored environments. The frame walkthrough deliberately stages proximity and is not evidence that every doorway, station and encounter is reachable by ordinary pathfinding. Final character art, animation/morph pipeline, authored collision/navmeshes, weather readability and low/high Forward+ visual acceptance remain separate tasks. No new character identities/designs were approved here.

## Repaired and verified in PR #11

- **TEST-STALE-001:** the three old assertions now test the valid contracts. A separate synthetic adult numeric fixture tests positive production; a separate Unknown fixture stays denied. No production character approval or runtime gate changes. Supply storage remains 2:1: the test restores 22 personal MP from 45 stored, leaving 42 personal / 1 stored; a subsequent 10 MP spell leaves 32 and must still apply its effect. No assertions were removed to conceal failures.
- **WORLD-ERROR-001:** the PR #10 log had 16 remaining invalid transform accesses: 15 from a greybox fixture calling _Ready outside the tree, one from setting a companion target's GlobalPosition before tree entry. Tests now use real scene-tree readiness and pre-tree local Position. The final full suite has zero such errors, without suppressing engine logging. Live camera code also refuses detached/freed targets instead of applying an invalid local/global fallback.
- **INPUT-ANALOG-001:** boolean direction reads and unit-vector normalization discarded analog strength. Movement now retains stick/touch magnitude and reads the current main viewport camera; UI/focus/pause clear residual horizontal motion and touch sprint.
- **CAMERA-OWNERSHIP-001:** first-person capture no longer relies on a stale boolean after pause changes the real mouse mode. Active-camera ownership is released for UI/focus/pause/disabled/hidden/exit states and restored on a valid active update. A retiring rig cannot steal another camera's ownership. Analog look, default mouse-up, Invert Y, one-press recenter and Reduced Motion behavior have dedicated regressions.
- **STORY-INPUT-001:** completing a fade now preserves a visible story dialogue's input lock, and closing a dialogue preserves a pending transition/management lock. The skip/night/bath/report path is tested alongside the ordinary first day.

The intermediate frame-test Escape failure in run `34516975557` was a **test harness issue**: Bootstrap had routed to MainMenu after the fixture captured the old scene, leaving MainMenu active beside the world. The corrected fixture isolates the actual current scene after the deferred route. Actual Escape/Back events remain in the test and pass in `34517743009`; this is not claimed as a separate production Escape fix.

## Prior repairs retained

**SAVE-REJECT-001:** future schema and null roster entries reject before mutation; failed loads preserve live session/services/source bytes. Schema stays 16, without a legacy-support expansion.

**INPUT-BACK-001:** parent and pause menu share GoBack and do not separately poll ui_cancel after a consumed input event. Existing direct-handler/source checks and the new real-frame Back sequence both pass.

**GAMEPLAY-BOARD-001:** real stock, existing economy ledger, one-per-day bounded saved receipts, stale-command/overflow rejection and saved delivery replay protection remain covered. This continuation does not change their economic rules.

## Earlier audit leads — not revalidated by this continuation

Importer reports previously cited an absent Core project and missing Main; do not regenerate current JSON from guessed source conversions. ContentValidator was reported to check limited .tres metadata instead of full runtime JSON/reference integrity. Prior source review raised night-growth multiplicity, settlement idempotency and report-accounting questions; reproduce current behavior before claiming a repair. The successful one-day walkthrough is not exhaustive proof against those concerns.

Raw development MCP endpoints previously lacked authentication/size/export hardening; keep them local. Source-audit snapshots can become stale after code changes and are not identity/design approval. Original-game parity still lacks a certified original-engine differential suite.

Historical counts, schema 14, local machine paths and PR #8/#10 pending status in older notes must not be applied to the current branch. Fresh games remain the development target under D-011; preserve current-version save/load and never implicitly delete personal files.

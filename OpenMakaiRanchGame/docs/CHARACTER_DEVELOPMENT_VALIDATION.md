# Character development: executed validation

Checkpoint 2026-09-12. Gameplay branch `feature/gameplay-progression-20260912`, draft PR #19.
Verified code head: **`1d70fe2e0dbfdeff17af44508b4e41920d3b4f2f`**.
This continues `edbaad84548aa86d21bc385e9d000e39579b7f98`. Documentation-only commits may follow.
The review artifact's `ui-source/commit.txt` identifies its tested PR merge as
`6768d32961979f1b1294aae5e741e85029475c44`; a PR merge ref is not the branch head.

## Executed checks

| Workflow | Run | Result |
| --- | --- | --- |
| Build Smoke Check #672 | `34658905987` | success |
| Godot 4.7 Mono CI #664 | `34658905971` | success |
| Rendered UI acceptance #101 | `34658906067` | success |

The downloaded smoke console contains **2,047 SMOKE OK, zero SMOKE FAIL, one SMOKE PASS**.
Of those, **47** are the new character-development assertions. The downloaded rendered
results contain **526/526 passing checks**, overall success, and 64 PNG captures. The UI
console has no ERROR or SCRIPT ERROR. These are existing rendered regression scenarios,
not a new transformation menu, rendered morph test or physical-device certification.

The inspected full-checkout CI Python run passes **76 tests**, including the two original
source-receipt tests. The local review-source run separately discovers 76: **74 pass and two
are explicitly skipped** because the review ZIP does not contain the original ERA source.
The existing 344-key UI locale validator also passes. Eighteen new non-UI English/German
character keys have separate checks. No local Godot or .NET execution is claimed.

Compilation succeeds but is not warning-free: inherited NuGet-source, obsolete drawing-call
and nullable warnings remain. Smoke retains five intentional invalid-save ERROR diagnostics;
imports retain the known EditorSettings shutdown diagnostic. Software rendering retains its
VSync warning. No test/error filter was removed to produce a passing result.

## Failures found and corrected

The first character commit `cf95a0c` failed compilation because its new test lacked the
resource-enum namespace import. Commit `0929797` corrected it and made original source
receipts use Git object bytes rather than potentially CRLF-converted checkout bytes.

That revision then failed the actual root save/load snapshot assertion. A blank body-profile
override resolved differently before and after the existing save loader normalized it.
`1d70fe2` uses the canonical `RosterService.DefinitionFor` for body identity and HP/SP
capacities. It also prevents an uncatalogued resident's larger effective capacity from being
replaced by an invented small default. The failing save/load assertion remains, now passing;
new fallback, authored-override and saturation assertions supplement it.

## What the new tests establish

EP and MP stay separate. Exact original/numeric and existing semantic ward IDs are recognized;
nearby IDs or a race name do not invent a ward. EP half-capacity boundaries, missing capacity,
large values and preservation of the owned trait are exercised. This only validates the
resource snapshot, not a complete original ward/action-permission system.

The pure original power reference preserves the level/EP-to-MP threshold, sequential integer
rounding and wide arithmetic. It rejects unsupported inputs. It does not replace modern combat.

Development checks cover read-only inspection/capture, before-value baselines, actual skill
changes, no resource refill, capped capacity benefits, bounded ordered history, ownership and
revision guards, stale/ambiguous identity rejection, and no repeated reward from losing and
regaining an old skill. Current-schema JSON retains the journal and first-observed baseline.

Real resident practice pays the existing costs and uses its original lesson budget once.
The root EndDay scenario records an actual patrol skill increase, performs ordinary
SaveSlot/LoadSlot, compares the full snapshot, and explicitly starts New Game+ to check the
new-run reset. It stages the prerequisite skill XP and day: it is not an organically earned
campaign playthrough. Tests use isolated profiles and refuse an occupied disposable slot 99.

The old resident-care receipt assertion still uses its previous limit, scoped to its own
six receipt IDs rather than unrelated character flags. Development has its own 81-entry
maximum-storage assertion. No old gameplay assertion was deleted to hide a regression.

## Evidence and reproduction

Successful Godot artifact: `10287005505` from run `34658905971`.
Successful rendered/review-source artifact: `10286870975` from run `34658906067`.
The evidence package contains the downloaded console/results/launch receipts, source merge
receipt, source-audit/development documents and a manifest with checksums of the actual ZIP
bytes used. Artifacts are expiring test evidence, not exported game builds or full checkouts.

```bash
python -m unittest discover -s Tools/Godot -p "test_*.py"
python Tools/Godot/validate_locales.py
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/ui_acceptance.py --rendered --timeout 360
```

Only use isolated launchers; never run raw test flags against personal save profiles.
Original files, schema 16, engine/SDK pins, graphics settings and visual branch refs remain.

## Limits

This is a non-explicit development foundation through resident practice and normal root
settlement. Raw legacy training/direct state edits are not universally observed. A complete
resistance/action matrix, detailed body/race/trait transformations, equipment-fit consequences,
resident stories, the original level-conversion logic and actual model morphs remain open.
The small HP/MP capacity stages are remake tuning, not copied original progression rates.
The tactical engine scales HP, so a five-point capacity gain is not a promise of five more
battle HP. No long-term balance, full original parity or physical-device claim follows.

Historical ranch policy results remain in GAMEPLAY_PROGRESSION_VALIDATION.md under their
original tested commit. They are not relabeled as new character-development evidence.

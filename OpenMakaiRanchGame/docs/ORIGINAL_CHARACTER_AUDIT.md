# Original character systems: scoped source audit

2026-09-12. Reference repository head `edbaad84548aa86d21bc385e9d000e39579b7f98`.
The files below are the bundled original, not inferred from the remake's display names.
They remain read-only. This is a focused audit, not certification of every original action.

## What the remembered protection actually is

`eraMakaiRanch-game-eng-translation/ERB/○TRAIN/処女結界保護フラグ.ERB`,
function `VARGIN_BARRIER_BREAK_FLAG`, blob `9c7197fd10f8abd4c26bc4aa5e3241942d4ed34d`:

The original has a specific intimate-action ward, not a general transformation, damage or
poison immunity. Its resource comparison uses **EP (霊力 / Spirit Energy)**, not
**MP (魔力 / Mana)**. The low-resource branch is `BASE:霊力 <= MAXBASE:霊力 /2`.
The owned talent and the temporary active flag are separate: this function does not delete
the talent merely because the resource is low. There are also original state-dependent
bypasses, including coercive conditions. Those bypasses are not implemented in this slice.

`CharacterProtectionService` exposes exact known catalog ID `2`, earlier `talent_2`, and
the remake's existing semantic ID `virginity_barrier`, EP reserve status and the separate MP
values. `WardReserveReady` means ONLY that the
resource prerequisite is met; it is NOT a complete reconstruction of the source's active
flag and is NOT permission for an interaction. Unknown capacities fail closed. Depletion,
affection or incapacity never removes the trait through this adapter. Nothing here authorizes
intimate actions, breaks the ward, or adds coercive action handlers. Existing content is not
silently rewritten. The shared recovery guard is used by ordinary practice and its new benefits.

Do not make every protective trait interchangeable. The catalog's other resistance names do
not establish blanket immunity to arbitrary commands. Wider effect-specific resistance needs
its own source audit, authored non-explicit actions and executor tests.

## Distinct resources and initial versus current values

`CSV/base.csv`, blob `f990d5208e6aec95c41a0ea89c6efae4d3bed929`, defines HP (0),
SP (1), EP (2), MP (3), level (4), battle power (10), aptitude (17), fatigue (19), and
separate mental values. The 100-series preserves initial base parameters at character creation.
These are not one interchangeable energy pool.

The remake retains its existing CharacterState fields and separate Player daily stamina.
New development does not spend ranch storage, restore current resources on a capacity increase,
or turn MagicPower (aptitude) into spendable Mana. A first-observed baseline is explicitly NOT
an invented reconstruction of a loaded resident's original creation values.

## Strength is not just a fixed portrait or class

`ERB/戦闘/あなた戦闘_キャラ選択.ERB`, `CALC_BATTLE_POWER`,
blob `725c1ebe5665ffb23a3383679d33e549d3d29e7d`:

Maximum potential is maximum EP plus maximum MP. Current power uses current EP and MP.
At a current MP-to-EP ratio of at least 10:1 AND level below 30, only one tenth of current
MP contributes. The integer HP percentage is calculated first, applied to power, then the
integer combat-aptitude percentage is applied. Sequential rounding is observable.

`OriginalCharacterRules.InspectCombatPower` implements that arithmetic for explicitly supplied
aptitude 0..100 and well-formed nonnegative resources within their maxima. It returns an
invalid result for unsupported input instead of guessing. The original routine's side effects
which establish aptitude are not ported; modern CombatSkill is not silently treated as that
percentage. This is a read-only reference calculator, not a new tactical damage formula.
Existing tactical attacks, equipment, defense and fatigue remain on their existing scale.

## Body changes have downstream consequences

`ERB/内部計算・ステータス増減/膨乳.ERB`, `BUSTUP`,
blob `5bbfc7fd315afee69c09b3978e32a5ad19091f82`, uses discrete body stages, records
experience/change counts, changes mutually exclusive trait bands at thresholds, calls capacity
recalculation, and can alter equipment, purposes, rooms or work arrangements. This establishes
a structural loop: **cause -> persistent change -> threshold -> gameplay consequences -> presentation**.
It does not establish that every height, race or body-shape field automatically changes daily.

The implemented non-explicit slice adopts that structural loop for physical conditioning and
magical development. It does not port erotic anatomical growth, equipment exposure, involuntary
sexual transformations or the original schedule substitutions. Height/body identity are exposed
without overwriting them, and the renderer receives continuous 0..1 channels without a mesh
or art decision. This is not complete original body-transformation parity.

## Recovery and level cautions

`ERB/内部計算・ステータス増減/回復_SP,EP,精神.ERB`, `EP_HEAL`,
blob `fa2e1faaddeaaf3d7b1255cb4d2f1f67f4af1a4a`, returns when TARGET is not the player.
Do not claim that automatic EP regeneration for every resident would preserve this source rule.
This slice adds no resident EP regeneration, mana drain or second recovery clock.

`ERB/内部計算・ステータス増減/霊力→レベル計算.ERB`,
blob `ad1b23dade874abfd203db2efa368eb74b2f1699`, also has conversion/loop semantics that
need caller-level verification. No inferred level formula was applied from its comments alone.

## Executable evidence

The Python source-receipt tests verify the exact ward and combat blob bytes and their key
conditions in a full checkout. They are explicitly skipped in review archives lacking the
original. Godot smoke tests exercise the C# EP boundaries, resource separation, integer combat
arithmetic, read-only inspection, and actual development/save boundaries. See
CHARACTER_DEVELOPMENT.md for implementation scope and CHARACTER_DEVELOPMENT_VALIDATION.md
for actual execution results; the historical ranch-progression counts are not new evidence.

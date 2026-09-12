# Character change is a core gameplay pillar

Gameplay branch `feature/gameplay-progression-20260912`, PR #19. Based on
`edbaad84548aa86d21bc385e9d000e39579b7f98`; graphics PR #18 is untouched.
Read ORIGINAL_CHARACTER_AUDIT.md for verified source rules and explicit adaptations.

## Design contract

The ranch is the place where characters live and change, not the entire goal by itself.
Every future development feature needs a real cause, persistent current values, a comparison
with its starting point, meaningful consequences and a presentation contract. A different
portrait alone is not a complete transformation system. Neither is a bonus that has no
observable history or costs. Keep skill, bodily condition, identity, emotional state and
resource capacity distinct; changing one does not silently rewrite all the others.

This slice implements a non-explicit foundation through existing Practice and normal EndDay.
It adds no daily task, minimum date, compulsory romance, extra currency, clothing decision,
world geometry or new scene. Original explicit/coercive mechanics are not reproduced here.

## Actual implemented loop

Before an accepted resident practice lesson, capture the resident's permanent values. The
existing TrainingService pays its original costs and changes its original skill. Afterwards,
observe the actual difference once. At the ordinary root day boundary, capture the roster
before settlement and compare after the existing growth passes. No second XP pass is run.

Track ranch/craft/combat skill, magical aptitude, HP/SP/EP/MP capacity, height in millimetres,
and two bounded development channels. The first committed observation stores its BEFORE values
as a baseline. Existing saves do not receive historical rewards or fabricated lifetime history.
A successful workday can establish a baseline without granting any bonus. Reading a character,
opening an interface, free conversation, a failed lesson and a redraw never initialize it.

A 12-entry ring journal stores field, cause, day, old value and new value. Actual losses can be
recorded too; they do not generate benefits. High-water marks prevent losing and regaining the
same skill from repeatedly earning capacity. Revisions and capture ownership reject duplicate,
stale, cross-state, renamed/replaced or ambiguous-character completions. Fixed IDs and numeric
history survive the current save schema; display strings are translated when read.

### Initial remake tuning, not original numeric rules

Two newly gained combat-skill points advance one conditioning stage and add **5 maximum HP**.
Four newly gained magical-aptitude points advance one attunement stage and add **5 maximum MP**.
Each channel has three stages: at most **15 added capacity** of its respective resource.
They track real new skill highs, not button presses or elapsed days. An incapacitated resident
does not earn these benefits. Existing ordinary work/lesson effects are otherwise preserved.

Neither stage restores current HP/MP, spends or creates EP, or resets SP/daily stamina.
The HP benefit is a persistent capacity; the existing tactical engine scales HP and has a
minimum floor, so a 5-point increase does NOT promise an immediate additional tactical HP.
Existing CombatSkill/MagicPower increases retain their separate, actual tactical effects.
The rates are deliberately small initial tuning, not measured long-term balance or new
source-equivalent character levels. No retrofit bonus is granted for high imported skills.

## Protection scope

The original resource-backed ward is reported as **trait ownership + EP reserve**, with MP
separately visible. This is not a universal action blocker. Its source-specific coercive
bypasses and intimate actions are excluded. Do not treat WardReserveReady=false, affection,
low spirit, missing data or collapsed state as consent or permission. No public arbitrary
stat-rewrite/ward-break command was added. Recovery protection in the existing resident
boundary now shares the same helper used by development benefits.

## Presentation handoff, without graphics changes

`GameRoot.GetCharacterDevelopment(id)` returns fresh, read-only values containing baseline,
current fields, revision, first observed day, recent changes and protection. The morph value
contains `Conditioning` and `Attunement` in 0..1, the current authored `BodyTypeId`, and
`HeightMillimetres` (1600 is 1.6 metres, NOT 1600 centimetres). Body identity and HP/SP
capacities resolve through the existing RosterService definition, including its fallback for
uncatalogued residents. Blank body overrides no longer produce a different snapshot merely
because the existing save loader normalizes them. No authored body override is replaced by
development. These channels carry gameplay state; no mesh, blendshape name, camera or shader
implementation is implied.

`GameRoot.GetCharacterProtection(id)` returns the protection snapshot alone. Existing
`StateChanged` is the invalidation signal after the real command; do not retain another
session's resident object after NewGame/Load. Capacity changes appear in the existing lesson
result; workday changes appear in the ordinary daily report. A dedicated journal/ward panel
is a later presentation integration, not a secretly added UI layout.

`OriginalCharacterRules.InspectCombatPower(character, maxHp, explicitAptitudePercent)` is a
pure source-reference function, not the battle engine's preview. Do not substitute it into
modern damage calculations without scaling and encounter balance tests.

Flags `1_231_000..1_231_159` are reserved per character. Actual maximum storage is 81 numeric
entries: six metadata/channel values, eleven baseline values, four high-water marks and
12*5 history fields. No per-day keys or global character-history table is allocated. The
canonical root FlagService saves them; do not write a second unsynchronized FlagStorage.

## Playable feedback and shared practice commands

`GameRoot.GetCharacterDevelopmentTrack(id, focus)` returns a read-only long-term forecast for
combat conditioning or magical development. It exposes stage, current skill, recorded high,
next target and actual capacity available, with explicit completed, skill-limit, capacity-limit
and invalid states. It does not authorize a lesson; `Residents.Inspect` remains the source of
current availability. Shared constants keep forecasting and awarding on the same tuning.

Existing available practice buttons show this guidance in their tooltips, and lesson results
explain the next threshold. No new panel or rendering code is needed. Free resident conversation
can now acknowledge a real recorded skill change; immediate fatigue or low morale takes
priority. Reading or talking does not write a receipt, award a reward or invent a character
history. These are contextual reactions, not completed resident story arcs.

See CHARACTER_DEVELOPMENT_CONTINUATION.md for the command contract, concrete examples,
verification and remaining limits. The original implementation's test counts remain historical
in CHARACTER_DEVELOPMENT_VALIDATION.md.

## Deliberate boundaries and next implementation work

ResidentInteractionService practical lessons and GameRoot EndDay are wired. The legacy root
`TrainCharacter` entry now delegates to the same stamped resident command via
`TryTrainCharacter`, so it cannot evade player costs, daily receipts or development recording.
Low-level `TrainingService.Train` simulation calls and arbitrary direct state edits still do
not gain a journal hook. Do not claim universal tracking of every stat mutation.
Other original transformations, detailed trait/race/body recipes, recovery choices, character
arcs, equipment-fit effects and dynamic model application remain separate authored work.
There is no completed transformation menu, full resistance matrix or original-level formula.

Prioritize those shared command boundaries and coherent resident-change stories over further
ranch checklists. Keep authored identity/age eligibility and all existing assets unchanged.
Test actual causes, failure/permission paths, current-save continuation and tactical economics
before adding a new branch of development. Historical reports in GAMEPLAY_PROGRESSION_VALIDATION
remain valid only for their own commit. The initial character evidence is in
CHARACTER_DEVELOPMENT_VALIDATION.md; the shared-command and feedback continuation is documented
with its own exact-head results in CHARACTER_DEVELOPMENT_CONTINUATION.md.

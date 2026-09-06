# Original Feature Matrix

Baseline 2026-09-05. This is a traceability seed, **not a parity certificate**. Paths starting `CSV/` are relative to `eraMakaiRanch-game-eng-translation/`; remake paths are relative to `OpenMakaiRanchGame/`.

Status vocabulary: NOT_ANALYZED, DOCUMENTED, CORE_IMPLEMENTED, 3D_INTEGRATED, TESTED, PARITY_VERIFIED. A remake smoke test does not elevate unknown original formulas to PARITY_VERIFIED. Separate remake implementation evidence from original-comparison status.

| Original feature / area | Source files | Original variables | Original formulas | Current remake implementation | 3D representation | Status | Tests / evidence |
|---|---|---|---|---|---|---|---|
| Hero/player identity metadata | All 11 Chara CSVs, exact paths/lines in ../audit/character-metadata.json | 番号, 呼び名, CSTR identity/appearance fields, フラグ 外見年齢/身長 | Height units documented; mm divided by 10 for cm; no gameplay formula claim | data/characters.json has 10 definitions, 9 supported source correspondences; no numeric source foreign key | Stable-ID avatar/profile adapter planned | DOCUMENTED | 11 source hashes, 136 field citations, 12 input hashes independently checked; no visual-age approval |
| Items/catalog | CSV/Item.csv exists; row/formula audit pending | Not analyzed | Not analyzed | data/items.json has 98 entries; Inventory/Shop/Equipment services exist | Same services from storage/shop/management UI | NOT_ANALYZED | Remake smoke coverage exists; original fixture comparison absent |
| Talents/skills | CSV/Talent.csv, CSV/Abl.csv exist; mapping audit pending | Not analyzed | Not analyzed | data/talents.json 47; skills.json 12; service/resource implementation exists | Character profile UI and derived visuals planned | NOT_ANALYZED | Counts are not formula parity |
| Time/day structure | CSV/Time.csv and CSV/Day.csv exist; actual ERB control flow not yet traced | Not analyzed | Not analyzed | DayCycleService and root AdvanceTime/EndDay; four explicit phases | Lighting and NPC locations derive from existing Calendar | NOT_ANALYZED | Day/phase smoke assertions; original control-flow comparison pending |
| Ranch work/production | Exact original ERB call chain not yet established | Not analyzed | Not analyzed | ScheduleService assignments; RanchService.ApplyJobOutput; DailySettlementService sole day reward path | Station changes assignment; travel/work visuals initially presentation only | NOT_ANALYZED | Remake settlement/output checks; no duplicate visual reward path implemented |
| Relationships/events | Exact original ERB event sources not yet established | Not analyzed | Not analyzed | BondService mentorship/completion, 30 bond events; daily events separate | Dialogue/camera staging over existing mutations planned | NOT_ANALYZED | Existing event smoke; no original text/formula coverage claim |
| Adventure/combat | Exact original battle/mission sources not yet established | Not analyzed | Not analyzed | 14 enemies, 12 missions, adventure/combat services | Future travel/combat presentation, retain current UI | NOT_ANALYZED | Remake smoke only; original differential fixtures absent |
| Imported action definitions | CSV/Train.csv exists; original command mapping/provenance audit pending | Not analyzed | Not analyzed | training_actions.json has 111 definitions; importer project currently cannot build | Adult-specific presentation blocked; no visual implementation | NOT_ANALYZED | Data existence is not full action parity or content safety |
| Persistence/progression | Original save variable schema not yet traced | Not analyzed | Not analyzed | SaveState schema 14, SaveService/SaveMigrator, flags/reports | Reconstruct world from IDs/state; no mesh internals in saves | NOT_ANALYZED | Isolated roundtrip/migration smoke; flag/null edge cases remain |

## Promotion rules

For each analyzed feature, record exact original ERB functions, variables, branches, units, formula inputs/outputs, remake call site and deterministic fixtures. Record intentional deviations separately. Use original source read-only; do not execute unknown reference binaries to obtain results. Never fill a formula cell from intuition or a filename. Preserve uncertainty and unmapped records.

No row currently reaches PARITY_VERIFIED. Prioritized first formula audit: non-explicit daily work/time/economy behavior that constrains the shared 3D loop. Adult eligibility is a separate safety gate, not something inherited from original mechanics.

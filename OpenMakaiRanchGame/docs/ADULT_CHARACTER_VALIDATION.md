# Adult Character Validation — Neutral Source Audit

## Decision and scope

**Not cleared for adult presentation. This audit is documentation, not an implemented runtime safeguard.** Block minors, minor-coded or ambiguous designs, unknown-age identities, and unreviewed assets from adult presentation. An apparent or asserted numeric age of 18+ is not sufficient: numeric relabeling does not clear a minor-coded design, school-age context, or conflicting evidence. Supernatural chronology is not a substitute for adult appearance and identity review.

The original `eraMakaiRanch-game-eng-translation` reference was read-only. This audit enumerates every `Chara*.csv` recursively, including the player, default template and sample. It preserves neutral identity, apparent age, height, race, occupation, hair and eye metadata only. No imagery was opened, generated, or redesigned; no sexual dialogue is reproduced. **No visual age certification is made.** Static code inspection and a deterministic replay of the age selector are not a runtime safety test.

Machine-readable evidence: [`audit/character-metadata.json`](audit/character-metadata.json). Each extracted field has its exact source value, repository-relative file and 1-based physical line. Source hashes, remake field lines, code-input hashes, correspondence confidence, missing-age status and a read-only Python extractor are included. Values not present in the source are not invented; `confirmed_age` is null throughout.

## Verified inventory

- **11 unique source files and 11 unique numeric IDs:** 8 heroes, 1 player, 2 templates/samples. No duplicate paths or IDs.
- **10 remake definitions:** the 8 heroes, `anon`, and `rancher`.
- **9 supported source-to-remake correspondences:** 8 heroes and source player ID 0 to `anon`.
- **2 source definitions without supported mappings:** ID 2 and ID 200. **1 remake-only/unresolved definition:** `rancher`.
- **2 source apparent ages below 18:** Surei/Slay (13) and Maria (15). All **11 confirmed chronological ages remain unknown**. All **10 remake definitions lack age fields** in `characters.json`.
- **0 images reviewed and 0 characters cleared.** Height alone does not determine whether someone is an adult.

### Complete source catalog

All ages below are **apparent**, not confirmed chronological ages. Height conversion is exact millimeters / 10, not rounded. The sample ID 200 documents millimeter units at line 340 and explains at line 316 that actual age is not implemented. A displayed 18, 20, 21, or 26 therefore does not confirm adulthood.

| Source ID | Source label | Role | Apparent age (line) | Height cm (line) | Remake ID | Adult presentation |
|---|---|---|---|---|---|---|
| 0 | Anon | player | 20 (L60) | 190 (L59) | anon | Blocked pending age/design review |
| 1 | Surei | hero | 13 (L60) | 144 (L57) | slay | Blocked: minor apparent age |
| 2 | Default-Chan | template or sample | 18 (L38) | 160 (L37) | No supported match | Blocked pending age/design review |
| 3 | Kagura | hero | 18 (L79) | 151 (L83) | kagura | Blocked pending age/design review |
| 4 | Maria | hero | 15 (L67) | 158 (L68) | maria | Blocked: minor apparent age |
| 5 | Sharon | hero | 21 (L72) | 149 (L69) | sharon | Blocked pending age/design review |
| 6 | Noir | hero | 26 (L69) | 162 (L66) | noir | Blocked pending age/design review |
| 100 | Ayaka | hero | 18 (L72) | 154.6 (L75) | ayaka | Blocked pending age/design review |
| 101 | En | hero | 18 (L74) | 165.4 (L79) | en | Blocked pending age/design review |
| 102 | Yukina | hero | 18 (L75) | 158.2 (L78) | yukina | Blocked pending age/design review |
| 200 | Kai | template or sample | 20 (L317) | 156 (L344) | No supported match | Blocked pending age/design review |

Source files for the line references above and below (paths relative to repository root):

- **ID 0:** `eraMakaiRanch-game-eng-translation/CSV/000～魔界牧場およびシステムキャラ/Chara0_あなた.csv`
- **ID 1:** `eraMakaiRanch-game-eng-translation/CSV/000～魔界牧場およびシステムキャラ/Chara1_スレイ.csv`
- **ID 2:** `eraMakaiRanch-game-eng-translation/CSV/000～魔界牧場およびシステムキャラ/Chara2_デフォ子.csv`
- **ID 3:** `eraMakaiRanch-game-eng-translation/CSV/000～魔界牧場およびシステムキャラ/Chara3_かぐら.csv`
- **ID 4:** `eraMakaiRanch-game-eng-translation/CSV/000～魔界牧場およびシステムキャラ/Chara4_マリア.csv`
- **ID 5:** `eraMakaiRanch-game-eng-translation/CSV/000～魔界牧場およびシステムキャラ/Chara5_シャロン.csv`
- **ID 6:** `eraMakaiRanch-game-eng-translation/CSV/000～魔界牧場およびシステムキャラ/Chara6_ノワール.csv`
- **ID 100:** `eraMakaiRanch-game-eng-translation/CSV/100～オリジナル/Chara100_彩華.csv`
- **ID 101:** `eraMakaiRanch-game-eng-translation/CSV/100～オリジナル/Chara101_縁.csv`
- **ID 102:** `eraMakaiRanch-game-eng-translation/CSV/100～オリジナル/Chara102_雪菜.csv`
- **ID 200:** `eraMakaiRanch-game-eng-translation/CSV/200～版権少数枠/Chara200_カイ_ドルアーガ（サンプル）.csv`

### Neutral appearance and occupation metadata

Original Japanese values are preserved rather than silently replaced with remake translations. “Not specified” means no active allowlisted field in that CSV; it does not infer a default or mirror the other eye. Other allowed metadata, including full names, identity identifiers, origin-work labels and explicit color codes, is in the JSON report.

| ID | Race | Occupation | Hair color / style / feature | Right / left eye color |
|---|---|---|---|---|
| 0 | 魔界人 (L41) | 酪農家 (L48) | 黒髪 (L53) / ショート (L54) / Not specified | 赤 (L55) / Not specified |
| 1 | 人間 (L28) | 捨て子 (L29) | 金髪 (L33) / ショート (L34) / Not specified | 青 (L32) / Not specified |
| 2 | 人間 (L18) | Not specified | 黒髪 (L22) / ロング (L23) / Not specified | 黒 (L21) / Not specified |
| 3 | 人間 (L49) | 退魔巫女 (L50) | 黒髪 (L55) / セミロング (L56) / Not specified | 黒 (L54) / Not specified |
| 4 | 人間 (L45) | バトルシスター (L46) | 金髪 (L51) / 一本結び (L52) / Not specified | 青 (L50) / Not specified |
| 5 | 人間 (L47) | 白魔術士 (L48) | 桃髪 (L54) / ロング (L56) / 片目隠れ (L55) | 空 (L52) / Not specified |
| 6 | 人間 (L45) | 黒魔術士 (L46) | 銀髪 (L52) / ベリーロング (L53) / Not specified | 赤 (L50) / Not specified |
| 100 | 人間 (L39) | 退魔師 (L40) | 赤髪 (L45) / セミロング (L47) / ストレート (L46) | 青 (L44) / Not specified |
| 101 | ダンピール (L41) | 退魔師 (L42) | 栗毛 (L47) / セミロング (L49) / エアリー (L48) | 茶 (L46) / Not specified |
| 102 | 人狼 (L41) | 退魔師 (L43) | 銀髪 (L48) / 一本結び (L50) / ナチュラル (L49) | 赤 (L47) / Not specified |
| 200 | 人間 (L209) | イシターの巫女 (L210) | 黒髪 (L231) / ロング (L233) / Not specified | 赤 (L228) / Not specified |

### Correspondence and uncertainty

- Source ID 1 says **Surei**, with identity identifier `スレイ`; remake uses **Slay / `slay`**. The 144 cm height, Human race, foundling occupation and blonde short hair/blue eyes support this correspondence. These distinct labels are preserved. The JSON has no explicit numeric source foreign key, so this is evidence-backed correspondence, not an authoritative ID link.
- Source ID 0 supports **`anon`**. **`rancher`** is a separate remake player-role definition with similar neutral metadata, but no independently established numeric source mapping. Do not map it to ID 2 simply to fill the inventory.
- **ID 2 Default-Chan** and **ID 200 Kai** remain included, despite having no supported remake definition. ID 200 is a sample referencing *The Tower of Druaga*; its apparent age 20 does not establish canonical chronological age or provide a license/visual clearance.
- **Maria:** source L63 has `童顔` (youthful/baby-faced trait), in addition to apparent age 15 at L67. **Ayaka:** source L70 has `ＪＫ` (schoolgirl/high-school-girl context marker), despite apparent age 18 at L72. These are contextual review concerns, not visual findings. Ayaka's actual age is not inferred from the marker.
- Source heights are **Ayaka 154.6 cm, En 165.4 cm, Yukina 158.2 cm**; remake strings are respectively **155 cm, 165 cm, 158 cm**. The remake approximations must not overwrite exact source measurements.

## Generator audit

`src/Gameplay/CharacterGenerationPools.cs:379–384` defines apparent ages **12, 14, 16, 18, 22, 28, 50**, labeled Childlike, Young Teen, Teen, Young Adult, Adult, Mature and Elderly. `GenerateApparentAge` at **420–435** assigns weights **5, 8, 15, 25, 20, 12, 5**. Despite the “percentages” comment, weights total **90**, not 100.

An exhaustive Python replay of all **90** possible integer rolls returned exactly those weights per age. **28/90 rolls (31.11%) select apparent ages below 18.** This is a static algorithm check, not a C# execution or a rendered-character test. The JSON records every outcome count and the calculation method.

`src/Gameplay/SaveStateFactory.cs:202–234` selects an archetype and visual inputs, then generates apparent age; **L289** stores it. The same creation path is used by initial generated recruits, rerolls and preference-based creation (**L126–199**). No age/asset-approval exclusion was found in that inspected path. An adult-valued random result would not validate the selected visual design anyway.

## Missing validation boundaries observed

All paths below are relative to `OpenMakaiRanchGame/`. Findings describe inspected code, not an assertion that every conceivable external extension was examined.

| Boundary | Evidence | Observed gap |
|---|---|---|
| Definition import / fallback seed | `src/Core/Resources/GameDefinitions.cs:50–79`; `src/Data/DataRegistry.cs:29–109,121–132,1361–1399` | CharacterDefinition has no age/provenance/design-approval field. Registry validation checks items, missions and enemies, not character adulthood. JSON and seed paths cannot establish source adulthood. |
| State defaults | `src/Core/Models/SaveModels.cs:138,242` | Player ApparentAge defaults to 20; character ApparentAge defaults to 18. Defaults are not confirmed ages or review evidence. |
| Generator / reroll / preferences | `src/Gameplay/CharacterGenerationPools.cs:379–435`; `src/Gameplay/SaveStateFactory.cs:126–234,289` | Minor apparent-age values can enter generated state; no inspected eligibility guard. |
| Player creation / mutation | `src/Ui/UiShellController.Screens.cs:2617–2623`; `src/App/GameRoot.cs:277–281` | Picker exposes the whole age pool. ModifyPlayer invokes the mutation directly without an age/design eligibility check. Players are not exempt. |
| Save import / migration | `src/Gameplay/SaveService.cs:116–132`; `src/Gameplay/SaveMigrator.cs:85–165` | Deserialization, migration and schema checks do not validate age/provenance/design status. Legacy or missing metadata must not acquire approval through default values. |
| Mature action entry | `src/Gameplay/MatureServices.cs:382–424` | Existing action preconditions do not check adult eligibility. Bond, consent, energy and inventory checks cannot substitute for age/design approval. |
| Visual presentation | `src/Gameplay/PortraitRenderer.cs:137–156` | Wrapper selects layered or fallback portraits without age/design review status. Asset existence, successful loading and portrait dimensions do not certify adulthood. |

The JSON records all `ApparentAge` references found in the C# source tree. Identifier search was supplementary to reading these boundaries; an absent identifier alone is not proof of universal absence of safety checks. No runtime guards were added by this documentation-only audit.

## Required author decisions and acceptance criteria

These are **requirements to implement and independently verify**, not completed work:

1. Keep confirmed chronological age, apparent-age metadata, provenance, contextual review and asset approval separate. Unknown is a real state, not an implicit 18. Require confirmed adult identity **and** unambiguous adult design/context before adult presentation.
2. Explicitly deny minors and minor-coded/ambiguous designs, including Surei/Slay and Maria as sourced. Ayaka's school-age marker requires resolution before any adult clearance. Do not “fix” eligibility by changing a number, name, race or label. This audit provides no redesign or art instructions.
3. Fail closed at definition import and fallback, generation/archetype selection, rerolls/preferences, player mutation, save loading/migration, mature action dispatch and every relevant visual path. Changing an asset must invalidate its prior approval; a generic content-mode switch is not a per-character review.
4. Keep non-adult catalog/audit use separate from adult presentation. Templates, player definitions and generated characters need the same eligibility policy. Decide safe handling for unknown, conflicting, custom and legacy records without automatically certifying them.
5. Add negative tests for ages 12/14/16, unknown/missing/invalid ages, conflicting source metadata, school-age context, unreviewed/replaced assets and legacy saves. Verify both service-level denial and presentation behavior, not merely UI labels. Positive cases need documented approval evidence, not only `age >= 18`.
6. Obtain independent non-explicit design/context review for any candidate before adult use. None has been performed here. Do not use this report as visual certification or release approval.

## Verify the evidence snapshot

Run the reviewed repository verifier with Python 3.10+. Do not execute `extraction_python` from the JSON report; that string is historical provenance, not a trusted command. The verifier uses the standard library and never modifies the original reference, snapshot or game data.

Default mode checks the original CSV inventory, source hashes, listed field citations and declared input hashes. Invalid evidence exits 1, including under `python -O`. Repository-relative paths, positive 1-based citation lines, source ID/file correspondence and duplicate paths are checked. Missing or malformed evidence cannot produce a pass marker.

```bash
python Tools/Godot/verify_character_audit.py
# Optional source-only diagnostic; explicitly does NOT verify the code snapshot:
python Tools/Godot/verify_character_audit.py --sources-only
python -m unittest discover -s Tools/Godot -p 'test_character_audit.py' -v
```

`--root` supports relocated repositories; `--report` selects a report relative to that root or an explicit absolute path. `--sources-only` prints a distinct `AUDIT_SOURCE_EVIDENCE_PASS` and `Code/data snapshot NOT CHECKED`; it must not be reported as full audit success.

At the evidence-tool checkpoint, **11 sources and 136 listed field citations still match**, but the full snapshot is **stale** after SAVE-001/CORE-002 changed `SaveMigrator.cs`, `GameRoot.cs` and `UiShellController.Screens.cs`. No hashes were refreshed. Re-audit the changed boundaries before updating their evidence. The verifier checks declared citations/hashes, not completeness of metadata extraction, uncited prose, derived summaries or human findings. It grants no age/design approval and adds no runtime protection; DATA-002 remains open.

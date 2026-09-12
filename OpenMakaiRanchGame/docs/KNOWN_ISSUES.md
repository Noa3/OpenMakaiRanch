# Known issues and open acceptance

Updated 2026-09-12 after documentation/source review of `astar` upload `1a65868b73e1af48277d8ac9ac338e3edcc5ce34`. No new runtime validation was executed for this documentation update. [The prior detailed issue snapshot](archive/20260912-before-living-world/KNOWN_ISSUES.md) remains unchanged as evidence for its own code checkpoint. Current design is [LIVING_WORLD_PLAN.md](LIVING_WORLD_PLAN.md); task status is [KANBAN.md](KANBAN.md).

## Upload-specific world blockers

### WORLD-HOME-001 — authored house and live layout disagree

[Asset recovery](art/RANCH_ASSET_RECOVERY.md) records a 20 x 16 m main house plus 12 x 12 m wing, while `data/world/organic_layout.json` reserves 7.2 x 6.4 m for the current house and separate kitchen/office plots. `RanchHomePrototype.tscn` is an authoring scene not referenced by ordinary ranch play. Placement, actual room functions, upgrades, collisions, tutorial entry and normal save/load remain to integrate (LW-02/LW-03/LW-05). Do not claim the new house is live or scale it down to hide the mismatch.

### WORLD-HOUSING-001 — no complete allocation/capacity or furniture-use contract

[The habitation audit](art/RANCH_HABITATION_FACTS.md) found no designed resident upper bound, bed/room/seat assignment system or household identity deduplication for player versus roster `anon`. UI/seed capacities are not proof of normal-runtime capacity. Seven prototype beds are examples, not a recruitment limit or sufficient unlimited housing. The recorded new furniture has zero imported collision shapes and no wired articulated sit/lie transitions. One measured neutral height is not all-body furniture/door acceptance. Resolve LW-04/LW-06/LW-07 without inventing rent, caps or character identity changes.

### WORLD-COAST-001 — exported regional candidate rejected

The [coastal execution record](execplan/COASTAL_REGION.md) reports 6 geometry tests passing and one failing: deviation 0.1394483787870735 exceeds 0.12 where stream carving cuts the valley route. Region hosting remains opt-in, and rejection metadata is not a runtime activation lock. Real player/follower travel, seams, current saves and Forward+ acceptance are not established. LW-08 requires a real crossing or reroute, matching terrain/collision and walked evidence. Preserve the failed threshold and logs.

### WORLD-GROWTH-001 — accepted direction is not implemented town finance

Visible civic development is requested, but civic identity/ownership, tax distribution, exact mana/energy pool, contribution thresholds and useful project outcomes are not verified/decided by this pass. Conversation references to original tax-funded facilities require actual source/current-code audit (LW-09). Do not silently repurpose contribution points, double-charge existing bills or use personal stamina as civic energy. Stage art can proceed; actual financial activation depends on LW-12 and ordinary-play acceptance on LW-13/LW-20.

### WORLD-LIFE-001 — population and expansion are scope decisions, not completed systems

The target is an eventual town suggesting roughly 300 inhabitants, with bounded background people and persistent important contacts, not a 300-agent mandate. No full city simulation was requested. Space for final growth, permanent green/shore areas, useful public interiors, responsive ambient audio/time/weather and background lifecycle/performance remain to implement. Do not conflate this with the separate need to house actual ranch residents (LW-02/LW-10/LW-14..LW-17).

### EVIDENCE-001 — different checkpoints must stay separate

The preceding handoff reports 1,889 smoke assertions, 526/526 rendered checks, 55 Python tests and 64 images for `4ffd97c`. These are not acceptance of the later `1a65868` asset upload. Asset-recovery Forward+ captures and kit checks have narrower scope and explicitly leave production integration open. Its references to prior UI failures and the rejected coastal candidate cannot be overwritten with a predecessor's green counts. LW-01 establishes the actual combined baseline.

## Retained unrelated issues — not solved by the world plan

| Existing issue | Remaining boundary |
| --- | --- |
| ENDING-001 | Real victory presentation, Continue Ranching and location-preserving return need input/world testing. Service-level early completion is not UI certification; previously excluded blocked writes were not integrated. |
| DESIGN-001 | Four starting projects do not form a complete campaign. Add useful content/choices and compare casual/efficient/specialist policies, not minimum-day gates or padding. The previous seven-day fixture stayed solvent but lost 8 G on ordinary days after meals; no indefinite-profit claim. |
| STATIONS-001 | Local scoped planning, actual quotes and gross output previews have bounded prior coverage. Final net forecasts, meaningful equipment marginal benefits, dense help and complete shop/research/result UI remain open. Current levels do not automatically multiply output. |
| INVENTORY-001 | The recorded unavailable-equipment duplication was repaired for named cases; all Equip/Unequip/consumable/extreme-count paths remain incompletely audited. |
| SETTLEMENT-001 | Prior root stale/reentrant guards and daily ledger fixes are not universal legacy-API guards, full exception rollback, concurrency or all stock/event arithmetic proof. |
| I18N-001 | Previously matching en/de keys and en/de/ja availability are partial coverage; creation/story/results, plural/RTL/font/device/export acceptance remain open. Preserve LocaleCatalog and canonical IDs. |
| UI-PLAY-001 | Stepwise creation, final title art, room/service layout density, resident stories and full physical-device/Alt-Tab/camera/accessibility acceptance remain incomplete. |
| WORLD-NAV-001 | Previous metre-scale shell/selected-door checks do not certify every asset, body, new wing, crowd, camera, slope or complete region. New furniture/terrain evidence must be tested separately. |
| TOOLS-003 | Existing build warnings, successful-import EditorSettings shutdown diagnostic, software VSync warning and five intentional invalid-save smoke diagnostics remain distinct. The previous 240-second UI timeout was a real failed run; later 360-second allowance was not frame-rate evidence. |

The archived snapshot retains exact wording, paths, prior test counts and limitations for all these issues. Preserve separate source/design/eligibility boundaries; ordinary neutral world art is not approval of new character identities or adult-specific content. No new family/pregnancy state, locale, engine version or export is implemented by this plan.

Importer provenance, complete runtime JSON/reference validation, original-engine differential parity and local MCP authentication/size/export hardening remain separate work. Fresh games are the development target while current-version personal saves stay protected. Use scoped cleanup with LW-18 evidence; do not delete fixtures or authoring sources to make a world pass appear complete.

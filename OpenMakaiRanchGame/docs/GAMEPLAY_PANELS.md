# Gameplay panel continuation

2026-09-12. New branch `feature/gameplay-panels-20260912` from main
`e9aac5c01e6244c91cf30d2ca86fba15e73c9708` after the owner merged PR #19.
The latest request explicitly includes UI panels. Existing world art, presence, sky, scenes,
models and shader work are inherited without editing. Original ERA sources remain read-only.

## Implemented routes

Resident overview -> Development and condition -> recent history or separate HP/energy/EP/MP.
The canonical baseline, capacity tracks and twelve-entry journal are rendered read-only;
unknown history is not fabricated and the ward reserve is not action permission. Practice
and care remain separate existing commands. A permanent Back/Close footer and Escape unwind
one local page before closing the root. Back navigation never triggers a different world area.

Kitchen, office and ranch house expose a food/supplies preview. Existing pantry/box allocation
is read from InspectNextLunch; upcoming maintenance demand uses current positive facility
levels. Later care/assignment/purchase changes can alter the forecast. Looking at food never
consumes it. Destination buttons only track physical kitchen/office/shop targets.

Places -> progress/challenges shows existing main-objective counts and five optional records,
with the actual first completion day. This adds no rewards, deadlines or main win conditions.
Workstation equipment shows its current/next existing production benefit and maintenance;
team forecasts clarify that the equipment contribution is already in their total.

Dedicated shop, stock-funded research, bag/ranch inventory and daily report use a shared
card/composition/navigation foundation. Each service retains its local root and existing fixed
Return to World action, not global management. Bag and ranch stock remain separate. Research
has available/unlocked pages and spends the real stock resource, not the unrelated legacy
research-tree gold/cooldown. Reports preserve older/newer browsing and reveal events, growth
and lines on demand. Stored prose retains its recording language.

Completion is presentation only: real GameComplete opens a genuine saved-victory screen;
a subsequent house report request cannot immediately overwrite it. Read Report releases the
full-screen flow lock without taking the old new-game ranch route. Continue Ranching closes
the interface in the same area, including town, and does not pay/settle/reset/start New Game+.
The normal main menu still owns the explicitly selected New Game+ action.

## Command boundaries

PurchaseOffer and ResearchOffer are immutable value quotes, validated against current catalog,
wallet/stock and state generation/day/phase before the existing Shop/Research service commits.
Wide price/stack checks reject overflow; stale or forged quotes charge nothing. One synchronous
command lock prevents a StateChanged observer from reentering purchases/research/time.
New UI handlers also reject hidden, retired, replaced-session and moved-location callbacks.
This is not a universal rewrite of every low-level service, arbitrary caller or transaction.
No new budget, minimum day, price increase or resource authority was introduced.

## Translation

Eighty new panel.* English/German keys plus a meal-box label and existing continuation label
use the existing root locale loader. The 344-key locale/ui slice is unchanged. Full sentences,
validated placeholder sets, wrapping, scrollable content and stable technical IDs are retained.
Old unlocalized catalog descriptions/report prose still need translation. Missing other-language
entries use existing fallback; this does not claim complete translations for ten languages.

## Coverage plan across the UI route atlas

Use `References/VisualTargets/UI_ROUTE_MAP.md` for the owner's presentation families, not as
proof of implemented routes. This pass implements/extends U04 workplace, U05 resident,
U08 house provisions, U09 projects/achievements, U11 shop, U12 storage, U13 research,
U17 report and U21 completion. U10 scheduling remains local workstation planning.
A common local-service navigation boundary also applies to retained service renderers.

U01 title/U02 stepwise creation/U03 HUD/U06 story retain their existing implementation and
need separate layout passes. U07 wardrobe/U14 pharmacy/U15 guild/U16 battle/U18 saves/U19 pause/
U20 options/U22 pets/U23 other skills/U24 shipments/U25 town retain existing routes; they are
not claimed as fully redesigned or transaction-audited here. Existing source-specific content
and its handoff remain untouched. The interrupted pre-merge CombatDevelopment draft was not
a complete committed feature and is not silently included or called tested by this panel pass.

## Verification status at initial commit

Local review-subset catalog tests pass; the existing 344-key UI validator passes. The full
review Python suite previously passed with two explicit original-source skips. No local
Godot/.NET execution is claimed. New numeric quote tests and a rendered connected panel journey
are registered in the existing suites; exact-head CI and screenshot inspection are pending.

Rendered fixture includes physical service entry, actual purchase/research clicks, nested Back,
locale/geometry checks, save/load and a synthetic all-objectives completion followed by actual
house Sleep/report/Continue. Research resources, completion conditions and proximity are staged;
this is not organic campaign progression or world route/long-term balance evidence. Tests use
isolated profiles and refuse an occupied slot99. Prior assertions remain registered. Do not run
raw acceptance flags against personal profiles. C#12/net8.0/engine pins/schema16 stay unchanged.

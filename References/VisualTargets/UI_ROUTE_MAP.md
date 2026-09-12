# UI route coverage

Pinned source: `71fb3a170b6f4baaf1cc8a1a45cdbd1dceab935f`, `UiShellController.cs`. These are presentation proposals, not implemented routes. Re-audit newly merged logic before integration. Preserve technical IDs and command owners.

| Family | Title | Existing routes / presentation tabs |
| --- | --- | --- |
| U01 | Main menu | `title` |
| U02 | Character creation | `character_creation` |
| U03 | World HUD | `ranch` |
| U04 | Workplace | `station.overview`, `station.team`, `station.upgrade` |
| U05 | Resident visit | `roster`, `character_detail`, `visit`, `bond`, `mental`, `resident.care`, `resident.gifts`, `resident.practice`, `resident.company`, `resident.work` |
| U06 | Conversation and story | `prologue` |
| U07 | Wardrobe | `clothing_list`, `clothing_change`, `clothing_strip` |
| U08 | House and evening | `house`, `room_assign` |
| U09 | Places and projects | `places`, `projects`, `milestones` |
| U10 | Daily schedule | `schedule` |
| U11 | General store | `shop` |
| U12 | Inventory | `inventory` |
| U13 | Workshop research | `research` |
| U14 | Laboratory recipes | `pharmacy_list`, `pharmacy_craft` |
| U15 | Adventure guild | `adventure` |
| U16 | Combat | `combat` |
| U17 | Daily report | `report` |
| U18 | Save and load | `saveload` |
| U19 | Pause | `pause` |
| U20 | Options | `options`, `settings` |
| U21 | Milestone completion | `victory` |
| U22 | Pet care | `pets` |
| U23 | Skills and practice | `ability`, `training`, `magic_basic`, `magic_forbidden`, `magic_tentacle` |
| U24 | Shipments | `milk` |
| U25 | Town guide | `town` |

All 35 captured legacy routes occur exactly once. Additional non-legacy IDs in this table describe presentation entry points only; they do not authorize new commands. `room_assign` remains within the house family; `settings` and `options` share one destination. Neutral technical route coverage for adult-game systems is not an explicit-content image brief.

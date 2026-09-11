# Ten localization targets (Russian excluded)

Planning snapshot 2026-09-11. Russian is excluded by the owner's request. This is a plan, not a declaration that ten translations already exist.

The official Steam Hardware & Software Survey page retrieved on this date presented **July 2026** (not August). It is an optional anonymous sample and a proxy for Steam client language, NOT all gamers, game purchases or this adult-oriented title's audience. Use the displayed month honestly; revisit priorities using actual wishlists, feedback and language requests before commissioning full translations.

| Priority by retrieved client share | Locale target | Share | State |
| --- | --- | --- | --- |
| 1 | English (`en`) | 39.61% | baseline, coverage audit still required |
| 2 | Simplified Chinese (`zh_Hans`) | 22.52% | planned |
| 3 | Spanish, Spain (`es_ES`) | 4.91% | planned |
| 4 | Portuguese, Brazil (`pt_BR`) | 4.38% | planned |
| 5 | German (`de`) | 2.82% | new world/project/evening slice translated; whole game incomplete |
| 6 | Japanese (`ja`) | 2.43% | existing partial catalog; new slices use fallback |
| 7 | French (`fr`) | 2.33% | planned |
| 8 | Polish (`pl`) | 1.74% | planned |
| 9 | Korean (`ko`) | 1.45% | planned |
| 10 | Traditional Chinese (`zh_Hant`) | 1.33% | planned |

This counts **ten localized editions**, not ten distinct spoken languages: Simplified/Traditional Chinese are separate localization targets. If the owner intends ten distinct languages, Turkish is the next candidate from this snapshot (1.24%), with both Chinese editions still worth considering. Spanish-Latin America is a distinct later locale (0.89%), not silently equivalent to Spain Spanish.

Current selectable locales stay unchanged until real reviewed catalogs exist. Do not add seven English-only files and label them completed translations. Native language labels must remain recognizable. Future locale normalization must preserve script/region (especially zh_Hans/zh_Hant and pt_BR), unlike the current three-language normalizer; add alias, fallback and migration-of-setting tests when enabling them.

Full-sentence keys with numbered placeholders allow different word order. Validate key/placeholder parity in CI. Never localize save IDs, job IDs, flag IDs or numeric serialization. Plurals and grammatical gender need an explicit message-format contract before promising all-ten support. Allow expanded labels, wrapping action buttons and scrolling; test CJK fonts and word breaks. New text in this continuation has matching English/German source keys. Existing untranslated source messages still use the established fallback and need a later coverage pass.

Primary sources, retrieved 2026-09-11:
- Valve survey: https://store.steampowered.com/hwsurvey/Steam-Hardware-Software-Survey-Welcome-to-Steam?l=english
- Valve locale variants/codes: https://partner.steamgames.com/doc/store/localization/languages
- Godot internationalization guidance: https://docs.godotengine.org/en/stable/tutorials/i18n/internationalizing_games.html

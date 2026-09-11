# Pastoral ranch revision: pending rendered verification

Work continues on `feature/makai-presence-and-worldstyle-20260911` / PR #18, based on prior branch head `152df1af02f11928298e3646068348faf387ec98`. No other agent's branch or main is modified. The user's green starter-area direction in [MAKAI_WORLD_STYLE_AND_LORE.md](MAKAI_WORLD_STYLE_AND_LORE.md) supersedes the earlier alien/volcanic ranch suggestion.

## Implementation under test

New original sky and mana-stone lantern components; a camera-local integration in the existing WorldGame scene; and an isolated pastoral material/environment specimen. The original character/presence lab remains unchanged. The existing launcher adds --pastoral with a separate required capture set and disposable run/profile directories. The existing workflow runs both studies rather than replacing prior acceptance.

Required pastoral captures per renderer: pastoral-day, pastoral-night, pastoral-moon-only, pastoral-overcast, pastoral-low, pastoral-lantern-on and pastoral-lantern-off. Tests check actual changed celestial/lamp pixels, daytime and overcast suppression, sky ownership/restore, finite inputs, bounded point lights and paused/reduced-motion river sampling. Four connected-world checks exercise mounting, camera-local ownership, disable and re-enable. These tests do not establish complete terrain navigation or production art quality.

C# compilation and rendered acceptance are pending the first workflow run for this implementation. No local Godot or .NET runtime is available. Final receipts will record the exact tested commit, retained warnings, downloaded artifact hashes and inspected screenshots. Prior e527a3a results remain historical evidence for the original presence work, not a pass for this revision.

## Limits

Normal grass and trees already present in the ranch are not replaced. The river and hills in the viewing specimen are placeholders, not new production ranch terrain or water physics. Celestial placement/cloud shape is static, not astronomy; no moon phases or time system is added. No new NPC logic, persistent settings, fuel/mana economy, character rig or original-game source changes are made. Software-rendered correctness does not certify player-PC frame rate, AAA quality or every camera/lighting/weather combination.

Reproduce with the installed engine and existing isolated launcher:

```bash
python -m unittest discover -s Tools/Godot -p 'test_*lookdev.py' -v
python Tools/Godot/anime_lookdev.py --pastoral --renderer forward_plus
python Tools/Godot/anime_lookdev.py --pastoral --renderer gl_compatibility
python Tools/Godot/anime_lookdev.py --pastoral --renderer forward_plus --interactive --timeout 3600
```

# Translation and UI authoring guide

## Scope

The game uses `OpenMakaiRanch.Locale.LocaleCatalog` as its existing shared display catalog. This continuation adds 200 English/German entries for the title screen, physical station panels, destinations, direction hints and ranch warnings. It is **not** a complete translation of story, character creation, service menus, command result messages or original content. The existing Japanese catalog is retained; untranslated new entries fall back to English. No additional language selection is a claim of complete translation coverage.

Files are UTF-8 JSON objects. Existing entries remain in `locale/en.json` and `locale/ja.json`; the new focused UI slice is `locale/ui/en.json` and `locale/ui/de.json`. Runtime merges each language's UI slice into the same catalog. Do not create a second localization manager. Blank or missing translated values retain readable English. Runtime loading is limited to 512 KiB per catalog and JSON depth 8.

## Translator contract

Keep every stable key unchanged. Translate its value, not station IDs, save fields, job IDs, input action names, resource IDs or character names supplied by the player. Preserve every numbered placeholder used by the English template, but reorder placeholders freely to suit the language. For example:

```json
{
  "world.work.assign": "Assign {0} here"
}
```

can have a corresponding German value such as `"{0} hier einteilen"`. `{0}` is the resident's display name, not a literal word to translate. Translate whole sentences rather than concatenating grammatical fragments in code. Escape literal braces as `{{` and `}}`. Keep numeric format specifications such as `{0:0.0}` or `{1:+0;-0;0}` intact unless the display requirements explicitly change. Do not use Python-style `{name}` or `{0!r}` fields.

Use native names in the language chooser: English, Deutsch, 日本語. Locale identifiers are normalized (`de-DE` and `de_DE` select `de`); an unsupported locale falls back to English. Display formatting uses the selected locale without changing process-wide culture, JSON/save parsing or gameplay arithmetic. Malformed, dropped, extra or excessively aligned format arguments fall back to English rather than interrupting the UI callback or silently hiding a price. Build validation additionally rejects mismatched new-catalog keys.

A single string formatter is not a grammatical pluralization system. For new count-dependent sentences, introduce explicit singular/plural keys or a reviewed locale-aware plural rule; do not append an English `s`. Right-to-left layout, additional scripts/font coverage and complete voice/subtitle localization are still separate acceptance tasks.

## UI authoring contract

Use `T("stable.key", "English fallback", arguments...)` with complete templates. Do not pass pre-interpolated English as the key or derive command IDs from translated labels. World names, jobs and resource display names have helpers keyed by canonical IDs. Names entered by the player stay untouched.

Prefer horizontal expansion, sensible wrapping and a vertical ScrollContainer. Keep Back and the primary action reachable outside long scrolling content where possible. Do not shrink fonts until a translation fits. Status lines may be bounded, but preserve full details in a tooltip or an accessible detail view. Keep explanatory text apart from the actual disabled-button reason.

Keyboard/controller focus must refer to stable control names, not translated text. Refresh an open panel after a language change without running any gameplay command, changing the selected destination ID, replacing the session, teleporting the player, advancing time or modifying resources. Remove old callbacks with the retired view and revalidate session/day/phase/proximity before mutations. Language changes persist only their language setting: do not reapply window mode, resolution, audio or input preferences.

## Validation

From the repository root:

```bash
python Tools/Godot/validate_locales.py
python -m unittest discover -s Tools/Godot -p "test_*.py"
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python Tools/Godot/ui_acceptance.py --rendered
```

Catalog validation checks duplicate keys, empty/null/non-text entries, file size, placeholder syntax and set parity, new literal world-key coverage, and the export filters that include raw data/catalog JSON. It never writes a save or changes the game. All five export presets include `data/*.json,locale/*.json,locale/ui/*.json`; a real exported-build playthrough remains a separate check.

The rendered acceptance extension sends mouse and keyboard events through `Input.ParseInputEvent`, with window IDs and balanced pressed/released events, to open the real picker and navigate its actual popup. It must not invoke ItemSelected or SetLocale directly for the choice. Viewport-only injection does not maintain the global mouse state used by the opening click, and emitting the WindowInput signal is not equivalent to calling the window's native input handler. Separate open-panel refresh and cleanup checks do call SetLocale explicitly.

The scenario checks German title/station/guide layouts at small landscape and portrait sizes, focus restoration and session/resource snapshots. This is engine-dispatched synthetic input, not physical-device or desktop automation certification. Use the isolated launcher only: acceptance fixtures use disposable save slots. A test's existence is not proof it passed; consult exact-head CI results and the latest validation checkpoint for executed evidence and limits.

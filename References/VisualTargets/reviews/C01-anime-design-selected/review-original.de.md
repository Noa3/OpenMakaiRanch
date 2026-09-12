# C01 – überarbeiteter Charaktervorschlag, nicht freigegeben

## Ergebnis
Echte lokal generierte PNG: `E:/OpenMakaiRanch/.dream-loop/style-revision/character/C01-anime-revised-proposal.png` (1536 × 864).
Genau zwei Generationsversuche; danach beendet. Versuch 1 wegen hochgeschobener Ärmel als Zielvorschlag verworfen. Versuch 2 ist der zur Prüfung vorgelegte Vorschlag, keine Freigabe.

Zwei erwachsen wirkende eigenständige Berufspersonen: Ranch-Arbeiterin mit kupferfarbenem Zopf, cremefarbenem Langarmhemd, olivfarbener Weste und Arbeitshose; Forscherin mit gebundenem aschgrauem Haar, marineblauem Mantel und langer Hose. Beide nichtsexualisiert, mit sichtbaren Händen und vollständig sichtbaren Arbeitsstiefeln, neutraler Studiokulisse. Die Ärmelkorrektur bis zum Handgelenk gelang im zweiten Versuch. Keine körperliche Entblößung oder sexualisierte Körperveränderung erkennbar.

## Stilprüfung / Grenzen
- Gegenüber dem Ausgangs-C01 deutlich vereinfachte, dunkel konturierte Anime-Augen und cel-shadingartige Flächenschatten; keine fotorealistische Hautstruktur.
- Haare mit großen geformten Strähnen und sichtbaren Glanzbändern, aber die Bänder bleiben weich statt so scharf und grafisch wie im Kopf-Stilreferenzbild.
- Insgesamt eher saubere 2D-/2.5D-Charakterillustration als überzeugend gerendertes 3D-Spielmodell. Der Referenzstil ist nur teilweise getroffen; PNG-Erfolg bedeutet keine Stilübereinstimmung.
- Ranch-Weste körpernäher als angefordert; Hemdkragen bleiben leicht geöffnet, obwohl der Korrekturprompt vollständig geschlossene Kragen verlangte. Gewöhnliche nichtsexualisierte Arbeitskleidung bleibt erhalten, aber diese Promptdetails sind nicht exakt umgesetzt.
- Hände sichtbar, Finger zeichnerisch vereinfacht und nicht als rig-/produktionsreif verifiziert. Keine geprüfte 3D-Geometrie, kein Rig, keine Spielintegration.

## Provenienz
Nur lokales ComfyUI unter `http://127.0.0.1:7860`; keine externen Generierungsdienste, Installationen oder Zahlungen.
Modell: `qwen_image_edit_2511_fp8mixed.safetensors`.
LoRA: `Qwen-Image-Edit-2511-Lightning-4steps-V1.0-bf16.safetensors`.
Textencoder: `qwen_2.5_vl_7b_fp8_scaled.safetensors`; VAE: `qwen_image_vae.safetensors`.
Euler / simple, 4 Schritte, CFG 1.0, Denoise 1.0; Seeds 26091241 und 26091242.

Versuch 1: vollständig bekleidetes `E:/OpenMakaiRanch/.dream-loop/generation-C01-c7c8157aa937/C01-lineup.png` als Ausgangsbild, ausschließlich `E:/OpenMakaiRanch/.dream-loop/style-revision/anime-face-style-only.png` als Kopf-/Haar-Stilreferenz.
Versuch 2: sichere Ausgabe von Versuch 1 als Ausgangsbild und dasselbe Kopf-Stilreferenzbild. Keine vollständigen erotischen Referenzaufnahmen verwendet.

Evidence: `E:/OpenMakaiRanch/.dream-loop/style-revision/character/evidence/` enthält je Versuch PNG, tatsächlichen API-Workflow, vollständigen Prompt, Submission, History, separat zurückgelesene History, Systemdaten, Quellenhashes und Receipt; zusätzlich Live-Node-/Modellinformationen und `index.json` mit beiden Generation-Verzeichnissen und Provenienzkette.
Beide Prompt-IDs erneut direkt gelesen: `8cc4ae76-1ad2-4210-853e-8ee47560005f` und `5a23680a-f24d-443e-b703-59191ec596c5`; beide erfolgreich abgeschlossen.
Finaler SHA-256: `2695f79e79c0ed9546f7b0ad63273942ef5ddc50a27b5508b60fe3892fe0fa65`.

Keine Spieldateien, Atlanten oder gemeinsam genutzten Helper verändert. Nur freigegebener Charakter-Arbeitsordner und zwei eindeutige Generation-Verzeichnisse erstellt; ComfyUI speichert außerdem seine regulären Ein-/Ausgabeartefakte.

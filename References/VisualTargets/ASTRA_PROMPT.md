# Arbeitsauftrag für Astra: von Referenzen zur spielbaren Ranch

Arbeite im Repository `Noa3/OpenMakaiRanch`. Das Referenzpaket liegt unter **`References/VisualTargets/`**, außerhalb von `OpenMakaiRanchGame/`. Es enthält derzeit Bildaufträge und Layoutentwürfe, **noch keine KI-generierten oder freigegebenen Zielbilder**. Behandle Platzhalter, historische Spielaufnahmen und SVG-Menüentwürfe nicht als fertige Screenshot-Ziele.

## 1. Arbeitsstand und Zuständigkeit

Prüfe zuerst aktuelle Branches, Arbeitsverzeichnis, offene PRs und die Projektanweisungen. Der Referenzbranch heißt `feature/visual-target-atlas-20260912` und basiert auf `feature/makai-presence-and-worldstyle-20260911` bei `71fb3a1`. Die Grafikbasis enthält den grünen Ranch-Himmel und Mana-Stein-Laternen; diese Basis nicht doppelt importieren. Andere Agenten arbeiten an Spiellogik. Arbeite in einem eigenen Branch und Worktree, ohne deren Dateien zurückzusetzen, Änderungen zu überschreiben, Force-Push oder automatische Zusammenführung.

Lies `AGENTS.md` und die aktuellen Übergabe-, Architektur-, Fehler- und Stilunterlagen in `OpenMakaiRanchGame/docs/`. Alte Handoff-Branchangaben nicht blind übernehmen. Behalte die vorhandenen Godot/.NET/C#-Versionen, den Spielstandvertrag, die Original-IDs und die bestehende Build-Pipeline bei. `eraMakaiRanch-game-eng-translation/` bleibt unverändert.

## 2. Verbindliche Gestaltung

Eine attraktive, gewöhnlich grüne Ranch als Startgebiet: Gras, normale Bäume, ein natürlich fließender Fluss, blauer Tageshimmel, Sonne und übliche Wettereffekte. Ruhige Fantasy-Details sind erwünscht: leuchtende Mana-Steine in warmen Laternen, dezente Mana-Erscheinungen, normaler Mond und optional eine kleine fiktive erdähnliche Begleitwelt nachts. Keine Vulkan-Grundlandschaft, schwebenden Inseln, umgekehrten Wasserfälle, Alien-Wurzeln, dominanten Dämonendächer oder dauerhaft violetten Himmel.

Charaktere: hochwertige weiche Anime-3D-Gestaltung, saubere Gesichter, ausdrucksstarke Augen, geformte Haarsträhnen, kontrollierte Reflexionen und glaubwürdige Kleidungsmaterialien. Eigenständige, eindeutig erwachsene und für diese Referenzen vollständig bekleidete Figuren. Referenz-IDs REF_A bis REF_D sind Vorschläge, keine Freigabe einer Produktionsidentität. Körperform/Spezies, Persönlichkeit und aktuelle Stimmung nicht gleichsetzen. Der Spieler behält direkte Kontrolle.

## 3. Bildziele tatsächlich erzeugen, nicht behaupten

Lies `README.md`, `style-lock.json`, `manifest.json`, `RESEARCH.md` und `UI_ROUTE_MAP.md`. Führe die Registry-Prüfungen aus. Prüfe, ob ein echtes Bildwerkzeug verfügbar und autorisiert ist. Ohne dieses keine angeblich erzeugten Bilder, keine Platzhalter unter finalen Dateinamen und keine behaupteten Renderergebnisse. Melde den konkreten Zugang als Blocker und arbeite nur an unabhängigen Aufgaben. Kostenpflichtige APIs, externe Uploads und Image-to-3D-Aufträge benötigen einen ausdrücklich genehmigten Umfang; keine Schlüssel im Repository, keine heimlichen Kosten.

Beginne mit W01 und einem charakterbezogenen Ziel C01, nicht mit 49 unverbundenen Bildern. Exportiere die konkreten Bildaufträge mit `python Tools/VisualTargets/atlas.py prompt W01` beziehungsweise C01. Nimm passende echte Aufnahmen des aktuellen Spiels als Eingang. Die historischen Baselines sind nur Ausgangsmaterial mit eigenen Commit-IDs. Für einen fehlenden Blickwinkel erst eine aktuelle Aufnahme anfertigen.

Nach Sichtprüfung die echten PNGs als Kandidaten importieren und dem Nutzer zeigen. Nur nach einer tatsächlichen Freigabe `approve` mit dem echten Review-Verweis verwenden. Erzeuge dann W02/W03 durch referenzgestützte Bearbeitung desselben Ortes, nicht durch unabhängige Neugenerierung. Fixiere Kamera, Grundstücke, Türen, zentrale Bäume und Flussverlauf. Ausbauphasen beschreiben Anlagenzustände, keine erfundenen Tagesgrenzen, Preise oder neuen Freischaltungen.

Erzeuge passende Gebäudeblätter B01–B04 und Innenräume I01–I05 mit übereinstimmenden Außenmaßen, Türen und Fenstern. Bestehende acht Bauplätze und deren Reservierungen sind verbindlich. Neue Anbauten erfordern eine separat abgestimmte Änderung. C02/C03 müssen die freigegebenen Figuren wiederverwenden. A01–A04 zeigen nachvollziehbare Interaktionen und Mikrogesten, ohne damit neue Gameplay-Autorisierung oder Beziehungsfolgen zu erfinden.

Speichere Bilder, Prompts, tatsächliche Provider-/Modellangaben, Varianten, Eingabereferenz-Hashes und Review-Status im Referenzpaket. Ziele bleiben gesperrt; kein ständiges Umschreiben des Zielbilds, damit die vorhandene Implementierung besser abschneidet. Verwende ignorierte `.dream-loop/`-Arbeitsdateien für Zwischenstände, nicht für die einzige Kopie eines freigegebenen Ziels.

## 4. Umsetzen und visuell vergleichen

Bearbeite zuerst einen zusammenhängenden kleinen Ranchbereich mit einem guten Charakter, einer begehbaren Tür, einem Innenraum und einer Interaktion. Beschaffe oder baue passende Assets mit dokumentierter Herkunft. Prüfe importierte Modelle auf Maße, Topologie, UVs, Materialzuordnung, Rig, LODs und Kollisionen. Ein Bild-zu-3D-Ergebnis ist noch kein fertiger animierbarer Charakter.

Nutze den Dream-Loop-Grundgedanken: implementieren, bauen/testen, echte Aufnahme, unabhängige Kritik, korrigieren. Das vorliegende Verfahren ist projektspezifisch angepasst. Ein Bild allein ist weder Bauplan noch Animationstest. Der Kritiker erhält Ziel, aktuelle Aufnahme, vorherige Aufnahme/Beurteilung und exakt denselben Kontext: Szene, Kamera/Projektion, Auflösung, Renderer, Qualität, Tagesphase, Wetter, Ausbauzustand, Figuren und Sprache. Die Quell-Commits dürfen sich ändern; der Vergleichskontext nicht. Nutze `check-context` zur Prüfung. Warte bei Godot-Aufnahmen auf einen tatsächlich fertig gerenderten Frame.

Priorisiere räumliche Anordnung und Maßstab, dann Formen, Materialien, Licht und zuletzt kleine Details. Prüfe Gegenschuss und bewegte Kamera. Keine 2D-Bildtafel vor die Spielkamera stellen, um eine 3D-Welt vorzutäuschen. Keine schönen Standbilder auf Kosten von Türen, Wegfindung, Bedienung oder Lesbarkeit. Speichere konkrete Abweichungen und nächste Maßnahmen. Fehlt ein unabhängiger Agent, kennzeichne eine eigene Beurteilung als Selbstprüfung; erfinde keine Fremdbewertung.

## 5. Menüs und Charakterpräsenz

Alle 35 gelesenen Menü-Routen sind auf 25 Oberflächenfamilien abgebildet. Prüfe neue Routen auf dem tatsächlichen Arbeitsstand nach. Konsolidiere die Darstellung ohne Funktionen zu verlieren: 2D-Hauptmenü, gemischte Charaktererstellung, sparsames Welt-HUD, lokale Stationsoberflächen und eine gemeinsame Optionsseite statt Settings/Options-Duplikat. Globale Übersichten dürfen navigieren; lokale Aktionen bleiben lokal. Übersetzbare Texte, echte Zahlen, Fokus, Tooltips, Bestätigungen und Fehlermeldungen nicht in Bildern festbacken.

Jede wichtige Oberfläche benötigt Normal-, Fokus-, Nicht-verfügbar-, Leer-, Fehler- und Bestätigungszustände, soweit anwendbar. Teste Tastatur und Controller, längere Übersetzungen, schmale Fenster und Textskalierung bis 200%. Kleine Atlasvorschauen sind nur ein Inhaltsverzeichnis und kein Lesbarkeitstest.

Mimik/Gestik mit echten kurzen Bewegungssequenzen prüfen: neutrales Zuhören, Blickwechsel, Blinzeln, Nicken, Unterbrechung, Müdigkeit, höfliche Ablehnung und leises Lächeln. Hände müssen sinnvoll greifen oder ruhen; keine zufällige Dauerbewegung. Erhalte AnimationTree-/Rig-Zuständigkeit und das vorhandene Freigabeverhalten der Presence-Anbindung. Bedürfnisse, Tagesablauf, freiwillige Begleitung und Trait-/Beziehungsänderungen bleiben bei den bestehenden Logikdiensten. Keine zweite Uhr, Wirtschaft oder autonome Spielersteuerung.

## 6. Abnahme und Abschluss

Eine visuelle Punktzahl ist weder AAA-Zertifikat noch Leistungsnachweis. Erfolg benötigt drei getrennte Belege: überzeugende geprüfte Darstellung, intakte Spielfunktionen und gemessene Leistung auf benannter Zielhardware. Bestehende isolierte Tests ausführen; keinen persönlichen Spielstand überschreiben. Für Performance Auflösung, Renderer, Qualität, GPU/CPU und Szenen-/Figurenumfang nennen. llvmpipe-CI nicht als Spieler-GPU-Benchmark ausgeben.

Nach einem sinnvollen Abschnitt Commits und einen überprüfbaren PR abliefern: tatsächlich geänderte Dateien, echte Aufnahmen, Tests mit Ergebnissen, verbleibende Abweichungen und Zuständigkeitskonflikte. Keine automatische Zusammenführung. Ohne passende Assets oder Bildzugang den konkreten offenen Punkt benennen, statt diesen als fertig zu markieren. Das langfristige Qualitätsziel nicht durch immer mehr primitive Platzhalter oder stärkeren Bloom ersetzen.

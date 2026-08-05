# Godot Universal MCP — Editor Plugin

Lightweight MCP bridge for AI-assisted Godot development.

## Architecture

```
┌─────────────┐     TCP/JSON      ┌──────────────┐
│  Godot 4.x  │ ◄──────────────► │ MCP Server   │
│  Editor     │   (port 9500)    │ (Node.js)    │
└─────────────┘                  └──────────────┘
     │
     │ TCP/JSON (port 9501)
     ▼
┌─────────────┐
│  Godot Run  │ ◄──────────────► │ MCP Server   │
│  / Debug    │   (runtime)      │ (Node.js)    │
└─────────────┘                  └──────────────┘
```

## Installation

### 1. Plugin aktivieren
1. **Project > Project Settings > Plugins**
2. Klicke **Reload** wenn nötig
3. Aktiviere **Godot Universal MCP**
4. Editor zeigt jetzt Dock-Pane links an

### 2. MCP Server installieren

```bash
# Node.js 18+ erforderlich
npm install -g @coding-solo/godot-mcp
```

### 3. Hermes konfigurieren

```bash
hermes mcp add godotMCP --command npx --args @coding-solo/godot-mcp
```

Oder manuell in `~/.hermes/config.yaml`:

```yaml
mcp_servers:
  godotMCP:
    enabled: true
    command: npx
    args: ["@coding-solo/godot-mcp"]
    tools: { all: true }
```

### 4. OpenMakaiRanch spezifisch

Für dieses Projekt: `opencode.json` prüfen:

```json
{
  "mcpServers": {
    "godot-universal": {
      "command": "npx",
      "args": ["@coding-solo/godot-mcp"]
    }
  }
}
```

## Ports

| Port | Beschreibung |
|------|-------------|
| 9500 | Editor Bridge (Scene Tree, Nodes, Properties) |
| 9501 | Runtime Bridge (Game State, Debug) |

## Verfügbare Tools

### Editor Bridge (port 9500)
- `editor.get_status` — Editor Status, open scenes
- `editor.get_scene_tree` — Scene Tree export
- `editor.get_node` — Node Details
- `editor.set_node_property` — Property schreiben
- `editor.get_output` — Debug Output
- `editor.save_all` — Alle Scenes speichern
- `editor.open_scene` — Scene öffnen
- `editor.filesystem_scan` — Resource FS scan
- `editor.run_project` — Projekt starten
- `editor.stop_project` — Projekt stoppen

### Runtime Bridge (port 9501, debug only)
- `runtime.get_status` — FPS, frame count, paused
- `runtime.get_tree` — Runtime Scene Tree
- `runtime.get_node` — Runtime Node Details
- `runtime.get_property` / `set_property` — Properties
- `runtime.get_logs` — Log capture
- `runtime.get_perf` — Performance stats
- `runtime.pause` / `resume` — Game pause
- `runtime.screenshot` — Screenshot (base64 PNG)

## Projekt Settings

| Setting | Default | Beschreibung |
|---------|---------|-------------|
| `godot_universal_mcp/editor_port` | 9500 | Editor Bridge Port |
| `godot_universal_mcp/runtime_port` | 9501 | Runtime Bridge Port |
| `godot_universal_mcp/runtime_enabled` | true | Runtime Bridge aktivieren |
| `godot_universal_mcp/allow_runtime_input` | false | Runtime Input erlauben |
| `godot_universal_mcp/allow_eval` | false | eval erlauben |
| `godot_universal_mcp/allow_remote` | false | Remote connections |
| `godot_universal_mcp/log_level` | info | Log level |

## Troubleshooting

### Plugin lädt nicht
- `Project > Project Settings > Plugins` → Reload
- Prüfe `plugin.cfg` existiert im Plugin-Ordner
- Prüfe `script` Pfad in `plugin.cfg`

### Port bereits belegt
- Ändere `editor_port` oder `runtime_port` in Project Settings
- Oder stoppe andere Dienste die auf 9500/9501 lauschen

### Runtime Bridge funktioniert nicht
- Nur aktiv in **debug/editor builds**
- Prüfe `runtime_enabled` Setting
- Prüfe dass `runtime_bridge.gd` als Autoload geladen ist

## Struktur

```
addons/godot_universal_mcp/
├── plugin.cfg          # Plugin metadata
├── plugin.gd           # EditorPlugin entrypoint
├── editor_bridge.gd    # TCP server + tools (editor)
├── runtime_bridge.gd   # TCP server + tools (runtime)
├── dock.tscn           # Editor dock UI
├── dock.gd             # Dock script
└── README.md           # Diese Datei
```

## Development

1. Plugin-Code in `godot_universal_mcp/` ändern
2. In Godot: **Plugins > Godot Universal MCP > Reload**
3. Oder Editor neustarten

## License

MIT

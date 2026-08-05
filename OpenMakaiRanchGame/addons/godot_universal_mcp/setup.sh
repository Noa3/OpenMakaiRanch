#!/usr/bin/env bash
# Godot Universal MCP — Setup-Skript
# Installiert den MCP Server und konfiguriert Hermes für OpenMakaiRanch.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
HERMES_CONFIG="$HOME/.hermes/profiles/code/config.yaml"

echo "[setup] Godot Universal MCP"
echo "  Projekt: $PROJECT_ROOT"
echo "  Plugin:  $SCRIPT_DIR"
echo ""

# 1. Node.js prüfen
if ! command -v node &>/dev/null; then
    echo "[setup] ERROR: node.js nicht gefunden. Bitte installieren."
    exit 1
fi
echo "[setup] node.js: $(node --version)"

# 2. npm prüfen
if ! command -v npm &>/dev/null; then
    echo "[setup] ERROR: npm nicht gefunden."
    exit 1
fi
echo "[setup] npm: $(npm --version)"

# 3. MCP Server installieren
echo ""
echo "[setup] Installiere @coding-solo/godot-mcp..."
npm install -g @coding-solo/godot-mcp 2>&1 || {
    echo "[setup] WARN: Global install fehlgeschlagen. Versuche lokal..."
    cd "$PROJECT_ROOT"
    npm install @coding-solo/godot-mcp 2>&1 || true
}
echo "[setup] @coding-solo/godot-mcp installiert."

# 4. Hermes MCP config prüfen
if command -v hermes &>/dev/null && [ -f "$HERMES_CONFIG" ]; then
    echo ""
    echo "[setup] Hermes config: $HERMES_CONFIG"
    if grep -q "godotMCP" "$HERMES_CONFIG" 2>/dev/null; then
        echo "[setup] godotMCP Server bereits konfiguriert."
    else
        echo "[setup] Füge godotMCP zu Hermes hinzu..."
        hermes mcp add godotMCP --command npx --args @coding-solo/godot-mcp 2>&1 || {
            echo "[setup] WARN: hermes mcp add fehlgeschlagen. Manuell konfigurieren:"
            echo "  hermes mcp add godotMCP --command npx --args @coding-solo/godot-mcp"
        }
    fi
else
    echo ""
    echo "[setup] WARN: hermes CLI nicht gefunden oder config nicht existiert."
    echo "  Manuell konfigurieren:"
    echo "  hermes mcp add godotMCP --command npx --args @coding-solo/godot-mcp"
fi

# 5. OpenMakaiRanch opencode.json prüfen
OPENCODE_JSON="$PROJECT_ROOT/opencode.json"
if [ -f "$OPENCODE_JSON" ]; then
    echo ""
    echo "[setup] opencode.json existiert bereits."
    if grep -q "godot-universal" "$OPENCODE_JSON" 2>/dev/null; then
        echo "[setup] godot-universal MCP Server bereits konfiguriert."
    else
        echo "[setup] WARN: Füge godot-universal zu opencode.json hinzu."
        echo "  Manuell:"
        echo "  {\"mcpServers\": {\"godot-universal\": {\"command\": \"npx\", \"args\": [\"@coding-solo/godot-mcp\"]}}}"
    fi
else
    echo ""
    echo "[setup] WARN: opencode.json nicht gefunden."
fi

echo ""
echo "[setup] Done!"
echo ""
echo "Nächste Schritte:"
echo "  1. Godot öffnen → Project Settings → Plugins → Godot Universal MCP aktivieren"
echo "  2. Editor neu starten"
echo "  3. Hermes: hermes mcp list (prüfen ob godotMCP connected)"
echo ""

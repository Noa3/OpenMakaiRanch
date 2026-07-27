# Godot MCP Setup - Automatic project integration
# Run this script to integrate GodotMCP into your project

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Godot MCP Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$PROJECT_ROOT = "E:\OpenMakaiRanch\OpenMakaiRanchGame"
$GODOT_MCP_REPO = "C:\Users\noa3\Documents\GodotMCP"
$HERMES_CONFIG = "C:\Users\noa3\AppData\Local\hermes\config.yaml"

Write-Host "[1/7] Checking prerequisites..." -ForegroundColor Yellow

# Check GodotMCP repo
if (!(Test-Path $GODOT_MCP_REPO)) {
    Write-Host "  [ERROR] GodotMCP repo not found: $GODOT_MCP_REPO" -ForegroundColor Red
    Write-Host "  Fix: git clone https://github.com/Noa3/GodotMCP.git $GODOT_MCP_REPO" -ForegroundColor Red
    exit 1
}
Write-Host "  [OK] GodotMCP repo found" -ForegroundColor Green

# Check project root
if (!(Test-Path $PROJECT_ROOT)) {
    Write-Host "  [ERROR] Project not found: $PROJECT_ROOT" -ForegroundColor Red
    exit 1
}
Write-Host "  [OK] Project found" -ForegroundColor Green

# Check Node.js
try {
    $nodeVersion = node --version
    Write-Host "  [OK] Node.js: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Node.js not installed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "[2/7] Building MCP server..." -ForegroundColor Yellow

Set-Location $GODOT_MCP_REPO
npm install --silent
npm run build
if ($LASTEXITCODE -ne 0) {
    Write-Host "  [ERROR] Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "  [OK] MCP server built" -ForegroundColor Green

Write-Host ""
Write-Host "[3/7] Copying addon to project..." -ForegroundColor Yellow

$ADDON_SRC = Join-Path $GODOT_MCP_REPO "addons\godot_universal_mcp"
$ADDON_DST = Join-Path $PROJECT_ROOT "addons\godot_universal_mcp"

# Remove old GodotMCP placeholder if exists
$oldPlugin = Join-Path $PROJECT_ROOT "addons\GodotMCP"
if (Test-Path $oldPlugin) {
    Remove-Item $oldPlugin -Recurse -Force
    Write-Host "  [INFO] Old GodotMCP placeholder removed" -ForegroundColor Gray
}

# Copy addon
if (Test-Path $ADDON_DST) {
    Remove-Item $ADDON_DST -Recurse -Force
}
Copy-Item $ADDON_SRC $ADDON_DST -Recurse -Force
Write-Host "  [OK] Addon copied to: $ADDON_DST" -ForegroundColor Green

Write-Host ""
Write-Host "[4/7] Fixing plugin.cfg (UTF8-NoBOM)..." -ForegroundColor Yellow

# Write plugin.cfg with correct format - matches working test plugin structure
# IMPORTANT: Use UTF8-NoBOM because PowerShell Set-Content adds BOM which breaks Godot parser
# IMPORTANT: [plugin] section header (not [configuration])
# IMPORTANT: No spaces around =, no plugin/ prefix, no type key
$pluginCfgContent = "[plugin]" + [Environment]::NewLine +
[Environment]::NewLine +
"name = `"Godot Universal MCP`"" + [Environment]::NewLine +
"description = `"Universal MCP bridge for AI-assisted Godot development`"" + [Environment]::NewLine +
"author = `"Godot Universal MCP Contributors`"" + [Environment]::NewLine +
"version = `"0.1.0`"" + [Environment]::NewLine +
"script = `"plugin.gd`"" + [Environment]::NewLine

[System.IO.File]::WriteAllText((Join-Path $ADDON_DST "plugin.cfg"), $pluginCfgContent, (New-Object System.Text.UTF8Encoding $false))
Write-Host "  [OK] plugin.cfg written (UTF8-NoBOM)" -ForegroundColor Green

Write-Host ""
Write-Host "[4b/7] Generating UID files..." -ForegroundColor Yellow

# Godot 4 requires .uid files for all .gd and .tscn files
# Generate content-based UIDs (hash of file content)
$uidFiles = Get-ChildItem $ADDON_DST -Include "*.gd","*.tscn" -Recurse
foreach ($f in $uidFiles) {
    $content = [System.IO.File]::ReadAllBytes($f.FullName)
    $hash = [System.Security.Cryptography.MD5]::Create().ComputeHash($content)
    $uidStr = "uid://" + (-join ($hash[0..7] | ForEach-Object { $_.ToString("x2") }))
    $uidPath = "$($f.FullName).uid"
    [System.IO.File]::WriteAllText($uidPath, $uidStr, (New-Object System.Text.UTF8Encoding $false))
}
Write-Host "  [OK] UID files generated for $($uidFiles.Count) files" -ForegroundColor Green

Write-Host ""
Write-Host "[5/7] Configuring project..." -ForegroundColor Yellow

# Patch project.godot
$projectGodot = Join-Path $PROJECT_ROOT "project.godot"
$projectContent = Get-Content $projectGodot -Raw -Encoding UTF8

# Add autoload if not present
if ($projectContent -notmatch "GodotUniversalMcpRuntime") {
    $autoloadPattern = "\[autoload\]"
    $replacement = @"
[autoload]
  GodotUniversalMcpRuntime="*res://addons/godot_universal_mcp/runtime_bridge.gd"
"@
    $projectContent = $projectContent -replace $autoloadPattern, $replacement
    Write-Host "  [OK] Autoload GodotUniversalMcpRuntime added" -ForegroundColor Green
} else {
    Write-Host "  [INFO] Autoload already present" -ForegroundColor Gray
}

# Add MCP settings section
$mcpSection = @"

[godot_universal_mcp]
editor_port = 9500
runtime_port = 9501
"@

if ($projectContent -notmatch "godot_universal_mcp") {
    $projectContent = $projectContent + $mcpSection
    Write-Host "  [OK] MCP settings added to project.godot" -ForegroundColor Green
} else {
    Write-Host "  [INFO] MCP settings already present" -ForegroundColor Gray
}

# Fix [plugins] section - ensure it has exactly one plugin entry
# Godot 4 requires: [plugins] followed by plugin_name: and enabled = true
# Remove any existing [plugins] section to avoid duplicates
$hasPluginsSection = $projectContent -match "\[plugins\]"
if ($hasPluginsSection) {
    # Remove existing [plugins] section and everything after it up to next section
    $lines = $projectContent -split "`n"
    $newLines = @()
    $inPlugins = $false
    $skipUntilNextSection = $false
    foreach ($line in $lines) {
        if ($line -match "^\[plugins\]") {
            $inPlugins = $true
            $skipUntilNextSection = $true
            continue
        }
        if ($skipUntilNextSection) {
            if ($line -match "^\[") {
                $skipUntilNextSection = $false
                $inPlugins = $false
                $newLines += $line
            }
            # Skip lines inside the old [plugins] section
            continue
        }
        $newLines += $line
    }
    $projectContent = $newLines -join "`n"
}

# Write the correct [plugins] section
$pluginsSection = @"

[plugins]

godot_universal_mcp:
    enabled = true
"@
$projectContent = $projectContent + $pluginsSection

[System.IO.File]::WriteAllText($projectGodot, $projectContent, (New-Object System.Text.UTF8Encoding $false))
Write-Host "  [OK] Plugin entry written to project.godot" -ForegroundColor Green

Write-Host ""
Write-Host "[6/7] Enabling plugin..." -ForegroundColor Yellow

# Create .godot/plugins directory
$pluginsDir = Join-Path $PROJECT_ROOT ".godot\plugins\godot_universal_mcp"
New-Item -ItemType Directory -Path $pluginsDir -Force | Out-Null

# Write enable config
$enableCfgContent = @"
[configuration]
enable = "on"
config = "addons/godot_universal_mcp/plugin.cfg"
version = "0.1.0"
"@

[System.IO.File]::WriteAllText((Join-Path $pluginsDir "plugin.cfg"), $enableCfgContent, (New-Object System.Text.UTF8Encoding $false))
Write-Host "  [OK] Plugin enabled (.godot/plugins)" -ForegroundColor Green

Write-Host ""
Write-Host "[7/7] Creating IDE configs..." -ForegroundColor Yellow

# VS Code mcp.json
$vscodeDir = Join-Path $PROJECT_ROOT ".vscode"
New-Item -ItemType Directory -Path $vscodeDir -Force | Out-Null

$vscodeMcp = @{
    servers = @{
        "godot-universal" = @{
            type = "stdio"
            command = "npx"
            args = @("godot-universal-mcp")
        }
    }
} | ConvertTo-Json -Depth 10

[System.IO.File]::WriteAllText((Join-Path $vscodeDir "mcp.json"), $vscodeMcp, [System.Text.Encoding]::UTF8)
Write-Host "  [OK] VS Code mcp.json created" -ForegroundColor Green

# Open Code config
$opencodeConfig = @{
    mcpServers = @{
        "godot-universal" = @{
            command = "npx"
            args = @("godot-universal-mcp")
        }
    }
} | ConvertTo-Json -Depth 10

[System.IO.File]::WriteAllText("E:\OpenMakaiRanch\opencode.json", $opencodeConfig, [System.Text.Encoding]::UTF8)
Write-Host "  [OK] Open Code config created" -ForegroundColor Green

# Hermes config
if (Test-Path $HERMES_CONFIG) {
    $hermesContent = Get-Content $HERMES_CONFIG -Raw -Encoding UTF8
    if ($hermesContent -notmatch "godotMCP") {
        if ($hermesContent -match "mcp_servers:") {
            $hermesContent = $hermesContent -replace "(mcp_servers:.*)", "`$1`n  godotMCP:`n    type: stdio`n    command: npx`n    args:`n      - godot-universal-mcp"
        } else {
            $hermesContent = $hermesContent + "`nmcp_servers:`n  godotMCP:`n    type: stdio`n    command: npx`n    args:`n      - godot-universal-mcp"
        }
        [System.IO.File]::WriteAllText($HERMES_CONFIG, $hermesContent, (New-Object System.Text.UTF8Encoding $false))
        Write-Host "  [OK] Hermes config updated" -ForegroundColor Green
    } else {
        Write-Host "  [INFO] Hermes config already present" -ForegroundColor Gray
    }
} else {
    Write-Host "  [WARN] Hermes config not found: $HERMES_CONFIG" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Setup complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Open Godot Editor (project: $PROJECT_ROOT)" -ForegroundColor White
Write-Host "  2. Editor -> Project -> Project Settings -> Plugins" -ForegroundColor White
Write-Host "  3. 'Godot Universal MCP' -> toggle ON" -ForegroundColor White
Write-Host "  4. Check output: '[GodotUniversalMCP] Editor bridge listening on 127.0.0.1:9500'" -ForegroundColor White
Write-Host "  5. Run: start-godot-mcp.bat" -ForegroundColor White
Write-Host ""
Write-Host "Ports: Editor=9500, Runtime=9501" -ForegroundColor Gray
Write-Host ""

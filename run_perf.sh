#!/bin/bash
set -euo pipefail
cd /e/OpenMakaiRanch
RUN="C:/Users/noa3/AppData/Local/Temp/omr_perf"
rm -rf "$RUN"
mkdir -p "$RUN/appdata" "$RUN/localappdata"
export APPDATA="$RUN/appdata"
export LOCALAPPDATA="$RUN/localappdata"
export OMR_EXPECTED_USER_ROOT="$RUN"
GODOT="E:\GodotEditor\Godot_v4.7.2-stable_mono_win64.exe"
PROJ="E:\OpenMakaiRanch\OpenMakaiRanchGame"
LOG="$RUN/out.log"
"$GODOT" --path "$PROJ" res://scenes/dev/PerfCapture.tscn > "$LOG" 2>&1
echo "EXIT: $?"
echo "--- PERF_STATS ---"
grep -E "PERF_STATS|PERF_STATS_OK|roster avatars" "$LOG" | head -30
echo "--- errors (non-GLB) ---"
grep -iE "error|exception" "$LOG" | grep -iv "backtrace\|NativeCalls\|CSharpInstance\|generated.cs\|GLB\|fallback\|texture" | head -10

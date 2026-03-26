#!/bin/bash
# sync_to_complete.sh
# 把 Scripts/ 下的最新脚本同步到 CompleteGame/Assets/Scripts/
# 用法: bash sync_to_complete.sh

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
SOURCE="$SCRIPT_DIR/Scripts"
DEST="$SCRIPT_DIR/CompleteGame/Assets/Scripts"

if [ ! -d "$SOURCE" ]; then
    echo "ERROR: $SOURCE not found"
    exit 1
fi

if [ ! -d "$DEST" ]; then
    echo "Creating $DEST"
    mkdir -p "$DEST"
fi

echo "Syncing $SOURCE → $DEST ..."

# Copy all .cs files
cp -v "$SOURCE"/*.cs "$DEST/"

# Count files
COUNT=$(ls "$DEST"/*.cs 2>/dev/null | wc -l)
echo ""
echo "✅ Synced $(ls "$SOURCE"/*.cs 2>/dev/null | wc -l) scripts to CompleteGame/Assets/Scripts/"
echo "Total scripts in destination: $COUNT"
echo ""
echo "Files:"
ls -la "$DEST"/*.cs | awk '{print "  "$NF" ("$5" bytes)"}'

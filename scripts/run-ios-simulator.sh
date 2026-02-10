#!/usr/bin/env bash
set -euo pipefail

PROJECT="$(dirname "$0")/../src/GourmetClient.Maui/GourmetClient.Maui.csproj"
FRAMEWORK="net10.0-ios"

# Find a booted simulator, or fall back to the first available iPhone simulator
DEVICE_ID=$(xcrun simctl list devices booted -j 2>/dev/null \
    | python3 -c "
import json, sys
data = json.load(sys.stdin)
for runtime, devices in data.get('devices', {}).items():
    for d in devices:
        if d.get('state') == 'Booted':
            print(d['udid'])
            sys.exit(0)
" 2>/dev/null || true)

if [ -z "$DEVICE_ID" ]; then
    echo "No booted simulator found. Looking for an available iPhone simulator..."
    DEVICE_ID=$(xcrun simctl list devices available -j 2>/dev/null \
        | python3 -c "
import json, sys
data = json.load(sys.stdin)
for runtime, devices in data.get('devices', {}).items():
    if 'iOS' not in runtime:
        continue
    for d in devices:
        if 'iPhone' in d.get('name', ''):
            print(d['udid'])
            sys.exit(0)
print('')
" 2>/dev/null || true)

    if [ -z "$DEVICE_ID" ]; then
        echo "Error: No iPhone simulator found. Install one via Xcode > Settings > Platforms."
        exit 1
    fi

    echo "Booting simulator $DEVICE_ID..."
    xcrun simctl boot "$DEVICE_ID" 2>/dev/null || true
    open -a Simulator
fi

echo "Running on iOS Simulator (device: $DEVICE_ID)..."
dotnet build "$PROJECT" -t:Run -f "$FRAMEWORK" -p:_DeviceName=:v2:udid="$DEVICE_ID"

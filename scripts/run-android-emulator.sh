#!/usr/bin/env bash
set -euo pipefail

PROJECT="$(dirname "$0")/../src/GourmetClient.Maui/GourmetClient.Maui.csproj"
FRAMEWORK="net10.0-android"

# Check for a running emulator
RUNNING=$(adb devices 2>/dev/null | grep -w "device" | head -1 | awk '{print $1}' || true)

if [ -z "$RUNNING" ]; then
    echo "No running Android emulator found. Attempting to start one..."

    AVD_NAME=$(emulator -list-avds 2>/dev/null | head -1 || true)

    if [ -z "$AVD_NAME" ]; then
        echo "Error: No Android AVDs found. Create one via Android Studio > Device Manager."
        exit 1
    fi

    echo "Starting emulator: $AVD_NAME"
    emulator -avd "$AVD_NAME" -no-snapshot-load &
    EMULATOR_PID=$!

    echo "Waiting for emulator to boot..."
    adb wait-for-device
    # Wait until boot animation finishes
    while [ "$(adb shell getprop sys.boot_completed 2>/dev/null | tr -d '\r')" != "1" ]; do
        sleep 2
    done
    echo "Emulator ready."
fi

echo "Building and deploying to Android emulator..."
dotnet build "$PROJECT" -t:Run -f "$FRAMEWORK"

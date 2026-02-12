#!/bin/sh
set -e

CSPROJ="./limm.csproj"
DLL="./bin/Debug/net472/limm.dll"
DEST="/home/pawbeans/Downloads/Len's Island/BepInEx/scripts/"

current_version=$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$CSPROJ" | head -n1)
[ -n "$current_version" ] || { echo "No <Version> found"; exit 1; }

IFS='.' read -r major minor patch <<EOF
$current_version
EOF
new_version="$major.$minor.$((patch + 1))"

sed -i "0,/<Version>.*<\/Version>/s//<Version>$new_version<\/Version>/" "$CSPROJ"
echo "Version bumped: $current_version -> $new_version"

if dotnet build -c Debug; then
  cp -f "$DLL" "$DEST"
  echo "Copied $DLL to $DEST"
else
  sed -i "0,/<Version>.*<\/Version>/s//<Version>$current_version<\/Version>/" "$CSPROJ"
  echo "Build failed, reverted version to $current_version"
  exit 1
fi

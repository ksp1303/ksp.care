#!/bin/sh
# Rebuilds wwwroot/css/tailwind.css from the classes used in Views/.
# Run this after adding new Tailwind utility classes to any .cshtml file.
set -e
cd "$(dirname "$0")/.."

CLI=/tmp/tailwindcss
if [ ! -x "$CLI" ]; then
  case "$(uname -sm)" in
    "Darwin arm64") ASSET=tailwindcss-macos-arm64 ;;
    "Darwin x86_64") ASSET=tailwindcss-macos-x64 ;;
    "Linux x86_64") ASSET=tailwindcss-linux-x64 ;;
    *) echo "Unsupported platform: $(uname -sm)"; exit 1 ;;
  esac
  curl -sL -o "$CLI" "https://github.com/tailwindlabs/tailwindcss/releases/download/v3.4.17/$ASSET"
  chmod +x "$CLI"
fi

"$CLI" -i Styles/tailwind.input.css -o wwwroot/css/tailwind.css --minify

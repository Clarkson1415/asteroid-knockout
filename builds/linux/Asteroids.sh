#!/bin/sh
printf '\033c\033]0;%s\a' Asteroid blast
base_path="$(dirname "$(realpath "$0")")"
"$base_path/Asteroids.x86_64" "$@"

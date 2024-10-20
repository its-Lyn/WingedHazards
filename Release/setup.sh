#!/usr/bin/env bash

if [ "$1" == "--remove" ]; then
    rm -rfv $HOME/.local/share/WingedHazards
    rm -v $HOME/.local/share/applications/WingedHazards.desktop

    exit 0
fi

mkdir -pv $HOME/.local/share/applications
mkdir -pv $HOME/.local/share/WingedHazards

cp -rv ./* $HOME/.local/share/WingedHazards

cat << EOF > $HOME/.local/share/applications/WingedHazards.desktop
[Desktop Entry]
Type=Application
Name=Winged Hazards
Icon=${HOME}/.local/share/WingedHazards/icon.png
Exec=${HOME}/.local/share/WingedHazards/GBGame
Categories=Games;
Comment=GameBoy-like game.
Terminal=false
EOF
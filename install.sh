#!/usr/bin/env bash

if [ "$1" == "--remove" ]; then
    rm -rfv $HOME/.local/share/WingedHazards
    rm -v $HOME/.local/share/applications/WingedHazards.desktop

    exit 0
fi

read -r -p "Before running, make sure you've ran \"dotnet publish -c Release -p:PublishSingleFile=true\". Continue? [Y/n] " RESPONSE
case $RESPONSE in
    [nN])
        exit 0
    ;;
esac

mkdir -pv $HOME/.local/share/applications
mkdir -pv $HOME/.local/share/WingedHazards

cp -rv GBGame/bin/Release/net8.0/linux-x64/publish/*   $HOME/.local/share/WingedHazards
rm -v $HOME/.local/share/WingedHazards/*.pdb
cp -v Release/icon.png $HOME/.local/share/WingedHazards

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
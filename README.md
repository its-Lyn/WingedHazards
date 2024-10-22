# Winged Hazards

My first original game outside a game engine!

Winged Hazards is a game following the [GameBoy](https://en.wikipedia.org/wiki/Game_Boy) rules! That means the game only has four directional controls, A and B, Start and Select controls, and the game resolution is a whopping 144x160 with a whole four colours!

## Game Description
Your name is River, a young scientist who has been tasked with seeing why the local forest has been suddenly invaded by an invasive bat species. Try to survive as long as you can while fending the bats away, and gaining new Super Powers along the way!

The game's stable builds are now available on [itch.io](https://itsLyn.itch.io/wingedhazards)!

## Key Binds

### Keyboard
| Key   | Description                           |
|-------|---------------------------------------|
| W     | Cycle inventory / Move up in the UI   |
| S     | Cycle inventory / Move down in the UI |
| A     | Move Left / Move left in the UI       |
| D     | Move Right / Move right in the UI     |
| Space | Jump                                  |
| Shift | Action / UI Accept                    |

### Controller
| Button (PS4, XBox, Switch) | Description                           |
|----------------------------|---------------------------------------|
| DPad Up                    | Cycle inventory / Move up in the UI   |
| DPad Down                  | Cycle inventory / Move down in the UI |
| DPad Left                  | Move Left / Move left in the UI       |
| DPad Right                 | Move Right / Move right in the UI     |
| X / A / B                  | Jump                                  |
| Circle / B / A             | Action / UI Accept                    |

## Installation
Instructions on installing the game.

### Using releases page (Recommended)
Click on the [Releases page](https://github.com/its-Lyn/WingedHazards/releases)! <br>
Once there, simply click on your OS's Release!

There are two currently supported Operating Systems.
- Linux
- Windows   

> [!TIP]
> For Linux based distributions, you can run `setup.sh` to create a desktop entry!

### Compiling yourself (Advanced)
If you do not wish to use the release pages, you can also compile the game yourself.

#### Pre-requisities
To compile the game you require a few apps; `git`, `dotnet-sdk` and `dotnet-runtime`

```bash
# Ubuntu/Fedora
sudo apt/dnf install git dotnet-sdk-8.0 dotnet-runtime-8.0

# Arch based distributions
sudo pacman -S git dotnet-sdk dotnet-runtime

# Windows
# Windows is less sigma so DOTNET is usually downloaded through Visual Studio.
# Download here: https://visualstudio.microsoft.com/
# And Git here: https://git-scm.com/downloads/win
# Or if you have winget installed
winget install --id Microsoft.VisualStudio.2022.Community
winget install --id Git.Git
# Good luck.
```

#### Compilation
Now, we can start installing the game! <br>
Begin by opening your favourite `terminal emulator`!

Aside the game, we will also need to clone the engine.

```bash
# They need to be NEXT to eachother.
# so, something like this
# YourDirectory/
#   MonoGayme/
#       ...
#   WingedHazards/
#       ...
# This will be fixed in the future when the engine is done
# and published as a nuget.
git clone https://github.com/its-Lyn/MonoGayme
git clone https://github.com/its-Lyn/WingedHazards
cd WingedHazards
```

All that is left now is to compile the game.
> [!TIP]
> For Linux systems, you can also run `install.sh`, it will create a desktop entry for you.

Run the following command to compile the game.
```bash
dotnet publish -c Release -p:PublishSingleFile=true
```

## Uninstalling
Uninstalling Winged Hazards is as easy as running one command!
```bash
# If you used the Releases Page
./setup.sh --remove

# If you compiled yourself
./install.sh --remove
```

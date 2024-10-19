# Winged Hazards

My first original game outside a game engine!

Winged Hazards is a game following the [GameBoy](https://en.wikipedia.org/wiki/Game_Boy) rules! That means the game only has four directional controls, A and B, Start and Select controls, and the game resolution is a whopping 144x160 with a whole four colours!

## Game Description
Your name is River, a young scientist who has been tasked with seeing why the local forest has been suddenly invaded by an invasive bat species. Try to survive as long as you can while fending the bats away, and gaining new Super Powers along the way!

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
| Circle / B / A             | Jump                                  |
| X / A / B                  | Action / UI Accept                    |

## Installation
Instructions on installing the game.

### Using releases page (Recommended)
Click on the [Releases page](https://github.com/its-Lyn/WingedHazards/releases)! <br>
Once there, simply click on your OS's Release!

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
```

#### Compilation
Now, we can start installing the game! <br>
Begin by opening your favourite `terminal emulator`!

```
git clone https://github.com/its-Lyn/WingedHazards
cd WingedHazards
```

All that is left now is to compile the game. For Linux systems, you can also run `install.sh`, it will create a desktop entry for you. <br>
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

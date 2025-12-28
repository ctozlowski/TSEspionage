# TSEspionage - A Twilight Struggle Mod

TSEspionage is a mod for the Playdek Twilight Struggle client. This mod adds various improvements and addresses bugs.

## Requirements

- Windows (the game is Windows-only)
- [BepInEx 6 Bleeding Edge](https://builds.bepinex.dev/projects/bepinex_be) (IL2CPP x64 build)

## Installation (Windows)

### Step 1: Install BepInEx

1. Find your Twilight Struggle install folder:
   - Open Steam, right-click "Twilight Struggle" → "Properties..." → "Installed Files" → "Browse..."
   
2. Download [BepInEx 6 Bleeding Edge](https://builds.bepinex.dev/projects/bepinex_be):
   - Choose the **Unity IL2CPP Windows x64** build (e.g., `BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.752+...`)
   
3. Extract the BepInEx ZIP directly into the Twilight Struggle folder
   - You should see `winhttp.dll`, `doorstop_config.ini`, and a `BepInEx` folder alongside `TwilightStruggle.exe`

4. Run the game once and close it
   - This generates the IL2CPP interop assemblies that BepInEx needs

### Step 2: Install TSEspionage

1. [Download the latest version of TSEspionage](https://github.com/xelrach/TSEspionage/releases)

2. Extract `TSEspionage.dll` into:
   ```
   Twilight Struggle/BepInEx/plugins/TSEspionage/
   ```
   (Create the `TSEspionage` folder if it doesn't exist)

3. Launch the game - the mod should now be active

## Uninstall

### Remove TSEspionage Only
Delete the `BepInEx/plugins/TSEspionage/` folder

### Remove BepInEx Completely
Delete from the Twilight Struggle directory:
- `winhttp.dll`
- `doorstop_config.ini`
- `.doorstop_version`
- `BepInEx/` folder

## Features

- Card count display on hand/discard/removed tabs
- Additional "Fastest" animation speed option
- Shuttle Diplomacy scoring indicator for Asia and Middle East
- Game log export

## Development

### Prerequisites

- .NET 6.0 SDK or later
- Windows (required for testing, though you can edit code on any platform)
- A C# IDE (Visual Studio, Rider, or VS Code with C# extension)

### Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/xelrach/TSEspionage.git
   cd TSEspionage
   ```

2. Install BepInEx in your game (see Installation above) and run the game once to generate interop assemblies

3. Create the project reference structure:
   ```
   TSEspionage/
   ├── game/
   │   └── Twilight Struggle/
   │       └── BepInEx/
   │           ├── core/        <- Copy from game install
   │           └── interop/     <- Copy from game install (generated on first run)
   ├── TSEspionage/
   │   └── ... (source files)
   └── TSEspionage.sln
   ```
   
   Copy these folders from your game's BepInEx installation into `game/Twilight Struggle/BepInEx/`:
   - `core/` - BepInEx runtime DLLs
   - `interop/` - Generated IL2CPP type stubs

4. Build the project:
   ```bash
   dotnet build
   ```
   
   The output DLL will be placed in `game/Twilight Struggle/BepInEx/plugins/TSEspionage/`

### Project Structure

- `TSEspionage/` - Main mod source code
  - `Plugin.cs` - BepInEx plugin entry point
  - `*Patches.cs` - Harmony patches for game modifications
  - `CardCountManager.cs`, `CardTabBehaviour.cs`, `RegionControlBar.cs` - Custom MonoBehaviour components

### Architecture Notes

This mod uses:
- **BepInEx 6** for mod loading (IL2CPP version)
- **HarmonyX** for runtime patching
- **Il2CppInterop** for IL2CPP type system integration

Custom MonoBehaviour classes require:
- An `IntPtr` constructor: `public MyClass(IntPtr ptr) : base(ptr) { }`
- Registration via `ClassInjector.RegisterTypeInIl2Cpp<T>()`
- `[HideFromIl2Cpp]` attribute on methods using managed-only types

## License

This project is licensed under the Mozilla Public License 2.0 - see the [LICENSE](LICENSE) file for details.

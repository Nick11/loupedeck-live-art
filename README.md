# Loupedeck Plugin for ART (RawTherapee Fork)

Control [ART](https://github.com/artraweditor/ART) with a Loupedeck Live: sliders on the dials, tool tabs and shortcuts on the buttons.
Plugin for the Logi Actions SDK (Loupedeck software 6.3+ / Logi Options+), macOS and Windows 11.

> Disclaimer: This plugin was generated with AI

## Use It

### Installation

**Requires:** Loupedeck Live (also CT, Live S, MX Creative Console) · Loupedeck software ≥ 6.3 (or Logi Options+) · ART ≥ 1.26 · macOS or Windows 11

1. Install the plugin, either
   - **Marketplace:** Loupedeck software (or Logi Options+) → *Marketplace* → search **ART Raw Editor** → *Install*, or
   - **GitHub (without Marketplace):** download `ArtRawEditor_<version>.lplug4` from the
     [Releases](https://github.com/Nick11/loupedeck-live-art/releases) and double-click it.
     The Loupedeck software installs it like a Marketplace plugin.
2. **macOS only:** System Settings → Privacy & Security → *Accessibility* → enable **Logi Plugin Service**.
   If it is already enabled but nothing reacts, remove it with "−" and add it again.
3. Start ART and open an image in the Editor. The Loupedeck switches to the **ART Raw Editor** profile automatically
   (the default layout for the Loupedeck Live ships with the plugin; other devices start with an empty profile).

**Update:** update in the Marketplace or install the newer `.lplug4`. Your own layout is kept; to get the new default layout,
reset the ART profile or create a new one.
**Uninstall:** Loupedeck software → plugin list → *Uninstall*.

### Use

Default layout (Loupedeck Live):
- **Round buttons:** Default · Exposure · Detail · Color · Local · Effects · Transform · Raw — switch the ART tab and the Loupedeck page.
- **Touch buttons:** export, export to external editor (GIMP), before/after, snapshot, zoom fit / 1:1, undo/redo, plus tools per tab.
- **Dials:** ART sliders per tab; bottom left is always the Hover Wheel.

The plugin adds these actions under **ART Raw Editor** in the Loupedeck software:

| Action | What it does |
|---|---|
| **ART Slider** (dial) | Turn = step an ART slider, press = reset it. Fast turns accelerate. |
| **ART Hover Wheel** (dial) | Turn = change the ART slider under the mouse pointer (Shift + wheel), press = reset it (double-click). |
| **ART Zoom** (dial) | Turn = zoom in/out, press = zoom to fit. |
| **ART Slider Step** (button) | +, − or reset for a slider — for devices without dials. |
| **ART Shortcut** (button) | ART tool tabs, undo/redo, before/after, zoom, crop, … |

Sliders available (ART's built-in hold-key sliders, press **F1** in ART to see them):
Exposure (compensation, black point) · Tone Curves (contrast) · Tone Equalizer (5 bands) · Log Tone Mapping (5) ·
White Balance (temperature) · Saturation/Vibrance · L\*a\*b\* (lightness, contrast, chromaticity) · Soft Light.

Tips:
- ART must be in front with the **Editor** open — click into the image once if dials don't react.
- Don't hold Shift/Ctrl/Alt/Cmd while turning a dial (the plugin pauses then, to avoid triggering ART shortcuts).
- **Hover Wheel:** point the mouse at an ART slider. It only acts while the pointer is over the ART window.
- **Zoom Fit** fits the cropped area when a crop is active.
- The value is shown by ART itself (on-screen info); the plugin cannot read ART's values.
- A warning on the plugin in the Loupedeck software means keystrokes are blocked (macOS Accessibility, or secure input).

## Devs

### Dependencies

| Tool | Version | Purpose |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | 8.x | build (`dotnet build`) |
| [LogiPluginTool](https://www.nuget.org/packages/LogiPluginTool) | 6.1.4.22672 | provides `PluginApi.dll` for the build; `pack` / `verify` the `.lplug4` |
| Loupedeck software (or Logi Options+) | ≥ 6.3 | runs the plugin (Logi Plugin Service) |
| [ART](https://github.com/artraweditor/ART) | ≥ 1.26 | target application |
| Python 3 | any | `tools/apply-icons-to-profile.py` |

The project builds against the net8 `PluginApi.dll` shipped with LogiPluginTool (the copy installed with the Logi Plugin Service targets .NET 10).

### Setup

```
dotnet tool install --global LogiPluginTool --version 6.1.4.22672
export PATH="$PATH:$HOME/.dotnet/tools"   # add to ~/.zprofile
```

On macOS, enable **Logi Plugin Service** under System Settings → Privacy & Security → Accessibility.

### Build and reload

```
cd src
dotnet build
```

The build writes `bin/Debug/`, copies `src/package/` into it and creates a `.link` file in the Logi Plugin Service `Plugins` folder,
so the service loads the plugin directly from `bin/Debug/`.

Reload after a build:
- `open loupedeck:plugin/ArtRawEditor/reload` (Windows: `start loupedeck:plugin/ArtRawEditor/reload`)
- If the plugin log shows `plugin 'ArtRawEditor' is already loaded`, the old version keeps running:
  quit and restart the Loupedeck software.

Plugin log: `~/Library/Application Support/Logi/LogiPluginService/Logs/plugin_logs/ArtRawEditor.log`
(Windows: `%LOCALAPPDATA%\Logi\LogiPluginService\Logs\plugin_logs\`).

### Project layout

| Path | Content |
|---|---|
| `src/Art/` | ART knowledge: sliders (hold keys), shortcuts, icon loader |
| `src/Actions/` | Loupedeck actions (dial, slider step, shortcut) |
| `src/Input/` | keystroke sending (`KeySender`), acceleration, OS input checks |
| `src/package/` | package content: metadata, plugin icon, default profile (`profiles/DefaultProfile30.lp5`) |
| `icons/buttons/` | **finished button icons** (`<action id>.svg`) — the single source of truth, loaded as-is |
| `icons/tabs/` | tab icons with label, for manually configured multi-actions |
| `icons/art/` | original ART icons (CC BY-SA 4.0) used to build the buttons |
| `tools/` | `apply-icons-to-profile.py` |

### Exporting profile for git

The default layout ships inside the plugin package as `src/package/profiles/DefaultProfile30.lp5`
(profile name **ART Raw Editor**). To change it:

1. In the Loupedeck software open the **ART Raw Editor** profile → **⋯** → **Export**.
2. Add the plugin icons (icon from `icons/buttons/`, automatic action text hidden) and save it as
   `src/package/profiles/DefaultProfile30.lp5` (`30` = Loupedeck Live, `50` = Live S, `20` = all devices):
   `tools/apply-icons-to-profile.py exported.lp5 src/package/profiles/DefaultProfile30.lp5`
3. Check for personal data before committing (it is a zip of JSON files):
   `unzip -p src/package/profiles/DefaultProfile30.lp5 ProfileInfo.json | grep -i -E "user|account|mail|/Users/"`
4. Build and commit. `dotnet build` copies everything under `src/package/` into the plugin.

Notes:
- The default profile is applied only on first install, profile reset or a newly connected device — **not** on plugin updates.
  On another machine, reset the ART profile or import the `.lp5` (double-click) to get the new layout.
- Don't edit `.lp5` files by hand.
- The profile references actions by class name and parameter ID (e.g. `ArtSliderAdjustment___exposure.compensation`):
  never rename those after a release, or saved profiles break.

### Release

```
cd src
dotnet build -c Release
logiplugintool pack ../bin/Release/ ../ArtRawEditor_<major>_<minor>.lplug4
logiplugintool verify ../ArtRawEditor_<major>_<minor>.lplug4
```

1. Bump `version` (`major.minor`) in `src/package/metadata/LoupedeckPackage.yaml`.
2. Build, pack and verify as above (file name `ArtRawEditor_1_0.lplug4` for version 1.0, as recommended by Logitech).
3. Test the `.lplug4` on macOS and Windows with a Loupedeck Live (install by double-click).
4. Attach the `.lplug4` to a GitHub release — this is the install path without the Marketplace.
5. Marketplace: submit the same `.lplug4` at [marketplace.logi.com/contribute](https://marketplace.logi.com/contribute)
   with [EULA.md](EULA.md) as the developer EULA (requires a Logi account and accepting the Developer Agreement).
   Each new version is reviewed again (up to ~10 working days). See the
   [approval guidelines](https://logitech.github.io/actions-sdk-docs/marketplace-approval-guidelines/).

`dotnet build -c Release` also points the local `.link` file to `bin/Release/`; run `dotnet build` again to go back to Debug.

## License and Credits

- Plugin code: [MIT](LICENSE). End-user terms: [EULA.md](EULA.md).
- ART icons in `icons/art/` and the buttons derived from them: © Maciej Dworak, [CC BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/).
  All third-party assets are listed in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
- ART is © its authors, GPL-3.0; this plugin contains no ART code and only sends keystrokes to it.

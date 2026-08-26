# My XrmToolBox Plugin

> **This project was created from the [XrmToolBox Plugin Template](https://github.com/HurleySk/XrmToolBox-Plugin-Template).**

TODO: Describe what your plugin does.

## Getting Started (Template Setup)

After creating your repository from this template:

### 1. Find and Replace

Do a global find-and-replace across all files for these tokens (in this order):

| Token | Replace with | Example |
|-------|-------------|---------|
| `MyXrmToolBoxPlugin` | Your full project name | `AttributeExporterXrmToolBoxPlugin` |
| `My XrmToolBox Plugin` | Display name shown in XrmToolBox | `Attribute Metadata Exporter` |
| `MyPlugin` | Your plugin class prefix | `AttributeExporter` |
| `YOUR_GITHUB_USERNAME` | Your GitHub username | `HurleySk` |
| `YOUR_NAME` | Your name | `Samuel Hurley` |
| `YOUR_COMPANY` | Your company name | `Procentrix` |

### 2. Rename Files

Rename these files to match your plugin name:

- `MyXrmToolBoxPlugin.sln` -> `YourPluginName.sln`
- `MyXrmToolBoxPlugin.csproj` -> `YourPluginName.csproj`
- `MyPluginPlugin.cs` -> `YourPrefixPlugin.cs`
- `MyPluginControl.cs` -> `YourPrefixControl.cs`
- `MyPluginControl.Designer.cs` -> `YourPrefixControl.Designer.cs`
- `MyPluginControl.resx` -> `YourPrefixControl.resx`

### 3. Replace Icons

Replace the placeholder PNGs in `Resources/` with your own icons:

- `icon-128.png` (128x128) - NuGet package icon
- `icon-80.png` (80x80) - Large icon in XrmToolBox
- `icon-32.png` (32x32) - Small icon in XrmToolBox

Then update the base64 strings in your plugin entry point file:

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("Resources\icon-32.png"))
[Convert]::ToBase64String([IO.File]::ReadAllBytes("Resources\icon-80.png"))
```

### 4. Generate a New GUID

Replace the placeholder GUID in `Properties/AssemblyInfo.cs`:

```powershell
[guid]::NewGuid().ToString()
```

### 5. Update the .sln File

Update the project reference in the `.sln` file to point to your renamed `.csproj` file. Alternatively, delete the `.sln` and regenerate it in Visual Studio.

### 6. Clean Up This Section

Delete this "Getting Started" section from the README once you're done setting up.

---

## Features

- TODO: List your plugin features

## Installation

### From XRM ToolBox Tool Store (Recommended)
1. Open XRM ToolBox
2. Open Tool Library (Ctrl+T)
3. Search for "My XrmToolBox Plugin"
4. Click Install and restart

### Manual Installation
1. Build the project: `dotnet build --configuration Release`
2. Run `.\deploy.ps1 -Force` to deploy to XRM ToolBox

## Usage

TODO: Describe how to use your plugin.

## Building from Source

```bash
git clone https://github.com/YOUR_GITHUB_USERNAME/MyXrmToolBoxPlugin.git
cd MyXrmToolBoxPlugin
dotnet build --configuration Release
```

### Testing Locally

1. Build and deploy the plugin:
   ```powershell
   .\deploy.ps1 -Force
   ```
2. Open (or restart) XRM ToolBox
3. If your plugin doesn't appear, delete the manifest cache and restart:
   ```
   %APPDATA%\MscrmTools\XrmToolBox\Plugins\manifest.json
   ```
4. Find your plugin in the tool list and open it
5. Connect to a Dataverse environment using the connection button in the toolbar
6. Test your plugin's functionality

**Tip:** During development, use `.\deploy.ps1 -Force` to automatically close XRM ToolBox, rebuild, deploy, and relaunch in one step.

Deploy script options:
- `-SkipBuild` - Skip the build step and use existing binaries
- `-Force` - Close XRM ToolBox automatically if running
- `-WhatIf` - Show what would happen without making changes

### Create NuGet Package

```bash
dotnet pack --configuration Release
```

### Publish to NuGet

```bash
dotnet nuget push bin\Release\MyXrmToolBoxPlugin.1.0.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## Troubleshooting

**Plugin not appearing in XRM ToolBox:**
Delete `%APPDATA%\MscrmTools\XrmToolBox\Plugins\manifest.json` and restart XRM ToolBox.

**"Object reference" or missing assembly errors:**
Make sure all dependency DLLs are deployed alongside the plugin DLL. Re-run `.\deploy.ps1`.

**"Could not load file or assembly" errors:**
If your plugin uses additional NuGet packages, make sure to:
1. Add their DLLs to `$pluginFiles` in `deploy.ps1`
2. Add `<None Include="$(OutputPath)\YourDependency.dll" Pack="true" PackagePath="Plugins\" />` to the `.csproj`

## License

MIT License - see [LICENSE](LICENSE) for details.

## Author

**YOUR_NAME** - [YOUR_GITHUB_USERNAME](https://github.com/YOUR_GITHUB_USERNAME)

Built on [XRM ToolBox](https://www.xrmtoolbox.com/) by Tanguy Touzard.

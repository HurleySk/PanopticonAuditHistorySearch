// =============================================================================
// XrmToolBox Plugin Entry Point
//
// To update the icon base64 strings after replacing the PNGs, run in PowerShell:
//   [Convert]::ToBase64String([IO.File]::ReadAllBytes("Resources\icon-32.png"))
//   [Convert]::ToBase64String([IO.File]::ReadAllBytes("Resources\icon-80.png"))
// =============================================================================

using System.ComponentModel.Composition;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace MyXrmToolBoxPlugin
{
    [Export(typeof(IXrmToolBoxPlugin))]
    [ExportMetadata("Name", "My XrmToolBox Plugin")]
    [ExportMetadata("Description", "TODO: Describe your plugin")]
    [ExportMetadata("SmallImageBase64", "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAD7SURBVFhHxZM9DoJAEEY5Hgey4BzUnsBLyCGIlQWxhMqOboz757oPVDT5nOQVvM0y3w5LVTW9/RUINRBqINRAqIFQA6EGQg2EGgg1EGogAruT+TpdgrvY0YnZhimsFXXsRhtKmfavAJE42941utqu6a3uZt/kUIb06861PsDQnd2z3zPbvi3f/VGA3qrD1TUdutGfvjjNuwBYXwLiiTiFe/EkaBAC5JVPbBGIJ74LECcQJ/gyBEROfEG8XBs/AZ6XgEiEWz+NVmd/xZZL+MMEHqNPm9P3fTRcC5DXy9OvBxACoQZCDYQaCDUQaiDUQKiBUAOhBkINhBoIMTdgXbYr3PWyNwAAAABJRU5ErkJggg==")]
    [ExportMetadata("BigImageBase64", "iVBORw0KGgoAAAANSUhEUgAAAFAAAABQCAYAAACOEfKtAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAJTSURBVHhe7dYxTsMwGIbhHo8DMXAO5p6AS8AhEFOHqiOd2NiMWlo19fsn9mc7iOGL9Cxv0uA4Nu1m8/SerAOCaRBMg2AaBNMgmAbBNAimQTANgmkQTINgGgTTIJgGwTQIpkEwDYJpEEyDYBoE0yCYBsE0CKZBMA2CaRBMg2AaBNMgmAbBNAimQVC8fCUeX+kxv+7i4e07vzi9vrynx4+81h+nz//ef5e2x/xsfOzfdhhbMwRRNCnhAJ8/0z6/8ONwPvfXE3g9bp/tgCA7pNd8ZMEq5CTdruG5+qNnAlP6Ttvn/HlECC2CrXy3CoPVV3z7uCdfyj1O4P1O4Hle0wChSTC442d6uJzHCpucmzV8Ak+C3XL5N9IModXcKkOv3DarTGDNNSKEDtFK22atesBrTGDry1yC0CXYIndHaRImhk9g6XwjhE7Rz5rrUfzimBowgfPHgJV3hdBt5kFqvjimVp3AAV8eVwidllagtGXWnsDzMWAlIvTAP+n8KE3CxIAJxAuLxqfujBxCs+gBDmjVW2eNCQzvK/5vziE04ta9bI/WAeNzgyYwWIVV45mD0IQ/X26D54NVbZu1JhD3/QcTiB/Q+cMGgw4fbvEzAyYwWH3l+xYgqPCgwcCDhyt+A+K+pQeN/kb54FhFCBJu3dkHjd7+0hfKH0xg9+SdIFSLB7w0KG71hevXnMClF6dCMA2CaRBMg2AaBNMgmAbBNAimQTANgmkQTINgGgTTIJgGwTQIpkEwDYJpEEyDYBoE0yCYBsE0CKZBMA2CaRBMg2AaBJP8AEUbkSEqIWYjAAAAAElFTkSuQmCC")]
    [ExportMetadata("BackgroundColor", "White")]
    [ExportMetadata("PrimaryFontColor", "Black")]
    [ExportMetadata("SecondaryFontColor", "Gray")]
    [ExportMetadata("IsOpenSource", true)]
    public class MyPluginPlugin : PluginBase, IGitHubPlugin
    {
        // IGitHubPlugin: links your plugin to its GitHub repository.
        // Remove IGitHubPlugin (and these properties) if not hosting on GitHub.
        public string RepositoryName => "MyXrmToolBoxPlugin";
        public string UserName => "YOUR_GITHUB_USERNAME";

        public override IXrmToolBoxPluginControl GetControl()
        {
            return new MyPluginControl();
        }
    }
}

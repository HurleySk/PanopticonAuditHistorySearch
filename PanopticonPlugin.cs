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

namespace PanopticonAuditHistorySearch
{
    [Export(typeof(IXrmToolBoxPlugin))]
    [ExportMetadata("Name", "Panopticon Audit History Search")]
    [ExportMetadata("Description", "Search Dataverse audit history across multiple tables with a local cache for fast filtering by table, user, event, date and changed field.")]
    [ExportMetadata("SmallImageBase64", "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAARuSURBVFhHxZdfbFNVHMf7yBt/Rv/39ty2672XdutthbluzcYmW1tt0GXIn3WFFZl2xDHGBlgfCFVDgsBCBOIixqHBGDHyJ0DURLIYlxgkMcEXeVDjgyZGfTD4Am8/8zt399xz710mmtg2+T605/Z8vud3fud773U4LB8SU5OCrJYFOVUTZLUmRA35qVoNRXjFa15UiNc6JjeRyy6ipK089iHKY2lBSd4LKikIyikQ5CQIki4VAlFUgsnf3Goo0gI+VDjO5A3FOK0Dj4hSwCPKv3qIlLfBKfh/h2tyExlV1uBk4wpBSf1UZzi4g/KfThLzGKuvJ5wakMAdiO5wCIo62Qi4KyiBU4jWHFq31x/uEqK6AbXWCLhmIFJz0DP+H+HhVDekCiMQfbz/X8NdQjM4/czAP8PFRAf0vXgcdl+4A5VL96B6G2zae+VHKM99DYWXz0NkwxPLwwOcgeXgaq4IW09cg0MLDxloev4vKM/dgeFztyB/8Bw8+/oVCh6/8YvJUOmtLyE9dGBpeCCiGcB4XQoeUjthx+lP2GQ4eXbyNC27vufhVBcknxqB5rZNrOy+5gRkdlVhz8W77L8vfPgdSO05M9ww0MoM8PDKR1qZRy/ehbaBiqnhlEzBBEDt/+wPWP/0qKnsUjoP22du0vGp+fugdG024IEINPlDhgG+7KPvf0v/tPXEVVu3x3sHTWCrcPXWPc/sPLxo8ncItqQpfK0/bBjg4R3bJ+jFO88v2OAoawPufvcb03dcKUl02vZ8076TdHzzkTkKNwzgrZTr9sFjl+iFiVzRBsfS66CDCw9BTHbDyrU+8EaTMPHpb2ysbcteW8N5QjFqbuzyD4YBL29g8ZzjnuMkVjie847iNIPsevsrCteVmz7LxnJTZ+wNF4jAyDu36Tg14AtxBriQ0TtfCxdzwsV6BhjkwK37OAEzwG9F93NHbHCEjl//mfYBhVMDBA3Ea3zC9Tz/Cp3kmaPvmeB4xPxRlWaADqp8/D30TZyC4pvz7Ddqvj1rg28YrNCxbaeuL8JFzQA+RvHxGpBS9Ejhxe3bxm3xil3Ow6waePUDG1xMddP9x3EpU1iE6wZChgG95InsEEu+/v0ztmzvm5gxJaOuLccv02bj4clCmcF7Kq9xcBFWuzkD1obDhNPLjRXBJhPVDAuZ8Ppe6B07Rhuub99JU8igiY7SIdrxujkrfI2HGAascL3sGLGY8/xqi2c/p2YQnsiXIJEfhkRuGLJTb1Bhp+srRpVmv4CW/iEbnDOAj812OF/2YKwNNu45Su921rIvJez0J1+aBTHZxTWcGa4ZEHgDS8Ott1RvuIWuHMMmO3WGrbyzdBhas0MgxNuNc74MfI0nyBt4NLg1Xq3dzvRIcN0AUaoNgmsGPCSWbxAcVruEMQch6iq3qDxoAByamrwKfTvyEHms3vBVzsCs6f3QTaSqKyg/qAvcFbhACFlhMqCZkEWXIE3iC4OmSA2f23jhQwSTVxdhwnAxJJjlFKornb4kz/wbcVNQElmRAG0AAAAASUVORK5CYII=")]
    [ExportMetadata("BigImageBase64", "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")]
    [ExportMetadata("BackgroundColor", "#1F2933")]
    [ExportMetadata("PrimaryFontColor", "White")]
    [ExportMetadata("SecondaryFontColor", "#9AA5B1")]
    [ExportMetadata("IsOpenSource", true)]
    public class PanopticonPlugin : PluginBase, IGitHubPlugin
    {
        // IGitHubPlugin: links your plugin to its GitHub repository.
        // Remove IGitHubPlugin (and these properties) if not hosting on GitHub.
        public string RepositoryName => "PanopticonAuditHistorySearch";
        public string UserName => "HurleySk";

        public override IXrmToolBoxPluginControl GetControl()
        {
            return new PanopticonControl();
        }
    }
}

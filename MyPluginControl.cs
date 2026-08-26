using McTools.Xrm.Connection;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Windows.Forms;
using XrmToolBox.Extensibility;

namespace MyXrmToolBoxPlugin
{
    public partial class MyPluginControl : PluginControlBase
    {
        public MyPluginControl()
        {
            InitializeComponent();
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            UpdateConnectionState();
        }

        /// <summary>
        /// Called by XrmToolBox when the connection changes.
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService,
            ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);
            UpdateConnectionState();
        }

        private void UpdateConnectionState()
        {
            bool connected = Service != null;
            btnDoSomething.Enabled = connected;

            if (connected)
            {
                lblStatus.Text = $"Connected to: {ConnectionDetail?.OrganizationFriendlyName}";
            }
            else
            {
                lblStatus.Text = "Not connected. Use the connection button above to connect.";
            }
        }

        /// <summary>
        /// ExecuteMethod checks the connection before calling the method.
        /// If not connected, it prompts the user to connect first.
        /// </summary>
        private void btnDoSomething_Click(object sender, EventArgs e)
        {
            ExecuteMethod(DoSomething);
        }

        private void DoSomething()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Working...",
                Work = (worker, args) =>
                {
                    // This runs on a background thread.
                    // Use 'Service' (IOrganizationService) to call Dataverse.
                    var response = (WhoAmIResponse)Service.Execute(new WhoAmIRequest());
                    args.Result = response.UserId;
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var userId = (Guid)args.Result;
                    txtResult.Text = $"Connected successfully!\r\n\r\nYour user ID: {userId}";
                }
            });
        }
    }
}

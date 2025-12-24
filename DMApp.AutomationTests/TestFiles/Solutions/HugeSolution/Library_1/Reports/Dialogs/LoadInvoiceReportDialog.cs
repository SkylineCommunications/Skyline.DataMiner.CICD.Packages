namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports
{
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ReportTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using System;

    public class LoadInvoiceReportDialog : LoadingDialog
    {
        public LoadInvoiceReportDialog(Helpers helpers) : base(helpers)
        {
        }

        public InvoiceReportDialog InvoiceReportDialog { get; private set; }

        protected override void GetScriptInput()
        {          
        }

        protected override void CollectActions()
        {
            methodsToExecute.Add(InitializeOrderLogger);
            methodsToExecute.Add(ConstructInvoiceReportDialog);
        }

        protected override void SendReportButton_Pressed(object sender, EventArgs e)
        {
            string title = "Exception while loading Invoice Report Dialog [" + DateTime.Now + "]";

            string message = $"Could not load Invoice Report script";

            NotificationManager.SendMailToSkylineDevelopers(Helpers, title, message);

            reportSuccessfullySentLabel.IsVisible = true;
        }

        private void InitializeOrderLogger()
        {
            Helpers.Log(nameof(LoadInvoiceReportDialog), "START SCRIPT", "Create Invoice Report");
        }

        private void ConstructInvoiceReportDialog()
        {
			var constructInvoiceReportDialogTask = Task.CreateNew(Helpers, () => new InvoiceReportDialog(Helpers), "Building UI");

            Tasks.Add(constructInvoiceReportDialogTask);

            IsSuccessful &= constructInvoiceReportDialogTask.Execute();

            if (!IsSuccessful) return;

            InvoiceReportDialog = constructInvoiceReportDialogTask.Result;
        }
    }
}

namespace NonLiveUserTasksBulkUpdate_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class NonLiveUserTasksBulkUpdateLoadingDialog : LoadingDialog
	{
		private List<string> fullTicketIds;
		private List<NonLiveUserTask> userTasks;

		public NonLiveUserTasksBulkUpdateLoadingDialog(Helpers helpers) : base(helpers)
		{

		}

		public NonLiveUserTasksBulkUpdateDialog NonLiveUserTasksBulkUpdateDialog { get; private set; }

		protected override void CollectActions()
		{
			methodsToExecute.Add(GetNonLiveUserTasks);
			methodsToExecute.Add(BuildUi);
		}

		protected override void GetScriptInput()
		{
			var scriptParam = Engine.GetScriptParam(2).Value;

			fullTicketIds = JsonConvert.DeserializeObject<List<string>>(scriptParam);
		}

		protected override void SendReportButton_Pressed(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void GetNonLiveUserTasks()
		{
			Helpers.ReportProgress($"Retrieving Non-Live user tasks...");

			userTasks = fullTicketIds.Select(tid => Helpers.UserTaskManager.GetUserTask(tid)).Cast<NonLiveUserTask>().ToList();

			Helpers.ReportProgress($"Retrieving Non-Live user tasks succeeded.");
		}

		private void BuildUi()
		{
			Helpers.ReportProgress($"Building UI...");

			NonLiveUserTasksBulkUpdateDialog = new NonLiveUserTasksBulkUpdateDialog(Helpers, userTasks);

			Helpers.ReportProgress($"Building UI succeeded.");
		}
	}
}

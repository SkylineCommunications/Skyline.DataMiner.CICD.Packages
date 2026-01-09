namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Type = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Type;

	public class LoadNonLiveUserTaskDialog : LoadingDialog
	{
		private readonly Dictionary<Type, List<string>> editRestrictions = new Dictionary<Type, List<string>>
		{
			{ Type.Import, new List<string> { "YLEAD\\sg_Dataminer_MediaMgmSupport", "YLEAD\\sg_Dataminer_MediaMgmtHkiFile", "YLEAD\\sg_Dataminer_MediaMgmtTre", "Administrators", "Configure" } },
			{ Type.NonInterplayProject, new List<string> { "YLEAD\\sg_Dataminer_MediaMgmSupport", "YLEAD\\sg_Dataminer_MediaMgmtHkiFile", "YLEAD\\sg_Dataminer_MediaMgmtTre", "Administrators", "Configure" } },
			{ Type.IplayFolderCreation, new List<string> { "YLEAD\\sg_Dataminer_MediaMgmSupport", "Administrators", "Configure" } },
		};

		private int ticketId;
		private int dataminerId;
		private NonLiveUserTask nonLiveOrderUserTask;
		
		public LoadNonLiveUserTaskDialog(Helpers helpers) : base(helpers)
		{
		}

		public NonLiveUserTaskDialog NonLiveUserTaskDialog { get; private set; }

		protected override void GetScriptInput()
		{
			var rawScriptInputParameter = Helpers.Engine.GetScriptParam("ticketId").Value;

			Helpers.Log(nameof(LoadNonLiveUserTaskDialog), nameof(GetScriptInput), $"Ticket id input parameter value: {rawScriptInputParameter}");

			try
			{
				var inputTicketIds = JsonConvert.DeserializeObject<string[]>(rawScriptInputParameter);

				if (inputTicketIds == null || inputTicketIds.Length == 0)
				{
					PrepareUiForManualErrorMessage("At least one user task need to be selected before you can proceed", showExceptionWidgets: false);
				}
				else if (inputTicketIds.Length > 1)
				{
					PrepareUiForManualErrorMessage("It is only allowed to select one user task at a time", showExceptionWidgets: false);
				}
				else
				{
					string[] dmaAndTicketId = inputTicketIds[0].Split(new[] { '/' });
					if (dmaAndTicketId.Length == 2)
					{
						if (!int.TryParse(dmaAndTicketId[1], out ticketId)) throw new ArgumentException("Script input is not valid.");
						if (!int.TryParse(dmaAndTicketId[0], out dataminerId)) throw new ArgumentException("Script input is not valid.");
					}
					else
					{
						ticketId = -1;
						dataminerId = -1;
					}
				}
			}
			catch (Exception e)
			{
				Helpers.Log(nameof(LoadNonLiveUserTaskDialog), nameof(GetScriptInput), $"Exception while parsing ticket id input property value {rawScriptInputParameter}: {e}");
			}
		}

		protected override void CollectActions()
		{
			methodsToExecute.Add(InitializeOrderLogger);
			methodsToExecute.Add(GetNonLiveUserTask);
			methodsToExecute.Add(GetUserInfo);
			methodsToExecute.Add(ConstructNonLiveUserTaskForm);
		}

		protected override void SendReportButton_Pressed(object sender, EventArgs e)
		{
			string title = "Exception while loading Non-Live User Task Form [" + DateTime.Now + "]";

			string message = $"Order: '{nonLiveOrderUserTask?.Name}'<br>Order ID: {dataminerId}/{ticketId}<br>User: {Engine.UserLoginName}<br>Timestamp: {DateTime.Now}<br>Exception: {informationMessageLabel.Text.Replace(" at ", "<br>at ")}";

			NotificationManager.SendMailToSkylineDevelopers(Helpers, title, message);

			reportSuccessfullySentLabel.IsVisible = true;
		}

		private void InitializeOrderLogger()
		{
			Helpers.Log(nameof(LoadNonLiveUserTaskDialog), "START SCRIPT", "UPDATE NON LIVE USER TASK");
		}

		private void GetNonLiveUserTask()
		{
			if (dataminerId == -1 && ticketId == -1) return;

			var getNonLiveUserTaskTask = Task.CreateNew(Helpers, () => Helpers.NonLiveUserTaskManager.GetNonLiveUserTask(dataminerId, ticketId), "Getting Non Live User Task", (userTask => userTask?.Name));

			Tasks.Add(getNonLiveUserTaskTask);

			IsSuccessful &= getNonLiveUserTaskTask.Execute();
			if (!IsSuccessful) return;

			nonLiveOrderUserTask = getNonLiveUserTaskTask.Result;
		}

		private void GetUserInfo()
		{
			var getBaseUserInfoTask = new GetBaseUserInfoTask(Helpers);
			Tasks.Add(getBaseUserInfoTask);

			IsSuccessful &= getBaseUserInfoTask.Execute();
			if (!IsSuccessful) return;

			UserInfo = getBaseUserInfoTask.UserInfo;
		}

		private void ConstructNonLiveUserTaskForm()
		{
            if (!IsNonLiveUserTaskEditAllowed())
            {
				return;
			}

			var constructNonLiveUserTaskFormTask = new ConstructNonLiveUserTaskDialogTask(Helpers, nonLiveOrderUserTask);
			Tasks.Add(constructNonLiveUserTaskFormTask);

			IsSuccessful &= constructNonLiveUserTaskFormTask.Execute();
			if (!IsSuccessful) return;

			NonLiveUserTaskDialog = constructNonLiveUserTaskFormTask.NonLiveUserTaskDialog;
		}

		private bool IsNonLiveUserTaskEditAllowed() 
		{
			bool foundAllowedUserGroups = editRestrictions.TryGetValue(nonLiveOrderUserTask.LinkedOrderType, out var allowedUserGroups);

			if (!foundAllowedUserGroups)
            {
				PrepareUiForManualErrorMessage($"User task of type {nonLiveOrderUserTask.LinkedOrderType} is not allowed to be edited", showExceptionWidgets: false);
				return false;
			}

			if (!UserInfo.UserGroups.Any(userGroup => allowedUserGroups.Contains(userGroup.Name)))
			{
				PrepareUiForManualErrorMessage($"User {UserInfo.User.Name} has no permission to edit this user task", showExceptionWidgets: false);
				return false;
			}

			return true;
		}
	}
}

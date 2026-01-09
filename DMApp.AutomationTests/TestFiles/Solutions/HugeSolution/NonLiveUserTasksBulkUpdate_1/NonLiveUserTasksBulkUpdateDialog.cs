namespace NonLiveUserTasksBulkUpdate_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class NonLiveUserTasksBulkUpdateDialog : YleDialog
	{
		private readonly TicketFieldSection<string> stateSection;
		private readonly TicketFieldSection<DateTime> deleteDateSection;
		private readonly TicketFieldSection<string> commentSection;

		private readonly YleButton saveButton = new YleButton("Save") { Style = ButtonStyle.CallToAction };
		private readonly Label invalidLabel = new Label(string.Empty);

		private readonly IEnumerable<NonLiveUserTask> userTasks;

		public NonLiveUserTasksBulkUpdateDialog(Helpers helpers, IEnumerable<NonLiveUserTask> userTasks) : base(helpers)
		{
			Title = "Non-Live User Task Bulk Update";

			this.userTasks = userTasks;

			stateSection = new TicketFieldSection<string>(NonLiveOrder.StateTicketField, new YleDropDown(GetUserTaskStatusOptions(userTasks)));
			deleteDateSection = new TicketFieldSection<DateTime>(NonLiveOrder.DeletionDateTicketField);
			commentSection = new TicketFieldSection<string>(NonLiveOrder.OperatorCommentTicketField);

			saveButton.Pressed += SaveButton_Pressed;

			GenerateUi();
		}

		public event EventHandler SaveCompleted;

		protected override void HandleEnabledUpdate()
		{
			stateSection.IsEnabled = IsEnabled;
			deleteDateSection.IsEnabled = IsEnabled;
			commentSection.IsEnabled = IsEnabled;

			saveButton.IsEnabled = IsEnabled;
		}

		private static List<string> GetUserTaskStatusOptions(IEnumerable<NonLiveUserTask> userTasks)
		{
			var statusOptions = new List<string> { UserTaskStatus.Pending.GetDescription() };

			if (userTasks.OfType<IplayFolderCreationUserTask>().Any())
			{
				// iPlay folder delete tasks

				statusOptions.Add(UserTaskStatus.DeleteDateNear.GetDescription());
				statusOptions.Add(UserTaskStatus.DeletionInProgress.GetDescription());
				statusOptions.Add(UserTaskStatus.FolderDeleted.GetDescription());
			}
			else if (userTasks.OfType<ImportUserTask>().Any() || userTasks.OfType<NonIplayProjectUserTask>().Any())
			{
				// Isilon Backup delete tasks

				statusOptions.Add(UserTaskStatus.BackupDeleteDateNear.GetDescription());
				statusOptions.Add(UserTaskStatus.BackupDeleted.GetDescription());
			}

			return statusOptions;
		}


		private void SaveButton_Pressed(object sender, YleValueWidgetChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				foreach (var userTask in userTasks)
				{
					UpdateUserTask(userTask);
					SaveCompleted?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		private void UpdateUserTask(NonLiveUserTask userTask)
		{
			userTask.Status = stateSection.LeaveUnchanged ? userTask.Status : stateSection.InputValue.GetEnumValue<UserTaskStatus>();

			userTask.DeleteDate = deleteDateSection.LeaveUnchanged ? userTask.DeleteDate : deleteDateSection.InputValue;

			userTask.DeleteComment = commentSection.LeaveUnchanged ? userTask.DeleteComment : commentSection.InputValue;

					bool updateStateBasedOnDeleteDate = stateSection.LeaveUnchanged && !deleteDateSection.LeaveUnchanged;
					if (updateStateBasedOnDeleteDate) UpdateStateBasedOnDeleteDate(userTask);

					userTask.AddOrUpdate(helpers);

					SaveCompleted?.Invoke(this, EventArgs.Empty);
		}
		private static void UpdateStateBasedOnDeleteDate(NonLiveUserTask userTask)
		{
			bool userTaskShouldHaveStatusPending = (userTask.DeleteDate - DateTime.Now.Date) > TimeSpan.FromDays(14);
			if (userTaskShouldHaveStatusPending)
			{
				userTask.Status = UserTaskStatus.Pending;
			}
			else if (userTask is IplayFolderCreationUserTask)
			{
				userTask.Status = UserTaskStatus.DeleteDateNear;
			}
			else if (userTask is ImportUserTask || userTask is NonIplayProjectUserTask)
			{
				userTask.Status = UserTaskStatus.BackupDeleteDateNear;
			}
			else
			{
				// No status change required
			}
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddSection(stateSection, new SectionLayout(++row, 0));
			row += stateSection.RowCount;

			AddSection(deleteDateSection, new SectionLayout(++row, 0));
			row += deleteDateSection.RowCount;

			AddSection(commentSection, new SectionLayout(++row, 0));
			row += commentSection.RowCount;

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(invalidLabel, ++row, 0);
			AddWidget(saveButton, row + 1, 0);
		}
	}
}

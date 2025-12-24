namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.UserTasks
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Type = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Type;

	public class NonLiveUserTaskDialog : Dialog
    {
        private readonly Label generalTitle = new Label("General") { Style = TextStyle.Heading };
        private readonly Label technicalTitle = new Label("Technical") { Style = TextStyle.Heading };

        private readonly Label linkToNonLiveOrderLabel = new Label("Link to non live order");
        private readonly Label nonLiveOrderNameLabel = new Label("Non Live Order Name");
        private readonly Label createdByLabel = new Label("Created By");
        private readonly Label createdByMailLabel = new Label("Created By Email");
        private readonly Label stateLabel = new Label("State");
        private readonly Label nonLiveTypeLabel = new Label("Non Live Type");
        private readonly Label userGroupLabel = new Label("User Group");

        private readonly Label deleteDateLabel = new Label("Delete Date");
        private readonly Label deadlineDateLabel = new Label("Deadline");
        private readonly Label deleteCommentLabel = new Label("Delete Date Comment");
        private readonly Label folderPathLabel = new Label("Folder Path");
       
        private readonly NonLiveUserTask nonLiveUserTask;
        private readonly Helpers helpers;

        public NonLiveUserTaskDialog(Helpers helpers, NonLiveUserTask nonLiveUserTask) : base(helpers.Engine)
        {
            Title = "Non-Live User Task Update";

            this.helpers = helpers;
            this.nonLiveUserTask = nonLiveUserTask ?? throw new ArgumentNullException(nameof(nonLiveUserTask));
          
            Initialize();
            CreateTechnicalSection();
            GenerateUI();
        }

        public NonLiveUserTaskSection LinkedUserTaskSection { get; private set; }

        public YleButton CloseButton { get; private set; } = new YleButton("Close") { Width = 150 };

        public YleButton ConfirmChangesButton { get; private set; } = new YleButton("Confirm Changes") { Width = 150, Style = ButtonStyle.CallToAction };

        public YleTextBox LinkToNonLiveOrderTextBox { get; private set; } = new YleTextBox { IsEnabled = false };

        public YleTextBox NonLiveOrderNameTextBox { get; private set; } = new YleTextBox { IsEnabled = false };

        public YleTextBox CreatedByTextBox { get; private set; } = new YleTextBox { IsEnabled = false };

        public YleTextBox CreatedByEmailTextBox { get; private set; } = new YleTextBox { IsEnabled = false };

        public YleDropDown StateDropDown { get; private set; } = new YleDropDown(EnumExtensions.GetEnumDescriptions<UserTaskStatus>());

        public YleDropDown NonLiveTypeDropDown { get; private set; } = new YleDropDown(EnumExtensions.GetEnumDescriptions<Type>()) { IsEnabled = false };

        public YleDropDown UserGroupDropDown { get; private set; } = new YleDropDown(new[] { "None", UserGroup.MessiSpecific.GetDescription(), UserGroup.MediamyllySpecific.GetDescription(), UserGroup.MediaputiikkiSpecific.GetDescription(), UserGroup.UaSpecific.GetDescription() });

        public YleDateTimePicker DeleteDateDatePicker { get; private set; }

        public YleDateTimePicker DeadlineDateDatePicker { get; private set; }

        public YleTextBox DeleteDateCommentTextBox { get; private set; } = new YleTextBox { IsMultiline = true, Height = 100 };

        public YleTextBox FolderPathTextBox { get; private set; } = new YleTextBox { IsEnabled = false, IsMultiline = true, Height = 50 };

        public void GenerateUI()
        {
            int row = -1;

            AddWidget(new Label($"User Task: {nonLiveUserTask.Name}") { Style = TextStyle.Bold }, ++row, 0, 1, 2);

            AddWidget(new WhiteSpace(), ++row, 0);

            AddWidget(generalTitle, ++row, 0);

            AddWidget(nonLiveOrderNameLabel, ++row, 0);
            AddWidget(NonLiveOrderNameTextBox, row, 1);

            AddWidget(linkToNonLiveOrderLabel, ++row, 0);
            AddWidget(LinkToNonLiveOrderTextBox, row, 1);

            AddWidget(nonLiveTypeLabel, ++row, 0);
            AddWidget(NonLiveTypeDropDown, row, 1);

            AddWidget(createdByLabel, ++row, 0);
            AddWidget(CreatedByTextBox, row, 1);

            AddWidget(createdByMailLabel, ++row, 0);
            AddWidget(CreatedByEmailTextBox, row, 1);

            AddWidget(stateLabel, ++row, 0);
            AddWidget(StateDropDown, row, 1);

            AddWidget(userGroupLabel, ++row, 0);
            AddWidget(UserGroupDropDown, row, 1);

            AddWidget(folderPathLabel, ++row, 0);
            AddWidget(FolderPathTextBox, row, 1);

            AddWidget(deleteDateLabel, ++row, 0);
            AddWidget(DeleteDateDatePicker, row, 1);

			AddWidget(deleteCommentLabel, ++row, 0, HorizontalAlignment.Left, VerticalAlignment.Top);
            AddWidget(DeleteDateCommentTextBox, row, 1);

			AddWidget(deadlineDateLabel, ++row, 0);
			AddWidget(DeadlineDateDatePicker, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);

            AddWidget(technicalTitle, ++row, 0);

            AddSection(LinkedUserTaskSection, ++row, 0);
            row += LinkedUserTaskSection.RowCount;

            AddWidget(new WhiteSpace(), ++row, 0);

            AddWidget(ConfirmChangesButton, ++row, 0);
            AddWidget(CloseButton, ++row, 0);
        }

        public void UpdateUserTask()
        {
            nonLiveUserTask.DeleteDate = DeleteDateDatePicker.DateTime;
			nonLiveUserTask.DeadlineDate = DeadlineDateDatePicker.DateTime;
            nonLiveUserTask.DeleteComment = DeleteDateCommentTextBox.Text;

            nonLiveUserTask.Status = EnumExtensions.GetEnumValueFromDescription<UserTaskStatus>(StateDropDown.Selected);
            nonLiveUserTask.UserGroup = EnumExtensions.GetEnumValueFromDescription<UserGroup>(UserGroupDropDown.Selected);

            LinkedUserTaskSection.UpdateUserTask();
        }

        private void Initialize()
        {
            LinkToNonLiveOrderTextBox.Text = nonLiveUserTask.IngestExportForeignKey;
            NonLiveOrderNameTextBox.Text = nonLiveUserTask.OrderName;
            CreatedByTextBox.Text = nonLiveUserTask.OrdererName;
            CreatedByEmailTextBox.Text = nonLiveUserTask.OrdererEmail;
            NonLiveTypeDropDown.Selected = nonLiveUserTask.LinkedOrderType.GetDescription();
            UserGroupDropDown.Selected = nonLiveUserTask.UserGroup.GetDescription();
            DeleteDateDatePicker = new YleDateTimePicker(nonLiveUserTask.DeleteDate) { DateTimeFormat = Automation.DateTimeFormat.ShortDate, ValidationPredicate = dateTime => (dateTime - DateTime.Now.Date) > TimeSpan.FromDays(14), ValidationText = "Delete date need to be set two weeks or more into the future" };
			DeadlineDateDatePicker = new YleDateTimePicker(nonLiveUserTask.DeadlineDate) { DateTimeFormat = Automation.DateTimeFormat.ShortDate, ValidationPredicate = dateTime => dateTime > DateTime.Now.Date, ValidationText = "Deadline needs to be in the future" };
			DeleteDateCommentTextBox.Text = nonLiveUserTask.DeleteComment;
            FolderPathTextBox.Text = nonLiveUserTask.FolderPath;

            InitializeStateDropDown();
            InitializeSubscriptions();
        }

        private void InitializeStateDropDown()
        {
            var options = Enum.GetValues(typeof(UserTaskStatus)).Cast<UserTaskStatus>().Except(new[] { UserTaskStatus.Complete, UserTaskStatus.Incomplete }).ToList();

            if (nonLiveUserTask.LinkedOrderType == Type.IplayFolderCreation)
            {
                options = options.Except(new[] { UserTaskStatus.BackupDeleted, UserTaskStatus.BackupDeleteDateNear }).ToList();
            }
            else
            {
                options = options.Except(new[] { UserTaskStatus.DeletionInProgress, UserTaskStatus.FolderDeleted, UserTaskStatus.DeleteDateNear }).ToList();
            }

            StateDropDown.Options = options.Select(x => x.GetDescription());

            StateDropDown.Selected = nonLiveUserTask.Status.GetDescription();
        }

        private void InitializeSubscriptions()
        {
            StateDropDown.Changed += StateDropDown_Changed;
            DeleteDateDatePicker.Changed += DeleteDateDatePicker_Changed;
        }

		private void CreateTechnicalSection()
        {
            switch (nonLiveUserTask.LinkedOrderType)
            {
                case Type.Import:
                    LinkedUserTaskSection = new ImportUserTaskSection(helpers, (ImportUserTask)nonLiveUserTask);
                    break;
                case Type.IplayFolderCreation:
                    LinkedUserTaskSection = new FolderCreationUserTaskSection(helpers, (IplayFolderCreationUserTask)nonLiveUserTask);
                    break;
                case Type.NonInterplayProject:
                    LinkedUserTaskSection = new ProjectUserTaskSection(helpers, (NonIplayProjectUserTask)nonLiveUserTask);
                    break;
                default:
                    // No action required
                    break;
            }
        }

        private void StateDropDown_Changed(object sender, YleValueWidgetChangedEventArgs e)
        {
            if ((nonLiveUserTask.Status == UserTaskStatus.DeleteDateNear || nonLiveUserTask.Status == UserTaskStatus.BackupDeleteDateNear) && Convert.ToString(e.Value) == UserTaskStatus.Pending.GetDescription())
            {
                DeleteDateDatePicker.DateTime = DateTime.Now;
            }
        }

        private void DeleteDateDatePicker_Changed(object sender, YleValueWidgetChangedEventArgs e)
        {
            bool userTaskShouldHaveStatusPending = (Convert.ToDateTime(e.Value) - DateTime.Now.Date) > TimeSpan.FromDays(14);
			if (userTaskShouldHaveStatusPending)
			{
				StateDropDown.Selected = UserTaskStatus.Pending.GetDescription();
			}
			else if (nonLiveUserTask is IplayFolderCreationUserTask)
			{
				StateDropDown.Selected = UserTaskStatus.DeleteDateNear.GetDescription();
			}
			else if (nonLiveUserTask is ImportUserTask || nonLiveUserTask is NonIplayProjectUserTask)
			{
				StateDropDown.Selected = UserTaskStatus.BackupDeleteDateNear.GetDescription();
			}
		}
    }
}

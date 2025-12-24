namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.UserTasks
{
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.UserTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Text;
	using Skyline.DataMiner.Automation;

    public class ImportUserTaskSection : NonLiveUserTaskSection
    {
        private readonly Label importDestinationLabel = new Label("Import Destination");
        private readonly Label isilonBackupFileLocationLabel = new Label("Isilon Backup File Location");
        private readonly Label deliveryDateLabel = new Label("Delivery Date");
        private readonly Label dateOfCompletionLabel = new Label("Date Of Completion");
        private readonly YleDropDown importDestinationDropDown = new YleDropDown(EnumExtensions.GetEnumDescriptions<InterplayPamElements>());
		private readonly YleTextBox isilonFilePathLocationTextbox = new YleTextBox();
		private readonly YleDateTimePicker deliveryDateDatePicker;
		private readonly YleDateTimePicker dateOfCreationDatePicker;

		private readonly ImportUserTask importUserTask;

		public ImportUserTaskSection(Helpers helpers, ImportUserTask importUserTask) : base(helpers)
        {
            this.importUserTask = importUserTask ?? throw new ArgumentNullException(nameof(importUserTask));

			deliveryDateDatePicker = new YleDateTimePicker(importUserTask.DeliveryDate) { DateTimeFormat = Automation.DateTimeFormat.ShortDate };
			dateOfCreationDatePicker = new YleDateTimePicker(importUserTask.DateOfCompletion) { DateTimeFormat = Automation.DateTimeFormat.ShortDate };

			importDestinationDropDown.Selected = importUserTask.ImportDestination;
			isilonFilePathLocationTextbox.Text = importUserTask.IsilonBackupFileLocation;

            GenerateUI();
        }

		public override void GenerateUI()
        {
            int row = -1;

			AddWidget(deliveryDateLabel, ++row, 0);
			AddWidget(deliveryDateDatePicker, row, 1);

			AddWidget(dateOfCompletionLabel, ++row, 0);
			AddWidget(dateOfCreationDatePicker, row, 1);

			AddWidget(importDestinationLabel, ++row, 0);
            AddWidget(importDestinationDropDown, row, 1);

			AddWidget(isilonBackupFileLocationLabel, ++row, 0);
			AddWidget(isilonFilePathLocationTextbox, row, 1);
        }

        public override void UpdateUserTask()
        {
            importUserTask.ImportDestination = importDestinationDropDown.Selected;
			importUserTask.IsilonBackupFileLocation = isilonFilePathLocationTextbox.Text;

			importUserTask.DeliveryDate = deliveryDateDatePicker.DateTime;
			importUserTask.DateOfCompletion = dateOfCreationDatePicker.DateTime;

			// Update non - live teams filter
			importUserTask.TeamMgmt = false;
            importUserTask.TeamNews = importDestinationDropDown.Selected == InterplayPamElements.UA.GetDescription();
            importUserTask.TeamHki = importDestinationDropDown.Selected == InterplayPamElements.Helsinki.GetDescription();
            importUserTask.TeamTre = importDestinationDropDown.Selected == InterplayPamElements.Tampere.GetDescription();
            importUserTask.TeamVsa = importDestinationDropDown.Selected == InterplayPamElements.Vaasa.GetDescription();

            importUserTask.AddOrUpdate(helpers);
        }
    }
}

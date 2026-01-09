namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.UserTasks
{
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Project;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;

	public class ProjectUserTaskSection : NonLiveUserTaskSection
	{
		private readonly Label productionDepartmentLabel = new Label("Production Department");
		private readonly Label importDepartmentLabel = new Label("Import Department");
		private readonly Label isilonBackupFileLocationLabel = new Label("Isilon Backup File Location");
		private readonly Label deliveryDateLabel = new Label("Delivery Date");
		private readonly Label dateOfCompletionLabel = new Label("Date Of Completion");
		private readonly YleTextBox isilonFilePathLocationTextbox = new YleTextBox();
		private readonly YleDropDown productionDepartmentDropDown = new YleDropDown();
		private readonly YleDropDown importDepartmentDropDown = new YleDropDown(EnumExtensions.GetEnumDescriptions<IngestDepartments>());
		private readonly YleDateTimePicker deliveryDateDatePicker;
		private readonly YleDateTimePicker dateOfCreationDatePicker;

		private readonly NonIplayProjectUserTask nonIplayProjectUserTask;

		public ProjectUserTaskSection(Helpers helpers, NonIplayProjectUserTask nonIplayProjectUserTask) : base(helpers)
		{
			this.nonIplayProjectUserTask = nonIplayProjectUserTask ?? throw new ArgumentNullException(nameof(nonIplayProjectUserTask));

			deliveryDateDatePicker = new YleDateTimePicker(nonIplayProjectUserTask.DeliveryDate) { DateTimeFormat = Automation.DateTimeFormat.ShortDate };
			dateOfCreationDatePicker = new YleDateTimePicker(nonIplayProjectUserTask.DateOfCompletion) { DateTimeFormat = Automation.DateTimeFormat.ShortDate };

			importDepartmentDropDown.Selected = nonIplayProjectUserTask.ImportDepartment;
			isilonFilePathLocationTextbox.Text = nonIplayProjectUserTask.IsilonBackupFileLocation;

			InitializeProductionDepartmenDropDown();
			GenerateUI();
		}
		public override void GenerateUI()
		{
			int row = -1;

			AddWidget(deliveryDateLabel, ++row, 0);
			AddWidget(deliveryDateDatePicker, row, 1);

			AddWidget(dateOfCompletionLabel, ++row, 0);
			AddWidget(dateOfCreationDatePicker, row, 1);

			AddWidget(productionDepartmentLabel, ++row, 0);
			AddWidget(productionDepartmentDropDown, row, 1);

			AddWidget(importDepartmentLabel, ++row, 0);
			AddWidget(importDepartmentDropDown, row, 1);

			AddWidget(isilonBackupFileLocationLabel, ++row, 0);
			AddWidget(isilonFilePathLocationTextbox, row, 1);
		}

		public override void UpdateUserTask()
		{
			nonIplayProjectUserTask.ProductionDepartmentName = productionDepartmentDropDown.Selected;
			nonIplayProjectUserTask.ImportDepartment = importDepartmentDropDown.Selected;
			nonIplayProjectUserTask.IsilonBackupFileLocation = isilonFilePathLocationTextbox.Text;

			nonIplayProjectUserTask.DeliveryDate = deliveryDateDatePicker.DateTime;
			nonIplayProjectUserTask.DateOfCompletion = dateOfCreationDatePicker.DateTime;

			// Update non - live teams filtering
			nonIplayProjectUserTask.TeamMgmt = false;
			nonIplayProjectUserTask.TeamNews = false;
			nonIplayProjectUserTask.TeamHki = importDepartmentDropDown.Selected == IngestDepartments.HELSINKI.GetDescription();
			nonIplayProjectUserTask.TeamTre = importDepartmentDropDown.Selected == IngestDepartments.TAMPERE.GetDescription();
			nonIplayProjectUserTask.TeamVsa = importDepartmentDropDown.Selected == IngestDepartments.VAASA.GetDescription();

			nonIplayProjectUserTask.AddOrUpdate(helpers);
		}

		private void InitializeProductionDepartmenDropDown()
		{
			if (nonIplayProjectUserTask.ImportDepartment == IngestDepartments.HELSINKI.GetDescription())
			{
				productionDepartmentDropDown.Options = Enum.GetValues(typeof(HelsinkiProductionDepartmentNames)).Cast<HelsinkiProductionDepartmentNames>().Select(x => x.GetDescription());
			}
			else if (nonIplayProjectUserTask.ImportDepartment == IngestDepartments.VAASA.GetDescription())
			{
				productionDepartmentDropDown.Options = Enum.GetValues(typeof(VaasaProductionDepartmentNames)).Cast<VaasaProductionDepartmentNames>().Select(x => x.GetDescription());
			}
			else
			{
				productionDepartmentDropDown.Options = Enum.GetValues(typeof(TampereProductionDepartmentNames)).Cast<TampereProductionDepartmentNames>().Select(x => x.GetDescription());
			}

			productionDepartmentDropDown.Selected = nonIplayProjectUserTask.ProductionDepartmentName;
		}
	}
}

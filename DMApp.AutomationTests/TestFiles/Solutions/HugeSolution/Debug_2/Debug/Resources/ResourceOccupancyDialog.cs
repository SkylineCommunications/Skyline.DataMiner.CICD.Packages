namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Resources
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class ResourceOccupancyDialog : Dialog
	{
		private readonly Helpers helpers;

		private readonly Label resourceSelectionLabel = new Label("Resource Selection");
		private readonly RadioButtonList resourceSelectionMethodRadioButtonList = new RadioButtonList(new[] {"ID", "Selection"}, "Selection");

		private readonly Label resourcePoolLabel = new Label("Resource Pool");
		private DropDown resourcePoolDropDown;

		private readonly Label resourceLabel = new Label("Resource");
		private DropDown resourceDropDown;

		private readonly Label resourceIdLabel = new Label("Resource ID") { IsVisible = false };
		private readonly TextBox resourceIdTextBox = new TextBox() { IsVisible = false };
		private readonly Button enterCurrentIdButton = new Button("Enter Current ID") { Width = 200, IsVisible = false };

		private readonly Label startTimeLabel = new Label("Start Time");
		private readonly DateTimePicker startTimeDateTimePicker = new DateTimePicker(DateTime.Now);

		private readonly Label endTimeLabel = new Label("End Time");
		private readonly DateTimePicker endTimeDateTimePicker = new DateTimePicker(DateTime.Now.AddHours(1));

		private readonly Label timeZoneLabel = new Label($"Times are displayed here in the time zone of the client.");
		private readonly Label timeZone2Label = new Label($"Times in Resource Occupancy page in Cube are displayed in the time zone of the server.");


		private readonly Button getResourceOccupancyButton = new Button("Get Resource Occupancy"){Width = 200};

		private readonly List<RequestResultSection> resultSections = new List<RequestResultSection>();

		private ResourcePool[] resourcePools;
		private Resource[] resources;

		public ResourceOccupancyDialog(Helpers helpers) : base(helpers.Engine)
		{
			this.helpers = helpers;

			Title = "Resource Occupancy";

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back..."){Width = 150};

		private ResourcePool SelectedResourcePool => resourcePools.SingleOrDefault(pool => pool.Name == resourcePoolDropDown.Selected);

		private Resource SelectedResource => resources.SingleOrDefault(r => r.Name == resourceDropDown.Selected);

		private void Initialize()
		{
			resourceSelectionMethodRadioButtonList.Changed += ResourceSelectionMethodRadioButtonList_Changed;

			enterCurrentIdButton.Pressed += (sender, args) => resourceIdTextBox.Text = helpers.Engine.GetScriptParam(1)?.Value;

			resourcePools = helpers.ResourceManager.GetResourcePools();
			var resourcePoolOptions = resourcePools.Select(p => p.Name).OrderBy(name => name).ToList();
			resourcePoolDropDown = new DropDown(resourcePoolOptions, resourcePoolOptions[0]) { IsDisplayFilterShown = true };
			resourcePoolDropDown.Changed += ResourcePoolDropDown_Changed;

			resources = DataMinerInterface.ResourceManager.GetResources(helpers, ResourceExposers.PoolGUIDs.Contains(SelectedResourcePool.GUID)).ToArray();
			var resourceOptions = resources.Select(r => r.Name).OrderBy(name => name).ToList();
			resourceDropDown = new DropDown(resourceOptions, resourceOptions.FirstOrDefault()) { IsDisplayFilterShown = true };

			getResourceOccupancyButton.Pressed += GetResourceOccupancyButton_Pressed;
		}

		private void ResourceSelectionMethodRadioButtonList_Changed(object sender, RadioButtonList.RadioButtonChangedEventArgs e)
		{
			resourcePoolLabel.IsVisible = e.SelectedValue == "Selection";
			resourcePoolDropDown.IsVisible = resourcePoolLabel.IsVisible;
			resourceLabel.IsVisible = resourcePoolLabel.IsVisible;
			resourceDropDown.IsVisible = resourcePoolLabel.IsVisible;

			resourceIdLabel.IsVisible = e.SelectedValue == "ID";
			resourceIdTextBox.IsVisible = resourceIdLabel.IsVisible;
			enterCurrentIdButton.IsVisible = resourceIdLabel.IsVisible;
		}

		private void ResourcePoolDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			resources = DataMinerInterface.ResourceManager.GetResources(helpers, ResourceExposers.PoolGUIDs.Contains(SelectedResourcePool.GUID)).ToArray();
			resourceDropDown.Options = resources.Select(r => r.Name).OrderBy(name => name).ToList();
		}

		private void GetResourceOccupancyButton_Pressed(object sender, EventArgs e)
		{
			Guid resourceId;
			string resourceName;
			if (resourceSelectionMethodRadioButtonList.Selected == "Selection")
			{
				resourceId = SelectedResource.ID;
				resourceName = SelectedResource.Name;
			}
			else if(!Guid.TryParse(resourceIdTextBox.Text, out resourceId))
			{
				ShowRequestResult("Invalid GUID", string.Empty);
				return;
			}
			else
			{
				resourceName = DataMinerInterface.ResourceManager.GetResource(helpers, resourceId)?.Name;
			}

			var start = startTimeDateTimePicker.DateTime.ToUniversalTime();
			var end = endTimeDateTimePicker.DateTime.ToUniversalTime();

			var resourceFilter = ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(resourceId);
			var startFilter = ReservationInstanceExposers.Start.LessThanOrEqual(end);
			var endFilter = ReservationInstanceExposers.End.GreaterThanOrEqual(start);

			var occupyingServices = new List<OccupyingService>();

			var services = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, new ANDFilterElement<ReservationInstance>(resourceFilter, startFilter, endFilter));

			foreach (var service in services)
			{
				var contributingResourceFilter = ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(service.ID);

				var orders = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, new ANDFilterElement<ReservationInstance>(contributingResourceFilter)).ToList();

				occupyingServices.Add(new OccupyingService(service, orders));
			}

			ShowRequestResult($"Reservations occupying '{resourceName}' from {start} ({start.Kind}) until {end} ({end.Kind})", string.Join("\n", occupyingServices.Select(o => o.ToString())));
		}

		private void ShowRequestResult(string header, params string[] results)
		{
			resultSections.Add(new RequestResultSection(header, results));

			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 5);
			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(resourceSelectionLabel, ++row, 0);
			AddWidget(resourceSelectionMethodRadioButtonList, row, 1);

			AddWidget(resourceIdLabel, ++row, 0);
			AddWidget(resourceIdTextBox, row, 1);
			AddWidget(enterCurrentIdButton, ++row, 1);

			AddWidget(resourcePoolLabel, ++row, 0);
			AddWidget(resourcePoolDropDown, row, 1);

			AddWidget(resourceLabel, ++row, 0);
			AddWidget(resourceDropDown, row, 1);

			AddWidget(startTimeLabel, ++row, 0);
			AddWidget(startTimeDateTimePicker, row, 1);

			AddWidget(endTimeLabel, ++row, 0);
			AddWidget(endTimeDateTimePicker, row, 1);

			AddWidget(timeZoneLabel, ++row, 1);
			AddWidget(timeZone2Label, ++row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(getResourceOccupancyButton, ++row, 0, 1, 5);
			AddWidget(new WhiteSpace(), ++row, 0);

			foreach (var resultSection in resultSections)
			{
				AddSection(resultSection,++row, 0);
				row += resultSection.RowCount;
			}
		}
	}
}

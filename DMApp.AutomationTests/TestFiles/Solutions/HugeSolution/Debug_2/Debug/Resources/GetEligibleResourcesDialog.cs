namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Resources
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class GetEligibleResourcesDialog : Dialog
	{
		private readonly Helpers helpers;

		private readonly Label startLabel = new Label("Start");
		private readonly DateTimePicker startDateTimePicker = new DateTimePicker(DateTime.Now);

		private readonly Label endLabel = new Label("End");
		private readonly DateTimePicker endDateTimePicker = new DateTimePicker(DateTime.Now.AddMinutes(60));
		private readonly Label timeZoneLabel = new Label($"Times are displayed here in the time zone of the client.");
		private readonly Label timeZone2Label = new Label($"Times in Resource Occupancy page in Cube are displayed in the time zone of the server.");

		private readonly Label resourcePoolLabel = new Label("Resource Pool");
		private DropDown resourcePoolDropDown;

		private readonly CheckBox reservationIdToIgnoreCheckBox = new CheckBox("Reservation ID to Ignore");
		private readonly TextBox reservationIdToIgnoreTextBox = new TextBox(string.Empty);

		private readonly CheckBox nodeIdToIgnoreCheckBox = new CheckBox("Node ID to Ignore");
		private readonly Numeric nodeIdToIgnoreNumeric = new Numeric(0) { Decimals = 0, Minimum = 0, StepSize = 1 };

		private readonly Button getEligibleResourcesButton = new Button("Get Eligible Resources");


		private readonly List<RequestResultSection> resultSections = new List<RequestResultSection>();

		private ResourcePool[] resourcePools;

		public GetEligibleResourcesDialog(Helpers helpers) : base(helpers.Engine)
		{
			Title = "Get Eligible Resources";

			this.helpers = helpers;

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		private ResourcePool SelectedResourcePool => resourcePools.SingleOrDefault(pool => pool.Name == resourcePoolDropDown.Selected);

		private void Initialize()
		{
			resourcePools = helpers.ResourceManager.GetResourcePools();
			var resourcePoolOptions = resourcePools.Select(p => p.Name).OrderBy(name => name).ToList();
			resourcePoolDropDown = new DropDown(resourcePoolOptions, resourcePoolOptions[0]) { IsDisplayFilterShown = true };

			reservationIdToIgnoreCheckBox.Changed += (s, e) => nodeIdToIgnoreCheckBox.IsChecked = e.IsChecked;
			nodeIdToIgnoreCheckBox.Changed += (s, e) => reservationIdToIgnoreCheckBox.IsChecked = e.IsChecked;

			getEligibleResourcesButton.Pressed += GetEligibleResourcesButton_Pressed;
		}

		private void GetEligibleResourcesButton_Pressed(object sender, EventArgs e)
		{
			var context = new EligibleResourceContext
			{
				TimeRange = new Net.Time.TimeRangeUtc(startDateTimePicker.DateTime, endDateTimePicker.DateTime, TimeZoneInfo.Utc),
				ResourceFilter = ResourceExposers.PoolGUIDs.Contains(SelectedResourcePool.GUID),
			};

			bool ignoreReservation = Guid.TryParse(reservationIdToIgnoreTextBox.Text, out var parsedGuid) && reservationIdToIgnoreCheckBox.IsChecked;
			if (ignoreReservation)
			{
				context.ReservationIdToIgnore = new Net.ReservationInstanceID(parsedGuid);
				context.NodeIdToIgnore = (int)nodeIdToIgnoreNumeric.Value;
			}

			var eligibleResources = DataMinerInterface.ResourceManager.GetEligibleResources(helpers, context);

			if (eligibleResources?.EligibleResources == null)
			{
				ShowRequestResult($"Eligible Resources for {SelectedResourcePool.Name} from {context.TimeRange.Start} (UTC) until {context.TimeRange.Stop} (UTC) {(ignoreReservation ? $"ignoring reservation {parsedGuid} node {(int)nodeIdToIgnoreNumeric.Value}" : String.Empty)}", "Empty response");
			}
			else
			{
				ShowRequestResult($"Eligible Resources for {SelectedResourcePool.Name} from {context.TimeRange.Start} (UTC) until {context.TimeRange.Stop} (UTC) {(ignoreReservation ? $"ignoring reservation {parsedGuid} node {(int)nodeIdToIgnoreNumeric.Value}" : String.Empty)}", string.Join("\n", eligibleResources.EligibleResources.Select(p => p.Name).OrderBy(n => n)));
			}
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

			AddWidget(startLabel, ++row, 0);
			AddWidget(startDateTimePicker, row, 1);

			AddWidget(endLabel, ++row, 0);
			AddWidget(endDateTimePicker, row, 1);

			AddWidget(timeZoneLabel, ++row, 1);
			AddWidget(timeZone2Label, ++row, 1);

			AddWidget(resourcePoolLabel, ++row, 0);
			AddWidget(resourcePoolDropDown, row, 1);

			AddWidget(reservationIdToIgnoreCheckBox, ++row, 1);
			AddWidget(reservationIdToIgnoreTextBox, row, 2);

			AddWidget(nodeIdToIgnoreCheckBox, ++row, 1);
			AddWidget(nodeIdToIgnoreNumeric, row, 2);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(getEligibleResourcesButton, ++row, 0, 1, 5);

			foreach (var resultSection in resultSections)
			{
				AddSection(resultSection, ++row, 0);
				row += resultSection.RowCount;
			}
		}
	}
}

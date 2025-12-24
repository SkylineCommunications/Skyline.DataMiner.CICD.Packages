namespace DeleteReservationsForCertainResource_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Library.Solutions.SRM.Helpers;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class DeleteReservationsFromResourceDialog : Dialog
	{
		private Resource[] currentFoundResourcesForSpecificPool;
		private readonly ResourcePool[] resourcePools;

		private readonly Engine engine;
		private readonly ResourceManagerHelper resourceManagerHelper;

		private readonly Label selectResourcePoolLabel = new Label("Select Resource Pool:");
		private readonly Label selectResourceLabel = new Label("Select Resource:");
		private readonly Label startTimeLabel = new Label("Start Time:");
		private readonly Label endTimeLabel = new Label("End Time:");
		private readonly Label linkedReservationsLabel = new Label("Linked Reservations:") { IsVisible = false };

		public DeleteReservationsFromResourceDialog(Engine engine) : base(engine)
		{
			this.engine = engine;

			resourceManagerHelper = new ResourceManagerHelperExtended();
			resourceManagerHelper.RequestResponseEvent += (s, e) => e.responseMessage = Skyline.DataMiner.Automation.Engine.SLNet.SendSingleResponseMessage(e.requestMessage);

			resourcePools = resourceManagerHelper.GetResourcePools();

			InitializeWidgets();
			GetSpecificResourcesBasedOnSelectedResourcePool();
			GenerateUi();
		}

		public DropDown ResourcePoolDropDown { get; private set; }

		public DropDown ResourcesDropDown { get; private set; }

		public DateTimePicker StartTimeDateTimePicker { get; private set; }

		public DateTimePicker EndTimeDateTimePicker { get; private set; }

		public CheckBoxList LinkedReservationsCheckBoxList { get; private set; }

		public Button DeleteSelectedReservationsButton { get; private set; }

		public List<ReservationInstance> CurrentFoundReservationInstances { get; private set; }

		public Resource CurrentSelectedResource => currentFoundResourcesForSpecificPool.Single(r => r.Name == ResourcesDropDown.Selected);

		public void RemoveSelectedReservationInstances()
		{
			try
			{
				var reservationsThatAreChecked = LinkedReservationsCheckBoxList.Checked.ToList();
				var reservationsToDelete = CurrentFoundReservationInstances.Where(r => r != null && reservationsThatAreChecked.Contains(r.Name)).ToArray();

				resourceManagerHelper.RemoveReservationInstances(reservationsToDelete);
			}
			catch (Exception ex)
			{
				engine.Log($"{nameof(DeleteReservationsFromResourceDialog)} | {nameof(GetSpecificResourcesBasedOnSelectedResourcePool)}| Something went wrong durin removal of the selected reservation instances" + ex.Message);
			}
		}

		private void InitializeWidgets()
		{
			this.Title = "Release or Delete Reservation";

			ResourcePoolDropDown = new DropDown();
			ResourcePoolDropDown.Options = resourcePools?.Select(r => r.Name).OrderBy(n => n);
			ResourcePoolDropDown.Selected = ResourcePoolDropDown.Options.First();

			ResourcesDropDown = new DropDown();

			StartTimeDateTimePicker = new DateTimePicker(DateTime.Now) { IsEnabled = false };
			EndTimeDateTimePicker = new DateTimePicker(DateTime.Now.AddMinutes(30)) { IsEnabled = false };

			LinkedReservationsCheckBoxList = new CheckBoxList { IsVisible = false, IsSorted = true };

			DeleteSelectedReservationsButton = new Button("Delete Reservations") { Width = 150, IsEnabled = false };

			// Handlers
			ResourcePoolDropDown.Changed += ResourcePoolDropDown_Changed;
			ResourcesDropDown.Changed += ResourcesDropDown_Changed;
			LinkedReservationsCheckBoxList.Changed += LinkedReservationsCheckBoxList_Changed;
			StartTimeDateTimePicker.Changed += StartTimeDateTimePicker_Changed;
			EndTimeDateTimePicker.Changed += EndTimeDateTimePicker_Changed;
		}

		private void InitializeReservationCheckBoxList(bool showReservationSelection)
		{
			LinkedReservationsCheckBoxList.SetOptions(CurrentFoundReservationInstances.Select(r => r?.Name));
			LinkedReservationsCheckBoxList.IsVisible = showReservationSelection;
			linkedReservationsLabel.IsVisible = showReservationSelection;
		}

		private void GenerateUi()
		{
			int row = -1;

			AddWidget(selectResourcePoolLabel, ++row, 0);
			AddWidget(ResourcePoolDropDown, row, 1);

			AddWidget(selectResourceLabel, ++row, 0);
			AddWidget(ResourcesDropDown, row, 1);

			AddWidget(startTimeLabel, ++row, 0);
			AddWidget(StartTimeDateTimePicker, row, 1);

			AddWidget(endTimeLabel, ++row, 0);
			AddWidget(EndTimeDateTimePicker, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(linkedReservationsLabel, ++row, 0, HorizontalAlignment.Left, VerticalAlignment.Top);
			AddWidget(LinkedReservationsCheckBoxList, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(DeleteSelectedReservationsButton, row + 1, 0);

			SetColumnWidth(0, 150);
			SetColumnWidth(1, 300);
		}

		private void GetSpecificResourcesBasedOnSelectedResourcePool()
		{
			try
			{
				var selectedResourcePool = resourcePools?.Single(r => r.Name == ResourcePoolDropDown.Selected);
				if (selectedResourcePool == null) throw new InvalidOperationException(nameof(selectedResourcePool));

				currentFoundResourcesForSpecificPool = resourceManagerHelper.GetResources(ResourceExposers.PoolGUIDs.Contains(selectedResourcePool.GUID));

				if (currentFoundResourcesForSpecificPool.Any())
				{
					engine.Log($"{nameof(DeleteReservationsFromResourceDialog)} | {nameof(GetSpecificResourcesBasedOnSelectedResourcePool)} | Currently found resources {String.Join(";", currentFoundResourcesForSpecificPool.Select(r => r.ToString()))} for resource pool: {selectedResourcePool}");

					ResourcesDropDown.Options = currentFoundResourcesForSpecificPool?.Select(r => r.Name).OrderBy(x => x);
					ResourcesDropDown.Selected = ResourcesDropDown.Options?.First();
				
					UpdateWidgetVisibility();
					GetReservationInstancesWithSpecificResourceAndTimeSpan(CurrentSelectedResource, StartTimeDateTimePicker.DateTime, EndTimeDateTimePicker.DateTime);
				}
			}
			catch (Exception ex)
			{
				engine.Log($"{nameof(DeleteReservationsFromResourceDialog)} | {nameof(GetSpecificResourcesBasedOnSelectedResourcePool)} |Failure while getting resources: " + ex.Message);
			}
		}

		private void GetReservationInstancesWithSpecificResourceAndTimeSpan(Resource resource, DateTime start, DateTime end)
		{
			if (resource == null) throw new ArgumentNullException(nameof(resource));
			if (resource.ID == Guid.Empty) throw new ArgumentException("Resource ID is empty GUID", nameof(resource));

			if (end < start) throw new ArgumentException("End date is before start date", nameof(end));

			var resourceFilter = ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(resource.ID);
			var startFilter = ReservationInstanceExposers.Start.LessThanOrEqual(end);
			var endFilter = ReservationInstanceExposers.End.GreaterThanOrEqual(start);

			CurrentFoundReservationInstances = resourceManagerHelper.GetReservationInstances(new ANDFilterElement<ReservationInstance>(resourceFilter, startFilter, endFilter)).ToList();

			if (CurrentFoundReservationInstances != null && CurrentFoundReservationInstances.Any())
			{
				engine.Log($"{nameof(DeleteReservationsFromResourceDialog)} | {nameof(GetReservationInstancesWithSpecificResourceAndTimeSpan)} | Currently found reservation instances based on resource selection and timing {String.Join(";", CurrentFoundReservationInstances)}, Timing Start = {start}, Timing End = {end}, Resource name = {resource.Name}");

				InitializeReservationCheckBoxList(CurrentFoundReservationInstances.Any());
			}
		}

		private void ResourcePoolDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			GetSpecificResourcesBasedOnSelectedResourcePool();
		}

		private void ResourcesDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			GetReservationInstancesWithSpecificResourceAndTimeSpan(CurrentSelectedResource, StartTimeDateTimePicker.DateTime, EndTimeDateTimePicker.DateTime);
		}

		private void EndTimeDateTimePicker_Changed(object sender, DateTimePicker.DateTimePickerChangedEventArgs e)
		{
			if (EndTimeDateTimePicker.DateTime <= StartTimeDateTimePicker.DateTime)
			{
				StartTimeDateTimePicker.DateTime = EndTimeDateTimePicker.DateTime.AddMinutes(-30);
			}

			GetReservationInstancesWithSpecificResourceAndTimeSpan(CurrentSelectedResource, StartTimeDateTimePicker.DateTime, e.DateTime);
		}

		private void StartTimeDateTimePicker_Changed(object sender, DateTimePicker.DateTimePickerChangedEventArgs e)
		{
			if (StartTimeDateTimePicker.DateTime >= EndTimeDateTimePicker.DateTime)
			{
				EndTimeDateTimePicker.DateTime = StartTimeDateTimePicker.DateTime.AddMinutes(30);
			}

			GetReservationInstancesWithSpecificResourceAndTimeSpan(CurrentSelectedResource, e.DateTime, EndTimeDateTimePicker.DateTime);
		}

		private void LinkedReservationsCheckBoxList_Changed(object sender, CheckBoxList.CheckBoxListChangedEventArgs e)
		{
			UpdateWidgetVisibility();
		}

		private void UpdateWidgetVisibility()
		{
			StartTimeDateTimePicker.IsEnabled = !String.IsNullOrEmpty(ResourcesDropDown.Selected);
			EndTimeDateTimePicker.IsEnabled = StartTimeDateTimePicker.IsEnabled;

			DeleteSelectedReservationsButton.IsEnabled = LinkedReservationsCheckBoxList.Checked.Any();
		}
	}
}
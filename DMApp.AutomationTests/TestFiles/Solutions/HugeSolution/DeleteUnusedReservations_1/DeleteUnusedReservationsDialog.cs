namespace DeleteUnusedReservations_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;

	public class DeleteUnusedReservationsDialog : Dialog, IDisposable
	{
		private readonly Engine engine;
		private readonly Helpers helpers;
		
		private readonly Label startTimeLabel = new Label("Start Time:");
		private readonly Label endTimeLabel = new Label("End Time:");
		private readonly Label linkedReservationsLabel = new Label("Linked Reservations:") { IsVisible = false };
		private bool disposedValue;
		private const string OrderReferencesPropertyName = "OrderIds";

		public DeleteUnusedReservationsDialog(Engine engine, Helpers helpers) : base(engine)
		{
			this.engine = engine;
			this.helpers = helpers;

			InitializeWidgets();
			GetReservationInstancesForSpecificTimeSpan(StartTimeDateTimePicker.DateTime, EndTimeDateTimePicker.DateTime);
			GenerateUi();
		}

		public DateTimePicker StartTimeDateTimePicker { get; private set; }

		public DateTimePicker EndTimeDateTimePicker { get; private set; }

		public CheckBoxList LinkedReservationsCheckBoxList { get; private set; }

		public Button DeleteSelectedReservationsButton { get; private set; }

		private List<ReservationInstance> CurrentFoundReservationInstances { get; set; }

		public List<ReservationInstance> ReservationsToRemove { get; private set; } = new List<ReservationInstance>();

		private void InitializeWidgets()
		{
			this.Title = "Delete Unused Reservations";

			StartTimeDateTimePicker = new DateTimePicker(DateTime.Now);
			EndTimeDateTimePicker = new DateTimePicker(DateTime.Now.AddMinutes(30));

			LinkedReservationsCheckBoxList = new CheckBoxList { IsVisible = false, IsSorted = true };

			DeleteSelectedReservationsButton = new Button("Delete Reservations") { Width = 150, IsEnabled = false };

			// Handlers
			LinkedReservationsCheckBoxList.Changed += LinkedReservationsCheckBoxList_Changed;
			StartTimeDateTimePicker.Changed += StartTimeDateTimePicker_Changed;
			EndTimeDateTimePicker.Changed += EndTimeDateTimePicker_Changed;
		}

		private void InitializeReservationCheckBoxList(bool showReservationSelection)
		{
			LinkedReservationsCheckBoxList.SetOptions(ReservationsToRemove.Select(x => x.Name));
			LinkedReservationsCheckBoxList.IsVisible = showReservationSelection;
			linkedReservationsLabel.IsVisible = showReservationSelection;
		}

		private void GenerateUi()
		{
			int row = -1;

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

		private void GetReservationInstancesForSpecificTimeSpan(DateTime start, DateTime end)
		{
			if (end < start) throw new ArgumentException("End date is before start date", nameof(end));

			try
			{
				var startFilter = ReservationInstanceExposers.Start.LessThanOrEqual(end);
				var endFilter = ReservationInstanceExposers.End.GreaterThanOrEqual(start);

				CurrentFoundReservationInstances = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, new ANDFilterElement<ReservationInstance>(startFilter, endFilter)).ToList();

				List<Service> ConvertedServiceReservations = new List<Service>();
				foreach (var reservationInstance in CurrentFoundReservationInstances)
				{
					if (reservationInstance != null) ConvertedServiceReservations.Add(FromReservationInstance(reservationInstance));
				}

				// filter out order reservation instances
				ConvertedServiceReservations = ConvertedServiceReservations.Where(c => c != null && c.OrderReferences.Any()).ToList();

				HandleReservationsToDelete(ConvertedServiceReservations);
				InitializeReservationCheckBoxList(ReservationsToRemove.Any());
			}
			catch (Exception ex)
			{
				engine.Log("GetReservationInstancesForSpecificTimeSpan|Something went wrong while converting into service contributings " + ex.Message);
			}
		}

		private Service FromReservationInstance(ReservationInstance reservationInstance)
		{
			if (reservationInstance == null) throw new ArgumentNullException(nameof(reservationInstance));

			var booking = reservationInstance.GetBookingData();

			var service = new DisplayedService(booking.Description)
			{
				UserTasks = new List<LiveUserTask>(),
				Id = reservationInstance.ID,
			};

			TimeSpan preRoll = reservationInstance.GetPreRoll();
			TimeSpan postRoll = reservationInstance.GetPostRoll();

			DateTime convertedStartTime = reservationInstance.Start.FromReservation().Truncate(TimeSpan.FromSeconds(1)).Truncate(TimeSpan.FromMilliseconds(1));
			DateTime convertedEndTime = reservationInstance.End.FromReservation().Truncate(TimeSpan.FromSeconds(1)).Truncate(TimeSpan.FromMilliseconds(1));

			service.Start = convertedStartTime.Add(preRoll);
			service.End = convertedEndTime.Subtract(postRoll);

			service.IsBooked = true;

			service.OrderReferences = GetOrderReferences(reservationInstance);

			return service;
		}

		private static HashSet<Guid> GetOrderReferences(ReservationInstance reservation)
		{
			var orders = new HashSet<Guid>();

			var orderIdsProperty = reservation.Properties.FirstOrDefault(p => String.Equals(p.Key, OrderReferencesPropertyName, StringComparison.InvariantCultureIgnoreCase));
			if (orderIdsProperty.Equals(default(KeyValuePair<string, object>)) || Convert.ToString(orderIdsProperty.Value) == String.Empty)
			{
				return orders;
			}

			try
			{
				var orderIds = Convert.ToString(orderIdsProperty.Value).Split(';').Select(id => Guid.Parse(id));
				foreach (var orderId in orderIds)
				{
					if (orderId == Guid.Empty) continue;

					orders.Add(orderId);
				}
			}
			catch (Exception)
			{
				return orders;
			}

			return orders;
		}

		private void HandleReservationsToDelete(List<Service> ConvertedServiceReservations)
		{
			bool allOrderReferencesOfReservationDontExist;
			foreach (var reservation in ConvertedServiceReservations)
			{
				allOrderReferencesOfReservationDontExist = true;
				foreach (var linkedOrderId in reservation.OrderReferences)
				{
					if (DataMinerInterface.ResourceManager.GetReservationInstance(helpers, linkedOrderId) != null)
					{
						allOrderReferencesOfReservationDontExist = false;
						break;
					}
				}

				if (allOrderReferencesOfReservationDontExist)
				{
					var reservationToDelete = CurrentFoundReservationInstances.Single(r => r.ID == reservation.Id);
					ReservationsToRemove.Add(reservationToDelete);
				}
			}
		}

		private void EndTimeDateTimePicker_Changed(object sender, DateTimePicker.DateTimePickerChangedEventArgs e)
		{
			if (EndTimeDateTimePicker.DateTime <= StartTimeDateTimePicker.DateTime)
			{
				StartTimeDateTimePicker.DateTime = EndTimeDateTimePicker.DateTime.AddMinutes(-30);
			}

			GetReservationInstancesForSpecificTimeSpan(StartTimeDateTimePicker.DateTime, e.DateTime);
		}

		private void StartTimeDateTimePicker_Changed(object sender, DateTimePicker.DateTimePickerChangedEventArgs e)
		{
			if (StartTimeDateTimePicker.DateTime >= EndTimeDateTimePicker.DateTime)
			{
				EndTimeDateTimePicker.DateTime = StartTimeDateTimePicker.DateTime.AddMinutes(30);
			}

			GetReservationInstancesForSpecificTimeSpan(e.DateTime, EndTimeDateTimePicker.DateTime);
		}

		private void LinkedReservationsCheckBoxList_Changed(object sender, CheckBoxList.CheckBoxListChangedEventArgs e)
		{
			UpdateWidgetVisibility();
		}

		private void UpdateWidgetVisibility()
		{ 
			DeleteSelectedReservationsButton.IsEnabled = LinkedReservationsCheckBoxList.Checked.Any();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					helpers.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
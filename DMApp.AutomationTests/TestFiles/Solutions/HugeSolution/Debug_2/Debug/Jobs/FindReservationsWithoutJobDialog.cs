namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Jobs
{
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class FindReservationsWithoutJobDialog : Dialog
	{
		private readonly Helpers helpers;
		private readonly JobManagerHelper jobManager;
		private readonly Button FindReservations = new Button("Find Reservations");
		private static readonly string description = "This will search all video orders on the system and check if the job id is filled out and the job is available.";
		private readonly List<ReservationWithoutJob> reservationInstancesWithoutJob = new List<ReservationWithoutJob>();
		private bool hasRun = false;

		public FindReservationsWithoutJobDialog(Helpers helpers) : base(helpers.Engine)
		{
			this.helpers = helpers;
			jobManager = new JobManagerHelper(m => helpers.Engine.SendSLNetMessages(m));

			Title = "Find Reservations Without Jobs";

			GenerateUI();
			FindReservations.Pressed += FindReservations_Pressed;
			FixAllButton.Pressed += FixAllButton_Pressed;
		}

		private void GenerateUI()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 4);
			AddWidget(new WhiteSpace(), ++row, 0, 1, 4);

			AddWidget(FindReservations, ++row, 0, 1, 4);
			AddWidget(new Label(description), ++row, 0, 1, 4);

			if (!hasRun) return;
			AddWidget(new WhiteSpace(), ++row, 0, 1, 4);
			if (reservationInstancesWithoutJob.Any())
			{
				AddWidget(new Label("Reservation ID") { Style = TextStyle.Heading }, ++row, 0);
				AddWidget(new Label("Reservation Name") { Style = TextStyle.Heading }, row, 1);
				AddWidget(new Label("Reason") { Style = TextStyle.Heading }, row, 2);
				AddWidget(FixAllButton, row, 3);

				foreach (var reservation in reservationInstancesWithoutJob)
				{
					AddWidget(new Label(reservation.ReservationInstance.ID.ToString()), ++row, 0);
					AddWidget(new Label(reservation.ReservationInstance.Name), row, 1);
					AddWidget(new Label(reservation.Reason), row, 2);
					AddWidget(reservation.FixButton, row, 3);
				}
			}
			else
			{
				AddWidget(new Label("No Reservations without job"), ++row, 0, 1, 4);
			}
		}

		private void FindReservations_Pressed(object sender, EventArgs e)
		{
			var reservations = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, ReservationInstanceExposers.Properties.DictStringField("Type").Equal("Video"));
			foreach (var reservation in reservations)
			{
				ReservationWithoutJob reservationWithoutJob;
				if (!reservation.Properties.ContainsKey(LiteOrder.PropertyNameEventId))
				{
					reservationWithoutJob = new ReservationWithoutJob 
					{
						ReservationInstance = reservation,
						Reason = "No event ID property",
						EventId = String.Empty
					};

					reservationWithoutJob.FixButton.Pressed += FixButton_Pressed;
					reservationInstancesWithoutJob.Add(reservationWithoutJob);
					continue;
				}

				string rawEventId = Convert.ToString(reservation.Properties.Dictionary[LiteOrder.PropertyNameEventId]);
				if (String.IsNullOrWhiteSpace(rawEventId))
				{
					reservationWithoutJob = new ReservationWithoutJob 
					{
						ReservationInstance = reservation,
						Reason = "Empty event ID property",
						EventId = rawEventId
					};

					reservationWithoutJob.FixButton.Pressed += FixButton_Pressed;
					reservationInstancesWithoutJob.Add(reservationWithoutJob);
					continue;
				}

				Guid eventId;
				if (!Guid.TryParse(rawEventId, out eventId))
				{
					reservationWithoutJob = new ReservationWithoutJob 
					{
						ReservationInstance = reservation,
						Reason = "Invalid event ID property",
						EventId = rawEventId
					};

					reservationWithoutJob.FixButton.Pressed += FixButton_Pressed;
					reservationInstancesWithoutJob.Add(reservationWithoutJob);
					continue;
				}

				var job = jobManager.Jobs.Read(JobExposers.ID.Equal(eventId)).FirstOrDefault();
				if (job == null)
				{
					reservationWithoutJob = new ReservationWithoutJob 
					{
						ReservationInstance = reservation,
						Reason = "Unable to find job",
						EventId = rawEventId
					};

					reservationWithoutJob.FixButton.Pressed += FixButton_Pressed;
					reservationInstancesWithoutJob.Add(reservationWithoutJob);
				}
			}

			hasRun = true;
			GenerateUI();
		}

		private void FixButton_Pressed(object sender, EventArgs e)
		{
			var reservationInstanceWithoutJob = reservationInstancesWithoutJob.FirstOrDefault(x => x.FixButton.Equals(sender));
			if (reservationInstancesWithoutJob == null) return;

			if (!reservationInstanceWithoutJob.Fix(helpers)) return;

			reservationInstancesWithoutJob.Remove(reservationInstanceWithoutJob);
			GenerateUI();
		}

		private void FixAllButton_Pressed(object sender, EventArgs e)
		{
			List<ReservationWithoutJob> fixedOrders = new List<ReservationWithoutJob>();
			foreach (var reservationWithoutJob in reservationInstancesWithoutJob)
			{
				if (!reservationWithoutJob.Fix(helpers)) continue;
				fixedOrders.Add(reservationWithoutJob);
			}

			reservationInstancesWithoutJob.RemoveAll(x => fixedOrders.Contains(x));
			GenerateUI();
		}

		public Button BackButton { get; private set; } = new Button("Back...");

		private Button FixAllButton { get; } = new Button("Fix All");
	}

	class ReservationWithoutJob
	{
		public ReservationInstance ReservationInstance { get; set; }

		public string Reason { get; set; }

		public string EventId { get; set; }

		public Button FixButton { get; } = new Button("Fix");

		public bool Fix(Helpers helpers)
		{
			try
			{
				helpers.Log(nameof(ReservationWithoutJob), nameof(Fix), $"Fixing event for reservationInstance with ID {ReservationInstance.ID}...");
				LiteOrder order = helpers.OrderManager.GetLiteOrder(ReservationInstance);
				order.Event = new Event.Event(helpers, order);

				helpers.EventManager.AddOrUpdateEvent(order.Event);
				order.UpdateEventReference(helpers);

				helpers.Log(nameof(ReservationWithoutJob), nameof(Fix), $"Fixing event for reservationInstance with ID {ReservationInstance.ID} was successful");
				return true;
			}
			catch (Exception exception)
			{
				helpers.Log(nameof(ReservationWithoutJob), nameof(Fix), $"Fixing event for reservationInstance with ID {ReservationInstance.ID} failed: {exception}");
				return false;
			}
		}
	}
}

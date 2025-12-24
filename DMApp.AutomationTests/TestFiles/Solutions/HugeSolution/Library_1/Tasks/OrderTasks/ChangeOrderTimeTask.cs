using System;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
	using NPOI.SS.Formula.Functions;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages;
	using static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface.DataMinerInterface;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class ChangeOrderTimeTask : Task
	{
		private readonly Order order;

		private readonly Order oldOrder;

		public ChangeOrderTimeTask(Helpers helpers, Order order, Order oldOrder = null)
			: base(helpers)
		{
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.oldOrder = oldOrder ?? base.helpers.OrderManager.GetOrder(order.Id);
			IsBlocking = true;
		}

		public override string Description => "Changing timing for Order " + order.Name;

		public override Task CreateRollbackTask()
		{
			return new ChangeOrderTimeTask(helpers, oldOrder, oldOrder);
		}

		protected override void InternalExecute()
		{
			order.Reservation = helpers.ResourceManager.GetReservationInstance(order.Id) ?? throw new ReservationNotFoundException(order.Id);

			var reservationPreRoll = order.Reservation.GetPreRoll();
			var reservationStart = order.Reservation.Start.FromReservation().Add(reservationPreRoll);
			var reservationPostRoll = order.Reservation.GetPostRoll();
			var reservationEnd = order.Reservation.End.FromReservation().Subtract(reservationPostRoll);

			Log(nameof(InternalExecute), $"Order preroll={order.PreRoll}, start={order.Start.ToFullDetailString()}, end={order.End.ToFullDetailString()}, postroll={order.PostRoll}. Order reservation preroll={reservationPreRoll}, start={reservationStart.ToFullDetailString()}, end={reservationEnd.ToFullDetailString()}, postroll={reservationPostRoll}.");

			bool isOrderTimingValidToBeExtended = order.Start == reservationStart && order.PreRoll == reservationPreRoll && order.PostRoll == reservationPostRoll && order.End > reservationEnd;

			// To allow extensions for ongoing EBU integration orders
			bool isOngoingEurovisionOrderAndValidForExtension = order.Start <= DateTime.Now && DateTime.Now <= order.End && order.IntegrationType == Utils.YLE.Integrations.IntegrationType.Eurovision && order.End > reservationEnd;

			if ((isOrderTimingValidToBeExtended && order.Reservation.Status == Net.Messages.ReservationStatus.Ongoing) || isOngoingEurovisionOrderAndValidForExtension)
			{
				Log(nameof(InternalExecute), $"Only order extension required");

				var timeToAdd = order.End - reservationEnd;

				if (!helpers.OrderManager.TryExtendOrder(order, timeToAdd)) throw new ChangeOrderTimingFailedException(order.Name);
			}
			else
			{
				Log(nameof(InternalExecute), $"Full order timing change required");

				helpers.OrderManager.ChangeOrderTime(order);
			}
		}
	}
}
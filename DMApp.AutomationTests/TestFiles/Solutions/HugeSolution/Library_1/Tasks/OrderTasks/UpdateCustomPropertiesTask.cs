using System.Collections.Generic;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;

	using Order = Order.Order;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class UpdateCustomPropertiesTask : Task
	{
		private readonly Order oldOrder;
		private readonly bool forceAllCustomPropertiesUpdate;
		private readonly Order order;

		public UpdateCustomPropertiesTask(Helpers helpers, Order order, Order oldOrder, bool forceAllCustomPropertiesUpdate = false)
			: base(helpers)
		{
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.oldOrder = oldOrder;
			this.forceAllCustomPropertiesUpdate = forceAllCustomPropertiesUpdate;
			IsBlocking = true;
		}

		public override string Description => "Updating properties for order " + order.Name;

		public override Task CreateRollbackTask()
		{
			return new UpdateCustomPropertiesTask(helpers, oldOrder, oldOrder);
		}

		protected override void InternalExecute()
		{
			var bookingManagerElement = helpers.Engine.FindElement(order.Definition.BookingManagerElementName);
			if (bookingManagerElement == null) throw new BookingManagerNotFoundException(order.Definition.BookingManagerElementName, order.Id);
			var bookingManager = new BookingManager((Engine)helpers.Engine, bookingManagerElement) { AllowPostroll = true, AllowPreroll = true, CustomProperties = true };

			Log(nameof(InternalExecute), $"ForceAllCustomPropertiesUpdate flag is set to {forceAllCustomPropertiesUpdate}");

			Dictionary<string, object> propertiesToUpdate;
			if (forceAllCustomPropertiesUpdate)
			{
				propertiesToUpdate = order.GetPropertiesFromBookingManager(helpers, bookingManager.Properties).ToDictionary(prop => prop.Name, prop => (object)prop.Value);
			}
			else
			{
				propertiesToUpdate = order.GetChangedProperties(helpers, oldOrder, bookingManager.Properties.ToList());
			}

			if (!order.TryUpdateCustomProperties(helpers, propertiesToUpdate)) throw new CustomPropertyUpdateFailedException(order.Name);
		}
	}
}
namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
    using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using System.Threading;

	public class ChangeOrderNameTask : Task
	{
		private readonly Order orderWithNewName;
		private readonly string previousName;

		public ChangeOrderNameTask(Helpers helpers, Order orderWithNewName, string previousName = null) : base(helpers)
		{
			IsBlocking = true;
			this.orderWithNewName = orderWithNewName ?? throw new ArgumentNullException(nameof(orderWithNewName));
			this.previousName = previousName;
		}

        public override string Description => $"Changing Order name from '{previousName}' to '{orderWithNewName?.Name}'";

		public override Task CreateRollbackTask()
		{
			if (!string.IsNullOrWhiteSpace(previousName))
			{
				orderWithNewName.ManualName = previousName;
				return new ChangeOrderNameTask(helpers, orderWithNewName);
			}

			return null;
		}

		protected override void InternalExecute()
		{
            helpers.OrderManager.ChangeOrderName(orderWithNewName);
            helpers.OrderManagerElement.UpdateOrderName(orderWithNewName);
		}
    }
}

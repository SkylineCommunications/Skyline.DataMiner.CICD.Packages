namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

    public class UpdateSatelliteRxSynopsisAttachmentsTask : Task
    {
        private readonly Order order;

        public UpdateSatelliteRxSynopsisAttachmentsTask(Helpers helpers, Order order) : base(helpers)
        {
            IsBlocking = true;
            this.order = order ?? throw new ArgumentNullException(nameof(order));
        }

        public override string Description => "Updating Satellite Rx Synopsis Attachments";

        public override Task CreateRollbackTask()
        {
            return null;
        }

        protected override void InternalExecute()
        {
            helpers.OrderManager.AddSatelliteReceptionSynopsisAttachments(order);
            helpers.OrderManager.RemoveSatelliteReceptionSynopsisAttachments(order);
        }
    }
}

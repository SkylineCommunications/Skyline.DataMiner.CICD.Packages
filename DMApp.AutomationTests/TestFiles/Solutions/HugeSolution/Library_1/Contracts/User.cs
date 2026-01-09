namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
    using System;

    [Flags]
    public enum Notifications : uint
    {
        None = 0,
        LiveOrderConfirmationByOperatorNotificationRequired = 1,
        LiveOrderRejectionByOperatorNotificationRequired = 2,
        LiveOrderCompletionByDataminerNotificationRequired = 4,
        LiveOrderCompletionWithErrorsByDataminerNotificationRequired = 8,
        LiveOrderCancellationByOperatorNotificationRequired = 16,
        LiveOrderCancellationByCustomerNotificationRequired = 32,
        LiveOrderCancellationByIntegrationNotificationRequired = 64,
        LiveOrderServicesBookedNotificationRequired = 128,
        LiveOrderServicesBookedByIntegrationNotificationRequired = 256,
        NonLiveOrderReassignedByOperatorNotificationRequired = 512,
        NonLiveOrderRejectedByOperatorNotificationRequired = 1024,
        NonLiveOrderCompletedByDataMinerNotificationRequired = 2048,
        NonLiveOrderCancellationByOperatorNotificationRequired = 4096,
        NonLiveOrderWorkInProgressByDataMinerNotificationRequired = 8192,
        NonLiveOrderEditedByDataMinerNotificationRequired = 16384,
        NonLiveOrderCreationByDataMinerNotificationRequired = 32768,
        NonLiveOrderIplayFolderCreationByOperatorNotificationRequired = 65536,
        UnableToBookRecurringVizremOrder = 131072,
    }

    public class User
	{
		public string ID { get; set; }

		public string Name { get; set; }

		public string Email { get; set; }

		public string Phone { get; set; }

		public string[] UsergroupIds { get; set; }

        public Notifications Notifications { get; set; }

        public override bool Equals(object obj)
		{
			if (!(obj is User other)) return false;
			return ID == other.ID;
		}

		public override int GetHashCode()
		{
			return ID.GetHashCode();
		}
	}
}
namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order
{
	using System;
	using System.Collections.Generic;
	using Library_1.Utilities;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class BillingInfo : IYleChangeTracking, ICloneable
	{
        private readonly Dictionary<string, object> initialPropertyValues = new Dictionary<string, object>();
        private string billableCompany;
        private string customerCompany;

		private BillingInfo(BillingInfo other)
		{
			CloneHelper.CloneProperties(other, this);
		}

        public BillingInfo()
		{
            AcceptChanges(null);
		}

        public event EventHandler<string> BillableCompanyChanged;

        [ChangeTracked]
		public string BillableCompany
        { 
            get => billableCompany;
            set 
            {
                billableCompany = value;
                BillableCompanyChanged?.Invoke(this, billableCompany);
            } 
        }

        public event EventHandler<string> CustomerCompanyChanged;

        [ChangeTracked]
        public string CustomerCompany
        {
            get => customerCompany;
            set
            {
                customerCompany = value;
                CustomerCompanyChanged?.Invoke(this, billableCompany);
            }
        }

        /// <summary>
        /// Gets a boolean indicating if Change Tracking has been enabled for this object.
        /// </summary>
        /// <see cref="IYleChangeTracking"/>
        [JsonIgnore]
        public bool ChangeTrackingStarted { get; private set; }

        [JsonIgnore]
        public Change Change => ChangeTrackingStarted ? ChangeTrackingHelper.GetUpdatedChange(this, initialPropertyValues, new ClassChange(nameof(BillingInfo))) : throw new InvalidOperationException($"Change Tracking has not been started for object {UniqueIdentifier}");

        [JsonIgnore]
        public string UniqueIdentifier => nameof(BillingInfo);

		[JsonIgnore]
		public string DisplayName => UniqueIdentifier;

		public void AcceptChanges(Helpers helpers = null)
        {
            ChangeTrackingStarted = true;
            ChangeTrackingHelper.AcceptChanges(this, initialPropertyValues, helpers);
        }

        public Change GetChangeComparedTo<T>(Helpers helpers, T oldObjectInstance)
        {
            if (!(oldObjectInstance is BillingInfo oldBillingInfo)) throw new ArgumentException($"Argument is not of type {nameof(BillingInfo)}", nameof(oldObjectInstance));

            return ChangeTrackingHelper.GetChangeComparedTo(this, oldBillingInfo, new ClassChange(nameof(BillingInfo)), helpers);
        }

		public object Clone()
		{
			return new BillingInfo(this);
		}
	}
}
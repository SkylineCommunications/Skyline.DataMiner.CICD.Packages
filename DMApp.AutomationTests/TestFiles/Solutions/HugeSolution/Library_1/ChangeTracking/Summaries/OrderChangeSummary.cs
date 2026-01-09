namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries;

	public class OrderChangeSummary : ChangeSummary
    {
        public override bool IsChanged
        {
            get
            {
                bool changed = false;
                changed |= TimingChangeSummary.IsChanged;
                changed |= PropertyChangeSummary.IsChanged;
                changed |= ServiceChangeSummary.IsChanged;
                changed |= CollectionChangesSummary.IsChanged;
                changed |= SecurityViewIdsChanged;
                return changed;
            }
        }

        public TimingChangeSummary TimingChangeSummary { get; } = new TimingChangeSummary();

        public PropertyChangeSummary PropertyChangeSummary { get; } = new PropertyChangeSummary();

        public ServiceChangeSummary ServiceChangeSummary { get; } = new ServiceChangeSummary();

        public CollectionChangesSummary CollectionChangesSummary { get; } = new CollectionChangesSummary();

        public bool NameChanged { get; set; }

        public bool SecurityViewIdsChanged { get; set; }

        public bool ServicesWereAdded { get; set; }

        public bool ServicesWereRemoved { get; set;}

        public bool SavedOrderIsBeingBooked { get; set; }

        public bool BookedOrderWasModifiedToStartNow { get; set; }

        public bool AreThereAnyCrucialServiceChanges => ServiceChangeSummary.TimingChangeSummary.IsChanged || ServiceChangeSummary.FunctionChangeSummary.IsChanged;

        public override bool TryAddChangeSummary(ChangeSummary changeSummaryToAdd)
        {
			if (changeSummaryToAdd is TimingChangeSummary timingChangeSummaryToAdd)
			{
                TimingChangeSummary.TryAddChangeSummary(timingChangeSummaryToAdd);
                return true;
            }
            else if (changeSummaryToAdd is ServiceChangeSummary serviceChangeSummaryToAdd)
			{
                ServiceChangeSummary.TryAddChangeSummary(serviceChangeSummaryToAdd);
                return true;
			}
            else if (changeSummaryToAdd is PropertyChangeSummary propertyChangeSummaryToAdd)
			{
                PropertyChangeSummary.TryAddChangeSummary(propertyChangeSummaryToAdd);
                return true;
			}
            else if (changeSummaryToAdd is CollectionChangesSummary collectionChangesSummaryToAdd)
			{
                CollectionChangesSummary.TryAddChangeSummary(collectionChangesSummaryToAdd);
                return true;
			}

            return false;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
	}
}

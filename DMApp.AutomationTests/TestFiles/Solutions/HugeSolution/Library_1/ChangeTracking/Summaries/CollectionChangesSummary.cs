using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries
{
	public class CollectionChangesSummary : ChangeSummary
	{
		public override bool IsChanged => ItemsAdded || ItemsRemoved;

		public bool ItemsAdded { get; private set; }
		
		public bool ItemsRemoved { get; private set; }

		public void MarkItemsAdded()
		{
			ItemsAdded = true;
		}

		public void MarkItemsRemoved()
		{
			ItemsRemoved = true;
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}

		public override bool TryAddChangeSummary(ChangeSummary changeSummaryToAdd)
		{
			if (changeSummaryToAdd is CollectionChangesSummary collectionChangesSummaryToAdd)
			{
				ItemsAdded |= collectionChangesSummaryToAdd.ItemsAdded;
				ItemsRemoved |= collectionChangesSummaryToAdd.ItemsRemoved;
				return true;
			}

			return false;
		}
	}
}

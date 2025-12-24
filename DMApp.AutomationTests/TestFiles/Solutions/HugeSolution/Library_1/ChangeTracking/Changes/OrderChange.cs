namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries;
	using Status = Order.Status;

	public class OrderChange : ClassChange
	{
		private readonly OrderChangeSummary summary = new OrderChangeSummary();

		[JsonConstructor]
		public OrderChange(string orderName, HashSet<ServiceChange> serviceChanges = null) : base(nameof(Order))
		{
			OrderName = orderName;
			ServiceChanges = serviceChanges ?? new HashSet<ServiceChange>();
		}

		[JsonIgnore]
		public string OrderName { get; }

		public HashSet<ServiceChange> ServiceChanges { get; }

		[JsonIgnore]
		public override ChangeSummary Summary => summary;

		public override bool TryAddChange(Change changeToAdd)
		{
			UpdateSummary(changeToAdd);

			if (changeToAdd is PropertyChange propertyChangeToAdd)
			{
				PropertyChanges.Add(propertyChangeToAdd);
				return true;
			}
			else if (changeToAdd is ServiceChange serviceChangeToAdd)
			{
				bool changeForThisServiceIsAlreadyAdded = ServiceChanges.Select(sc => sc.ServiceName).Contains(serviceChangeToAdd.ServiceName);

				if (!changeForThisServiceIsAlreadyAdded)
				{
					ServiceChanges.Add(serviceChangeToAdd);
				}

				return true;
			}
			else if (changeToAdd is ClassChange classChangeToAdd)
			{
				ClassChanges.Add(classChangeToAdd);
				return true;
			}
			else if (changeToAdd is CollectionChanges collectionChangesToAdd)
			{
				var existingCollectionChange = CollectionChanges.SingleOrDefault(c => c.CollectionName == collectionChangesToAdd.CollectionName);

				if (existingCollectionChange is null)
				{
					CollectionChanges.Add(collectionChangesToAdd);
				}
				else
				{
					existingCollectionChange.TryAddChange(collectionChangesToAdd);
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets a new Change object containing only values that really changed and without summaries. This is the object that should be saved on the Order Manager driver. 
		/// </summary>
		/// <returns>A new Change object containing only values that really changed and without summaries.</returns>
		public override Change GetActualChanges()
		{
			var change = new OrderChange(OrderName);

			change.TryAddChanges(ServiceChanges.Select(s => s.GetActualChanges()).OfType<Change>().ToList());
			change.TryAddChanges(CollectionChanges.Select(s => s.GetActualChanges()).OfType<Change>().ToList());
			change.TryAddChanges(ClassChanges.Select(s => s.GetActualChanges()).OfType<Change>().ToList());
			change.TryAddChanges(PropertyChanges.Select(s => s.GetActualChanges()).OfType<Change>().ToList());

			return change;
		}

		public OrderChange GetChangeForCreationHistory()
		{
			var change = new OrderChange(OrderName);

			var allServicesChange = GetCollectionChanges(nameof(Order.AllServices));

			if (allServicesChange != null)
			{
				change.TryAddChange(allServicesChange);
			}

			foreach (var serviceChange in ServiceChanges)
			{
				var serviceChangeForCreationHistory = serviceChange.GetChangeForCreationHistory();
				if (serviceChangeForCreationHistory is null) continue;
				
				change.TryAddChange(serviceChangeForCreationHistory);
			}

			foreach (var serviceChange in ServiceChanges)
			{
				var serviceChangeForCreationHistory = serviceChange.GetChangeForCreationHistory();
				if (serviceChangeForCreationHistory is null) continue;
				
				change.TryAddChange(serviceChangeForCreationHistory);
			}

			return change;
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}

		private void UpdateSummary(Change changeToAdd)
		{
			summary.TryAddChangeSummary(changeToAdd.Summary);

			if (changeToAdd is PropertyChange propertyChangeToAdd)
			{
				switch (propertyChangeToAdd.PropertyName)
				{
					case nameof(LiteOrder.Status):
						if (propertyChangeToAdd.Change.OldValue == Status.Preliminary.ToString() && propertyChangeToAdd.Change.NewValue != Status.Preliminary.ToString())
						{
							summary.SavedOrderIsBeingBooked = true;
						}
						break;
					case nameof(LiteOrder.StartNow):
						if (propertyChangeToAdd.Change.OldValue == false.ToString() && propertyChangeToAdd.Change.NewValue == true.ToString())
						{
							summary.BookedOrderWasModifiedToStartNow = true;
						}
						break;
					case nameof(Order.PreRoll):
						summary.TimingChangeSummary.MarkPrerollChanged();
						break;
					case nameof(LiteOrder.Start):
						summary.TimingChangeSummary.MarkStartTimingChanged();
						break;
					case nameof(LiteOrder.End):
						summary.TimingChangeSummary.MarkEndTimingChanged();
						break;
					case nameof(Order.PostRoll):
						summary.TimingChangeSummary.MarkPostrollChanged();
						break;
					case nameof(LiteOrder.Name):
						summary.NameChanged = true;
						break;

					default:
						break;
				}
			}
			else if (changeToAdd is CollectionChanges collectionChangesToAdd)
			{
				if (collectionChangesToAdd.CollectionName == nameof(Order.AllServices))
				{
					summary.ServicesWereAdded |= collectionChangesToAdd.Changes.Any(c => c.Type == CollectionChangeType.Add);
					summary.ServicesWereRemoved |= collectionChangesToAdd.Changes.Any(c => c.Type == CollectionChangeType.Remove);
				}

				summary.SecurityViewIdsChanged |= collectionChangesToAdd.CollectionName == nameof(Order.SecurityViewIds);
			}
		}
	}
}

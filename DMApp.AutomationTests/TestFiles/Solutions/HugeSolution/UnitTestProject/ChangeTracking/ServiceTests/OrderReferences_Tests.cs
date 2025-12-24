namespace UnitTestProject.ChangeTracking.ServiceTests
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.Net.Profiles;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.Library.Solutions.SRM.Model;
	using Function = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;

	[TestClass]
	public class OrderReferences_Tests
	{

		[TestMethod]
		public void NotChanged_ComparedToSelf()
		{
			var service = new DisplayedService
			{
				IsBooked = true,
				OrderReferences = new HashSet<Guid> { Guid.Empty },
				Definition = new ServiceDefinition(VirtualPlatform.Recording)
				{
					Id = ServiceDefinitionGuids.RecordingMessiNews,
					Description = "Messi News",
				},
			};

			service.AcceptChanges(null);

			Assert.IsFalse(service.Change.Summary.IsChanged);
		}

		[TestMethod]
		public void Changed_ComparedToSelf()
		{
			var service = new DisplayedService
			{
				IsBooked = true,
				OrderReferences = new HashSet<Guid> { Guid.Empty },
				Definition = new ServiceDefinition(VirtualPlatform.Recording)
				{
					Id = ServiceDefinitionGuids.RecordingMessiNews,
					Description = "Messi News",
				},
			};

			service.AcceptChanges(null);

			service.OrderReferences.Add(Guid.NewGuid());

			Assert.IsTrue(service.Change.Summary.IsChanged);
		}

		[TestMethod]
		public void PropertyChanged_ComparedToSelf()
		{
			var service = new DisplayedService
			{
				IsBooked = true,
				OrderReferences = new HashSet<Guid> { Guid.Empty },
				Definition = new ServiceDefinition(VirtualPlatform.Recording)
				{
					Id = ServiceDefinitionGuids.RecordingMessiNews,
					Description = "Messi News",
				},
			};

			service.AcceptChanges(null);

			service.OrderReferences.Add(Guid.NewGuid());

			var change = service.Change.Summary as ServiceChangeSummary;

			Assert.IsTrue(change.PropertyChangeSummary.IsChanged);
		}

		[TestMethod]
		public void ItemsAdded_ComparedToSelf()
		{
			var service = new DisplayedService
			{
				IsBooked = true,
				OrderReferences = new HashSet<Guid> { Guid.Empty },
				Definition = new ServiceDefinition(VirtualPlatform.Recording)
				{
					Id = ServiceDefinitionGuids.RecordingMessiNews,
					Description = "Messi News",
				},
			};

			service.AcceptChanges(null);

			service.OrderReferences.Add(Guid.NewGuid());

			var serviceChange = service.Change as ServiceChange;

			var collectionChangeSummary = serviceChange.CollectionChanges.Single().Summary as CollectionChangesSummary;

			Assert.IsTrue(collectionChangeSummary.ItemsAdded);
		}

		[TestMethod]
		public void ItemsRemoved_ComparedToSelf()
		{
			var service = new DisplayedService
			{
				IsBooked = true,
				OrderReferences = new HashSet<Guid> { Guid.Empty },
				Definition = new ServiceDefinition(VirtualPlatform.Recording)
				{
					Id = ServiceDefinitionGuids.RecordingMessiNews,
					Description = "Messi News",
				},
			};

			service.AcceptChanges(null);

			service.OrderReferences.Remove(Guid.Empty);

			var serviceChange = service.Change as ServiceChange;

			var collectionChangeSummary = serviceChange.CollectionChanges.Single().Summary as CollectionChangesSummary;

			Assert.IsTrue(collectionChangeSummary.ItemsRemoved);
		}

		[TestMethod]
		public void ItemsRemoved_ComparedToSelf_Part2()
		{
			var service = new DisplayedService
			{
				IsBooked = true,
				OrderReferences = new HashSet<Guid> { Guid.Empty },
				Definition = new ServiceDefinition(VirtualPlatform.Recording)
				{
					Id = ServiceDefinitionGuids.RecordingMessiNews,
					Description = "Messi News",
				},
			};

			service.AcceptChanges(null);

			service.OrderReferences.Remove(Guid.Empty);
			service.OrderReferences.Add(Guid.NewGuid());

			var serviceChange = service.Change as ServiceChange;

			var collectionChangeSummary = serviceChange.CollectionChanges.Single().Summary as CollectionChangesSummary;

			Assert.IsTrue(collectionChangeSummary.ItemsRemoved);
		}

		[TestMethod]
		public void ItemsAdded_ComparedToSelf_Part2()
		{
			var service = new DisplayedService
			{
				IsBooked = true,
				OrderReferences = new HashSet<Guid> { Guid.Empty },
				Definition = new ServiceDefinition(VirtualPlatform.Recording)
				{
					Id = ServiceDefinitionGuids.RecordingMessiNews,
					Description = "Messi News",
				},
			};

			service.AcceptChanges(null);

			service.OrderReferences.Remove(Guid.Empty);
			service.OrderReferences.Add(Guid.NewGuid());

			var serviceChange = service.Change as ServiceChange;

			var collectionChangeSummary = serviceChange.CollectionChanges.Single().Summary as CollectionChangesSummary;

			Assert.IsTrue(collectionChangeSummary.ItemsAdded);
		}

		[TestMethod]
		public void NotChanged_ComparedToOther()
		{
			var service = new DisplayedService
			{
				IsBooked = true,
				OrderReferences = new HashSet<Guid> { Guid.Empty },
				Definition = new ServiceDefinition(VirtualPlatform.Recording)
				{
					Id = ServiceDefinitionGuids.RecordingMessiNews,
					Description = "Messi News",
				},
			};

			var other = new DisplayedService
			{
				IsBooked = true,
				OrderReferences = new HashSet<Guid> { Guid.Empty },
				Definition = new ServiceDefinition(VirtualPlatform.Recording)
				{
					Id = ServiceDefinitionGuids.RecordingMessiNews,
					Description = "Messi News",
				},
			};

			service.AcceptChanges(null);

			var change = service.GetChangeComparedTo(null, other).Summary as ServiceChangeSummary;

			Assert.IsFalse(change.PropertyChangeSummary.IsChanged);
		}

		[TestMethod]
		public void Changed_ComparedToOther()
		{
			var service = new DisplayedService
			{
				IsBooked = true,
				OrderReferences = new HashSet<Guid> { Guid.Empty },
				Definition = new ServiceDefinition(VirtualPlatform.Recording)
				{
					Id = ServiceDefinitionGuids.RecordingMessiNews,
					Description = "Messi News",
				},
			};

			var other = new DisplayedService
			{
				IsBooked = true,
				OrderReferences = new HashSet<Guid> { Guid.NewGuid() },
				Definition = new ServiceDefinition(VirtualPlatform.Recording)
				{
					Id = ServiceDefinitionGuids.RecordingMessiNews,
					Description = "Messi News",
				},
			};

			service.AcceptChanges(null);

			var change = service.GetChangeComparedTo(null, other).Summary as ServiceChangeSummary;

			Assert.IsTrue(change.PropertyChangeSummary.IsChanged);
		}
	}
}

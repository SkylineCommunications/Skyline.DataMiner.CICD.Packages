namespace UnitTestProject.ChangeTracking.OrderTests
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
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
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	[TestClass]
	public class Service_Tests
	{
		[TestMethod]
		public void NotChanged_ComparedToSelf()
		{
			var order = new Order
			{
				Sources = new List<Service> { Helper.GetDefaultServiceForTesting() }
			};

			order.AcceptChanges(null);

			var changeSummary = order.Change.Summary as OrderChangeSummary;

			Assert.IsFalse(changeSummary.IsChanged);
		}

		[TestMethod]
		public void NotAdded_ComparedToSelf()
		{
			var order = new Order
			{
				Sources = new List<Service> { Helper.GetDefaultServiceForTesting() }
			};

			order.AcceptChanges(null);

			var changeSummary = order.Change.Summary as OrderChangeSummary;

			Assert.IsFalse(changeSummary.ServicesWereAdded);
		}

		[TestMethod]
		public void Changed_ComparedToSelf()
		{
			var order = new Order
			{
				Sources = new List<Service> { Helper.GetDefaultServiceForTesting() }
			};

			order.AcceptChanges(null);

			order.Sources[0].RecordingConfiguration = new RecordingConfiguration();

			var changeSummary = order.Change.Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.ServiceChangeSummary.IsChanged);
		}

		[TestMethod]
		public void Added_ComparedToSelf()
		{
			var order = new Order
			{
				Sources = new List<Service> { Helper.GetDefaultServiceForTesting() }
			};

			order.AcceptChanges(null);

			var newService = Helper.GetDefaultServiceForTesting();
			newService.AcceptChanges(null);

			order.Sources.Add(newService);

			var changeSummary = order.Change.Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.ServicesWereAdded);
		}

		[TestMethod]
        public void Changed_ComparedShortDescription()
        {
			var order = new Order();

            order.AcceptChanges(null);
			Identifiers identifier = Identifiers.CreateRandom();
			var service = Helper.GetDefaultServiceForTesting(identifier);
			order.Sources.Add(service);
			service.GetShortDescription(order);
			service.AcceptChanges(null);

			service.Functions.ForEach(x => x.Resource = new FunctionResource() { Name = "UMX 01", ID = Guid.Empty });
			service.GetShortDescription(order);
			var change = order.Change as OrderChange;

            var shortDescription = change.CollectionChanges.Single().Changes.Single().DisplayName;
			
			Assert.AreEqual("None - UMX 01", shortDescription);
        }

        [TestMethod]
		public void Removed_ComparedToSelf()
		{
			var order = new Order
			{
				Sources = new List<Service> { Helper.GetDefaultServiceForTesting() }
			};

			order.AcceptChanges(null);

			order.Sources.Clear();

			var changeSummary = order.Change.Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.ServicesWereRemoved);
		}

		[TestMethod]
		public void NotChanged_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();

			var otherService = Helper.GetDefaultServiceForTesting();
			otherService.Id = service.Id;
			otherService.Functions = service.Functions.Select(f => new Function
			{
				Name = "function",
				Id = f.Id,
				Definition = new FunctionDefinition
				{
					Label = f.Definition.Label
				},
				RequiresResource = true,
				Parameters = new List<ProfileParameter> { Helper.GetDefaultProfileParameterForTesting() },
				Resource = f.Resource
			}).ToList();

			var order = new Order
			{
				Sources = new List<Service> { service }
			};

			var other = new Order
			{
				Sources = new List<Service> { otherService }
			};

			var change = order.GetChangeComparedTo(null, other) as OrderChange;

			var changeSummary = change.Summary as OrderChangeSummary;

			Assert.IsFalse(changeSummary.IsChanged);
		}

		[TestMethod]
		public void Changed_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();

			var otherService = Helper.GetDefaultServiceForTesting();
			otherService.Id = service.Id;
			otherService.Name = service.Name;
			otherService.Functions = service.Functions.Select(f => new Function
			{
				Name = "function",
				Id = f.Id,
				Definition = new FunctionDefinition
				{
					Label = f.Definition.Label
				},
				RequiresResource = true,
				Parameters = new List<ProfileParameter> { Helper.GetDefaultProfileParameterForTesting() },
				Resource = f.Resource
			}).ToList();

			var order = new Order
			{
				Sources = new List<Service> { service }
			};

			var other = new Order
			{
				Sources = new List<Service> { otherService }
			};

			other.Sources[0].RecordingConfiguration.NameOfServiceToRecord = "new value";

			var changeSummary = order.GetChangeComparedTo(null, other).Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.ServiceChangeSummary.IsChanged);
		}

		[TestMethod]
		public void Added_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();
			service.AcceptChanges(null);

			var order = new Order
			{
				Sources = new List<Service> { service }
			};

			var other = new Order
			{
				Sources = new List<Service> { }
			};

			var changeSummary = order.GetChangeComparedTo(null, other).Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.ServicesWereAdded);
		}

		[TestMethod]
		public void Removed_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();
			service.AcceptChanges(null);

			var order = new Order
			{
				Sources = new List<Service> { service }
			};

			var other = new Order
			{
				Sources = new List<Service> { }
			};

			var changeSummary = order.GetChangeComparedTo(null, other).Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.ServicesWereAdded);
		}
	}
}

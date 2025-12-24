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

	[TestClass]
	public class SportsPlanning_Tests
	{
		[TestMethod]
		public void NotChanged_ComparedToSelf()
		{
			var order = new Order
			{
				ManualName = "test order",
				Sources = new List<Service> { Helper.GetDefaultServiceForTesting() },
				SportsPlanning = new SportsPlanning()
			};

			order.AcceptChanges(null);

			Assert.IsFalse(order.Change.Summary.IsChanged);
		}

		[TestMethod]
		public void Removed_ComparedToSelf()
		{
			var order = new Order
			{
				ManualName = "test order",
				Sources = new List<Service> { Helper.GetDefaultServiceForTesting() },
				SportsPlanning = new SportsPlanning()
			};

			order.AcceptChanges(null);

			order.SportsPlanning = null;

			var changeSummary = order.Change.Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.PropertyChangeSummary.IsChanged);
		}

		[TestMethod]
		public void Changed_ComparedToSelf()
		{
			var order = new Order
			{
				ManualName = "test order",
				Sources = new List<Service> { Helper.GetDefaultServiceForTesting() },
				SportsPlanning = new SportsPlanning()
			};

			order.AcceptChanges(null);

			order.SportsPlanning.AdditionalInformation = "random value";

			var changeSummary = order.Change.Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.PropertyChangeSummary.IsChanged);
		}

		[TestMethod]
		public void NotChanged_ComparedToOther()
		{
			var order = new Order
			{
				ManualName = "test order",
				SportsPlanning = new SportsPlanning()
			};

			var other = new Order
			{
				ManualName = "test order",
				SportsPlanning = new SportsPlanning()
                {
					CompetitionTime = order.SportsPlanning.CompetitionTime,
					RequestedBroadcastTime = order.SportsPlanning.RequestedBroadcastTime
				}
			};

			var changeSummary = order.GetChangeComparedTo(null, other).Summary as OrderChangeSummary;

			Assert.IsFalse(changeSummary.PropertyChangeSummary.IsChanged);
		}

		[TestMethod]
		public void Changed_ComparedToOther()
		{
			var order = new Order
			{
				ManualName = "test order",
				SportsPlanning = new SportsPlanning()
			};

			var other = new Order
			{
				ManualName = "test order",
				SportsPlanning = new SportsPlanning
				{
					AdditionalInformation = "random value"
				}
			};

			var changeSummary = order.GetChangeComparedTo(null, other).Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.PropertyChangeSummary.IsChanged);
		}
	}
}

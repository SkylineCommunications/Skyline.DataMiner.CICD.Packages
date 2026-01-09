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
	public class Timing_Tests
	{
		[TestMethod]
		public void NotChanged_ComparedToSelf()
		{
			var order = new Order
			{
				ManualName = "test order",
				Start = DateTime.Now,
			};

			order.AcceptChanges(null);

			var changeSummary = order.Change.Summary as OrderChangeSummary;

			Assert.IsFalse(changeSummary.TimingChangeSummary.IsChanged);
		}

		[TestMethod]
		public void Changed_ComparedToSelf()
		{
			var order = new Order
			{
				ManualName = "test order",
				Start = DateTime.Now,
			};

			order.AcceptChanges(null);

			order.Start = DateTime.Now.AddHours(1);

			var changeSummary = order.Change.Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.TimingChangeSummary.IsChanged);
		}

		[TestMethod]
		public void NotChanged_ComparedToOther()
		{
			var now = DateTime.Now;

			var order = new Order
			{
				ManualName = "test order",
				Start = now,
			};

			var other = new Order
			{
				ManualName = "test order",
				Start =now,
			};

			var changeSummary = order.GetChangeComparedTo(null, other).Summary as OrderChangeSummary;

			Assert.IsFalse(changeSummary.TimingChangeSummary.IsChanged);
		}

		[TestMethod]
		public void Changed_ComparedToOther()
		{
			var now = DateTime.Now;

			var order = new Order
			{
				ManualName = "test order",
				Start = now,
			};

			var other = new Order
			{
				ManualName = "test order",
				Start = now,
			};

			other.Start = DateTime.Now.AddHours(1);

			var changeSummary = order.GetChangeComparedTo(null, other).Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.TimingChangeSummary.IsChanged);
		}
	}
}

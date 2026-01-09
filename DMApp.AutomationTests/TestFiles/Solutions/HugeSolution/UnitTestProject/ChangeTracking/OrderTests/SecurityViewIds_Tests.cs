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
	public class SecurityViewIds_Tests
	{
		[TestMethod]
		public void NotChanged_ComparedToSelf()
		{
			var order = new Order
			{
				ManualName = "test order",
			};

			order.SetSecurityViewIds(new HashSet<int> { 1 });

			order.AcceptChanges(null);

			Assert.IsFalse(order.Change.Summary.IsChanged);
		}

		[TestMethod]
		public void AddedValue_ComparedToSelf()
		{
			var order = new Order
			{
				ManualName = "test order",
			};

			order.SetSecurityViewIds(new HashSet<int> { 1 });

			order.AcceptChanges(null);

			order.SecurityViewIds.Add(2);

			var changeSummary = order.Change.Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.SecurityViewIdsChanged);
		}

		[TestMethod]
		public void RemovedValue_ComparedToSelf()
		{
			var order = new Order
			{
				ManualName = "test order",
			};

			order.SetSecurityViewIds(new HashSet<int> { 1 });

			order.AcceptChanges(null);

			order.SecurityViewIds.Remove(1);

			var changeSummary = order.Change.Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.SecurityViewIdsChanged);
		}

		[TestMethod]
		public void Added_ComparedToSelf()
		{
			var order = new Order
			{
				ManualName = "test order",
			};

			order.AcceptChanges(null);

			order.SetSecurityViewIds(new HashSet<int> { 1 });

			var changeSummary = order.Change.Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.IsChanged);
		}

		[TestMethod]
		public void Removed_ComparedToSelf()
		{
			var order = new Order
			{
				ManualName = "test order",
			};

			order.SetSecurityViewIds(new HashSet<int> { 1 });

			order.AcceptChanges(null);

			order.SecurityViewIds = null;

			var changeSummary = order.Change.Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.IsChanged);
		}

		[TestMethod]
		public void NotChanged_ComparedToOther()
		{
			var order = new Order
			{
				ManualName = "test order",
			};

			order.SetSecurityViewIds(new HashSet<int> { 1 });

			var other = new Order
			{
				ManualName = "test order",
			};

			other.SetSecurityViewIds(new HashSet<int> { 1 });

			var changeSummary = order.GetChangeComparedTo(null, other).Summary as OrderChangeSummary;

			Assert.IsFalse(changeSummary.SecurityViewIdsChanged);
		}

		[TestMethod]
		public void AddedValue_ComparedToOther()
		{
			var order = new Order
			{
				ManualName = "test order",
			};

			order.SetSecurityViewIds(new HashSet<int> { 1 });

			var other = new Order
			{
				ManualName = "test order",
			};

			other.SetSecurityViewIds(new HashSet<int> { 1, 2 });

			var changeSummary = order.GetChangeComparedTo(null, other).Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.SecurityViewIdsChanged);
		}

		[TestMethod]
		public void RemovedValue_ComparedToOther()
		{
			var order = new Order
			{
				ManualName = "test order",
			};

			order.SetSecurityViewIds(new HashSet<int> { 1 });

			var other = new Order
			{
				ManualName = "test order",
			};

			other.SetSecurityViewIds(new HashSet<int>());

			var changeSummary = order.GetChangeComparedTo(null, other).Summary as OrderChangeSummary;

			Assert.IsTrue(changeSummary.SecurityViewIdsChanged);
		}
	}
}

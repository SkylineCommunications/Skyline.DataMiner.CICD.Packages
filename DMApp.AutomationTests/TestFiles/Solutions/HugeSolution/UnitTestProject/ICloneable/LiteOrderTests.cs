using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;

namespace UnitTestProject.ICloneable
{
	[TestClass]
	public class LiteOrderTests
	{
		[TestMethod]
		public void Check_DateTime_NotChanged()
		{
			var liteOrder = new LiteOrder
			{
				Id = Guid.Empty,
				ManualName = "Name",
				Start = DateTime.Now,
			};

			var clone = liteOrder.Clone() as LiteOrder;

			Assert.AreEqual(clone.Start, liteOrder.Start);
		}

		[TestMethod]
		public void Check_DateTime_Changed()
		{
			var liteOrder = new LiteOrder
			{
				Id = Guid.Empty,
				ManualName = "Name",
				Start = DateTime.Now,
			};

			var clone = liteOrder.Clone() as LiteOrder;

			liteOrder.Start = DateTime.Now.AddDays(1);

			Assert.AreNotEqual(clone.Start, liteOrder.Start);
		}

		[TestMethod]
		public void Check_ICloneableProperty_Reference()
		{
			var liteOrder = new LiteOrder
			{
				Id = Guid.Empty,
				ManualName = "Name",
				BillingInfo = new BillingInfo
				{
					BillableCompany = "company"
				},
			};

			var clone = liteOrder.Clone() as LiteOrder;

			Assert.AreNotSame(clone.BillingInfo, liteOrder.BillingInfo);
		}

		[TestMethod]
		public void Check_ICloneableProperty_NotChanged()
		{
			var liteOrder = new LiteOrder
			{
				Id = Guid.Empty,
				ManualName = "Name",
				BillingInfo = new BillingInfo
				{
					BillableCompany = "company"
				},
			};

			var clone = liteOrder.Clone() as LiteOrder;

			Assert.AreEqual(clone.BillingInfo.BillableCompany, liteOrder.BillingInfo.BillableCompany);
		}

		[TestMethod]
		public void Check_ICloneableProperty_Changed()
		{
			var liteOrder = new LiteOrder
			{
				Id = Guid.Empty,
				ManualName = "Name",
				BillingInfo = new BillingInfo
				{
					BillableCompany = "company"
				},
			};

			var clone = liteOrder.Clone() as LiteOrder;

			liteOrder.BillingInfo.BillableCompany = "new company";

			Assert.AreNotEqual(clone.BillingInfo.BillableCompany, liteOrder.BillingInfo.BillableCompany);
		}

		[TestMethod]
		public void Check_SecurityViewIds_NotChanged()
		{
			var liteOrder = new LiteOrder
			{
				Id = Guid.Empty,
				ManualName = "Name",
				BillingInfo = new BillingInfo
				{
					BillableCompany = "company"
				},
			};

			liteOrder.SetSecurityViewIds(new[] { 1, 2, 3 });

			var clone = liteOrder.Clone() as LiteOrder;

			Assert.IsTrue(clone.SecurityViewIds.SequenceEqual(liteOrder.SecurityViewIds));
		}

		[TestMethod]
		public void Check_SecurityViewIds_Changed()
		{
			var liteOrder = new LiteOrder
			{
				Id = Guid.Empty,
				ManualName = "Name",
				BillingInfo = new BillingInfo
				{
					BillableCompany = "company"
				},
			};

			liteOrder.SetSecurityViewIds(new[] { 1, 2, 3 });

			var clone = liteOrder.Clone() as LiteOrder;

			liteOrder.SetSecurityViewIds(new[] { 4, 5 });

			Assert.IsFalse(clone.SecurityViewIds.SequenceEqual(liteOrder.SecurityViewIds));
		}
	}
}

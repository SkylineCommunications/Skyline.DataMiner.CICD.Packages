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
	public class HistoryChapter_Tests
	{
		[TestMethod]
		public void Free_Testing_Method()
		{
			var order = new Order
			{
				ManualName = "test order name",
				Sources = new List<Service> { Helper.GetDefaultServiceForTesting() }
			};

			order.AcceptChanges(null);

			var newService = Helper.GetDefaultServiceForTesting();
			newService.AcceptChanges(null);

			order.Sources.Add(newService);

			var firstHistoryChapter = new OrderHistoryChapter(order.Change as OrderChange, "victor scherpereel", DateTime.Now, "unit test");

			order.AcceptChanges(null);

			order.ManualName = "new order name";
			order.Sources[0].Functions[0].Parameters[0].Value = "new value";
			order.SportsPlanning.AdditionalInformation = "new value";

			newService.Functions[0].Resource = null;

			var secondHistoryChapter = new OrderHistoryChapter(order.Change as OrderChange, "victor scherpereel", DateTime.Now, "unit test");
			Assert.IsTrue(secondHistoryChapter.OrderChange.ServiceChanges.SelectMany(s => s.FunctionChanges).Any(f => f.Summary.IsChanged));

		}		
	}
}

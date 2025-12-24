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
	public class Timing_Tests
	{
		[TestMethod]
		public void Changed_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.AcceptChanges(null);

			service.Start = DateTime.Now;

			Assert.IsTrue(service.Change.Summary.IsChanged);
		}

		[TestMethod]
		public void StartChanged_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.AcceptChanges(null);

			service.Start = DateTime.Now;

			var summary = service.Change.Summary as ServiceChangeSummary;

			Assert.IsTrue(summary.TimingChangeSummary.StartTimingChanged);
		}

		[TestMethod]
		public void Changed_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();

			var other = Helper.GetDefaultServiceForTesting();

			service.AcceptChanges(null);

			other.Start = DateTime.Now;

			var change = service.GetChangeComparedTo(null, other);

			Assert.IsTrue(change.Summary.IsChanged);
		}

		[TestMethod]
		public void StartChanged_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();

			var other = Helper.GetDefaultServiceForTesting();

			service.AcceptChanges(null);

			other.Start = DateTime.Now;

			var changeSummary = service.GetChangeComparedTo(null, other).Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.TimingChangeSummary.StartTimingChanged);
		}
	}
}

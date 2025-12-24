namespace UnitTestProject.ChangeTracking.FunctionTests
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.Net.Profiles;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.Library.Solutions.SRM.Model;
	using Function = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	[TestClass]
	public class Summary_ResourceChangeSummary_Tests
	{
		[TestMethod]
		public void TestMethod1()
		{
			var function = new Function
			{
				Name = "test function",
				Definition = new FunctionDefinition
				{
					Label = "test function"
				},
				RequiresResource = true,
				Resource = new FunctionResource
				{
					ID = Guid.NewGuid(),
					Name = "test resource 1"
				}
			};

			function.AcceptChanges(null);

			var resourceChangeSummary = ((FunctionChangeSummary)function.Change.Summary).ResourceChangeSummary;

			Assert.IsFalse(resourceChangeSummary.IsChanged);
		}

		[TestMethod]
		public void TestMethod2()
		{
			var function = new Function
			{
				Name = "test function",
				Definition = new FunctionDefinition
				{
					Label = "test function"
				},
				RequiresResource = true,
				Resource = new FunctionResource
				{
					ID = Guid.NewGuid(),
					Name = "test resource 1"
				}
			};

			function.AcceptChanges(null);

			function.Resource = new FunctionResource
			{
				ID = Guid.NewGuid(),
				Name = "test resource 2"
			};

			var resourceChangeSummary = ((FunctionChangeSummary)function.Change.Summary).ResourceChangeSummary;

			Assert.IsTrue(resourceChangeSummary.IsChanged);
		}

		[TestMethod]
		public void TestMethod3()
		{
			var function = new Function
			{
				Name = "test function",
				Definition = new FunctionDefinition
				{
					Label = "test function"
				},
				RequiresResource = true,
				Resource = new FunctionResource
				{
					ID = Guid.NewGuid(),
					Name = "test resource 1"
				}
			};

			function.AcceptChanges(null);

			function.Resource = new FunctionResource
			{
				ID = Guid.NewGuid(),
				Name = "test resource 2"
			};

			var resourceChangeSummary = ((FunctionChangeSummary)function.Change.Summary).ResourceChangeSummary;

			Assert.IsTrue(resourceChangeSummary.ResourcesAddedOrSwapped);
		}

		[TestMethod]
		public void TestMethod4()
		{
			var function = new Function
			{
				Name = "test function",
				Definition = new FunctionDefinition
				{
					Label = "test function"
				},
				RequiresResource = true,
				Resource = new FunctionResource
				{
					ID = Guid.NewGuid(),
					Name = "test resource 1"
				}
			};

			function.AcceptChanges(null);

			function.Resource = null;

			var resourceChangeSummary = ((FunctionChangeSummary)function.Change.Summary).ResourceChangeSummary;

			Assert.IsTrue(resourceChangeSummary.ResourcesRemoved);
		}
	}
}

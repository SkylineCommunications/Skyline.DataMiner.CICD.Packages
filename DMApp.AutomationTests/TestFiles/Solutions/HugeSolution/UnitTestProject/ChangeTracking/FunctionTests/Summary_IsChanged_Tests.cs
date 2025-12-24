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
	public class Summary_IsChanged_Tests
	{
		[TestMethod]
		public void TestMethod1()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "this",
				Value = "default value",
				Id = ProfileParameterGuids.AudioDeembeddingRequired,
				DefaultValue = new ParameterValue { StringValue = "default value" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
			};

			var function = new Function
			{
				Definition = new FunctionDefinition
				{
					Label = "test function"
				},
				RequiresResource = true,
				Parameters = new List<ProfileParameter> { profileParameter }
			};

			function.AcceptChanges(null);

			Assert.IsFalse(function.Change.Summary.IsChanged);
		}

		[TestMethod]
		public void TestMethod2()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "this",
				Value = "default value",
				Id = ProfileParameterGuids.AudioDeembeddingRequired,
				DefaultValue = new ParameterValue { StringValue = "default value" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
			};

			var function = new Function
			{
				Name = "test function",
				Definition = new FunctionDefinition
				{
					Label = "test function"
				},
				RequiresResource = true,
				Parameters = new List<ProfileParameter> { profileParameter }
			};

			function.AcceptChanges(null);

			function.Resource = new FunctionResource
			{
				ID = Guid.NewGuid(),
				Name = "test resource"
			};

			Assert.IsTrue(function.Change.Summary.IsChanged);
		}

		[TestMethod]
		public void TestMethod3()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "this",
				Value = "default value",
				Id = ProfileParameterGuids.AudioDeembeddingRequired,
				DefaultValue = new ParameterValue { StringValue = "default value" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
			};

			var function = new Function
			{
				Name = "test function",
				Definition = new FunctionDefinition
				{
					Label = "test function"
				},
				RequiresResource = true,
				Parameters = new List<ProfileParameter> { profileParameter }
			};

			function.AcceptChanges(null);

			function.Parameters.Single().Value = "new value";

			Assert.IsTrue(function.Change.Summary.IsChanged);
		}

		[TestMethod]
		public void TestMethod4()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "this",
				Value = "default value",
				Id = ProfileParameterGuids.AudioDeembeddingRequired,
				DefaultValue = new ParameterValue { StringValue = "default value" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
			};

			var function = new Function
			{
				Name = "test function",
				Definition = new FunctionDefinition
				{
					Label = "test function"
				},
				RequiresResource = true,
				Parameters = new List<ProfileParameter> { profileParameter }
			};

			function.AcceptChanges(null);

			function.Parameters.Single().Value = "new value";

			Assert.IsTrue(function.Change.Summary.IsChanged);
		}

		[TestMethod]
		public void TestMethod5()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "first",
				Value = "default value",
				Id = ProfileParameterGuids.AudioDeembeddingRequired,
				DefaultValue = new ParameterValue { StringValue = "default value" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
			};

			var function = new Function
			{
				Name = "test function",
				Definition = new FunctionDefinition
				{
					Label = "test function"
				},
				RequiresResource = true,
				Parameters = new List<ProfileParameter> { profileParameter }
			};

			var profileParameter2 = new ProfileParameter
			{
				Name = "second",
				Value = "value 2",
				Id = ProfileParameterGuids.AudioDeembeddingRequired,
				DefaultValue = new ParameterValue { StringValue = "default value" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
			};

			var function2 = new Function
			{
				Name = "second test function",
				Definition = new FunctionDefinition
				{
					Label = "test function"
				},
				RequiresResource = true,
				Parameters = new List<ProfileParameter> { profileParameter2 }
			};

			function.AcceptChanges(null);

			var change = function.GetChangeComparedTo(null, function2) as FunctionChange;

			Assert.IsTrue(change.Summary.IsChanged);
		}
	}
}

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
	public class Summary_ProfileParameterSummary_Tests
	{
		[TestMethod]
		public void TestMethod1()
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

			var profileParameter2 = new ProfileParameter
			{
				Name = "second",
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
				Parameters = new List<ProfileParameter> { profileParameter, profileParameter2 }
			};

			function.AcceptChanges(null);

			var profileParamaterChangeSummary = ((FunctionChangeSummary)function.Change.Summary).ProfileParameterChangeSummary;

			Assert.IsFalse(profileParamaterChangeSummary.IsChanged);
		}

		[TestMethod]
		public void TestMethod2()
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

			var profileParameter2 = new ProfileParameter
			{
				Name = "second",
				Value = "default value",
				Id = ProfileParameterGuids.VideoFormat,
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
				Parameters = new List<ProfileParameter> { profileParameter, profileParameter2 }
			};

			function.AcceptChanges(null);

			profileParameter.Value = "new value";

			var profileParamaterChangeSummary = ((FunctionChangeSummary)function.Change.Summary).ProfileParameterChangeSummary;

			Assert.IsTrue(profileParamaterChangeSummary.IsChanged);
		}

		[TestMethod]
		public void TestMethod3()
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

			var profileParameter2 = new ProfileParameter
			{
				Name = "second",
				Value = "default value",
				Id = ProfileParameterGuids.VideoFormat,
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
				Parameters = new List<ProfileParameter> { profileParameter, profileParameter2 }
			};

			function.AcceptChanges(null);

			profileParameter.Value = "new value";

			var profileParamaterChangeSummary = ((FunctionChangeSummary)function.Change.Summary).ProfileParameterChangeSummary;

			Assert.IsTrue(profileParamaterChangeSummary.AudioProcessingProfileParametersChanged);
		}
	}
}

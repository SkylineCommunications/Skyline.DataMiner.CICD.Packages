namespace UnitTestProject.ChangeTracking.ProfileParameterTests
{
	using System;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.Net.Profiles;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.Library.Solutions.SRM.Model;

	[TestClass]
	public class Summary_GraphicsProcessingProperties_Tests
	{
		[TestMethod]
		public void GraphicsProcessing_NotChanged()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "this",
				Value = "default value",
				Id = ProfileParameterGuids.RemoteGraphics,
				DefaultValue = new ParameterValue { StringValue = "default value" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
			};

			profileParameter.AcceptChanges(null);

			Assert.IsFalse(((ProfileParameterChangeSummary)profileParameter.Change.Summary).GraphicsProcessingProfileParametersHaveChanged);
		}

		[TestMethod]
		public void GraphicsProcessing_Changed()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "this",
				Value = "default value",
				Id = ProfileParameterGuids.RemoteGraphics,
				DefaultValue = new ParameterValue { StringValue = "default value" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
			};

			profileParameter.AcceptChanges(null);

			profileParameter.Value = "new value";

			Assert.IsTrue(((ProfileParameterChangeSummary)profileParameter.Change.Summary).GraphicsProcessingProfileParametersHaveChanged);
		}

		[TestMethod]
		public void GraphicsProcessing_NotChanged_2()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "this",
				Value = "default value",
				Id = ProfileParameterGuids.RemoteGraphics,
				DefaultValue = new ParameterValue { StringValue = "default value" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
			};

			profileParameter.AcceptChanges(null);

			Assert.IsFalse(((ProfileParameterChangeSummary)profileParameter.Change.Summary).NewGraphicsProcessingNeedsToBeAdded);
		}

		[TestMethod]
		public void GraphicsProcessing_Changed_2()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "this",
				Value = "default value",
				Id = ProfileParameterGuids.RemoteGraphics,
				DefaultValue = new ParameterValue { StringValue = "default value" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
			};

			profileParameter.AcceptChanges(null);

			profileParameter.Value = "non-default value";

			Assert.IsTrue(((ProfileParameterChangeSummary)profileParameter.Change.Summary).NewGraphicsProcessingNeedsToBeAdded);
		}

		[TestMethod]
		public void GraphicsProcessing_NotChanged_3()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "this",
				Value = "default value",
				Id = ProfileParameterGuids.RemoteGraphics,
				DefaultValue = new ParameterValue { StringValue = "default value" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
			};

			profileParameter.AcceptChanges(null);

			Assert.IsFalse(((ProfileParameterChangeSummary)profileParameter.Change.Summary).CapabilitiesChanged);
		}

		[TestMethod]
		public void GraphicsProcessing_Changed_3()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "this",
				Value = "default value",
				Id = ProfileParameterGuids.RemoteGraphics,
				DefaultValue = new ParameterValue { StringValue = "default value" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
			};

			profileParameter.AcceptChanges(null);

			profileParameter.Value = "non-default value";

			Assert.IsTrue(((ProfileParameterChangeSummary)profileParameter.Change.Summary).CapabilitiesChanged);
		}
	}
}

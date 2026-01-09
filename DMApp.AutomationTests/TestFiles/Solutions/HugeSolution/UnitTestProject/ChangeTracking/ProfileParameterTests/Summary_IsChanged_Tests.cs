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
	public class Summary_IsChanged_Tests
	{
		[TestMethod]
		public void CompareToSelf_NotChanged_1()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "this",
				Value = "value 1"
			};

			profileParameter.AcceptChanges(null);

			Assert.IsFalse(profileParameter.Change.Summary.IsChanged);
		}

		[TestMethod]
		public void CompareToSelf_Changed_1()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "this",
			};

			profileParameter.AcceptChanges(null);

			profileParameter.Value = "value 1";

			Assert.IsTrue(profileParameter.Change.Summary.IsChanged);
		}

		[TestMethod]
		public void CompareToSelf_Changed_2()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "this",
				Value = "value 1"
			};

			profileParameter.AcceptChanges(null);

			profileParameter.Value = "new value";

			Assert.IsTrue(profileParameter.Change.Summary.IsChanged);
		}

		[TestMethod]
		public void CompareToOther_Changed()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "this",
				Id = ProfileParameterGuids.VideoFormat,
				DefaultValue = new ParameterValue { StringValue = "test 1" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
				Value = "value 2"
			};

			var otherProfileParameter = new ProfileParameter
			{
				Name = "other",
				Id = ProfileParameterGuids.VideoFormat,
				DefaultValue = new ParameterValue { StringValue = "test 1" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
				Value = "value 1"
			};

			var change = profileParameter.GetChangeComparedTo(null, otherProfileParameter);

			Assert.IsTrue(change.Summary.IsChanged);
		}

		[TestMethod]
		public void CompareToOther_NotChanged()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "this",
				Id = ProfileParameterGuids.VideoFormat,
				DefaultValue = new ParameterValue { StringValue = "test 1" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
				Value = "value 1"
			};

			var otherProfileParameter = new ProfileParameter
			{
				Name = "other",
				Id = ProfileParameterGuids.VideoFormat,
				DefaultValue = new ParameterValue { StringValue = "test 1" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
				Value = "value 1"
			};

			var change = profileParameter.GetChangeComparedTo(null, otherProfileParameter);

			Assert.IsFalse(change.Summary.IsChanged);
		}
	}
}

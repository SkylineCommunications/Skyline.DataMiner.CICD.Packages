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
	public class RecordingConfiguration_Tests
	{
		[TestMethod]
		public void NotChanged_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.RecordingConfiguration = new RecordingConfiguration();

			service.AcceptChanges(null);

			Assert.IsFalse(service.Change.Summary.IsChanged);
		}

		[TestMethod]
		public void Removed_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.RecordingConfiguration = new RecordingConfiguration();

			service.AcceptChanges(null);

			service.RecordingConfiguration = null;

			var changeSummary = service.Change.Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.PropertyChangeSummary.IsChanged);
		}

		[TestMethod]
		public void Property_Changed_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.RecordingConfiguration = new RecordingConfiguration();

			service.AcceptChanges(null);

			service.RecordingConfiguration.FastRerunCopy = true;

			var changeSummary = service.Change.Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.PropertyChangeSummary.IsChanged);
		}

		[TestMethod]
		public void Property_Added_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.RecordingConfiguration = new RecordingConfiguration();

			service.AcceptChanges(null);

			service.RecordingConfiguration.AddSubRecording(new SubRecording
			{
				Name = "first"
			});

			var changeSummary = service.Change.Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.PropertyChangeSummary.IsChanged);
		}

		[TestMethod]
		public void Property_Removed_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.RecordingConfiguration = new RecordingConfiguration();

			service.RecordingConfiguration.AddSubRecording(new SubRecording
			{
				Name = "first"
			});

			service.AcceptChanges(null);

			service.RecordingConfiguration.ClearSubRecordings();

			var changeSummary = service.Change.Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.PropertyChangeSummary.IsChanged);
		}

		[TestMethod]
		public void SubRecording_Changed_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.RecordingConfiguration = new RecordingConfiguration();

			service.RecordingConfiguration.AddSubRecording(new SubRecording
			{
				Name = "first"
			});

			service.AcceptChanges(null);

			service.RecordingConfiguration.SubRecordings[0].AdditionalInformation = "random text";

			var changeSummary = service.Change.Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.PropertyChangeSummary.IsChanged);
		}




		[TestMethod]
		public void NotChanged_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.RecordingConfiguration = new RecordingConfiguration();

			var other = Helper.GetDefaultServiceForTesting();

			other.Functions = service.Functions.Select(f => new Function
			{
				Name = "function",
				Id = f.Id,
				Definition = new FunctionDefinition
				{
					Label = f.Definition.Label
				},
				RequiresResource = true,
				Parameters = new List<ProfileParameter> { Helper.GetDefaultProfileParameterForTesting() },
				Resource = f.Resource
			}).ToList();

			other.RecordingConfiguration = new RecordingConfiguration();

			var changeSummary = service.GetChangeComparedTo(null, other).Summary as ServiceChangeSummary;

			Assert.IsFalse(changeSummary.PropertyChangeSummary.IsChanged);
		}

		[TestMethod]
		public void Property_Changed_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.RecordingConfiguration = new RecordingConfiguration();

			var other = Helper.GetDefaultServiceForTesting();

			other.Functions = service.Functions.Select(f => new Function
			{
				Name = "function",
				Id = f.Id,
				Definition = new FunctionDefinition
				{
					Label = f.Definition.Label
				},
				RequiresResource = true,
				Parameters = new List<ProfileParameter> { Helper.GetDefaultProfileParameterForTesting() },
				Resource = f.Resource
			}).ToList();

			other.RecordingConfiguration = new RecordingConfiguration { SubtitleProxy = true };

			var changeSummary = service.GetChangeComparedTo(null, other).Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.PropertyChangeSummary.IsChanged);
		}

		[TestMethod]
		public void Property_Added_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.RecordingConfiguration = new RecordingConfiguration();

			var other = Helper.GetDefaultServiceForTesting();

			other.Functions = service.Functions.Select(f => new Function
			{
				Name = "function",
				Id = f.Id,
				Definition = new FunctionDefinition
				{
					Label = f.Definition.Label
				},
				RequiresResource = true,
				Parameters = new List<ProfileParameter> { Helper.GetDefaultProfileParameterForTesting() },
				Resource = f.Resource
			}).ToList();

			other.RecordingConfiguration = new RecordingConfiguration();

			other.RecordingConfiguration.AddSubRecording(new SubRecording { Name = "test" });

			var changeSummary = service.GetChangeComparedTo(null, other).Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.PropertyChangeSummary.IsChanged);
		}

		[TestMethod]
		public void Property_Removed_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.RecordingConfiguration = new RecordingConfiguration();

			service.RecordingConfiguration.AddSubRecording(new SubRecording { Name = "test" });

			var other = Helper.GetDefaultServiceForTesting();

			other.Functions = service.Functions.Select(f => new Function
			{
				Name = "function",
				Id = f.Id,
				Definition = new FunctionDefinition
				{
					Label = f.Definition.Label
				},
				RequiresResource = true,
				Parameters = new List<ProfileParameter> { Helper.GetDefaultProfileParameterForTesting() },
				Resource = f.Resource
			}).ToList();

			other.RecordingConfiguration = new RecordingConfiguration();

			var changeSummary = service.GetChangeComparedTo(null, other).Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.PropertyChangeSummary.IsChanged);
		}

		[TestMethod]
		public void SubRecording_Changed_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.RecordingConfiguration = new RecordingConfiguration();

			service.RecordingConfiguration.AddSubRecording(new SubRecording { Name = "test", AdditionalInformation = "first" });

			var other = Helper.GetDefaultServiceForTesting();

			other.Functions = service.Functions.Select(f => new Function
			{
				Name = "function",
				Id = f.Id,
				Definition = new FunctionDefinition
				{
					Label = f.Definition.Label
				},
				RequiresResource = true,
				Parameters = new List<ProfileParameter> { Helper.GetDefaultProfileParameterForTesting() },
				Resource = f.Resource
			}).ToList();

			other.RecordingConfiguration = new RecordingConfiguration();

			other.RecordingConfiguration.AddSubRecording(new SubRecording { Name = "test", AdditionalInformation = "second" });

			var changeSummary = service.GetChangeComparedTo(null, other).Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.PropertyChangeSummary.IsChanged);
		}
	}
}

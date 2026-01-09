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
	public class Function_Tests
	{
		[TestMethod]
		public void NotChanged_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.AcceptChanges(null);

			Assert.IsFalse(service.Change.Summary.IsChanged);
		}

		[TestMethod]
		public void NotChanged_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();

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

			var change = service.GetChangeComparedTo(null, other) as ServiceChange;

			var changeSummary = change.Summary as ServiceChangeSummary;

			Assert.IsFalse(changeSummary.IsChanged);
		}

		[TestMethod]
		public void Changed_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.AcceptChanges(null);

			service.Functions[0].Parameters[0].Value = "new random value";

			Assert.IsTrue(service.Change.Summary.IsChanged);
		}

		[TestMethod]
		public void ProfileParameterChanged_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.AcceptChanges(null);

			service.Functions[0].Parameters[0].Value = "new random value";

			var changeSummary = service.Change.Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.FunctionChangeSummary.ProfileParameterChangeSummary.IsChanged);
		}

		[TestMethod]
		public void ResourceChanged_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.AcceptChanges(null);

			service.Functions[0].Resource = new FunctionResource
			{
				GUID = Guid.NewGuid()
			};

			var changeSummary = service.Change.Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.FunctionChangeSummary.ResourceChangeSummary.IsChanged);
		}

		[TestMethod]
		public void IsMissingResources_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.AcceptChanges(null);

			service.Functions[0].Resource = null;

			var changeSummary = service.Change.Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.IsMissingResources);
		}

		[TestMethod]
		public void ResourceAtBeginningOfServiceDefinitionChanged_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.AcceptChanges(null);

			service.Functions.Single(f => service.Definition.FunctionIsFirst(f)).Resource = new FunctionResource
			{
				GUID = Guid.NewGuid()
			};

			var changeSummary = service.Change.Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.FunctionChangeSummary.ResourceChangeSummary.ResourceAtBeginningOfServiceDefinitionChanged);
		}

		[TestMethod]
		public void ResourceAtEndOfServiceDefinitionChanged_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.AcceptChanges(null);

			service.Functions.Single(f => service.Definition.FunctionIsLast(f)).Resource = new FunctionResource
			{
				GUID = Guid.NewGuid()
			};

			var changeSummary = service.Change.Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.FunctionChangeSummary.ResourceChangeSummary.ResourceAtEndOfServiceDefinitionChanged);
		}

		[TestMethod]
		public void ResourceAtBeginningOfServiceDefinitionRemoved_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.Functions.Single(f => service.Definition.FunctionIsFirst(f)).Resource = new FunctionResource
			{
				GUID= Guid.NewGuid()
			};

			service.AcceptChanges(null);

			service.Functions.Single(f => service.Definition.FunctionIsFirst(f)).Resource = null;

			var changeSummary = service.Change.Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.FunctionChangeSummary.ResourceChangeSummary.ResourceAtBeginningOfServiceDefinitionChanged);
		}

		[TestMethod]
		public void ResourceAtEndOfServiceDefinitionRemoved_ComparedToSelf()
		{
			var service = Helper.GetDefaultServiceForTesting();

			service.Functions.Single(f => service.Definition.FunctionIsLast(f)).Resource = new FunctionResource
			{
				GUID = Guid.NewGuid()
			};

			service.AcceptChanges(null);

			service.Functions.Single(f => service.Definition.FunctionIsLast(f)).Resource = null;

			var changeSummary = service.Change.Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.FunctionChangeSummary.ResourceChangeSummary.ResourceAtEndOfServiceDefinitionChanged);
		}

		[TestMethod]
		public void Changed_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();
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
				Parameters = new List<ProfileParameter>{ Helper.GetDefaultProfileParameterForTesting() },
				Resource = f.Resource
			}).ToList();

			other.Functions[0].Parameters[0].Value = "new random value";

			var changeSummary = service.GetChangeComparedTo(null, other).Summary as ServiceChangeSummary; 

			Assert.IsTrue(changeSummary.IsChanged);
		}

		[TestMethod]
		public void ProfileParameterChanged_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();
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

			other.Functions[0].Parameters[0].Value = "new random value";

			var changeSummary = service.GetChangeComparedTo(null, other).Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.FunctionChangeSummary.ProfileParameterChangeSummary.IsChanged);
		}

		[TestMethod]
		public void ResourceChanged_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();
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

			service.AcceptChanges(null);

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

			other.Functions[0].Resource = new FunctionResource
			{
				GUID = Guid.NewGuid()
			};

			var changeSummary = service.GetChangeComparedTo(null, other).Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.FunctionChangeSummary.ResourceChangeSummary.IsChanged);
		}

		[TestMethod]
		public void IsMissingResources_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();
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

			service.Functions[0].Resource = null;

			var changeSummary = service.GetChangeComparedTo(null, other).Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.IsMissingResources);
		}

		[TestMethod]
		public void ResourceAtBeginningOfServiceDefinitionChanged_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();
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

			other.Functions.Single(f => service.Definition.FunctionIsFirst(f.Definition.Label)).Resource = new FunctionResource
			{
				GUID = Guid.NewGuid()
			};

			service.AcceptChanges(null);

			var changeSummary = service.GetChangeComparedTo(null, other).Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.FunctionChangeSummary.ResourceChangeSummary.ResourceAtBeginningOfServiceDefinitionChanged);
		}

		[TestMethod]
		public void ResourceAtEndOfServiceDefinitionChanged_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();
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

			other.Functions.Single(f => service.Definition.FunctionIsLast(f)).Resource = new FunctionResource
			{
				GUID = Guid.NewGuid()
			};

			service.AcceptChanges(null);

			var changeSummary = service.GetChangeComparedTo(null, other).Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.FunctionChangeSummary.ResourceChangeSummary.ResourceAtEndOfServiceDefinitionChanged);
		}

		[TestMethod]
		public void ResourceAtBeginningOfServiceDefinitionRemoved_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();
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

			service.Functions.Single(f => service.Definition.FunctionIsFirst(f)).Resource = new FunctionResource
			{
				GUID = Guid.NewGuid()
			};

			service.AcceptChanges(null);

			other.Functions.Single(f => service.Definition.FunctionIsFirst(f.Definition.Label)).Resource = null;

			var changeSummary = service.GetChangeComparedTo(null, other).Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.FunctionChangeSummary.ResourceChangeSummary.ResourceAtBeginningOfServiceDefinitionChanged);
		}

		[TestMethod]
		public void ResourceAtEndOfServiceDefinitionRemoved_ComparedToOther()
		{
			var service = Helper.GetDefaultServiceForTesting();
			var other = Helper.GetDefaultServiceForTesting();

			other.Definition = service.Definition;

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

			service.Functions.Single(f => service.Definition.FunctionIsLast(f)).Resource = new FunctionResource
			{
				GUID = Guid.NewGuid()
			};

			service.AcceptChanges(null);

			other.Functions.Single(f => other.Definition.FunctionIsLast(f)).Resource = null;

			var changeSummary = service.GetChangeComparedTo(null, other).Summary as ServiceChangeSummary;

			Assert.IsTrue(changeSummary.FunctionChangeSummary.ResourceChangeSummary.ResourceAtEndOfServiceDefinitionChanged);
		}
	}
}

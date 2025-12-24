using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace UnitTestProject.ICloneable
{
	[TestClass]
	public class ServiceTests
	{
		[TestMethod]
		public void ID_NotChanged()
		{
			var identifiers = Identifiers.CreateRandom();

			var service = Helper.GetDefaultServiceForTesting(identifiers);

			var clone = service.Clone() as Service;

			Assert.AreEqual(service.Id, clone.Id);
		}

		[TestMethod]
		public void ID_Changed()
		{
			var identifiers = Identifiers.CreateRandom();

			var service = Helper.GetDefaultServiceForTesting(identifiers);

			var clone = service.Clone() as Service;

			service.Id = Guid.NewGuid();

			Assert.AreNotEqual(service.Id, clone.Id);
		}

		[TestMethod]
		public void Name_NotChanged()
		{
			var identifiers = Identifiers.CreateRandom();

			var service = Helper.GetDefaultServiceForTesting(identifiers);

			var clone = service.Clone() as Service;

			Assert.AreEqual(service.Name, clone.Name);
		}

		[TestMethod]
		public void Name_Changed()
		{
			var identifiers = Identifiers.CreateRandom();

			var service = Helper.GetDefaultServiceForTesting(identifiers);

			var clone = service.Clone() as Service;

			service.Name = "new name";

			Assert.AreNotEqual(service.Name, clone.Name);
		}

		[TestMethod]
		public void ServiceDefinition_NotChanged()
		{
			var identifiers = Identifiers.CreateRandom();

			var service = Helper.GetDefaultServiceForTesting(identifiers);

			var clone = service.Clone() as Service;

			Assert.AreEqual(service.Definition.Id, clone.Definition.Id);
		}

		[TestMethod]
		public void ServiceDefinition_Changed()
		{
			var identifiers = Identifiers.CreateRandom();

			var service = Helper.GetDefaultServiceForTesting(identifiers);

			var clone = service.Clone() as Service;

			service.Definition = Helper.GetDefaultServiceDefinitionForTesting();

			Assert.AreNotEqual(service.Definition.Id, clone.Definition.Id);
		}

		[TestMethod]
		public void OrderRefs_NotChanged()
		{
			var identifiers = Identifiers.CreateRandom();

			var service = Helper.GetDefaultServiceForTesting(identifiers);
			service.OrderReferences = new System.Collections.Generic.HashSet<Guid> { Guid.NewGuid() };

			var clone = service.Clone() as Service;

			Assert.IsTrue(service.OrderReferences.SequenceEqual(clone.OrderReferences));
		}

		[TestMethod]
		public void OrderRefs_Changed()
		{
			var identifiers = Identifiers.CreateRandom();

			var service = Helper.GetDefaultServiceForTesting(identifiers);
			service.OrderReferences = new System.Collections.Generic.HashSet<Guid> { Guid.NewGuid() };

			var clone = service.Clone() as Service;

			service.OrderReferences = new System.Collections.Generic.HashSet<Guid> { Guid.NewGuid() };

			Assert.IsFalse(service.OrderReferences.SequenceEqual(clone.OrderReferences));
		}

		[TestMethod]
		public void Children_NotChanged()
		{
			var identifiers = Identifiers.CreateRandom();

			var service = Helper.GetDefaultServiceForTesting(identifiers);

			var childIdentifiers = Identifiers.CreateRandom();
			var childService = Helper.GetDefaultServiceForTesting(childIdentifiers);

			service.Children.Add(childService);

			var clone = service.Clone() as Service;

			Assert.AreEqual(service.Children.Single().Id, clone.Children.Single().Id);
		}

		[TestMethod]
		public void Children_Changed()
		{
			var identifiers = Identifiers.CreateRandom();

			var service = Helper.GetDefaultServiceForTesting(identifiers);

			var childIdentifiers = Identifiers.CreateRandom();
			var childService = Helper.GetDefaultServiceForTesting(childIdentifiers);

			service.Children.Add(childService);

			var clone = service.Clone() as Service;

			service.Children.Remove(childService);
			service.Children.Add(Helper.GetDefaultServiceForTesting());

			Assert.AreNotEqual(service.Children.Single().Id, clone.Children.Single().Id);
		}
	}
}

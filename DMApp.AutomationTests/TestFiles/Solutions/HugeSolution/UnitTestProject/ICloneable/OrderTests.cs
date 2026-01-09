namespace UnitTestProject.ICloneable
{
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;

	[TestClass]
	public class OrderTests
	{
		[TestMethod]
		public void SourceService_ReferenceIsSame()
		{
			var sourceServiceIdentifiers = Identifiers.CreateRandom();
			var sourceService = Helper.GetDefaultServiceForTesting(sourceServiceIdentifiers);

			var routingServiceIdentifiers = Identifiers.CreateRandom();
			var routingService = Helper.GetDefaultServiceForTesting(routingServiceIdentifiers);

			var destinationServiceIdentifiers = Identifiers.CreateRandom();
			var destinationService = Helper.GetDefaultServiceForTesting(destinationServiceIdentifiers);

			sourceService.Children.Add(routingService);
			routingService.Children.Add(destinationService);

			var order = new Order
			{
				Sources = new List<Service> { sourceService },
			};

			var clone = order.Clone() as Order;

			Assert.AreSame(clone.SourceService, clone.Sources.Single());
		}
	}
}

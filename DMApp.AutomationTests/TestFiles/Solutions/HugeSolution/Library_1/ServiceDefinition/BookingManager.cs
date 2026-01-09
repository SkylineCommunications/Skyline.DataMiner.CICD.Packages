namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;

	public class YleBookingManager
	{
		private const int BookingManagerProtocolVirtualPlatformParameterId = 123;

		private readonly Helpers helpers;
		private string virtualPlatform;
		private BookingManager bookingManager;

		public YleBookingManager(Helpers helpers, Element element)
		{
			this.helpers = helpers;
			Element = element;
		}

		public Element Element { get; }

		public BookingManager BookingManager => bookingManager ?? (bookingManager = new BookingManager((Engine)helpers.Engine, Element));

		public VirtualPlatform VirtualPlatform => (virtualPlatform ?? (virtualPlatform = Convert.ToString(DataMinerInterface.Element.GetParameter(helpers, Element, BookingManagerProtocolVirtualPlatformParameterId)))).GetEnumValue<VirtualPlatform>();
	}
}

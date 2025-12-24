namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.EventArguments
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;

	public class ServiceReplacedEventArgs : EventArgs
	{
		public ServiceReplacedEventArgs(Service replacedService, Service replacingService)
		{
			ReplacedService = replacedService;
			ReplacingService = replacingService;
		}

		public Service ReplacedService { get; }

		public Service ReplacingService { get; }
	}
}

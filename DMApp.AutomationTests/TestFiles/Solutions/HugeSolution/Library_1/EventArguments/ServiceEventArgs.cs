namespace Library_1.EventArguments
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;

	public class ServiceEventArgs : EventArgs
	{
		public ServiceEventArgs(Service service)
		{
			Service = service;
		}

		public Service Service { get; }
	}
}

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.FixedResourceHandlers
{
	using System;

	public interface IFixedResourceConfiguration
	{
		Guid FunctionId { get; }

		string ResourceName { get; }
	}
}

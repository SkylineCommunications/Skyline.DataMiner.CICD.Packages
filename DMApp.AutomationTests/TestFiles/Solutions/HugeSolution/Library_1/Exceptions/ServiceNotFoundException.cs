namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	[Serializable]
	public class ServiceNotFoundException : MediaServicesException
	{
		public ServiceNotFoundException()
		{
		}

		public ServiceNotFoundException(string nameOrMessage, bool useMessageInsteadOfName = false)
			: base(useMessageInsteadOfName ? nameOrMessage : $"Unable to find Service with name {nameOrMessage}")
		{
		}

		public ServiceNotFoundException(Guid ID)
			: base($"Unable to find Service with ID {ID}")
		{
		}

		public ServiceNotFoundException(string name, Guid ID)
			: base($"Unable to find Service with name {name} and ID {ID}")
		{
		}

		public ServiceNotFoundException(VirtualPlatform virtualPlatform)
			: base($"Unable to find Service with Virtual Platform {EnumExtensions.GetDescriptionFromEnumValue(virtualPlatform)}")
		{
		}

		public ServiceNotFoundException(VirtualPlatformType virtualPlatformType)
			: base($"Unable to find Service with Virtual Platform Type {EnumExtensions.GetDescriptionFromEnumValue(virtualPlatformType)}")
		{
		}

		public ServiceNotFoundException(VirtualPlatformName virtualPlatformName)
			: base($"Unable to find Service with Virtual Platform Name {EnumExtensions.GetDescriptionFromEnumValue(virtualPlatformName)}")
		{
		}

		public ServiceNotFoundException(BackupType backupType)
			: base($"Unable to find Service with Backup Type {backupType.ToString()}")
		{
		}

		public ServiceNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
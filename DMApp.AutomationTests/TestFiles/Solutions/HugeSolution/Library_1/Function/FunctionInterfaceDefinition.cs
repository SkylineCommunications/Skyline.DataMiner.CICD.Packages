namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function
{
	using System;
	using Library_1.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ServiceManager.Objects;

	public class FunctionInterfaceDefinition : ICloneable
	{
		public FunctionInterfaceDefinition(Helpers helpers, Net.Messages.FunctionInterface netInterface)
		{
			Id = netInterface.Id;
			Type = netInterface.InterfaceType;
			ProfileDefinition = helpers.ProfileManager.GetProfileDefinition(netInterface.ProfileDefinition);
		}

		public int Id { get; set; }

		public InterfaceType Type { get; set; }

		public ProfileDefinition ProfileDefinition { get; set; }

		private FunctionInterfaceDefinition(FunctionInterfaceDefinition other)
		{
			CloneHelper.CloneProperties(other, this);
		}

		public object Clone()
		{
			return new FunctionInterfaceDefinition(this);
		}
	}
}

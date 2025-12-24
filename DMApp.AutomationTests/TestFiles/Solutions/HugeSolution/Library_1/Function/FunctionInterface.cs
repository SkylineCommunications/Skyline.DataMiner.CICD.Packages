namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Library_1.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.Net.ServiceManager.Objects;

	public class FunctionInterface : ICloneable
	{
		public FunctionInterface(FunctionInterfaceDefinition interfaceDefinition)
		{
			Id = interfaceDefinition.Id;
			Type = interfaceDefinition.Type;

			Parameters = interfaceDefinition.ProfileDefinition.ProfileParameters.Select(pp => pp.Clone()).Cast<ProfileParameter>().ToList();
		}

		private FunctionInterface(FunctionInterface other)
		{
			Parameters = other.Parameters.Select(p => p.Clone()).Cast<ProfileParameter>().ToList();

			CloneHelper.CloneProperties(other, this);
		}

		public int Id { get; set; }

		public InterfaceType Type { get; set; }

		public List<ProfileParameter> Parameters { get; }

		public Library.Solutions.SRM.Model.Interface GetInterfaceForBooking()
		{
			return new Library.Solutions.SRM.Model.Interface
			{
				Id = Id,
				Parameters = Parameters.Select(pp => pp.GetParameterForBooking()).ToList(),
			};
		}

		public object Clone()
		{
			return new FunctionInterface(this);
		}
	}
}

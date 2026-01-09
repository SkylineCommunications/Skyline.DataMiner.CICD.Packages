namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System;
	using System.Globalization;

	public static class SrmParameterExtensions
	{
		public static object GetRealValue(this Skyline.DataMiner.Library.Solutions.SRM.Model.Parameter parameter, Library.Solutions.SRM.Model.ParameterType type)
		{
			switch (type)
			{
				case Library.Solutions.SRM.Model.ParameterType.Number:
					if (double.TryParse(parameter.Value, out double result)) return result; //VSC: convert from current culture!
					else throw new InvalidOperationException($"Unable to parse profile parameter '{parameter.Name}' ({parameter.Id}) value '{parameter.Value}' to a double");	

				case Library.Solutions.SRM.Model.ParameterType.Undefined:
				case Library.Solutions.SRM.Model.ParameterType.Path:
				case Library.Solutions.SRM.Model.ParameterType.Text:
				case Library.Solutions.SRM.Model.ParameterType.Discrete:
				case Library.Solutions.SRM.Model.ParameterType.ResourcePool:
					return parameter.Value;

				default:
					throw new InvalidOperationException("Unknown Parameter type");
			}	
		}
	}
}

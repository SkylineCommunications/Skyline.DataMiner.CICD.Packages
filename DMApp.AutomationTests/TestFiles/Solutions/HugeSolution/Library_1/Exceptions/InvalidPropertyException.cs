using System;
namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	public class InvalidPropertyException : Exception
	{
		public InvalidPropertyException(string propertyName, object propertyValue, object expectedPropertyValue) : base($"Property {propertyName} has value {Convert.ToString(propertyValue)} while expected value is {Convert.ToString(expectedPropertyValue)}")
		{

		}
	}
}

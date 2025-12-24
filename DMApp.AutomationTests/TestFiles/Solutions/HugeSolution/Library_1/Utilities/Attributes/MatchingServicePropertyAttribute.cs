namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes
{
	using System;

	/// <summary>
	/// Presence of this attribute means it contains the value of a service property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class MatchingServicePropertyAttribute : Attribute
	{
		public MatchingServicePropertyAttribute(string matchingServicePropertyName)
		{
			MatchingServicePropertyName = matchingServicePropertyName;
		}

		public string MatchingServicePropertyName { get; set; }
	}
}

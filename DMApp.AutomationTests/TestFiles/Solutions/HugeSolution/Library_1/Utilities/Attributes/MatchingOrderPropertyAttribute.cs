namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes
{
	using System;

	/// <summary>
	/// Presence of this attribute means it contains the value of an Order property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class MatchingOrderPropertyAttribute : Attribute
	{
		public MatchingOrderPropertyAttribute(string matchingOrderPropertyName)
		{
			MatchingOrderPropertyName = matchingOrderPropertyName;
		}

		public string MatchingOrderPropertyName { get; set; }
	}
}

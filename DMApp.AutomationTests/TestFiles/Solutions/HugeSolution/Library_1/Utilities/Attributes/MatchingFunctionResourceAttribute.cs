namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes
{
	using System;

	/// <summary>
	/// Presence of this attribute means the linked property contains the name of the resource assigned to the given function.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class MatchingFunctionResourceAttribute : Attribute
	{
		public MatchingFunctionResourceAttribute(string matchingFunctionId)
		{
			MatchingFunctionId = Guid.Parse(matchingFunctionId);
		}

		public Guid MatchingFunctionId { get; private set; }
	}
}

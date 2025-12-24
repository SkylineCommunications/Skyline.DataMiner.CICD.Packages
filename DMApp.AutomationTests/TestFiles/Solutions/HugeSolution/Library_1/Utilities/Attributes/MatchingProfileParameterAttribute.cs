namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes
{
	using System;

	/// <summary>
	/// Presence of this attribute means it contains the value of a profile parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class MatchingProfileParameterAttribute : Attribute
	{
		public MatchingProfileParameterAttribute(string profileParamId)
		{
			MatchingProfileParameterId = Guid.Parse(profileParamId);
		}

		public Guid MatchingProfileParameterId { get; set; }
	}
}

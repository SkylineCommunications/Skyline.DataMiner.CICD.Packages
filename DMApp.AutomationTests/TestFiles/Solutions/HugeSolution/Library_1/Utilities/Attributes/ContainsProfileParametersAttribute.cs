namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes
{
	using System;

	/// <summary>
	/// Presence of this attribute means the properties below contain profile parameter data, marked by <see cref="MatchingProfileParameterAttribute"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ContainsProfileParametersAttribute : Attribute
	{
	}
}

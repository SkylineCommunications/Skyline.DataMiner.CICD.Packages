namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes
{
	using System;

	/// <summary>
	/// Presence of this attribute means it contains the name of a resource.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class IsResourceNameAttribute : Attribute
	{
	}
}

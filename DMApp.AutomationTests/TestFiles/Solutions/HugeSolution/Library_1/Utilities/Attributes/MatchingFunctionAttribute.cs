using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes
{
	/// <summary>
	/// Presence of this attribute means the properties below contain resource or profile parameter data, marked by <see cref="IsResourceNameAttribute"/> and <see cref="MatchingProfileParameterAttribute"/> .
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class MatchingFunctionAttribute : Attribute
	{
		public MatchingFunctionAttribute(string matchingFunctionId)
		{
			MatchingFunctionId = Guid.Parse(matchingFunctionId);
		}

		public Guid MatchingFunctionId { get; set; }
	}
}

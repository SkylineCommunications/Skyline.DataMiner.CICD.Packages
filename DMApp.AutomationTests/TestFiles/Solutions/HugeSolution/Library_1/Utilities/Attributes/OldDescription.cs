namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	[AttributeUsage(AttributeTargets.All)]
	public sealed class OldDescriptionAttribute : Attribute
	{
		public OldDescriptionAttribute(string oldDescription)
		{
			Description = oldDescription;
		}

		public string Description { get; private set; }
	}
}

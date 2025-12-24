using System;
using System.Collections.Generic;
using System.Text;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.History
{
	public class ClassChangeSectionConfiguration
	{
		public List<string> HiddenSections { get; set; } = new List<string>();

		public bool PropertyShouldBeHidden(string propertyName)
		{
			return HiddenSections.Contains(propertyName);
		}

	}
}

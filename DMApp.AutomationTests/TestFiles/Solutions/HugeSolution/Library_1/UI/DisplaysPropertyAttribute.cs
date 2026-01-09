using System;
using System.Collections.Generic;
using System.Text;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class DisplaysPropertyAttribute : Attribute
	{
		public DisplaysPropertyAttribute(string propertyName)
		{
			PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
		}

		public string PropertyName { get; }
	}
}

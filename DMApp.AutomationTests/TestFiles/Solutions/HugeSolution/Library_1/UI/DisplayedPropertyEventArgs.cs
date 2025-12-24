using System;
using System.Collections.Generic;
using System.Text;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI
{
	public class DisplayedPropertyEventArgs
	{
		public DisplayedPropertyEventArgs(string propertyName, object propertyValue)
		{
			PropertyName = propertyName;
			PropertyValue = propertyValue;
		}

		public string PropertyName { get; }

		public object PropertyValue { get; }

	}
}

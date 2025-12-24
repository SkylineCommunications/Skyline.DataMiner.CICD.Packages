namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public interface IYleWidget
	{
		Guid Id { get; set; }

		string Name { get; set; }

		Helpers Helpers { get; set; }
	}
}

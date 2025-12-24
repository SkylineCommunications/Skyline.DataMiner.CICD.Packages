using System;
using System.Collections.Generic;
using System.Text;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation
{
	internal class AdditionalProfileParameterConfiguration
	{
		public Guid ProfileParameterId { get; set; }

		public List<Guid> Dependencies { get; set; } = new List<Guid>();

		public List<object> ValuesToNotSet { get; set; } = new List<object>();
	}
}

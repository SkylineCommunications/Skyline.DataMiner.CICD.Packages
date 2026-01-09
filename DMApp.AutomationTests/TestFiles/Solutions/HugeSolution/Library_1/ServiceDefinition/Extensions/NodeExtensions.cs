using System;
using System.Collections.Generic;
using System.Text;
using Skyline.DataMiner.Net.ServiceManager.Objects;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition
{
	public static class NodeExtensions
	{
		public static int GetHashCodeForYleProject(this Node node)
		{
			int hash = 17;
			
			hash = hash * 23 + node.ID.GetHashCode();
			hash = hash * 23 + node.Configuration.FunctionID.GetHashCode();
			hash = hash * 23 + node.Position.GetHashCode();
			
			return hash;
		}
	}
}

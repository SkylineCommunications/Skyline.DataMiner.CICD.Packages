namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition
{
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
    using System;

    public static class ServiceDefinitionExtensions
	{
		public static bool IsNewOrHasChangedComparedToExistingServiceDefinition(this Net.ServiceManager.Objects.ServiceDefinition serviceDefinition, Helpers helpers)
		{
			var existingServiceDefinition = DataMinerInterface.ServiceManager.GetServiceDefinition(helpers, serviceDefinition.ID);
			if (existingServiceDefinition == null) return true;

			return !serviceDefinition.Diagram.Matches(existingServiceDefinition.Diagram);
		}

		public static string DiagramToString(this Net.ServiceManager.Objects.Graph diagram)
		{
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));

			var sb = new StringBuilder();

			foreach (var node in diagram.Nodes)
			{
				var parentEdges = diagram.Edges.Where(e => e.ToNode.ID == node.ID);
				var parentNode = string.Join(",", parentEdges.Select(e => e.FromNode.ID));

				var childEdges = diagram.Edges.Where(e => e.FromNode.ID == node.ID);
				var childNodes = string.Join(",", childEdges.Select(e => e.ToNode.ID));

				var nodeInfo = $"Node {node.ID}: position=[{node.Position.Column},{node.Position.Row}], parents={parentNode}, children={childNodes}";

				sb.Append($"{nodeInfo} ; ");
			}

			return sb.ToString();
		}
	}
}

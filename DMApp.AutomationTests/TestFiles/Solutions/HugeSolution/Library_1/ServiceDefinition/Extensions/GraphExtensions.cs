namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ServiceManager.Objects;

	public static class GraphExtensions
	{
		public static int GetHashCodeForYleProject(this Graph graph)
		{
			int hash = 0;
			hash += graph.Nodes.Any() ? (int)graph.Nodes.Average(n => n.GetHashCodeForYleProject()) : 0;
			hash += graph.Edges.Any() ? (int)graph.Edges.Average(e => e.GetHashCode()) : 0;

			return hash;
		}

		/// <summary>
		/// Checks if a graph has the same function definitions as this graph and if the edges from this graph match with the edges of the other graph.
		/// </summary>
		/// <param name="graph">This graph.</param>
		/// <param name="otherGraph">Graph to compare with.</param>
		/// <returns>True if both graphs have the same function definitions and edges.</returns>
		public static bool Matches(this Graph graph, Graph otherGraph)
		{
			if (graph.Nodes.Count != otherGraph.Nodes.Count || graph.Edges.Count != otherGraph.Edges.Count)
				return false;

			foreach (var node in graph.Nodes)
			{
				var matchingOtherGraphNode = otherGraph.Nodes.FirstOrDefault(x => x.Configuration.FunctionID.Equals(node.Configuration.FunctionID) && x.ID == node.ID); // Function ID and Node ID should match

				if (matchingOtherGraphNode == null) return false;
			}

			var functionDefinitionEdges = graph.GetFunctionDefinitionEdges();
			var otherfunctionDefinitionEdges = otherGraph.GetFunctionDefinitionEdges();

			if (functionDefinitionEdges.Count != otherfunctionDefinitionEdges.Count) return false;

			foreach (var edge in functionDefinitionEdges)
			{
				if (!otherfunctionDefinitionEdges.Contains(edge))
				{
					return false;
				}
				else
				{
					otherfunctionDefinitionEdges.Remove(edge);
				}
			}

			return true;
		}

		public static List<Tuple<int, int>> GetFunctionDefinitionEdges(this Graph graph)
		{
			var functionDefinitionEdges = new List<Tuple<int, int>>();

			foreach (var edge in graph.Edges)
			{
				functionDefinitionEdges.Add(new Tuple<int, int>(edge.FromNodeID, edge.ToNodeID));
			}

			return functionDefinitionEdges;
		}

		public static int GetFunctionPosition(this Graph graph, Function function)
		{
			return graph.GetFunctionPosition(function.Definition.Label);
		}

		/// <summary>
		/// Gets the 0-based position of the function in the (straight-line) service definition.
		/// </summary>
		/// <remarks>The NodeId of a function does not always correspond to the position of the function in the service definition, hence this method.</remarks>
		public static int GetFunctionPosition(this Graph graph, string functionLabel)
		{
			bool nodeExistsWithMoreThanTwoEdges = graph.Nodes.Any(n => graph.Edges.Count(e => e.ToNode.Equals(n) || e.FromNode.Equals(n)) > 2);
			if (nodeExistsWithMoreThanTwoEdges) throw new ArgumentException("Graph is not a straight line", nameof(graph));

			var nodeOfFunction = graph.Nodes.SingleOrDefault(n => n.Label == functionLabel) ?? throw new NodeNotFoundException($"Unable to find node with label {functionLabel} between node labels {string.Join(", ", graph.Nodes.Select(nameof => nameof.Label))}");

			int position = 0;
			var node = nodeOfFunction;
			bool nodeHasChild = graph.Edges.Any(e => e.ToNode.Equals(node));
			while (nodeHasChild)
			{
				var edge = graph.Edges.FirstOrDefault(e => e.ToNode.Equals(node)) ?? throw new EdgeNotFoundException();

				if (edge.FromNode != null)
				{
					node = edge.FromNode;
				}
				else
				{
					break;
				}

				position++;
				nodeHasChild = graph.Edges.Any(e => e.ToNode.Equals(node));
			}

			return position;
		}

		public static bool IsValid(this Graph graph, Helpers helpers)
		{
			bool isValid = true;
			
			foreach (var node in graph.Nodes)
			{
				bool nodeHasMultipleParents = graph.Edges.Count(e => e.ToNodeID == node.ID) >= 2;

				if (nodeHasMultipleParents)
				{
					helpers.Log(nameof(Graph), nameof(IsValid), $"Node {node.ID} {node.Label} has multiple edges coming in");
				}

				isValid &= !nodeHasMultipleParents;
			}

			return isValid;
		}
	}
}
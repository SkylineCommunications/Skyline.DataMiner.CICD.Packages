namespace UnitTestProject
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.Library.Solutions.SRM.Model;
	using Skyline.DataMiner.Net.Profiles;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.ServiceManager.Objects;
	using Function = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function;
	using ServiceDefinition = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition;

	public static class Helper
	{
		public static Service GetDefaultServiceForTesting(Identifiers identifiers = null)
		{
			var random = new Random();

			var firstFunction = GetDefaultFunctionForTesting(random, identifiers?.FunctionAndResourceIdentifiers?.FirstOrDefault());
			var secondFunction = GetDefaultFunctionForTesting(random, identifiers?.FunctionAndResourceIdentifiers?.Skip(1)?.FirstOrDefault());

			var service = new DisplayedService
			{
				Id = identifiers?.ServiceIdentifier?.ID ?? Guid.NewGuid(),
				Name = identifiers?.ServiceIdentifier?.Name ?? $"Dummy [{Guid.NewGuid()}]",
				IsBooked = true,
				Definition = GetDefaultServiceDefinitionForTesting(identifiers, firstFunction, secondFunction),
				Functions = new List<Function> { firstFunction, secondFunction }
			};

			return service;
		}

		public static ServiceDefinition GetDefaultServiceDefinitionForTesting(Identifiers identifiers = null, params Function[] functions) 
		{
			var definition = new ServiceDefinition(VirtualPlatform.ReceptionNone)
			{
				Id = identifiers?.ServiceDefinitionIdentifier?.ID ?? Guid.NewGuid(),
				Name = identifiers?.ServiceDefinitionIdentifier?.Name ?? "Default Name",
				Description = "Default description",
				Diagram = GetGraphForFunctions(functions)
			};

			return definition;
		}

		public static Function GetDefaultFunctionForTesting(Random random = null, FunctionAndResourceIdentifiers identifiers = null)
		{
			random = random ?? new Random();

			var function = new Function
			{
				Name = identifiers?.FunctionIdentifier?.Name ?? "default function",
				Id = identifiers?.FunctionIdentifier?.ID ?? Guid.NewGuid(),
				Definition = new FunctionDefinition
				{
					Id = identifiers?.FunctionDefinitionIdentifier?.ID ?? Guid.NewGuid(),
					Label = identifiers?.FunctionDefinitionIdentifier?.Name ?? $"function {random.Next(1000000)}"
				},
				RequiresResource = true,
				Parameters = new List<ProfileParameter> { GetDefaultProfileParameterForTesting() },
				Resource = new FunctionResource 
				{ 
					Name = identifiers?.ResourceIdentifier?.Name ?? $"resource {random.Next(1000000)}",
					GUID = identifiers?.ResourceIdentifier?.ID ?? Guid.NewGuid()
				}
			};

			return function;
		}

		public static ProfileParameter GetDefaultProfileParameterForTesting()
		{
			var profileParameter = new ProfileParameter
			{
				Name = "profile parameter",
				Value = "default value",
				Id = Guid.Empty,
				DefaultValue = new ParameterValue { StringValue = "default value" },
				Type = ParameterType.Text,
				Category = ProfileParameterCategory.Capability,
			};

			return profileParameter;
		}

		public static Graph GetGraphForFunctions(params Function[] functions)
		{
			var functionLabels = functions.Select(f => f.Definition.Label).ToList();
			if (functionLabels.Any(l => functionLabels.Count(l2 => l2 == l) > 1)) throw new ArgumentException($"Multiple functions with same label: {string.Join(", ", functionLabels)}", nameof(functions));

			var graph = new Graph();

			var random = new Random();

			foreach (var function in functions)
			{
				graph.Nodes.Add(new Node
				{
					ID = random.Next(100),
					Label = function.Definition.Label,
				});
			}

			for (int i = 0; i < graph.Nodes.Count - 1; i++)
			{
				graph.Edges.Add(new Edge
				{
					FromNode = graph.Nodes[i],
					ToNode = graph.Nodes[i + 1],
				});
			}

			return graph;
		}
	}
}

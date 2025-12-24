namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.Library.Exceptions;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class RoutingService : LiveVideoService
	{
		private bool matrixInputSdiIsValid;
		private bool matrixOutputSdiIsValid;

		public RoutingService(Helpers helpers, Service service, LiveVideoOrder order)
: base(helpers, service, order)
		{
			if (!service.Functions.Any(f => f.Id == FunctionGuids.MatrixInputSdi)) throw new ArgumentException($"Service is missing a function with ID {FunctionGuids.MatrixInputSdi}", nameof(service));
			if (!service.Functions.Any(f => f.Id == FunctionGuids.MatrixOutputSdi)) throw new ArgumentException($"Service is missing a function with ID {FunctionGuids.MatrixOutputSdi}", nameof(service));

			MatrixInputSdiEnforceNone = MatrixInputSdi is null && MatrixInputSdiFunction.EnforceSelectedResource;
			MatrixOutputSdiEnforceNone = MatrixOutputSdi is null && MatrixOutputSdiFunction.EnforceSelectedResource;
		}

		public bool MatrixInputSdiEnforceNone { get; }

		public bool MatrixOutputSdiEnforceNone { get; }

		public FunctionResource MatrixInputSdi
		{
			get => MatrixInputSdiFunction.Resource;

			set
			{
				var currentMatrixInputSdiResource = MatrixInputSdi;

				if (currentMatrixInputSdiResource == null && value == null) return;
				if (currentMatrixInputSdiResource != null && currentMatrixInputSdiResource.Equals(value)) return;

				if (MatrixInputSdiIsValid)
				{
					Log($"{nameof(MatrixInputSdi)}.Set", $"Matrix Input SDI resource is already set to '{MatrixInputSdi?.Name}' and should not be changed anymore.");
					throw new InvalidOperationException($"Service {Service?.Name} matrix input SDI resource is already set to '{MatrixInputSdi?.Name}' and should not be changed anymore to '{value?.Name}'");
				}

				MatrixInputSdiFunction.Resource = value;
				MatrixInputSdiIsValid = true;
				Log($"{nameof(MatrixInputSdi)}.Set", $"Set service {Service.Name} function {MatrixInputSdiFunction.Name} resource to '{MatrixInputSdiFunction.ResourceName}'.");
			}
		}

		public bool MatrixInputSdiIsValid
		{
			get => matrixInputSdiIsValid;
			set
			{
				matrixInputSdiIsValid = value;
				Log($"{nameof(MatrixInputSdiIsValid)}.Set", $"Set service {Service.Name} Matrix Input SDI is Valid property to {matrixInputSdiIsValid}.");
			}
		}

		public FunctionResource MatrixOutputSdi
		{
			get => MatrixOutputSdiFunction.Resource;

			set
			{
				var currentMatrixOutputSdiResource = MatrixOutputSdi;

				if (currentMatrixOutputSdiResource == null && value == null) return;
				if (currentMatrixOutputSdiResource != null && currentMatrixOutputSdiResource.Equals(value)) return;

				if (MatrixOutputSdiIsValid)
				{
					Log($"{nameof(MatrixOutputSdi)}.Set", $"Matrix Output SDI resource is already set to '{MatrixOutputSdi?.Name}' and should not be changed anymore.");
					throw new InvalidOperationException($"Service {Service?.Name} matrix output SDI resource is already set to '{MatrixOutputSdi?.Name}' and should not be changed anymore to '{value?.Name}'");
				}

				MatrixOutputSdiFunction.Resource = value;
				MatrixOutputSdiIsValid = true;
				Log($"{nameof(MatrixOutputSdi)}.Set", $"Set service {Service.Name} function {MatrixOutputSdiFunction.Name} resource to '{MatrixOutputSdiFunction.ResourceName}'.");
			}
		}

		public bool MatrixOutputSdiIsValid
		{
			get => matrixOutputSdiIsValid;
			set
			{
				matrixOutputSdiIsValid = value;
				Log($"{nameof(MatrixOutputSdiIsValid)}.Set", $"Set service {Service.Name} Matrix Output SDI is Valid property to {matrixOutputSdiIsValid}.");
			}
		}

		public bool IsHmxMatrix => (MatrixInputSdi?.Name?.Contains("HMX") ?? false) || (MatrixOutputSdi?.Name?.Contains("HMX") ?? false);

		public bool IsNmxMatrix => (MatrixInputSdi?.Name?.Contains("NMX") ?? false) || (MatrixOutputSdi?.Name?.Contains("NMX") ?? false);

		public bool IsEduskuntaMatrix => (MatrixInputSdi?.Name?.Contains("EDUSKUNTA") ?? false) || (MatrixOutputSdi?.Name?.Contains("EDUSKUNTA") ?? false);

		private Function MatrixInputSdiFunction => Service.Functions.Single(f => f.Id == FunctionGuids.MatrixInputSdi);

		private Function MatrixOutputSdiFunction => Service.Functions.Single(f => f.Id == FunctionGuids.MatrixOutputSdi);

		public static RoutingService GenerateNewRoutingService(Helpers helpers, FunctionResource inputResource, FunctionResource outputResource, LiveVideoService output)
		{
			var serviceDefinition = helpers.ServiceDefinitionManager.RoutingServiceDefinition ?? throw new ServiceDefinitionNotFoundException("Unable to find Routing SD");

			var service = new DisplayedService
			{
				Start = output.Service.Start,
				End = output.Service.End,
				PreRoll = GetPreRoll(serviceDefinition, output.Service),
				PostRoll = ServiceManager.GetPostRollDuration(serviceDefinition),
				Definition = serviceDefinition,
				BackupType = output.Service.BackupType,
				IntegrationType = output.Service.IntegrationType
			};

			foreach (var node in serviceDefinition.Diagram.Nodes)
			{
				var functionDefinition = serviceDefinition.FunctionDefinitions.Single(fd => fd.Label == node.Label);

				var function = new DisplayedFunction(helpers, node, functionDefinition);

				if (functionDefinition.Id == FunctionGuids.MatrixInputSdi)
				{
					function.Resource = inputResource;
				}
				else if (functionDefinition.Id == FunctionGuids.MatrixOutputSdi)
				{
					function.Resource = outputResource;
				}

				service.Functions.Add(function);
			}

			var newRoutingService = new RoutingService(helpers, service, output.LiveVideoOrder)
			{
				MatrixInputSdiIsValid = true,
				MatrixOutputSdiIsValid = true
			};

			newRoutingService.Service.AcceptChanges();

			return newRoutingService;
		}

		/// <summary>
		/// Verifies if the resources from the other routing service match this service resources.
		/// </summary>
		/// <param name="otherRoutingService">The other routing service to compare with.</param>
		/// <returns>True if the resources match.</returns>
		public bool HasMatchingResources(RoutingService otherRoutingService)
		{
			if (MatrixInputSdi == null || MatrixOutputSdi == null) return false;

			if (otherRoutingService?.MatrixInputSdi == null || otherRoutingService.MatrixOutputSdi == null) return false;

			return MatrixInputSdi.Equals(otherRoutingService.MatrixInputSdi) && MatrixOutputSdi.Equals(otherRoutingService.MatrixOutputSdi);
		}

		public override string ToString()
		{
			return $"{Service.Name} ({MatrixInputSdi?.Name},{MatrixOutputSdi?.Name})";
		}
	}
}
namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Library_1.Utilities;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Constants = Configuration.Constants;
	using Service = Service.Service;

	public class Function : IYleChangeTracking, ICloneable
	{
		private FunctionResource initialResource;
		private FunctionResource resource;
		private bool enforceSelectedResource;

		private Func<string, string> resourceNameConverter;
		private readonly Func<string, string> defaultResourceNameConverter;

		public Function()
		{
			// Should only be used for unit tests or dummy services
			Id = FunctionGuids.Dummy;
			Name = "Dummy Function";
			Definition = FunctionDefinition.DummyFunctionDefinition();

			defaultResourceNameConverter = resourceName => ResourceExtensions.GetDisplayName(resourceName, Id);
			ResourceNameConverter = resourceName => resourceName;
		}

		protected Function(Helpers helpers, Net.ServiceManager.Objects.Node serviceDefinitionNode, FunctionDefinition functionDefinition)
		{
			if (serviceDefinitionNode == null) throw new ArgumentNullException(nameof(serviceDefinitionNode));
			if (functionDefinition == null) throw new ArgumentNullException(nameof(functionDefinition));

			defaultResourceNameConverter = resourceName => ResourceExtensions.GetDisplayName(resourceName, Id);
			ResourceNameConverter = resourceName => resourceName;

			var optionsProperty = serviceDefinitionNode.Properties.FirstOrDefault(p => p.Name == "Options");
			var isOptional = optionsProperty != null && optionsProperty.Value.Contains("Optional");

			var configurationOrderProperty = serviceDefinitionNode.Properties.FirstOrDefault(p => p.Name == "ConfigurationOrder");
			var configurationOrder = configurationOrderProperty != null ? Convert.ToInt32(configurationOrderProperty.Value) : 0;

			Id = serviceDefinitionNode.Configuration.FunctionID;
			NodeId = serviceDefinitionNode.ID;
			Definition = functionDefinition;
			Name = functionDefinition.Name;
			ConfigurationOrder = configurationOrder;
			IsOptional = isOptional;

			Parameters = functionDefinition.ProfileDefinition.ProfileParameters.Select(pp => pp.Clone()).Cast<ProfileParameter>().ToList();

			InputInterfaces = functionDefinition.InputInterfaces.Select(ii => new FunctionInterface(ii)).ToList();
			OutputInterfaces = functionDefinition.OutputInterfaces.Select(ii => new FunctionInterface(ii)).ToList();

			InitializeDefaultParameterValues();
		}

		protected Function(Helpers helpers, ReservationInstance reservation, Net.ServiceManager.Objects.Node serviceDefinitionNode, FunctionDefinition functionDefinition) : this(helpers, serviceDefinitionNode, functionDefinition)
		{
			if (reservation == null) throw new ArgumentNullException(nameof(reservation));

			// update function parameters and resource
			Library.Solutions.SRM.Model.Function functionInReservation = null;
			ServiceResourceUsageDefinition resourceUsageDefinition = null;

			var reservationFunctionData = reservation.GetFunctionData();
			if (reservationFunctionData.Length > 1)
			{
				functionInReservation = reservationFunctionData.SingleOrDefault(f => f.Id == NodeId);
				resourceUsageDefinition = reservation.ResourcesInReservationInstance.OfType<ServiceResourceUsageDefinition>().SingleOrDefault(r => r.ServiceDefinitionNodeID == NodeId);
			}
			else
			{
				functionInReservation = reservationFunctionData.SingleOrDefault();
				resourceUsageDefinition = reservation.ResourcesInReservationInstance.OfType<ServiceResourceUsageDefinition>().SingleOrDefault();
			}

			UpdateValuesBasedOnReservation(helpers, functionDefinition, functionInReservation, resourceUsageDefinition);
		}

		private Function(Function other)
		{
			this.defaultResourceNameConverter = other.defaultResourceNameConverter;

			Parameters = other.Parameters.Select(p => p.Clone()).Cast<ProfileParameter>().ToList();
			InputInterfaces = other.InputInterfaces.Select(p => p.Clone()).Cast<FunctionInterface>().ToList();
			OutputInterfaces = other.OutputInterfaces.Select(p => p.Clone()).Cast<FunctionInterface>().ToList();

			CloneHelper.CloneProperties(other, this);
		}

		/// <summary>
		/// The id of this function, this matches the Id of the FunctionDefinition.
		/// Only required when action is "NEW" or "EDIT"
		/// </summary>s
		public Guid Id { get; set; }

		/// <summary>
		/// The name of this function, this matches the name of the FunctionDefinition.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The node id of this function node in the service definition
		/// Only required when action is "NEW" or "EDIT"
		/// </summary>
		public int NodeId { get; set; }

		/// <summary>
		/// The configuration order property value in the service definition.
		/// </summary>
		public int ConfigurationOrder { get; set; }

		/// <summary>
		/// The list of parameters for the service
		/// </summary>
		public List<ProfileParameter> Parameters { get; set; } = new List<ProfileParameter>();

		public List<ProfileParameter> InterfaceParameters => InputInterfaces.SelectMany(ii => ii.Parameters).Concat(OutputInterfaces.SelectMany(oi => oi.Parameters)).ToList();

		public List<FunctionInterface> InputInterfaces { get; set; } = new List<FunctionInterface>();

		public List<FunctionInterface> OutputInterfaces { get; set; } = new List<FunctionInterface>();

		[JsonIgnore]
		public IEnumerable<ProfileParameter> NonDtrNonAudioProfileParameters
		{
			get
			{
				return Parameters.Where(p => !p.IsNonInterfaceDtrParameter && !ProfileParameterGuids.AllAudioChannelConfigurationGuids.Contains(p.Id));
			}
		}

		public IEnumerable<ProfileParameter> Capabilities => Parameters.Where(p => p.IsCapability).Concat(InterfaceParameters.Where(p => p.IsCapability));

		/// <summary>
		/// Gets or sets the resource used for this function.
		/// Should never be passed (internal property).
		/// </summary>
		public FunctionResource Resource
		{
			get => resource;

			set
			{
				bool sameResource = resource != null && resource.Equals(value);

				resource = value;

				// Clear Other Satellite Name profile parameter in case of Satellite Function and selected Resource is not Other
				if (Name.Equals("Satellite", StringComparison.InvariantCultureIgnoreCase) && resource != null && !resource.Name.Equals("Other", StringComparison.InvariantCultureIgnoreCase))
				{
					ProfileParameter otherSatelliteNameProfileParameter = Parameters.FirstOrDefault(x => x.Name.Equals("Other Satellite Name", StringComparison.InvariantCultureIgnoreCase));
					if (otherSatelliteNameProfileParameter != null) otherSatelliteNameProfileParameter.Value = String.Empty;
				}

				if (sameResource) return;

				ResourceChanged?.Invoke(this, new ResourceChangedEventArgs(ResourceName));
			}
		}

		public event EventHandler<ResourceChangedEventArgs> ResourceChanged;

		/// <summary>
		/// Property used by UI.
		/// </summary>
		public string ResourceName
		{
			get
			{
				if (Resource != null)
				{
					return ResourceNameConverter.Invoke(Resource.Name);
				}
				else
				{
					return Constants.None;
				}
			}
		}

		public Func<string, string> ResourceNameConverter
		{
			get => resourceNameConverter;
			set
			{
				resourceNameConverter = name => value(defaultResourceNameConverter(name));
				ResourceNameConverterChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public event EventHandler ResourceNameConverterChanged;

		/// <summary>
		/// Gets a boolean indicating if Change Tracking is enabled.
		/// </summary>
		/// <see cref="IYleChangeTracking"/>
		[JsonIgnore]
		public bool ChangeTrackingStarted { get; private set; }

		/// <summary>
		/// Function Definition for this function.
		/// Used in the function sections.
		/// </summary>
		[JsonIgnore]
		public FunctionDefinition Definition { get; set; }

		/// <summary>
		/// Indicates if this node is optional or not
		/// Only required when action is "NEW" or "EDIT"
		/// This can be read out from the "Options" property on the node, value will be "Optional" in case the node is optional
		/// </summary>
		public bool IsOptional { get; set; }

		public bool IsDummy => Parameters.SingleOrDefault(p => p.Id == ProfileParameterGuids._Dummy)?.Value?.Equals(true.ToString()) ?? false;

		public bool RequiresResource { get; set; } = true;

		public bool EnforceSelectedResource
		{
			get => enforceSelectedResource;
			set
			{
				enforceSelectedResource = value;
				EnforceSelectedResourceChanged?.Invoke(this, enforceSelectedResource);
			}
		}

		public event EventHandler<bool> EnforceSelectedResourceChanged;

		public bool McrHasOverruledFixedTieLineLogic { get; set; }

		public FunctionConfiguration Configuration
		{
			get
			{
				var functionConfiguration = new FunctionConfiguration
				{
					Id = Id,
					Name = Name,
					ResourceId = Resource?.ID ?? Guid.Empty,
					ResourceName = ResourceName,
					ProfileParameters = new Dictionary<Guid, object>(),
					RequiresResource = RequiresResource,
					ConfiguredByMcr = EnforceSelectedResource,
					McrHasOverruledFixedTieLineLogic = McrHasOverruledFixedTieLineLogic
				};

				if (Parameters != null)
				{
					foreach (var functionParameter in Parameters)
						functionConfiguration.ProfileParameters[functionParameter.Id] = functionParameter.Value;
				}

				if (InterfaceParameters != null)
				{
					foreach (var functionParameter in InterfaceParameters)
						functionConfiguration.ProfileParameters[functionParameter.Id] = functionParameter.Value;
				}

				return functionConfiguration;
			}
		}

		public void UpdateValuesBasedOnFunctionConfiguration(Helpers helpers, FunctionConfiguration functionConfiguration)
		{
			RequiresResource = functionConfiguration.RequiresResource;
			EnforceSelectedResource = functionConfiguration.ConfiguredByMcr;
			McrHasOverruledFixedTieLineLogic = functionConfiguration.McrHasOverruledFixedTieLineLogic;

			if (functionConfiguration.ResourceId != Guid.Empty)
			{
				Resource = DataMinerInterface.ResourceManager.GetResource(helpers, functionConfiguration.ResourceId) as FunctionResource;

				helpers.Log(nameof(Service), nameof(UpdateValuesBasedOnFunctionConfiguration), $"Set function {Name} resource to '{Resource?.Name}' based on service configuration");
			}

			foreach (var parameter in Parameters.Concat(InterfaceParameters))
			{
				parameter.Value = functionConfiguration.ProfileParameters.TryGetValue(parameter.Id, out var profileParameterValue) ? profileParameterValue : string.Empty;
			}

			helpers.Log(nameof(Service), nameof(UpdateValuesBasedOnFunctionConfiguration), $"All profile parameters based on configuration for function {Name}: '{string.Join(", ", Parameters.Concat(InterfaceParameters).Select(p => $"{p.Name}={p.Value}"))}'");
		}

		public void UpdateValuesBasedOnReservation(Helpers helpers, FunctionDefinition functionDefinition, Library.Solutions.SRM.Model.Function functionInReservation, ResourceUsageDefinition resourceUsageDefinition)
		{
			if (functionDefinition is null) throw new ArgumentNullException(nameof(functionDefinition));
			if (functionInReservation is null) throw new ArgumentNullException(nameof(functionInReservation));

			foreach (var definitionProfileParameterId in functionDefinition.ProfileDefinition.ProfileParameters.Select(pp => pp.Id))
			{
				// Loop over all profile params in the function definition to make sure all profile params are added/updated.
				// Warning: looping over profile params in the function from the reservation will not include empty params.

				var profileParameterToUpdate = Parameters.SingleOrDefault(p => p.Id == definitionProfileParameterId) ?? throw new NotFoundException($"Unable to find a profile parameter with ID {definitionProfileParameterId} in the {nameof(Parameters)} collection on Function object {Name}");

				// Update the value of the profile param with the value found in the reservation
				var reservationProfileParameter = functionInReservation.Parameters.SingleOrDefault(p => p.Id == definitionProfileParameterId);

				if (reservationProfileParameter != null)
				{
					profileParameterToUpdate.Value = reservationProfileParameter.GetRealValue(profileParameterToUpdate.Type);
				}
			}

			helpers.Log(nameof(Function), nameof(UpdateValuesBasedOnReservation), "WARNING: skipping getting values for interface profile params from reservation as we are not saving them on the reservation.");

			var inputInterfaceProfileParameterIds = functionDefinition.InputInterfaces.SelectMany(ii => ii.ProfileDefinition.ProfileParameters).Select(pp => pp.Id);
			var outputInterfaceProfileParameterIds = functionDefinition.OutputInterfaces.SelectMany(ii => ii.ProfileDefinition.ProfileParameters).Select(pp => pp.Id);

			foreach (var definitionProfileParameterId in inputInterfaceProfileParameterIds.Concat(outputInterfaceProfileParameterIds))
			{
				// Loop over all profile params in the function definition to make sure all profile params are added/updated.
				// Warning: looping over profile params in the function from the reservation will not include empty params.

				var profileParameterToUpdate = InterfaceParameters.SingleOrDefault(p => p.Id == definitionProfileParameterId) ?? throw new NotFoundException($"Unable to find an interface profile parameter with ID {definitionProfileParameterId} in the {nameof(InterfaceParameters)} collection on Function object {Name}");

				// Update the value of the profile param with the value found in the reservation
				var reservationProfileParameter = functionInReservation.Parameters.SingleOrDefault(p => p.Id == definitionProfileParameterId);

				if (reservationProfileParameter != null)
				{
					profileParameterToUpdate.Value = reservationProfileParameter.GetRealValue(profileParameterToUpdate.Type);
				}
			}

			if (resourceUsageDefinition != null && resourceUsageDefinition.GUID != Guid.Empty)
			{
				Resource = (FunctionResource)DataMinerInterface.ResourceManager.GetResource(helpers, resourceUsageDefinition.GUID);
			}
			else
			{
				Resource = null;
			}
		}

		/// <summary>
		/// Resets Change Tracking.
		/// </summary>
		/// <see cref="IYleChangeTracking"/>
		public void AcceptChanges(Helpers helpers = null)
		{
			helpers?.Log(nameof(Function), nameof(AcceptChanges), $"Accepting changes for function {Definition.Label}");

			ChangeTrackingStarted = true;

			foreach (var parameter in Parameters.Concat(InterfaceParameters))
			{
				parameter.AcceptChanges(helpers);
			}

			initialResource = Resource;
		}

		public Change GetChangeComparedTo<T>(Helpers helpers, T oldObjectInstance)
		{
			if (!(oldObjectInstance is Function oldFunction)) throw new ArgumentException($"Argument is not of type {nameof(Function)}", nameof(oldObjectInstance));

			oldFunction.AcceptChanges(helpers);

			var functionChange = new FunctionChange(this, oldFunction.initialResource, Resource);

			var oldProfileParameters = oldFunction.Parameters.Concat(oldFunction.InterfaceParameters).ToList();

			foreach (var profileParameter in Parameters.Concat(InterfaceParameters))
			{
				var oldProfileParameter = oldProfileParameters.SingleOrDefault(pp => pp.Id == profileParameter.Id) ?? throw new ProfileParameterNotFoundException(profileParameter.Id);

				var profileParameterChange = profileParameter.GetChangeComparedTo(helpers, oldProfileParameter);

				functionChange.TryAddChange(profileParameterChange);
			}

			return functionChange;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Function otherFunction)) return false;

			bool isEqual = true;

			var otherParameters = otherFunction.Parameters.Concat(otherFunction.InterfaceParameters).ToList();

			foreach (var parameter in Parameters.Concat(InterfaceParameters))
			{
				var otherParameter = otherParameters.SingleOrDefault(p => p.Id == parameter.Id);
				if (otherParameter is null) return false;

				isEqual &= otherParameter.Equals(parameter);
			}

			isEqual &= (Resource is null && otherFunction.Resource is null) || (Resource != null && Resource.Equals(otherFunction.Resource));

			isEqual &= Id == otherFunction.Id && Definition.Label == otherFunction.Definition.Label;

			return isEqual;
		}

		public bool TryUpdateFunctionMainElementProperty(Helpers helpers, string propertyName, string newPropertyValue, string existingPropertyValue = null)
		{
			try
			{
				var dms = Engine.SLNetRaw.GetDms();

				if (Resource == null)
				{
					helpers.Log(nameof(Function), nameof(TryUpdateFunctionMainElementProperty), $"Skipping property update as function: {Name} has no resource assigned.");
					return true;
				}

				var mainElementId = new DmsElementId(Resource.MainDVEDmaID, Resource.MainDVEElementID);
				if (!dms.ElementExists(mainElementId))
				{
					helpers.Log(nameof(Function), nameof(TryUpdateFunctionMainElementProperty), $"Skipping property: {propertyName} update for function: {Name} as the Main DVE Element with ID {mainElementId} could not be found.");
					return true;
				}

				var mainElement = helpers.Engine.FindElementByKey(mainElementId.Value);
				string savedValue = mainElement.GetPropertyValue(propertyName);

				if (newPropertyValue == String.Empty)
				{
					if (savedValue != existingPropertyValue)
					{
						helpers.Log(nameof(Function), nameof(TryUpdateFunctionMainElementProperty), $"Skipping property: {propertyName} clear for function: {Name} cause property has already a new value.");
						return true;
					}
				}
				else if (savedValue != newPropertyValue)
				{
					helpers.Log(nameof(Function), nameof(TryUpdateFunctionMainElementProperty), $"Update property: {propertyName} with value: {newPropertyValue} for function: {Name}.");

					mainElement.SetPropertyValue(propertyName, newPropertyValue);
					return Retry(() => { return mainElement.GetPropertyValue(propertyName) == newPropertyValue; }, TimeSpan.FromSeconds(5)); //Checking if property is eventually updated
				}
				else
				{
					// nothing
				}

				return false;
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Function), nameof(TryUpdateFunctionMainElementProperty), $"Something went wrong while updating property: {propertyName} value for function: {Name}: {e}");
				return false;
			}
		}

		public override int GetHashCode()
		{
			int hashCode = Id.GetHashCode();
			hashCode ^= (Definition?.Label == null) ? 1 : Definition.Label.GetHashCode();

			return hashCode;
		}

		[JsonIgnore]
		public Change Change
		{
			get
			{
				if (!ChangeTrackingStarted) throw new InvalidOperationException($"Change Tracking has not been started for object {UniqueIdentifier}");

				var functionChange = new FunctionChange(this, initialResource, Resource);

				functionChange.TryAddChanges(Parameters.Concat(InterfaceParameters).Select(pp => pp.Change).ToList());

				return functionChange;
			}
		}

		[JsonIgnore]
		public string UniqueIdentifier => Definition.Label;

		[JsonIgnore]
		public string DisplayName => UniqueIdentifier;

		/// <summary>
		/// Retry until success or until timeout. 
		/// </summary>
		/// <param name="func">Operation to retry.</param>
		/// <param name="timeout">Max TimeSpan during which the operation specified in <paramref name="func"/> can be retried.</param>
		/// <returns><c>true</c> if one of the retries succeeded within the specified <paramref name="timeout"/>. Otherwise <c>false</c>.</returns>
		private static bool Retry(Func<bool> func, TimeSpan timeout)
		{
			bool success = false;

			Stopwatch sw = new Stopwatch();
			sw.Start();

			do
			{
				success = func();
				if (!success)
				{
					System.Threading.Thread.Sleep(100);
				}
			}
			while (!success && sw.Elapsed <= timeout);

			return success;
		}

		private void InitializeDefaultParameterValues()
		{
			foreach (var profileParameter in Parameters)
			{
				switch (profileParameter.Type)
				{
					case Library.Solutions.SRM.Model.ParameterType.Text:
						profileParameter.Value = profileParameter.DefaultValue?.StringValue ?? String.Empty;
						break;
					case Library.Solutions.SRM.Model.ParameterType.Discrete:
						if (profileParameter.DefaultValue?.StringValue != null)
						{
							profileParameter.Value = profileParameter.DefaultValue.StringValue;
						}
						else if (!profileParameter.IsNonInterfaceDtrParameter) // don't set the value of a discrete DTR param if there is no default value defined
						{
							var defaultDiscreteValue = profileParameter.Discreets.FirstOrDefault(x => !String.IsNullOrWhiteSpace(x.InternalValue));
							profileParameter.Value = defaultDiscreteValue?.InternalValue ?? String.Empty;
						}
						else
						{
							// nothing
						}
						break;
					case Library.Solutions.SRM.Model.ParameterType.Number:
						profileParameter.Value = (profileParameter.DefaultValue != null) ? profileParameter.DefaultValue.DoubleValue : profileParameter.RangeMin;
						break;
					default:
						// Unsupported type
						break;
				}
			}
		}

		public object Clone()
		{
			return new Function(this);
		}

		public class ResourceChangedEventArgs : EventArgs
		{
			internal ResourceChangedEventArgs(string resourceName)
			{
				ResourceName = resourceName;
			}

			public string ResourceName { get; private set; }
		}

		public class SelectableResourcesChangedEventArgs : EventArgs
		{
			internal SelectableResourcesChangedEventArgs(IEnumerable<string> selectableResourceNames)
			{
				SelectableResourceNames = selectableResourceNames;
			}

			public IEnumerable<string> SelectableResourceNames { get; private set; }
		}
	}
}
namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	/// <summary>
	/// Base class for any Live Video service.
	/// </summary>
	public abstract class LiveVideoService : IEquatable<LiveVideoService>
	{
		private readonly List<LiveVideoService> children;
		private readonly Function firstResourceRequiringFunction;
		private readonly Function lastResourceRequiringFunction;

		protected readonly Helpers Helpers;

		protected LiveVideoService(Helpers helpers, Service service, LiveVideoOrder liveVideoOrder)
		{
			Helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			Service = service ?? throw new ArgumentNullException(nameof(service));
			LiveVideoOrder = liveVideoOrder ?? throw new ArgumentNullException(nameof(liveVideoOrder));

			firstResourceRequiringFunction = service.FirstResourceRequiringFunction;
			lastResourceRequiringFunction = service.LastResourceRequiringFunction;

			children = new List<LiveVideoService>();
			Children = children.AsReadOnly();
		}

		public LiveVideoOrder LiveVideoOrder { get; }

		/// <summary>
		/// The service object of this Live Video service.
		/// </summary>
		public Service Service { get; }

		public bool IsNew { get; protected set; }

		/// <summary>
		/// Indicates the parent of this Live Video service.
		/// </summary>
		public LiveVideoService Parent { get; set; }

		/// <summary>
		/// The list of children of this Live Video service.
		/// </summary>
		public ReadOnlyCollection<LiveVideoService> Children { get; private set; }

		public IReadOnlyCollection<LiveVideoService> AllChildrenAndGrandChildren => LiveVideoHelpers.FlattenServices(Children);

		/// <summary>
		/// The Input Routing service for this Live Video service.
		/// </summary>
		public RoutingService RoutingParent { get; set; }

		/// <summary>
		/// The Output Routing service for this Live Video service.
		/// </summary>
		public RoutingService RoutingChild { get; set; }

		/// <summary>
		/// The input resource for this Live Video service.
		/// </summary>
		public virtual FunctionResource InputResource => firstResourceRequiringFunction?.Resource;

		/// <summary>
		/// The output resource for this Live Video service.
		/// </summary>
		public virtual FunctionResource OutputResource => lastResourceRequiringFunction?.Resource;

		protected List<ProfileParameter> GetAllServiceProfileParameters()
		{
			return Service.Functions.SelectMany(f => f.Parameters).ToList();
		}

		public bool StartAndEndMatches(LiveVideoService other)
		{
			return Service.Start == other.Service.Start && Service.End == other.Service.End;
		}

		public bool StartsBeforeAndEndsLaterThan(LiveVideoService other)
		{
			return Service.Start <= other.Service.Start && other.Service.End <= Service.End;
		}

		/// <summary>
		/// Add a child to the children of this Live Video service.
		/// </summary>
		/// <param name="child">The child service to add.</param>
		/// <param name="addServiceChild">Indicates if this child also needs to be added to the Service object.</param>
		public virtual void AddChild(LiveVideoService child, bool addServiceChild = true)
		{
			if (children.Contains(child)) return;

			children.Add(child);
			if (addServiceChild) Service.Children.Add(child.Service);
		}

		/// <summary>
		/// Remove a child from the children of this Live Video service.
		/// </summary>
		/// <param name="childToRemove">The child service to remove.</param>
		/// <param name="removeChildFromServiceObject">Indicates if this child also needs to be removed from the Service object.</param>
		public virtual void RemoveChild(LiveVideoService childToRemove, bool removeChildFromServiceObject = true)
		{
			if (children == null) return;

			if (children.Contains(childToRemove))
			{
				children.Remove(childToRemove);
				if (removeChildFromServiceObject) Service.Children.Remove(childToRemove.Service);
				return;
			}

			foreach (var child in children)
			{
				child.RemoveChild(childToRemove, removeChildFromServiceObject);
			}
		}

		/// <summary>
		/// Gets a boolean indicating if the given function  is connected to a matrix function of a neighbor routing service.
		/// </summary>
		/// <param name="function"></param>
		/// <returns></returns>
		public bool FunctionIsConnectedToNeighborRoutingService(Function function)
		{
			if (function == null) throw new ArgumentNullException(nameof(function));

			if (Service.Definition.FunctionIsFirst(function) && Parent is RoutingService) return true;

			if (Service.Definition.FunctionIsLast(function) && Children.OfType<RoutingService>().Any()) return true;

			return false;
		}

		public LiveVideoService GetOldestRoutingParentServiceOrThis(bool considerSharedRoutingServices = true)
		{
			LiveVideoService oldestInputRoutingOrThis = this;
			var serviceToConsider = oldestInputRoutingOrThis;
			while (serviceToConsider.RoutingParent != null)
			{
				if (!considerSharedRoutingServices && serviceToConsider.RoutingParent.Children.Count > 1)
				{
					Helpers.Log(nameof(LiveVideoService), nameof(GetOldestRoutingParentServiceOrThis), $"Service: {serviceToConsider.Service?.Name} has children: '{string.Join(";", serviceToConsider.Children.Select(c => c?.Service?.Name))}'");
					break;
				}

				oldestInputRoutingOrThis = serviceToConsider.RoutingParent;

				serviceToConsider = serviceToConsider.RoutingParent;
			}

			return oldestInputRoutingOrThis;
		}

		public LiveVideoService GetYoungestNonRoutingParent(bool excludingProcessingServices = false)
		{
			if (Parent == null) return this;

			bool nextParentIsNotNull;
			bool parentDoesNotMeetRequirements;
			var parent = this;
			do
			{
				parent = parent.Parent;

				var parentIsRouting = parent is RoutingService;
				var parentIsProcessing = parent is AudioProcessingService || parent is GraphicsProcessingService || parent is VideoProcessingService;
				parentDoesNotMeetRequirements = parentIsRouting || (excludingProcessingServices && parentIsProcessing);
				nextParentIsNotNull = parent.Parent != null;
			}
			while (parentDoesNotMeetRequirements && nextParentIsNotNull);

			return parent;
		}

		public List<LiveVideoService> GetDirectNonRoutingChildren(bool exludeProcessingRelatedService = false)
		{
			var nonRoutingChildren = new List<LiveVideoService>();

			if (Children == null || !Children.Any()) return nonRoutingChildren;

			foreach (var child in Children)
			{
				bool skipProcessing = exludeProcessingRelatedService && (child is GraphicsProcessingService || child is VideoProcessingService || child is AudioProcessingService || child is VizremConverterService);

				if (!(child is RoutingService) && !skipProcessing)
				{
					nonRoutingChildren.Add(child);
				}
				else
				{
					nonRoutingChildren.AddRange(child.GetDirectNonRoutingChildren(exludeProcessingRelatedService));
				}
			}

			return nonRoutingChildren;
		}

		/// <summary>
		/// Check if the provided object is the same as this service.
		/// </summary>
		/// <param name="obj">The object to check.</param>
		/// <returns>True if both objects are the same.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			LiveVideoService other = obj as LiveVideoService;
			if (other == null)
			{
				return false;
			}

			return Equals(other);
		}

		/// <summary>
		/// Check if the provided Live Video service object is the same as this service.
		/// </summary>
		/// <param name="other">The Live Video service to check.</param>
		/// <returns>True if both services are the same.</returns>
		public bool Equals(LiveVideoService other)
		{
			return other != null && other.Service.Equals(Service);
		}

		public override string ToString()
		{
			return Service.Name;
		}

		/// <summary>
		/// Get the hash code for this Live Video service.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode()
		{
			return Service.GetHashCode();
		}

		protected static TimeSpan GetPreRoll(ServiceDefinition serviceDefinition, Service linkedService)
		{
			TimeSpan defaultOutputPreRoll = ServiceManager.GetPreRollDuration(linkedService.Definition);
			TimeSpan preRoll = ServiceManager.GetPreRollDuration(serviceDefinition);
			if (defaultOutputPreRoll != linkedService.PreRoll)
			{
				preRoll = linkedService.PreRoll;
			}

			return preRoll;
		}

		/// <summary>
		/// This method can be used to log the current hierarchy of the services in the order.
		/// </summary>
		public virtual void LogHierarchy()
		{
			Log(nameof(LogHierarchy), $"Service: {Service?.Name}");
			Log(nameof(LogHierarchy), $"Service Input Routing Service: {((RoutingParent?.Service != null) ? RoutingParent.Service.Name : "None")}");
			Log(nameof(LogHierarchy), $"Service Output Routing Service: {((RoutingChild?.Service != null) ? RoutingChild.Service.Name : "None")}");
		}

		protected void UpdateResourceRequirements(string functionDefinitionName, bool resourceRequired)
		{
			var functionToUpdate = Service.Functions.FirstOrDefault(f => f.Definition?.Name == functionDefinitionName);
			if (functionToUpdate != null)
			{
				functionToUpdate.RequiresResource = resourceRequired;
				functionToUpdate.Resource = functionToUpdate.RequiresResource ? functionToUpdate.Resource : null;
			}
			else
			{
				Helpers?.Log(nameof(LiveVideoService), nameof(UpdateResourceRequirements), $"Couldn't find any {functionDefinitionName} function for this service to update the resource requirements => Requires resource: {resourceRequired}", Service?.Name);
			}
		}

		protected void LogMethodStart(string nameOfMethod, out Stopwatch stopwatch, string nameOfObject = null)
		{
			Helpers.LogMethodStart(this.GetType().Name, nameOfMethod, out stopwatch, nameOfObject);
		}

		protected void LogMethodCompleted(string nameOfMethod, Stopwatch stopwatch = null)
		{
			Helpers.LogMethodCompleted(this.GetType().Name, nameOfMethod, null, stopwatch);
		}

		protected void Log(string nameOfMethod, string message)
		{
			Helpers.Log(this.GetType().Name, nameOfMethod, message, Service.Name);
		}
	}
}
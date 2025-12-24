namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using Library_1.Utilities;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Utils.YLE.Integrations;

	// TODO: move methods from EventManager to Event that directly manipulate the Event object itself
	public class Event : ICloneable
	{
		public const string OrderReservationFieldDescriptorName = "Order Reservation Field Descriptor";
		public const string OrderIsIntegrationFieldDescriptorName = "Order Is Integration Field Descriptor";

		public const string PropertyNameName = "Name";
		public const string PropertyNameStartTime = "Start Time";
		public const string PropertyNameEndTime = "End Time";
		public const string PropertyNameInfo = "Info";
		public const string PropertyNameProjectNumber = "Project Number";
		public const string PropertyNameProductNumbers = "Product Number";
		public const string PropertyNameAttachments = "Attachments";
		public const string PropertyNameContract = "Contract";
		public const string PropertyNameStatus = "Status";
		public const string PropertyNameSubType = "SubType";
		public const string PropertyNameCompany = "Company";
		public const string PropertyNameInternal = "Internal";
		public const string PropertyNameIntegration = "Integration";
		public const string PropertyNameOperatorNotes = "Operator Notes";
		public const string PropertyNameCompanyOfCreator = "Company Of Creator";

		private readonly JobManager jobManager;
		private readonly Job job;
		private readonly SectionDefinition customEventSectionDefinition;
		private readonly Section customEventSection;
		private readonly SectionDefinition orderSectionDefinition;

		private Event(Event other)
		{
			jobManager = other.jobManager;
			job = other.job.Clone() as Job;
			customEventSectionDefinition = other.customEventSectionDefinition;
			customEventSection = other.customEventSection;
			orderSectionDefinition = other.orderSectionDefinition;

			CloneHelper.CloneProperties(other, this);
		}

		public Event(Helpers helpers)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));

			helpers.LogMethodStart(nameof(Event), "Constructor", out var stopwatch);

			jobManager = new JobManager(Engine.SLNetRaw, helpers);

			job = new Job();

			var defaultJobDomain = jobManager.GetDefaultJobDomain();
			if (defaultJobDomain == null) throw new AddOrUpdateEventFailedException("Default job domain not found");
			job.JobDomainID = defaultJobDomain.ID;

			this.customEventSectionDefinition = helpers.EventManager.CustomEventSectionDefinition;
			this.customEventSection = new Section(customEventSectionDefinition);
			job.Sections.Add(customEventSection);

			this.orderSectionDefinition = helpers.EventManager.OrderSectionDefinition;

			IsInternal = false; // Default value. Should be set to make sure Event is visible in app UI.

			helpers.LogMethodCompleted(nameof(Event), "Constructor", null, stopwatch);
		}

		public Event(Helpers helpers, LiteOrder order) : this(helpers)
		{
			if (order == null) throw new ArgumentNullException(nameof(order));

			helpers.LogMethodStart(nameof(Event), "Constructor", out var stopwatch);

			Name = order.RecurringSequenceInfo?.Name ?? order.Name;
			Start = order.Start;
			End = order.End;
			Status = Status.Confirmed;
			Contract = order.Contract;
			Company = order.Company;
			CompanyOfCreator = order.Company;
			IntegrationType = order.IntegrationType;
			SecurityViewIds = new HashSet<int>(order.SecurityViewIds);

			if (order.Id != Guid.Empty)
			{
				AddOrUpdateOrderSection(order, helpers);
				helpers.Log(nameof(Event), "Constructor", $"Added order section with ID '{order.Id}' to job object.");
			}
			else
			{
				helpers.Log(nameof(Event), "Constructor", $"Order ID is empty, no order section added to job object.");
			}

			helpers.LogMethodCompleted(nameof(Event), "Constructor", null, stopwatch);
		}

		public Event(Helpers helpers, Job job, SectionDefinition orderSectionDefinition, SectionDefinition customEventSectionDefinition)
		{
			this.job = job ?? throw new ArgumentNullException(nameof(job));

			jobManager = new JobManager(Engine.SLNetRaw, helpers);

			this.customEventSectionDefinition = customEventSectionDefinition;
			this.customEventSection = job.GetCustomEventSection(customEventSectionDefinition.GetID().Id);
			this.orderSectionDefinition = orderSectionDefinition;

			Id = job.ID.Id;
		}

		public static Event FromTemplate(Helpers helpers, EventTemplate template, string name, DateTime startTime)
		{
			return new Event(helpers)
			{
				Id = Guid.Empty,
				Name = name,
				Start = startTime,
				End = startTime.Add(template.Duration),
				Status = Status.Preliminary,
				ProjectNumber = template.ProjectNumber,
				ProductNumbers = new string[0],
				Info = template.Info,
				Attachments = new string[0],
				Contract = template.Contract,
				Company = template.Company,
				//CompanyOfCreator = userCompany,
				IsInternal = template.IsInternal,
				OperatorNotes = template.OperatorNotes,
				IntegrationType = IntegrationType.None,
				SecurityViewIds = template.SecurityViewIds,
			};
		}

		public Guid Id { get; set; }

		public string Name
		{
			get => job.GetJobName();
			set => job.SetJobName(value);
		}

		public DateTime Start
		{
			get => job.GetStartTime();
			set => job.SetJobStartTime(value);
		}

		public DateTime End
		{
			get => job.GetEndTime();
			set => job.SetJobEndTime(value);
		}

		public Status Status
		{
			get => Convert.ToString(job.GetFieldValue(PropertyNameStatus) ?? string.Empty).GetEnumValue<Status>();
			set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, PropertyNameStatus, value.GetDescription());
		}

		public string ProjectNumber
		{
			get => Convert.ToString(job.GetFieldValue(PropertyNameProjectNumber) ?? string.Empty);
			set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, PropertyNameProjectNumber, value ?? string.Empty);
		}

		public IEnumerable<string> ProductNumbers
		{
			get => Convert.ToString(job.GetFieldValue(PropertyNameProductNumbers) ?? string.Empty).Split('/');
			set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, PropertyNameProductNumbers, value != null ? string.Join("/", value) : Constants.NotApplicable);
		}

		public string Info
		{
			get => Convert.ToString(job.GetFieldValue(PropertyNameInfo) ?? string.Empty);
			set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, PropertyNameInfo, value ?? Constants.NotApplicable);
		}

		public IEnumerable<string> Attachments
		{
			get => Convert.ToString(job.GetFieldValue(PropertyNameInfo) ?? string.Empty).Split('\n');
			set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, PropertyNameAttachments, value != null ? string.Join("\n", value) : Constants.NotApplicable);
		}

		/// <summary>
		/// The Contract for this Event.
		/// </summary>
		public string Contract
		{
			get => Convert.ToString(job.GetFieldValue(PropertyNameContract) ?? string.Empty);
			set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, PropertyNameContract, value ?? Constants.NotApplicable);
		}

		/// <summary>
		/// The Company of this Event.
		/// </summary>
		public string Company
		{
			get => Convert.ToString(job.GetFieldValue(PropertyNameCompany) ?? string.Empty);
			set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, PropertyNameCompany, value ?? Constants.NotApplicable);
		}

		/// <summary>
		/// The Company of the user who created this Event.
		/// </summary>
		public string CompanyOfCreator
		{
			get => Convert.ToString(job.GetFieldValue(PropertyNameCompanyOfCreator));
			set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, PropertyNameCompanyOfCreator, value ?? Constants.NotApplicable);
		}

		public bool IsInternal
		{
			get => Convert.ToBoolean(job.GetFieldValue(PropertyNameInternal));
			set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, PropertyNameInternal, value.ToString());
		}

		public string OperatorNotes
		{
			get => Convert.ToString(job.GetFieldValue(PropertyNameOperatorNotes));
			set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, PropertyNameOperatorNotes, value ?? string.Empty);
		}

		public IntegrationType IntegrationType
		{
			get => Convert.ToString(job.GetFieldValue(PropertyNameIntegration)).GetEnumValue<IntegrationType>();
			set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, PropertyNameIntegration, value.GetDescription());
		}

		/// <summary>
		/// Gets a collection of Cube View IDs of views where this event is visible.
		/// </summary>
		public HashSet<int> SecurityViewIds
		{
			get => new HashSet<int>(job.SecurityViewIDs);
			set => job.SecurityViewIDs = value != null ? value.ToList() : new List<int>();
		}

		/// <summary>
		/// A collection of Guids representing the Orders that belong to this Event.
		/// </summary>
		public HashSet<Guid> OrderIds => new HashSet<Guid>(job.GetOrderSections().Select(s => s.OrderId));

		/// <summary>
		/// A Dictionary indicating whether an order is made by an integration or not. Used for performance reasons when merging events.
		/// </summary>
		public Dictionary<Guid, bool> OrderIsIntegrations => job.GetOrderSections().ToDictionary(s => s.OrderId, s => s.OrderIsIntegration);

		public void CopyValuesFrom(Event eventToCopyValuesFrom)
		{
			Name = eventToCopyValuesFrom.Name;
			Start = eventToCopyValuesFrom.Start;
			End = eventToCopyValuesFrom.End;
			Status = eventToCopyValuesFrom.Status == Status.Planned || eventToCopyValuesFrom.Status == Status.Preliminary ? Status.Confirmed : eventToCopyValuesFrom.Status;
			IsInternal = eventToCopyValuesFrom.IsInternal;
			Info = eventToCopyValuesFrom.Info;
			Company = eventToCopyValuesFrom.Company;
			Contract = eventToCopyValuesFrom.Contract;
			ProjectNumber = eventToCopyValuesFrom.ProjectNumber;
			ProductNumbers = eventToCopyValuesFrom.ProductNumbers;
			OperatorNotes = eventToCopyValuesFrom.OperatorNotes;
			SecurityViewIds = eventToCopyValuesFrom.SecurityViewIds;
			IntegrationType = eventToCopyValuesFrom.IntegrationType;
		}

		public bool TryAddJobToJobDomain()
		{
			bool addedSuccessful = jobManager.TryAddJob(job, out var resultingJob);

			if (addedSuccessful) Id = resultingJob.ID.Id;

			return addedSuccessful;
		}

		public bool TryUpdateJobToJobDomain()
		{
			bool updateSuccessful = jobManager.TryUpdateJob(job, out var resultingJob);

			if (updateSuccessful) Id = resultingJob.ID.Id;

			return updateSuccessful;
		}

		public void AddOrUpdateOrder(LiteOrder order, Helpers helpers, bool orderEventReferenceUpdateRequired = false)
		{
			if (order == null) throw new ArgumentNullException(nameof(order));

			helpers.LogMethodStart(nameof(Event), nameof(AddOrUpdateOrder), out var stopWatch);

			AddOrUpdateOrderSection(order, helpers);

			UpdateTiming(order, helpers);

			UpdateStatus(helpers);

			order.Event = this;

			if (orderEventReferenceUpdateRequired)
			{
				bool orderEventReferenceUpdateSucceeded = order.UpdateEventReference(helpers);

				helpers.Log(nameof(Event), nameof(AddOrUpdateOrder), $"Update Order Event Reference {(orderEventReferenceUpdateSucceeded ? "succeeded" : "failed")}");
			}

			helpers.LogMethodCompleted(nameof(Event), nameof(AddOrUpdateOrder), Name, stopWatch);
		}

		public bool RemoveOrder(Guid orderId)
		{
			var orderSection = job.GetOrderSection(orderId);
			if (orderSection == null) return true;

			bool removeSuccessful = job.Sections.Remove(orderSection.Section);

			UpdateStatus();

			return removeSuccessful;
		}

		public void AddOrUpdateOrderSection(LiteOrder order, Helpers helpers)
		{
			if (order == null) throw new ArgumentNullException(nameof(order));

			helpers.LogMethodStart(nameof(Event), nameof(AddOrUpdateOrderSection), out var stopWatch);

			bool addOrUpdateNotRequired = order.Id == Guid.Empty || (HasOrder(order.Id) && OrderSectionIsValid(order.Id));

			helpers.Log(nameof(Event), nameof(AddOrUpdateOrderSection), $"Add or Update Order Section is {(addOrUpdateNotRequired ? "not " : string.Empty)}required");

			if (addOrUpdateNotRequired)
			{
				helpers.LogMethodCompleted(nameof(Event), nameof(AddOrUpdateOrderSection), null, stopWatch);
				return;
			}

			var newOrUpdatedOrderSection = new OrderSection(helpers, order, orderSectionDefinition);

			job.AddOrUpdateOrderSection(newOrUpdatedOrderSection);

			helpers.LogMethodCompleted(nameof(Event), nameof(AddOrUpdateOrderSection), null, stopWatch);
		}

		public void UpdateTiming(LiteOrder order, Helpers helpers)
		{
			if (order == null) throw new ArgumentNullException(nameof(order));

			helpers.LogMethodStart(nameof(Event), nameof(UpdateTiming), out var stopWatch);

			var jobStartTime = Start; // UTC
			var jobEndTime = End; // UTC

			Start = new[] { Start, order.Start }.Min();
			End = new[] { End, order.End }.Max();

			helpers.Log(nameof(Event), nameof(UpdateTiming), $"Set Event start time to {Start}, which is the minimum of Event start {jobStartTime} and Order start {order.Start}. Set Event end time to {End}, which is the maximum of Event end {jobEndTime} and Order end {order.End}");

			helpers.LogMethodCompleted(nameof(Event), nameof(UpdateTiming), Name, stopWatch);
		}

		public void UpdateStatus(Helpers helpers = null)
		{
			Stopwatch stopWatch = null;
			helpers?.LogMethodStart(nameof(Event), nameof(UpdateStatus), out stopWatch);

			if (job.HasOrderSections())
			{
				bool eventIsOngoing = Start <= DateTime.Now;

				Status = eventIsOngoing ? Status.Ongoing : Status.Confirmed;
			}
			else
			{
				Status = Status.Planned;
			}

			helpers?.LogMethodCompleted(nameof(Event), nameof(UpdateStatus), Name, stopWatch);
		}

		public List<LiteOrder> GetLiteOrders(Helpers helpers)
		{
			var orders = new List<LiteOrder>();

			foreach (var orderId in OrderIds)
			{
				var order = helpers.OrderManager.GetLiteOrder(orderId);
				orders.Add(order);
			}

			return orders;
		}

		public bool HasOrder(Guid orderId)
		{
			return OrderIds.Contains(orderId);
		}

		public bool OrderSectionIsValid(Guid orderId)
		{
			var orderSection = job.GetOrderSection(orderId);

			if (orderSection == null) return false;

			return orderSection.IsValid;
		}

		public bool IsUpdated(Event oldEvent)
		{
			if (oldEvent == null) return true;

			bool isUpdated = false;

			isUpdated |= Name != oldEvent.Name;
			isUpdated |= Start != oldEvent.Start;
			isUpdated |= End != oldEvent.End;
			isUpdated |= Status != oldEvent.Status;
			isUpdated |= IsInternal != oldEvent.IsInternal;
			isUpdated |= Info != oldEvent.Info;
			isUpdated |= Company != oldEvent.Company;
			isUpdated |= Contract != oldEvent.Contract;
			isUpdated |= ProjectNumber != oldEvent.ProjectNumber;
			isUpdated |= (ProductNumbers == null && oldEvent.ProductNumbers != null) || (ProductNumbers != null && oldEvent.ProductNumbers == null) || (ProductNumbers != null && oldEvent.ProductNumbers != null && !ProductNumbers.OrderBy(x => x).SequenceEqual(oldEvent.ProductNumbers.OrderBy(x => x)));
			isUpdated |= OperatorNotes != oldEvent.OperatorNotes;
			isUpdated |= !SecurityViewIds.OrderBy(x => x).SequenceEqual(oldEvent.SecurityViewIds.OrderBy(x => x));
			isUpdated |= !OrderIds.ScrambledEquals(oldEvent.OrderIds);
			isUpdated |= !OrderIsIntegrations.ScrambledEquals(oldEvent.OrderIsIntegrations);

			return isUpdated;
		}

		/// <summary>
		/// Retrieving all existing file attachments from this event
		/// </summary>
		public List<string> GetEventAttachments(IEngine engine, string path)
		{
			string[] filePaths = new string[0];

			try
			{
				string fullPath = Path.Combine(path, Convert.ToString(Id));
				if (Directory.Exists(fullPath)) filePaths = Directory.GetFiles(fullPath);

			}
			catch (Exception e)
			{
				engine.Log($"Something went wrong during while collecting the event files: " + e);
			}

			return filePaths.ToList();
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this, Formatting.None);
		}

		public object Clone()
		{
			return new Event(this);
		}
	}
}
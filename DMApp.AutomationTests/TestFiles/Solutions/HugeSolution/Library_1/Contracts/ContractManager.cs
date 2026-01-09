namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Skyline.DataMiner.Library.Automation;
    using Skyline.DataMiner.Library.Exceptions;

	public class ContractManager : IContractManager
	{
		private readonly Helpers helpers;
		private const int externalRequestParameterId = 50;
		private const int externalRequestTableParameterId = 10000;
		private const int externalRequestTableStatusParameterId = 10002;
		private const int externalRequestTableResponseParameterId = 10004;
		private const int externalRequestTableRemoveEntryParameterId = 10005;

		private const int OrderTemplatesTableTemplateColumnPid = 15004;
		private const int EventTemplatesTableTemplateColumnPid = 16004;

		private const int CompaniesTableKeyIndex = 0;
		private const int CompaniesTableSecurityViewIdIndex = 2;

		public const string DataminerUserLoginName = "DataMiner Agent";

		/// <summary>
		/// Initializes a new instance of the ContractManager class.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when the provided Engine is null.</exception>
		/// <exception cref="ElementNotFoundException">Thrown when no Finnish Broadcasting Company Contract Manager element running the production version of the protocol was found on the DMS.</exception>
		/// <exception cref="ElementNotStartedException">Thrown when the found Finnish Broadcasting Company Contract Manager element is not running.</exception>
		public ContractManager(Helpers helpers)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));

			Element = helpers.Engine.FindElementsByProtocol("Finnish Broadcasting Company Contract Manager", "Production").FirstOrDefault();
			if (Element == null) throw new ElementNotFoundException("Unable to find a Contract Manager element");

			if (!Element.IsActive) throw new ElementNotStartedException("The Contract Manager element is not running");
		}

		public UserInfo GetUserInfo(Order order, string userLoginName)
		{
			return GetUserInfo(userLoginName, order?.Event);
		}

		/// <summary>
		/// Gets the User Info from the contract manager.
		/// </summary>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="InvalidContractResponseException"/>
		/// <exception cref="NoUserGroupsException"/>
		/// <exception cref="NoContractsException"/>
		/// <exception cref="ContractNotFoundException"/>
		public UserInfo GetUserInfo(string userLoginName, Event @event = null)
		{
			switch (userLoginName)
			{
				case DataminerUserLoginName when @event == null:
					throw new ArgumentNullException(nameof(@event), "Event cannot be null when userLoginName is " + DataminerUserLoginName);
				case DataminerUserLoginName:
					return GetUserInfoForCompany(@event);
				default:
					return GetUserInfoForUser(userLoginName, @event);
			}
		}

		/// <summary>
		/// Gets the UserInfo for the given user login name. This method is used for NonLive Orders.
		/// </summary>
		/// <param name="userLoginName"></param>
		/// <returns></returns>
		public UserInfo GetBaseUserInfo(string userLoginName)
		{
			helpers.LogMethodStart(nameof(ContractManager), nameof(GetBaseUserInfo), out var stopwatch);

			var response = RequestUserContractDetails(helpers.Engine.UserLoginName);
			var contract = response.Contracts.FirstOrDefault(c => c.Type == ContractType.BaseContract);

			if (contract == null)
			{
				helpers.Log(nameof(ContractManager), nameof(GetBaseUserInfo), "No base contract found for user " + userLoginName);
				helpers.LogMethodCompleted(nameof(ContractManager), nameof(GetBaseUserInfo), null, stopwatch);
				return null;
			}

            User currentUser = null;
            if (response.AllUsers != null && response.AllUsers.Any())
            {
                currentUser = response.AllUsers.SingleOrDefault(u => u.Name == userLoginName);
            }

			var userInfo = new UserInfo(contract, response.UserGroups, response.Contracts, response.AllUserGroups, response.McrSecurityViewId, response.AllContracts, currentUser);

			helpers.LogMethodCompleted(nameof(ContractManager), nameof(GetBaseUserInfo), null, stopwatch);

			return userInfo;
		}

		/// <summary>
		/// Requests the contract details for the company linked to the given event and filters them based on the event start and end times.
		/// </summary>
		/// <param name="event">Event for which the company contacts should be retrieved.</param>
		/// <returns>Response containing the contracts that are available for the given event.</returns>
		public ExternalCompanyResponse RequestCompanyContractDetails(Event @event)
		{
			var contractDetails = RequestCompanyContractDetails(@event.Company);

			if (contractDetails == null) return null;

			// Filter active contracts
			if (contractDetails.Contracts != null)
			{
				contractDetails.Contracts = contractDetails.Contracts.Where(x => x.Name.Equals(@event.Contract) && x.Start < @event.Start && x.End > @event.End).ToArray();
			}

			return contractDetails;
		}

		/// <summary>
		/// Requests the contract details for the given company.
		/// </summary>
		/// <param name="company">Company for which the company contracts should be retrieved.</param>
		/// <returns>Response containing the contracts for the given company.</returns>
		public ExternalCompanyResponse RequestCompanyContractDetails(string company)
		{
			Guid id = Guid.NewGuid();
			ExternalCompanyRequest request = new ExternalCompanyRequest { ID = id.ToString(), Company = company };
			string response = SendRequest(id, request.Serialize());

			return ExternalCompanyResponse.Deserialize(response);
		}

		public ExternalUserResponse RequestUserContractDetails(string userName)
		{
			Guid id = Guid.NewGuid();
			ExternalUserRequest request = new ExternalUserRequest { ID = id.ToString(), Username = userName };
			string response = SendRequest(id, request.Serialize());

			ExternalUserResponse contractDetails = ExternalUserResponse.Deserialize(response);

            if (contractDetails == null) return null;

			// Filter applicable contracts
			if (contractDetails.Contracts != null)
			{
				contractDetails.Contracts = contractDetails.Contracts.Where(x => x.Type == ContractType.BaseContract).ToArray();
			}

			return contractDetails;
		}

		public bool TryGetOrderTemplate(string templateName, out OrderTemplate template)
		{
			if (String.IsNullOrWhiteSpace(templateName)) throw new ArgumentException("templateName");

			template = null;

			var orderTemplatesTable = Element.GetTable(helpers.Engine, 15000);
			foreach (object[] orderTemplateRow in orderTemplatesTable.Values)
			{
				string orderTemplateName = Convert.ToString(orderTemplateRow[1]);
				if (templateName == orderTemplateName)
				{
					string serializedOrderTemplate = Convert.ToString(orderTemplateRow[3]);
					template = OrderTemplate.Deserialize(serializedOrderTemplate);
					if (template != null) return true;
				}
			}

			return false;
		}

		public List<OrderTemplate> GetOrderTemplates(IEnumerable<string> templateNames)
		{
			if (templateNames is null) throw new ArgumentNullException(nameof(templateNames));
			if (!templateNames.Any()) return new List<OrderTemplate>();

			helpers.LogMethodStart(nameof(ContractManager), nameof(GetOrderTemplates), out var stopwatch);

			var templates = new List<OrderTemplate>();

			var orderTemplatesTable = Element.GetTable(helpers.Engine, 15000);
			foreach (var orderTemplateRow in orderTemplatesTable.Values)
			{
				string orderTemplateName = Convert.ToString(orderTemplateRow[1]);

				if (templateNames.Contains(orderTemplateName))
				{
					string serializedOrderTemplate = Convert.ToString(orderTemplateRow[3]);
					var template = OrderTemplate.Deserialize(serializedOrderTemplate);
					if (template != null)
					{
						templates.Add(template);
					}
				}
			}

			var notFoundTemplateNames = templateNames.Except(templates.Select(t => t.Name));

			Log(nameof(GetOrderTemplates), $"Could not find order templates '{string.Join(", ", notFoundTemplateNames)}'");

			helpers.LogMethodCompleted(nameof(ContractManager), nameof(GetOrderTemplates), null, stopwatch);

			return templates;
		}

		public bool TryGetOrderTemplate(Guid templateId, out OrderTemplate template)
		{
			template = null;

			string serializedOrderTemplate = Convert.ToString(Element.GetParameterByPrimaryKey(OrderTemplatesTableTemplateColumnPid, templateId.ToString()));
			if (string.IsNullOrWhiteSpace(serializedOrderTemplate)) return false;

			template = OrderTemplate.Deserialize(serializedOrderTemplate);

			return template != null;
		}

		public bool TryAddOrderTemplate(string templateName, string[] userGroups, Order order, out Guid templateId, bool isTemplateForRecurringOrder = false)
		{
			if (string.IsNullOrWhiteSpace(templateName)) throw new ArgumentException(nameof(templateName));
			if (userGroups == null) throw new ArgumentNullException(nameof(userGroups));
			if (order == null) throw new ArgumentNullException(nameof(order));

			var template = OrderTemplate.FromOrder(helpers, order, templateName, isTemplateForRecurringOrder);
			templateId = template.Id;

			Guid requestId = Guid.NewGuid();
			ExternalOrderTemplateRequest request = new ExternalOrderTemplateRequest { ID = requestId.ToString(), TemplateId = template.Id.ToString(), OrderTemplateName = templateName, UserGroups = userGroups, Template = template.Serialize(), Action = TemplateAction.Add };
			string response = SendRequest(requestId, request.Serialize(helpers) ?? throw new InvalidOperationException("Failed to serialize request"));

			return !String.IsNullOrWhiteSpace(response) && response.Equals("OK", StringComparison.InvariantCultureIgnoreCase);
		}

		public bool TryEditOrderTemplate(Guid templateId, string templateName, string[] userGroups, Order order, bool isPartOfEventTemplate, bool isTemplateForRecurringOrder = false)
		{
			if (string.IsNullOrWhiteSpace(templateName)) throw new ArgumentException(nameof(templateName));
			if (userGroups == null) throw new ArgumentNullException(nameof(userGroups));
			if (order == null) throw new ArgumentNullException(nameof(order));
			if (templateId == Guid.Empty) throw new ArgumentException(nameof(templateId));

			var template = OrderTemplate.FromOrder(helpers, order, templateName, isTemplateForRecurringOrder);
			template.Id = templateId; // override OrderTemplate ID with existing template ID
			template.IsPartOfEventTemplate = isPartOfEventTemplate;

			return TryEditOrderTemplate(template, userGroups);
		}

		public bool TryEditOrderTemplate(OrderTemplate template, string[] userGroups)
		{
			if (template == null) throw new ArgumentNullException(nameof(template));
			if (userGroups == null) throw new ArgumentNullException(nameof(userGroups));

			var requestId = Guid.NewGuid();
			var request = new ExternalOrderTemplateRequest
			{
				ID = requestId.ToString(),
				TemplateId = template.Id.ToString(),
				OrderTemplateName = template.Name,
				Template = template.Serialize(),
				UserGroups = userGroups,
				Action = TemplateAction.Edit
			};

			string response = SendRequest(requestId, request.Serialize(helpers) ?? throw new InvalidOperationException("Failed to serialize request"));

			return !string.IsNullOrWhiteSpace(response) && response.Equals("OK", StringComparison.InvariantCultureIgnoreCase);
		}

		public bool TryDeleteOrderTemplate(Guid templateId)
		{
			if (templateId == Guid.Empty) throw new ArgumentException(nameof(templateId));

			try
			{
				Guid id = Guid.NewGuid();
				var request = new ExternalOrderTemplateRequest { ID = id.ToString(), TemplateId = templateId.ToString(), Action = TemplateAction.Delete };
				string response = SendRequest(id, request.Serialize(helpers) ?? throw new InvalidOperationException("Failed to serialize request"));

				return !String.IsNullOrWhiteSpace(response) && response.Equals("OK", StringComparison.InvariantCultureIgnoreCase);
			}
			catch (Exception ex)
			{
				Log(nameof(TryDeleteOrderTemplate), $"Exception while deleting order template {templateId}: {ex}");
				return false;
			}
		}

		public bool TryDeleteOrderTemplate(string templateName)
		{
			if (templateName == null) throw new ArgumentException("templateName");

			try
			{
				Guid id = Guid.NewGuid();
				ExternalOrderTemplateRequest request = new ExternalOrderTemplateRequest { ID = id.ToString(), OrderTemplateName = templateName, Action = TemplateAction.Delete };
				string response = SendRequest(id, request.Serialize(helpers) ?? throw new InvalidOperationException("Failed to serialize request"));

				return !String.IsNullOrWhiteSpace(response) && response.Equals("OK", StringComparison.InvariantCultureIgnoreCase);
			}
			catch (Exception ex)
			{
				Log(nameof(TryDeleteOrderTemplate), $"Exception while deleting order template {templateName}: {ex}");
				return false;
			}
		}

		public bool TryGetEventTemplate(string templateName, out EventTemplate template)
		{
			if (String.IsNullOrWhiteSpace(templateName)) throw new ArgumentException("templateName");

			Guid id = Guid.NewGuid();
			ExternalEventTemplateRequest request = new ExternalEventTemplateRequest { ID = id.ToString(), EventTemplateName = templateName, Action = TemplateAction.Get };
			string response = SendRequest(id, request.Serialize());

			template = EventTemplate.Deserialize(response);
			return template != null;
		}

		public bool TryAddEventTemplate(string templateName, string[] userGroups, Event @event, IEnumerable<Order> orders)
		{
			if (String.IsNullOrWhiteSpace(templateName)) throw new ArgumentException("templateName");
			if (userGroups == null) throw new ArgumentNullException(nameof(userGroups));
			if (@event == null) throw new ArgumentNullException(nameof(@event));
			if (orders == null) throw new ArgumentNullException(nameof(orders));

			// Generate Event Order Templates
			List<OrderTemplate> orderTemplates = new List<OrderTemplate>();
			List<Tuple<Guid, DateTime>> orderTemplateStartTimes = new List<Tuple<Guid, DateTime>>();
			foreach(Order order in orders)
			{
				string orderTemplateName = DetermineUniqueOrderTemplateName(order.Name);
				OrderTemplate orderTemplate = OrderTemplate.FromOrder(helpers, order, orderTemplateName);
				orderTemplate.IsPartOfEventTemplate = true;

				orderTemplates.Add(orderTemplate);

				orderTemplateStartTimes.Add(new Tuple<Guid, DateTime>(orderTemplate.Id, order.Start.RoundToMinutes()));
			}

			if (!CouldTemplatesBeSaved(orderTemplates)) throw new InvalidOperationException($"All linked order templates should be of the same type (vizrem or normal live)");

			// Generate Event Template
			EventTemplate eventTemplate = EventTemplate.FromEvent(@event, orderTemplateStartTimes, templateName);
			eventTemplate.EventSubType = orderTemplates.Any(o => o.SubType == OrderSubType.Vizrem) ? EventSubType.Vizrem : EventSubType.Normal;

			// Send request
			Guid id = Guid.NewGuid();
			ExternalEventTemplateRequest request = new ExternalEventTemplateRequest 
			{ 
				ID = id.ToString(),
				TemplateId = eventTemplate.Id.ToString(),
				EventTemplateName = templateName, 
				UserGroups = userGroups, 
				Template = eventTemplate.Serialize(),
				OrderTemplates = orderTemplates.Select(o => new EventOrderTemplate { TemplateId = o.Id.ToString(), Name = o.Name, Template = o.Serialize()}).ToArray(),
				Action = TemplateAction.Add
			};

			string response = SendRequest(id, request.Serialize());
			return !String.IsNullOrWhiteSpace(response) && response.Equals("OK", StringComparison.InvariantCultureIgnoreCase);
		}

		private string DetermineUniqueOrderTemplateName(string name)
		{
			string templateName = name;
			int index = 0;
			var allOrderTemplates = GetAllOrderTemplateNamesAndIds().Keys;
			while (allOrderTemplates.Contains(templateName))
			{
				index++;
				templateName = $"{name}_{index}";
			}

			return templateName;
		}

		public bool TryEditEventTemplate(Event @event, EventTemplate templateToEdit, string templateName, string[] userGroups, IReadOnlyList<OrderTemplate> linkedOrderTemplatesToKeep, IReadOnlyList<OrderTemplate> newOrderTemplates, Dictionary<Guid, TimeSpan> newOrderTemplateOffsets)
		{
			ArgumentNullCheck.ThrowIfNull(@event, nameof(@event));
			ArgumentNullCheck.ThrowIfNull(templateToEdit, nameof(templateToEdit));
			ArgumentNullCheck.ThrowIfNull(userGroups, nameof(userGroups));
			ArgumentNullCheck.ThrowIfNull(linkedOrderTemplatesToKeep, nameof(linkedOrderTemplatesToKeep));
			ArgumentNullCheck.ThrowIfNull(newOrderTemplates, nameof(newOrderTemplates));
			ArgumentNullCheck.ThrowIfNull(newOrderTemplateOffsets, nameof(newOrderTemplateOffsets));
			ArgumentNullCheck.ThrowIfNullOrWhiteSpace(templateName, nameof(templateName));

			var allTemplatesToAddOrUpdate = linkedOrderTemplatesToKeep.Concat(newOrderTemplates).ToList();

			if (!CouldTemplatesBeSaved(allTemplatesToAddOrUpdate)) throw new InvalidOperationException($"All linked order templates should be of the same type (vizrem or normal live)");

			List<EventOrderTemplate> eventOrderTemplates = new List<EventOrderTemplate>();
			foreach(OrderTemplate orderTemplate in linkedOrderTemplatesToKeep)
			{
				eventOrderTemplates.Add(new EventOrderTemplate { TemplateId = orderTemplate.Id.ToString(), Name = orderTemplate.Name, Template = orderTemplate.Serialize() });
			}

			// Add new Order Templates
			foreach(OrderTemplate orderTemplate in newOrderTemplates)
			{
				orderTemplate.IsPartOfEventTemplate = true;
				eventOrderTemplates.Add(new EventOrderTemplate { TemplateId = orderTemplate.Id.ToString(), Name = orderTemplate.Name, Template = orderTemplate.Serialize() });
			}

			// Generate Event Template
			EventTemplate eventTemplate = EventTemplate.FromEvent(@event, new List<Tuple<Guid, DateTime>>(), templateName);
			eventTemplate.EventSubType = allTemplatesToAddOrUpdate.Any(o => o.SubType == OrderSubType.Vizrem) ? EventSubType.Vizrem : EventSubType.Normal;
			eventTemplate.OrderOffsets = templateToEdit.OrderOffsets;
			eventTemplate.Id = templateToEdit.Id;

			// Remove OrderOffsets for Order Templates that will be removed
			var existingOrderOffsetGuids = eventTemplate.OrderOffsets.Keys.ToList();
			var orderOffsetGuidsToKeep = linkedOrderTemplatesToKeep.Select(x => x.Id).ToList();
			foreach (Guid orderOffsetGuid in existingOrderOffsetGuids)
			{
				if (!orderOffsetGuidsToKeep.Contains(orderOffsetGuid)) eventTemplate.OrderOffsets.Remove(orderOffsetGuid);
			}

			// Add OrderOffsets for Order Templates that will be added
			foreach (var kvp in newOrderTemplateOffsets)
			{
				eventTemplate.OrderOffsets.Add(kvp.Key, kvp.Value);
			}

			// Send request
			Guid requestId = Guid.NewGuid();
			ExternalEventTemplateRequest request = new ExternalEventTemplateRequest 
			{ 
				ID = requestId.ToString(),
				TemplateId = eventTemplate.Id.ToString(),
				EventTemplateName = templateName, 
				UserGroups = userGroups,
				OrderTemplates = eventOrderTemplates.ToArray(),
				Action = TemplateAction.Edit,
				Template = eventTemplate.Serialize()
			};

			string response = SendRequest(requestId, request.Serialize());

			return !String.IsNullOrWhiteSpace(response) && response.Equals("OK", StringComparison.InvariantCultureIgnoreCase);
		}

		private static bool CouldTemplatesBeSaved(List<OrderTemplate> orderTemplatesToSave)
		{
			if (orderTemplatesToSave.All(o => o.SubType == OrderSubType.Vizrem))
			{
				return true;
			}
			else if (orderTemplatesToSave.All(o => o.SubType == OrderSubType.Normal))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// This method does not use the Contract Manager requests.
		/// It immediately updates the template in the Event Templates table.
		/// </summary>
		/// <param name="template">Eventtemplate to update.</param>
		/// <returns>True if update was successful.</returns>
		public bool TryEditEventTemplate(EventTemplate template)
        {
			if (!GetAllEventTemplates().Contains(template.Name)) return false;
			if (!Element.GetTablePrimaryKeys(16000).Contains(template.Id.ToString())) return false;

			try
            {
				Element.SetParameterByPrimaryKey(EventTemplatesTableTemplateColumnPid, template.Id.ToString(), JsonConvert.SerializeObject(template));
            }
			catch (Exception)
            {
				return false;
            }

			return true;
        }

		public bool TryDeleteEventTemplate(string templateName)
		{
			if (templateName == null) throw new ArgumentException("templateName");

			Guid id = Guid.NewGuid();
			ExternalEventTemplateRequest request = new ExternalEventTemplateRequest { ID = id.ToString(), EventTemplateName = templateName, Action = TemplateAction.Delete };
			string response = SendRequest(id, request.Serialize());

			return !String.IsNullOrWhiteSpace(response) && response.Equals("OK", StringComparison.InvariantCultureIgnoreCase);
		}

		public List<OrderTemplate> GetLinkedOrderTemplates(Guid eventTemplateId)
		{
			if (eventTemplateId == Guid.Empty) throw new ArgumentException("eventTemplateId");

			string[] orderTemplateNames = Convert.ToString(Element.GetParameterByPrimaryKey(16003, eventTemplateId.ToString())).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			var orderTemplatesTable = Element.GetTable(helpers.Engine, 15000);

			List<OrderTemplate> orderTemplates = new List<OrderTemplate>();
			foreach (object[] orderTemplateRow in orderTemplatesTable.Values)
			{
				string orderTemplateName = Convert.ToString(orderTemplateRow[1]);
				if (orderTemplateNames.Contains(orderTemplateName))
				{
					string serializedOrderTemplate = Convert.ToString(orderTemplateRow[3]);
					OrderTemplate orderTemplate = OrderTemplate.Deserialize(serializedOrderTemplate);
					if (orderTemplate != null) orderTemplates.Add(orderTemplate);
				}
			}

			return orderTemplates;
		}

        /// <summary>
        /// Returns all Companies defined in the Contract Manager element.
        /// </summary>
        /// <returns>All Companies from the Companies table in the Contract Manager Element.</returns>
        public string[] GetAllCompanies()
        {
            return Element.GetTablePrimaryKeys(1000);
        }

        /// <summary>
        /// Returns all Companies defined in the Contract Manager element together with their security view id.
        /// </summary>
        /// <returns>All Companies from the Companies table in the Contract Manager Element with their security view id.</returns>
        public Dictionary<string, int> GetAllCompanySecurityViewIds()
        {
            object[] companyTableColumns = Element.GetColumns(1000, new int[] { CompaniesTableKeyIndex, CompaniesTableSecurityViewIdIndex });
            object[] primaryKeyColumn = (object[])companyTableColumns[0];
            object[] securityViewIdColumn = (object[])companyTableColumns[1];

            Dictionary<string, int> allCompanies = new Dictionary<string, int>(); /* Key = Company Name, Value = Security View Id */
            for (int i = 0; i < primaryKeyColumn.Length; i++)
            {
                string primaryKey = Convert.ToString(primaryKeyColumn[i]);
                if (!allCompanies.ContainsKey(primaryKey))
                {
                    allCompanies.Add(primaryKey, Convert.ToInt16(securityViewIdColumn[i]));
                }
            }

            return allCompanies;
        }

        /// <summary>
        /// Returns all Order Templates defined in the Contract Manager element.
        /// </summary>
        /// <returns>The names of all saved Order Templates.</returns>
        public Dictionary<string,string> GetAllOrderTemplateNamesAndIds()
		{
			var names = Element.GetTableDisplayKeys(15000);
			var ids = Element.GetTablePrimaryKeys(15000);

			return names.ToDictionary(name => name, name => ids[Array.FindIndex(names, n => n == name)]);
		}

		/// <summary>
		/// Returns all Event Templates defined in the Contract Manager element.
		/// </summary>
		/// <returns>The names of all saved Event Templates.</returns>
		public string[] GetAllEventTemplates()
		{
			return Element.GetTableDisplayKeys(16000);
		}

		/// <summary>
		/// Returns all of the user groups known to the Contract Manager element.
		/// </summary>
		/// <returns>The names of all use groups known in the Contract Manager element.</returns>
		public string[] GetAllUserGroups()
		{
			return Element.GetTableDisplayKeys(3000);
		}

		private UserInfo GetUserInfoForUser(string userLoginName, Event @event = null)
		{
			ExternalResponse response = RequestUserContractDetails(userLoginName);

			if (response == null) throw new InvalidContractResponseException();
			if (response.UserGroups == null || !response.UserGroups.Any()) throw new NoUserGroupsException(userLoginName);
			if (response.Contracts == null || !response.Contracts.Any()) throw new NoContractsException(userLoginName);

			Contract contract;

			var userCompanies = response.Contracts.Select(c => c.Company).ToList();

			if (@event != null && userCompanies.Contains(@event.Company))
			{
				// If current user is part of the selected company in the event, use the contract defined in the event
				contract = response.Contracts.FirstOrDefault(c => c.Name == @event.Contract) ?? throw new ContractNotFoundException(@event.Contract, @event);
			}
			else
			{
				// if Event is null or user is not part of the selected company in the event, use the user company base contract
				contract = response.Contracts.FirstOrDefault(c => c.Type == ContractType.BaseContract) ?? throw new ContractNotFoundException(ContractType.BaseContract);
			}

            User currentUser = null;
            if (response.AllUsers != null && response.AllUsers.Any())
            {
                currentUser = response.AllUsers.SingleOrDefault(u => u.Name == userLoginName);
            }

            return new UserInfo(contract, response.UserGroups, response.Contracts, response.AllUserGroups, response.McrSecurityViewId, response.AllContracts, currentUser);
		}

		private UserInfo GetUserInfoForCompany(Event @event)
		{
			if (@event == null) throw new ArgumentNullException(nameof(@event));

			ExternalResponse response = RequestCompanyContractDetails(@event);

			if (response == null) throw new InvalidContractResponseException();
			if (response.UserGroups == null || !response.UserGroups.Any()) throw new NoUserGroupsException(@event.Company);
			if (response.Contracts == null || !response.Contracts.Any()) throw new NoContractsException(@event.Company);

			var contract = response.Contracts.FirstOrDefault(c => c.Name == @event.Contract) ?? throw new ContractNotFoundException(@event.Contract);

            return new UserInfo(contract, response.UserGroups, response.Contracts, response.AllUserGroups, response.McrSecurityViewId, response.AllContracts);
		}

		private string SendRequest(Guid id, string request)
		{
			if (string.IsNullOrWhiteSpace(request)) throw new ArgumentException($"'{nameof(request)}' cannot be null or whitespace.", nameof(request));
			
			return ElementAPI.SendRequestAndGetResponse(helpers, Element, externalRequestParameterId, externalRequestTableParameterId, externalRequestTableStatusParameterId, externalRequestTableResponseParameterId, externalRequestTableRemoveEntryParameterId, id, request, 5, 20);
		}

		/// <summary>
		/// Gets the ContractManager element that this class instance interacts with.
		/// </summary>
		public Element Element { get; private set; }

		private void Log(string nameOfMethod, string message)
		{
			helpers.Log(nameof(ContractManager), nameOfMethod, message);
		}
	}
}
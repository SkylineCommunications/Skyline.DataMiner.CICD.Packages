namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order
{
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Service = Service.Service;

    public class OrderTemplate
	{
        private OrderTemplate()
        {
			Id = default;
			Name = default;
			OrderName = default;
			SubType = default;
			IsPartOfEventTemplate = default;
			Sources = new List<ServiceTemplate>();
			ServiceOffsets = new Dictionary<Guid, TimeSpan>();
			Duration = default;
			Company = default;
			Contract = default;
			UserGroupIds = new HashSet<int>();
			Comments = default;
			SportsPlanning = default;
			NewsInformation = default;
			OperatorNotes = default;
			McrOperatorNotes = default;
			SecurityViewIds = new HashSet<int>();
			BillingInfo = new BillingInfo();
			CreatedByUserName = default(string);
        }

		public Guid Id { get; set; }

		public string Name { get; set; }

        public string OrderName { get; set; }

        public OrderSubType SubType { get; set; }

        public List<ServiceTemplate> Sources { get; set; }

		/// <summary>
		/// Dictionary that holds the service start time offsets from the order start time.
		/// Key: node id of the service template.
		/// </summary>
		public Dictionary<Guid, TimeSpan> ServiceOffsets { get; set; }

		public TimeSpan Duration { get; set; }

        public string Company { get; set; }

        public string Contract { get; set; }

        public HashSet<int> UserGroupIds { get; set; }

        public string Comments { get; set; }

		public SportsPlanning SportsPlanning { get; set; }

		public NewsInformation NewsInformation { get; set; }

		public string OperatorNotes { get; set; }

		public string McrOperatorNotes { get; set; }

		public HashSet<int> SecurityViewIds { get; set; }

		/// <summary>
		/// True if this template is part of an Event Template.
		/// </summary>
		public bool IsPartOfEventTemplate { get; set; }

		public RecurringSequenceInfo RecurringSequenceInfo { get; set; }

		public BillingInfo BillingInfo { get; set; }

		public string CreatedByUserName { get; set; }

		public bool IsTemplateForRecurringOrder { get; set; }

		public static OrderTemplate FromOrder(Helpers helpers, Order order, string templateName, bool isTemplateForRecurringOrder = false)
		{
			DateTime start = order.Start.RoundToMinutes();
			DateTime end = order.End.RoundToMinutes();

			order.RemoveAutogenerateServices();

			var template = new OrderTemplate 
			{
				Id = Guid.NewGuid(),
				Name = templateName,
				OrderName = order.Name,
				IsPartOfEventTemplate = false,
				Duration = end.Subtract(start),
				Company = order.Company,
				Contract = order.Contract,
				UserGroupIds = new HashSet<int>(order.UserGroupIds),
				Comments = order.Comments,
				SportsPlanning = order.SportsPlanning,
				NewsInformation = order.NewsInformation,
				OperatorNotes = order.MediaOperatorNotes,
				McrOperatorNotes = order.McrOperatorNotes,
				SecurityViewIds = new HashSet<int>(order.SecurityViewIds),
				Sources = GenerateServiceTemplates(helpers, order.Sources),
				ServiceOffsets = CalculateOffsets(order.AllServices, start),
				IsTemplateForRecurringOrder = isTemplateForRecurringOrder,
				RecurringSequenceInfo = (order.RecurringSequenceInfo.Recurrence?.IsConfigured == true) ? order.RecurringSequenceInfo : null,
				BillingInfo = order.BillingInfo,
				CreatedByUserName = order.CreatedByUserName,
				SubType = order.Subtype
			};

			return template;
		}

		public static OrderTemplate Deserialize(string serializedRequest)
		{
			try
			{
				return JsonConvert.DeserializeObject<OrderTemplate>(serializedRequest);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static bool OrderTemplatesAreEqual(Helpers helpers, OrderTemplate template1, OrderTemplate template2)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));
			if (template1 == null) throw new ArgumentNullException(nameof(template1));
			if (template2 == null) throw new ArgumentNullException(nameof(template2));

			string dummyName = "dummy name";
			var now = DateTime.Now;

			var order1 = Order.FromTemplate(helpers, template1, dummyName, now);
			var order2 = Order.FromTemplate(helpers, template2, dummyName, now);

			var duplicateTemplate1 = FromOrder(helpers, order1, dummyName, true);
			var duplicateTemplate2 = FromOrder(helpers, order2, dummyName, true);

			var templates = new List<OrderTemplate>
			{
				duplicateTemplate1,
				duplicateTemplate2
			};

			var serviceOffsetLists = new List<List<TimeSpan>>();

			// Set all random Guids in the template duplicates to same value and clear the serviceOffsets
			foreach (var orderTemplate in templates)
			{
				orderTemplate.Id = Guid.Empty;
				foreach (var orderTemplateSource in orderTemplate.Sources)
				{
					orderTemplateSource.Id = Guid.Empty;
					foreach (var serviceTemplate in orderTemplateSource.Children)
					{
						serviceTemplate.Id = Guid.Empty;
					}
				}

				serviceOffsetLists.Add(new List<TimeSpan>(orderTemplate.ServiceOffsets.Values));

				orderTemplate.ServiceOffsets = new Dictionary<Guid, TimeSpan>(); // clear for serialization comparison
			}

			var serializedTemplate1 = duplicateTemplate1.Serialize();
			var serializedTemplate2 = duplicateTemplate2.Serialize();

			bool serializationsAreEqual = serializedTemplate1 == serializedTemplate2;

			bool offsetsAreEqual = serviceOffsetLists[0].SequenceEqual(serviceOffsetLists.Last());

			return serializationsAreEqual && offsetsAreEqual;
		}

		public string Serialize()
		{
			try
			{
				return JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings{ReferenceLoopHandling = ReferenceLoopHandling.Serialize});
			}
			catch (Exception)
			{
				return null;
			}
		}

		public override bool Equals(object obj)
		{
			OrderTemplate other = obj as OrderTemplate;
			if (other == null) return false;
			return Id.Equals(other.Id);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public static List<ServiceTemplate> FlattenServiceTemplates(List<ServiceTemplate> serviceTemplates)
		{
			List<ServiceTemplate> templates = new List<ServiceTemplate>();
			if (serviceTemplates == null || !serviceTemplates.Any()) return templates;

			foreach (var serviceTemplate in serviceTemplates)
			{
				templates.Add(serviceTemplate);
				templates.AddRange(FlattenServiceTemplates(serviceTemplate.Children));
			}

			return templates;
		}

		private static List<ServiceTemplate> GenerateServiceTemplates(Helpers helpers, List<Service> sources)
		{
			var sourceTemplates = new List<ServiceTemplate>();
			var serviceNameToTemplateIdMapping = new Dictionary<string, Guid>();
			foreach (var service in sources)
			{
				if (service.IsAutogenerated) continue;
				sourceTemplates.Add(GenerateServiceTemplate(service, serviceNameToTemplateIdMapping));
			}

			FillOutServiceTemplateIdToRecordOrTransmit(helpers, sourceTemplates, OrderManager.FlattenServices(sources), serviceNameToTemplateIdMapping);

			return sourceTemplates;
		}

		private static ServiceTemplate GenerateServiceTemplate(Service service, Dictionary<string, Guid> serviceNameToTemplateIdMapping)
		{
			var template = ServiceTemplate.FromService(service);
			serviceNameToTemplateIdMapping.Add(service.Name, template.Id);

			foreach (var child in service.Children)
			{
				template.Children.Add(GenerateServiceTemplate(child, serviceNameToTemplateIdMapping));
			}

			return template;
		}

		private static Dictionary<Guid, TimeSpan> CalculateOffsets(List<Service> allServices, DateTime orderStart)
		{
			var offsets = new Dictionary<Guid, TimeSpan>();

			foreach (var service in allServices)
			{
				var start = service.Start.RoundToMinutes();
				offsets.Add(service.Id, start.Subtract(orderStart));
			}

			return offsets;
		}

		private static void FillOutServiceTemplateIdToRecordOrTransmit(Helpers helpers, List<ServiceTemplate> templates, List<Service> allServices, Dictionary<string, Guid> serviceNameToTemplateIdMapping)
		{
			foreach (var template in templates)
			{
				var service = allServices.SingleOrDefault(s => s.Id.Equals(template.Id));
				if (service != null) 
				{
					string serviceToRecordOrTransmit = service.NameOfServiceToTransmitOrRecord;

					if (serviceToRecordOrTransmit != null && serviceNameToTemplateIdMapping.ContainsKey(serviceToRecordOrTransmit))
					{
						template.ServiceTemplateIdToTransmitOrRecord = serviceNameToTemplateIdMapping[serviceToRecordOrTransmit];

						helpers.Log(nameof(OrderTemplate), nameof(FillOutServiceTemplateIdToRecordOrTransmit), $"Name of service to record/transmit is '{serviceToRecordOrTransmit}', set template ID to record/transmit to {template.ServiceTemplateIdToTransmitOrRecord}", service.Name);
					}
					else
					{
						helpers.Log(nameof(OrderTemplate), nameof(FillOutServiceTemplateIdToRecordOrTransmit), $"Name of service to record/transmit is '{serviceToRecordOrTransmit}' and/or can not be found between dict keys {string.Join(", ", serviceNameToTemplateIdMapping.Keys)}", service.Name);
					}
				}
				else
				{
					helpers.Log(nameof(OrderTemplate), nameof(FillOutServiceTemplateIdToRecordOrTransmit), $"Unable to find service with ID {template.Id} between services {string.Join(", ", allServices.Select(s => $"{s.Name}({s.Id})"))}");
				}

				FillOutServiceTemplateIdToRecordOrTransmit(helpers, template.Children, allServices, serviceNameToTemplateIdMapping);
			}
		}
	}
}

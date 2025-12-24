namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service
{
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using System;
	using System.Collections.Generic;

	public class ServiceTemplate
	{
        private ServiceTemplate()
        {
            Id = default(Guid);
			Duration = default(TimeSpan);
			BackupType = default(Order.BackupType);
			ServiceDefinitionName = default(string);
			ServiceDefinitionId = default(Guid);
			IsEurovisionService = default(bool);
			IsGlobalEventLevelReceptionService = default(bool);
			EurovisionBookingDetails = default(EurovisionBookingDetails);
			RequiresRouting = default(bool);
			RoutingConfigurationUpdateRequired = default(bool);
			Functions = new Dictionary<string, FunctionConfiguration>();
			Children = new List<ServiceTemplate>();
			Comments = default(string);
			ContactInformationName = default(string);
			ContactInformationTelephoneNumber = default(string);
			LiveUDeviceName = default(string);
            AudioReturnInfo = default(string);
			SecurityViewIds = new HashSet<int>();
			RecordingConfiguration = default(RecordingConfiguration);
			LinkedServiceTemplateId = default(Guid);
			ServiceTemplateIdToTransmitOrRecord = default(Guid);
			VidigoStreamSourceLink = default(string);
			AdditionalDescriptionUnknownSource = default(string);
			IsUnknownSourceService = false;
		}

		/// <summary>
		/// This ID is only used for linking the service with the list of StartTimeOffsets in the OrderTemplate.
		/// Do not assign this to the Service.Id field when creating a new Service instance!
		/// </summary>
		public Guid Id { get; set; }

        public TimeSpan Duration { get; set; }

        public YLE.Order.BackupType BackupType { get; set; }

        public string ServiceDefinitionName { get; set; }

		public Guid ServiceDefinitionId { get; set; }

		public bool IsEurovisionService { get; set; }

		public bool IsGlobalEventLevelReceptionService { get; set; }

		public EurovisionBookingDetails EurovisionBookingDetails { get; set; }

		public bool RequiresRouting { get; set; }

		public bool RoutingConfigurationUpdateRequired { get; set; }

        /// <summary>
        /// The function configurations for each function in this service.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, FunctionConfiguration> Functions { get; set; }

        public List<ServiceTemplate> Children { get; set; }

		public string Comments { get; set; }

		public string ContactInformationName { get; set; }

		public string ContactInformationTelephoneNumber { get; set; }

		public string LiveUDeviceName { get; set; }

        public string AudioReturnInfo { get; set; }

        public string VidigoStreamSourceLink { get; set; }

		public HashSet<int> SecurityViewIds { get; set; }

        public RecordingConfiguration RecordingConfiguration { get; set; }

		/// <summary>
		/// It only applies to Child Services (Destination/Recording) of a backup source when the Service Level of the Backup is Active.
		/// This is the Id of the ServiceTemplate to which this service is linked.
		/// </summary>
		public Guid LinkedServiceTemplateId { get; set; }

		public Guid ServiceTemplateIdToTransmitOrRecord { get; set; }

        public string AdditionalDescriptionUnknownSource { get; set; }

        public bool IsUnknownSourceService { get; set; }

		public static ServiceTemplate FromService(Service service)
		{
			DateTime start = service.Start.RoundToMinutes();
			DateTime end = service.End.RoundToMinutes();

			var template = new ServiceTemplate
			{
				Id = service.Id,
				Duration = end.Subtract(start),
				BackupType = service.BackupType,
				ServiceDefinitionName = service.Definition.Name,
				ServiceDefinitionId = service.Definition.Id,
                IsGlobalEventLevelReceptionService = service.IsSharedSource,
				IsEurovisionService = service.IsEurovisionService,
				EurovisionBookingDetails = service.EurovisionBookingDetails,
				RequiresRouting = service.RequiresRouting,
				Functions = new Dictionary<string, FunctionConfiguration>(),
				Comments = service.Comments.Clean(allowSiteContent: true),
				ContactInformationName = service.ContactInformationName,
				ContactInformationTelephoneNumber = service.ContactInformationTelephoneNumber,
				LiveUDeviceName = service.LiveUDeviceName,
				AudioReturnInfo = service.AudioReturnInfo,
				RecordingConfiguration = service.RecordingConfiguration,
				LinkedServiceTemplateId = (service.LinkedService?.Id == null) ? Guid.Empty : service.LinkedService.Id,
				SecurityViewIds = service.SecurityViewIds,
				VidigoStreamSourceLink = service.VidigoStreamSourceLink,
				AdditionalDescriptionUnknownSource = service.AdditionalDescriptionUnknownSource,
				IsUnknownSourceService = service.IsUnknownSourceService
			};

			foreach (var function in service.Functions)
			{
				template.Functions.Add(function.Definition.Label, function.Configuration);
			}

			return template;
		}

		public override bool Equals(object obj)
		{
			ServiceTemplate other = obj as ServiceTemplate;
			if (other == null) return false;
			return Id.Equals(other.Id);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}
}

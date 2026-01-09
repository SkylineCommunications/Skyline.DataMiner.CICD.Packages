namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using System;
	using System.Collections.Generic;

	public class LinkedServiceSectionCollection
    {
        private Guid displayedServiceId;

        private readonly Dictionary<Guid, ServiceSelectionSection> serviceSections = new Dictionary<Guid, ServiceSelectionSection>();

		public LinkedServiceSectionCollection(ServiceSelectionSection section)
		{
			AddSection(section.Service.Id, section);
			SetDisplayedSection(section.Service.Id);
		}

        public bool AddSection(Guid serviceId, ServiceSelectionSection section)
        {
            if (serviceSections.ContainsKey(serviceId)) return false;
            serviceSections.Add(serviceId, section);
            return true;
        }

        public void SetDisplayedSection(Guid serviceId)
        {
            displayedServiceId = serviceId;
        }

        public ServiceSelectionSection DisplayedSection
        {
            get
            {
                if (!serviceSections.ContainsKey(displayedServiceId)) return null;
                return serviceSections[displayedServiceId];
            }
        }

        public bool Contains(Guid serviceId)
        {
            return serviceSections.ContainsKey(serviceId);
        }

        public void SetVisibility(bool isVisible, Dictionary<Guid, ServiceSelectionSectionConfiguration> configurations = null)
        {
            configurations = configurations ?? new Dictionary<Guid, ServiceSelectionSectionConfiguration>();

            foreach(ServiceSelectionSection section in serviceSections.Values)
            {
                bool sectionCanBeVisible = configurations.TryGetValue(section.Service.Id, out var sectionConfig) ? sectionConfig.IsVisible : true;

                section.IsVisible = isVisible && sectionCanBeVisible;
            }
        }

        public void SetEnabled(bool isEnabled)
        {
            foreach (ServiceSelectionSection section in serviceSections.Values)
            {
                section.IsEnabled = isEnabled;
            }
        }
    }
}

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public static class LiveExtensions
	{
		public static Dictionary<string, string> GetEventProperties(this Event @event, Dictionary<string, int> allCompanies, List<Service> allEventLevelReceptions, List<string> files)
		{
			if (@event == null) return new Dictionary<string, string>();

			List<string> matchingCompanies = null;
			if (allCompanies != null && @event.SecurityViewIds != null && @event.SecurityViewIds.Any())
			{
				matchingCompanies = allCompanies.Where(x => @event.SecurityViewIds.Contains(x.Value)).Select(x => x.Key).ToList();
			}

			Dictionary<string, string> properties = new Dictionary<string, string>
			{
				{ "Id", Convert.ToString(@event.Id) },
				{ "Name", @event.Name },
				{ "Start Time", Convert.ToString(@event.Start, CultureInfo.InvariantCulture) },
				{ "End Time", Convert.ToString(@event.End, CultureInfo.InvariantCulture) },
				{ "Event Level RX", String.Empty },
				{ "Info", @event.Info },
				{ "Customer", @event.Company },
				{ "Contract", @event.Contract },
				{ "Visibility Rights", matchingCompanies != null && matchingCompanies.Any() ? string.Join(";", matchingCompanies) : string.Empty },
				{ "Status", EnumExtensions.GetDescriptionFromEnumValue(@event.Status) },
				{ "Attachments", string.Join(";", FilterOutFileNames(files)) },
			};

			return properties;
		}

		public static Dictionary<string, string> GetOrderProperties(this Order order, List<string> files)
		{
			if (order == null) return new Dictionary<string, string>();

			Dictionary<string, string> properties = new Dictionary<string, string>
			{
				{ "Id", Convert.ToString(order.Id) },
				{ "Name", order.Name },
				{ "Start Time", Convert.ToString(order.Start, CultureInfo.InvariantCulture) },
				{ "End Time", Convert.ToString(order.End, CultureInfo.InvariantCulture) },
				{ "Additional Information", order.Comments },
				{ "Created By", order.CreatedByUserName },
				{ "Customer", order.Event?.Company },
				{ "Status", EnumExtensions.GetDescriptionFromEnumValue(order.Status) },
				{ "Comments", order.Comments },
				{ "Attachments", string.Join(";", FilterOutFileNames(files)) },
				{ "Linked Event Id", Convert.ToString(order.Event?.Id) },
				{ "Billable Company", order.BillingInfo?.BillableCompany },
				{ "Customer Company", order.BillingInfo?.CustomerCompany },
				{ "Sources", order.SourceDescriptions },
				{ "Destinations", order.DestinationDescriptions },
				{ "Recordings", order.RecordingDescriptions },
				{ "Transmissions", order.TransmissionDescriptions },
				{ "Processing", order.GetProcessingDescription() },
			};

			return properties;
		}

		public static Dictionary<string, string> GetServiceProperties(this Service service, Order order)
		{
			if (service == null) return new Dictionary<string, string>();

			Dictionary<string, string> properties = new Dictionary<string, string>
			{
				{ "Id",  Convert.ToString(service.Id)},
				{ "Name",  service.Name },
				{ "Start Time",  Convert.ToString(service.Start, CultureInfo.InvariantCulture)},
				{ "End Time", Convert.ToString(service.End, CultureInfo.InvariantCulture)},
				{ "Technology", EnumExtensions.GetDescriptionFromEnumValue(service.Definition.VirtualPlatformServiceName) },
				{ "Tech Details", service.GetFormattedTechnicalDetails() },
				{ "Audio Format Details", service.AudioChannelConfiguration != null ? service.AudioChannelConfiguration.ToString() : string.Empty },
				{ "Status", EnumExtensions.GetDescriptionFromEnumValue(service.Status) },
				{ "Description" , order != null ? service.GetShortDescription(order) : string.Empty },
				{ "Recording Location" , service.RecordingConfiguration != null ? EnumExtensions.GetDescriptionFromEnumValue(service.RecordingConfiguration.RecordingFileDestination) : string.Empty },
				{ "Recording Signal", service.NameOfServiceToTransmitOrRecord },
				{ "Recording Time Codec", service.RecordingConfiguration != null ? EnumExtensions.GetDescriptionFromEnumValue(service.RecordingConfiguration.RecordingFileTimeCodec) : string.Empty },
				{ "Recording Video Codec", service.RecordingConfiguration != null ? EnumExtensions.GetDescriptionFromEnumValue(service.RecordingConfiguration.RecordingFileVideoCodec) : string.Empty },
				{ "Plasma ID", service.RecordingConfiguration != null ? service.RecordingConfiguration.PlasmaIdForArchive : string.Empty },
				{ "Additional Recording Needs", service.RecordingConfiguration != null ?  service.RecordingConfiguration.ToString() : string.Empty },
				{ "Recording File Destination" , service.RecordingConfiguration != null ? EnumExtensions.GetDescriptionFromEnumValue(service.RecordingConfiguration.RecordingFileDestination) : string.Empty },
				{ "Recording File Destination Path" , service.RecordingConfiguration?.EvsMessiNewsTarget ?? String.Empty },
				{ "Target" , string.Empty },
				{ "Linked Order Ids", service.OrderReferences != null && service.OrderReferences.Any() ? string.Join(";", service.OrderReferences) : string.Empty },
			};

			HandleServiceUserTaskProperties(service, properties);

			return properties;
		}

		private static void HandleServiceUserTaskProperties(Service service, Dictionary<string, string> properties)
		{
			if (service.UserTasks == null) return;

			properties["User Task 1 Description"] = service.UserTasks.Count >= 1 ? service.UserTasks[0].Description : string.Empty;
			properties["User Task 1 Status"] = service.UserTasks.Count >= 1 ? EnumExtensions.GetDescriptionFromEnumValue(service.UserTasks[0].Status) : string.Empty;
			properties["User Task 2 Description"] = service.UserTasks.Count >= 2 ? service.UserTasks[1].Description : string.Empty;
			properties["User Task 2 Status"] = service.UserTasks.Count >= 2 ? EnumExtensions.GetDescriptionFromEnumValue(service.UserTasks[1].Status) : string.Empty;
			properties["User Task 3 Description"] = service.UserTasks.Count >= 3 ? service.UserTasks[2].Description : string.Empty;
			properties["User Task 3 Status"] = service.UserTasks.Count >= 3 ? EnumExtensions.GetDescriptionFromEnumValue(service.UserTasks[2].Status) : string.Empty;
		}

		private static List<string> FilterOutFileNames(List<string> files)
		{
			List<string> existingFileNames = new List<string>();
			foreach (var file in files)
			{
				string[] splittedFilePath = file.Split(new[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
				existingFileNames.Add(splittedFilePath.Last());
			}

			return existingFileNames;
		}
	}
}

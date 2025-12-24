namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Sections;

	public static class JobExtensions
	{
		public static Section GetCustomEventSection(this Job job, Guid customEventSectionDefinitionId)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));

			return job.Sections.SingleOrDefault(s => s.GetSectionDefinition().GetID().Id == customEventSectionDefinitionId);
		}

		public static IEnumerable<OrderSection> GetOrderSections(this Job job)
		{
			var orderSections = new List<OrderSection>();

			foreach (var section in job.Sections)
			{
				foreach (var fieldValue in section.FieldValues)
				{
					bool sectionContainsOrderId = fieldValue.Value.Type == typeof(Guid);
					if (!sectionContainsOrderId) continue;

					orderSections.Add(new OrderSection(section));
				}
			}

			return orderSections;
		}

		public static OrderSection GetOrderSection(this Job job, Guid orderId)
		{
			return GetOrderSections(job).FirstOrDefault(s => s.OrderId == orderId);
		}

		public static void AddOrUpdateOrderSection(this Job job, OrderSection orderSection)
		{
			var existingSection = job.GetOrderSection(orderSection.OrderId);

			if (existingSection != null)
			{
				job.Sections.Remove(existingSection.Section);
			}

			job.Sections.Add(orderSection.Section);
		}

		public static bool HasOrderSections(this Job job)
		{
			foreach (var section in job.Sections)
			{
				foreach (var fieldValue in section.FieldValues)
				{
					bool sectionContainsOrderId = fieldValue.Value.Type == typeof(Guid);

					if (sectionContainsOrderId) return true;
				}
			}

			return false;
		}

		public static DateTime GetStartTime(this Job job)
		{
			var start = job.GetFieldValue(Event.PropertyNameStartTime);
			if (start == null) return default(DateTime);
			return DateTime.TryParse(start.ToString(), out var startTime) ? startTime : default(DateTime);
		}

		public static DateTime GetEndTime(this Job job)
		{
			var start = job.GetFieldValue(Event.PropertyNameEndTime);
			if (start == null) return default(DateTime);
			return DateTime.TryParse(start.ToString(), out var startTime) ? startTime : default(DateTime);
		}

		public static object GetFieldValue(this Job job, string fieldDescriptorName)
		{
			foreach (var section in job.Sections)
			{
				foreach (var fieldValue in section.FieldValues)
				{
					var fieldDescriptor = fieldValue.GetFieldDescriptor();
					if (fieldDescriptor == null) continue;

					if (String.Equals(fieldDescriptor.Name, fieldDescriptorName))
					{
						return fieldValue.Value.Value;
					}
				}
			}

			return null;
		}
	}
}

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event
{
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class EventTemplate
	{
		public Guid Id { get; set; }

		public string Name { get; set; }

		public string EventName { get; set; }

		public EventSubType EventSubType { get; set; }

		public TimeSpan Duration { get; set; }

		/// <summary>
		/// Dictionary that holds the service start time offsets from the order start time.
		/// Key: Id of the Order template.
		/// </summary>
		public Dictionary<Guid, TimeSpan> OrderOffsets { get; set; } = new Dictionary<Guid, TimeSpan>();

		public string ProjectNumber { get; set; }

		public string Info { get; set; }

		public string Contract { get; set; }

		public string Company { get; set; }

		public bool IsInternal { get; set; }

		public string OperatorNotes { get; set; }

		/// <summary>
		/// Gets a collection of Cube View IDs of views where this event is visible.
		/// </summary>
		public HashSet<int> SecurityViewIds { get; set; } = new HashSet<int>();

		public static EventTemplate FromEvent(Event @event, List<Tuple<Guid, DateTime>> orderTemplateStartTimes, string templateName)
		{
			DateTime start = @event.Start.RoundToMinutes();
			DateTime end = @event.End.RoundToMinutes();

			var template = new EventTemplate 
			{
				Id = Guid.NewGuid(),
				Name = templateName,
				EventName = @event.Name,
				Duration = end.Subtract(start),
				ProjectNumber = @event.ProjectNumber,
				Info = @event.Info,
				Contract = @event.Contract,
				Company = @event.Company,
				IsInternal = @event.IsInternal,
				OperatorNotes = @event.OperatorNotes,
				SecurityViewIds = @event.SecurityViewIds,
				OrderOffsets = CalculateOffsets(orderTemplateStartTimes, start)
			};

			return template;
		}

		private static Dictionary<Guid, TimeSpan> CalculateOffsets(List<Tuple<Guid, DateTime>> orderTemplateStartTimes, DateTime eventStart)
		{
			Dictionary<Guid, TimeSpan> offsets = new Dictionary<Guid, TimeSpan>();

			DateTime start;
			Guid templateId;
			for (int i = 0; i < orderTemplateStartTimes.Count; i++)
			{
				templateId = orderTemplateStartTimes[i].Item1;
				start = orderTemplateStartTimes[i].Item2.RoundToMinutes();

				offsets.Add(templateId, start.Subtract(eventStart));
			}

			return offsets;
		}

		public static EventTemplate Deserialize(string serializedRequest)
		{
			try
			{
				return JsonConvert.DeserializeObject<EventTemplate>(serializedRequest);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public string Serialize()
		{
			try
			{
				return JsonConvert.SerializeObject(this, Formatting.None);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public override bool Equals(object obj)
		{
			EventTemplate other = obj as EventTemplate;
			if (other == null) return false;
			return Id.Equals(other.Id);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}
}

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    public class Event
	{
		public string displayValue;

		public string EventId { get; set; }
		public string EventNumber { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string EventType { get; set; }
		public string Title { get; set; }
		public string City { get; set; }
		public List<BroadcastCenter> Feedpoints { get; set; }
		public List<MultilateralTransmission> MultilateralTransmissions { get; set; }

		public string DisplayValue
		{
			get
			{
				if (displayValue == null)
					displayValue = String.Format("{0} [{1} - {2}] ({3})", Title, StartDate.ToShortDateString(), EndDate.ToShortDateString(), EventNumber);

				return displayValue;
			}
		}

		public void UpdateFeedpoints(IActionableElement eurovision, IEngine engine)
		{
			// it's not necessary to update this again if we already retrieved this for this event
			if (Feedpoints != null) return;

			var columns = eurovision.GetTable(engine, 5900, new string[] { String.Format("fullFilter=5902 == '{0}';forcefulltable=true", EventNumber) });
			if (columns == null || columns.Count < 3) return;

			Feedpoints = new List<BroadcastCenter>();

			string[] broadcastCenterIds = columns[2];
			string code;
			string name;
			foreach (string broadcastCenterId in broadcastCenterIds)
			{
				if (String.IsNullOrWhiteSpace(broadcastCenterId)) continue;
				code = Convert.ToString(eurovision.GetParameterByPrimaryKey(8002, broadcastCenterId));
				name = Convert.ToString(eurovision.GetParameterByPrimaryKey(8003, broadcastCenterId));

				if (String.IsNullOrEmpty(code) || String.IsNullOrEmpty(name)) continue;

				var feedpoint = new BroadcastCenter
				{
					Id = broadcastCenterId,
					Code = code,
					Name = name
				};

				feedpoint.Update(eurovision, engine);
				Feedpoints.Add(feedpoint);
			}
		}

		public void UpdateMultilateralTransmissions(IActionableElement eurovision, IEngine engine)
		{
			// it's not necessary to update this again if we already retrieved this for this event
			if (MultilateralTransmissions != null)
				return;

			var columns = eurovision.GetTable(engine, 5300, new string[] { String.Format("fullFilter=5303 == '{0}';forcefulltable=true", EventNumber) });
			if (columns == null || columns.Count < 12) return;

			string[] ids = columns[0];
			string[] types = columns[1];
			string[] beginDates = columns[3];
			string[] endDates = columns[4];
			string[] programBeginDates = columns[5];
			string[] programEndDates = columns[6];
			string[] nature1s = columns[7];
			string[] nature2s = columns[8];
			string[] transmissionNumbers = columns[9];
			string[] states = columns[10];
			string[] productCodes = columns[11];

			var workOrdersColumns = eurovision.GetTable(engine, 7200, new string[] {$"columns=7222';forcefulltable=true"});
			if (workOrdersColumns == null || workOrdersColumns.Count < 2) return;

			var workOrdersTransmissionNumbersColumn = workOrdersColumns[1];

			MultilateralTransmissions = new List<MultilateralTransmission>();
			for (int i = 0; i < ids.Length; i++)
			{
				bool workOrderExistsForThisTransmission = workOrdersTransmissionNumbersColumn.Any(nr => nr == transmissionNumbers[i]);

				DateTime parsedStartDate = DateTime.FromOADate(Convert.ToDouble(beginDates[i], CultureInfo.InvariantCulture));
				DateTime parsedEndDate = DateTime.FromOADate(Convert.ToDouble(endDates[i], CultureInfo.InvariantCulture));
				DateTime parsedProgramStart = DateTime.FromOADate(Convert.ToDouble(programBeginDates[i], CultureInfo.InvariantCulture));
				DateTime parsedProgramEnd = DateTime.FromOADate(Convert.ToDouble(programEndDates[i], CultureInfo.InvariantCulture));

				DateTime startDate = new DateTime(parsedStartDate.Ticks, DateTimeKind.Utc);
				DateTime endDate = new DateTime(parsedEndDate.Ticks, DateTimeKind.Utc);
				DateTime programStart = new DateTime(parsedProgramStart.Ticks, DateTimeKind.Utc);
				DateTime programEnd = new DateTime(parsedProgramEnd.Ticks, DateTimeKind.Utc);

				MultilateralTransmissions.Add(new MultilateralTransmission(
					ids[i],
					types[i],
					startDate,
					endDate,
					programStart,
					programEnd,
					nature1s[i],
					nature2s[i],
					transmissionNumbers[i],
					states[i],
					productCodes[i],
					workOrderExistsForThisTransmission));
			}
		}
	}
}
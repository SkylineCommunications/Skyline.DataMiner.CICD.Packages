namespace ShowPebbleBeachDetails_2
{
	using System;
	using System.Linq;
	using ShowPebbleBeachDetails_2.PebbleBeach;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class PebbleBeachManager
	{
		private const string PROTOCOL_NAME = "Pebble Beach EMS";
		private const int EVENTS_TABLE = 2100;

		private readonly Engine engine;
		private readonly IDmsElement element;

		public PebbleBeachManager(Engine engine)
		{
			this.engine = engine;
			var dms = Engine.SLNetRaw.GetDms();
			Element auxElement = engine.FindElementsByProtocol(PROTOCOL_NAME).FirstOrDefault();
			if (auxElement == default(Element) || !auxElement.IsActive)
			{
				throw new PebbleBeachException("Plasma element not found or inactive");
			}

			element = dms.GetElement(new DmsElementId(String.Format("{0}/{1}", auxElement.DmaId, auxElement.ElementId)));
		}

		public bool PebbleBeachEventExists(string plasmaId)
		{
			try
			{
				object[][] eventTableRows = element.GetTable(EVENTS_TABLE).GetRows();
				foreach (object[] row in eventTableRows)
				{
					if (Convert.ToString(row[4]).Equals(plasmaId))
					{
						return true;
					}
				}

				return false;
			}
			catch (Exception e)
			{
				engine.Log("[PlasmaManager] ProgramExists: error while checking if program " + plasmaId + " exists: " + e);
				return false;
			}
		}

		public PebbleBeachEvent GetEvent(string pebbleBeachEventId)
		{
			if (!PebbleBeachEventExists(pebbleBeachEventId))
			{
				return null;
			}

			try
			{
				object[][] eventTableRows = element.GetTable(EVENTS_TABLE).GetRows();
				foreach (object[] row in eventTableRows)
				{
					if (Convert.ToString(row[4]).Equals(pebbleBeachEventId))
					{
						return new PebbleBeachEvent
						{
							Id = Convert.ToString(row[0]),

							Uid = Convert.ToString(row[1]),

							PlaylistId = Convert.ToString(row[2]),

							Title = Convert.ToString(row[3]),

							HouseId = Convert.ToString(row[4]),

							Status = EnumExtensions.GetDescriptionFromEnumValue((EventStatus) Convert.ToInt32(row[5])),

							Type = Convert.ToString(row[6]),

							Start = DateTime.FromOADate(Convert.ToDouble(row[7])),

							Duration = new TimeSpan(hours: 0, minutes: Convert.ToInt32(row[8]), seconds: 0),

							ReconcileKey = Convert.ToString(row[9]),

							Source = Convert.ToString(row[10]),

							BackupSource = Convert.ToString(row[11]),

							Destination = Convert.ToString(row[12]),

							BackupDestination = Convert.ToString(row[13]),

							BlockId = Convert.ToString(row[14]),

							RunningState = Convert.ToString(row[15]),

							Response = Convert.ToString(row[16]),

							PossibleSources = Convert.ToString(row[17])
						};
					}
				}

				return null;
			}
			catch (Exception e)
			{
				engine.Log("[PebbleBeachManager] Get: error while getting project " + pebbleBeachEventId + ": " + e.Message + e.StackTrace + e.Data);
				return null;
			}
		}
	}
}
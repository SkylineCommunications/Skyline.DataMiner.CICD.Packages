namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Threading;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    public static class EurovisionExtensions
	{
		public static Dictionary<string, Event> GetEvents(this IActionableElement eurovision, IEngine engine)
		{
			var filter = new string[] { "fullFilter=5107 != 'CANCELLED' AND 5107 != 'BINNED' AND 5107 != 'N/A';forcefulltable=true" };
			var columns = eurovision.GetTable(engine, 5100, filter);
			if (columns == null || columns.Count < 12)
				return null;

			var eventIds = columns[0];
			var eventNumbers = columns[1];
			var eventStartDates = columns[2];
			var eventEndDates = columns[3];
			var eventTitles = columns[4];
			var eventTypes = columns[8];
			var eventCityCodes = columns[11];

			var dtNow = DateTime.Now;
			var eventDictionary = new Dictionary<string, Event>();
			for (int i = 0; i < eventIds.Length; i++)
			{
				DateTime parsedStartTime = DateTime.FromOADate(Convert.ToDouble(eventStartDates[i], CultureInfo.InvariantCulture));
				DateTime startTime = new DateTime(parsedStartTime.Ticks, DateTimeKind.Utc);

				DateTime parsedEndTime = DateTime.FromOADate(Convert.ToDouble(eventEndDates[i], CultureInfo.InvariantCulture));
				DateTime endTime = new DateTime(parsedEndTime.Ticks, DateTimeKind.Utc);

				var eurovisionEvent = new Event
				{
					EventId = eventIds[i],
					EventNumber = eventNumbers[i],
					EventType = eventTypes[i],
					StartDate = startTime,
					EndDate = endTime,
					Title = eventTitles[i],
					City = eventCityCodes[i]
				};

				if (eurovisionEvent.EndDate > dtNow)
					eventDictionary[eurovisionEvent.DisplayValue] = eurovisionEvent;
			}

			return eventDictionary;
		}

		public static List<Audio> GetAudios(this IActionableElement eurovision, IEngine engine)
		{
			var columns = eurovision.GetTable(engine, 4000, new string[] { "forcefulltable=true" });
			if (columns == null || columns.Count < 2) return null;

			var audioCodes = columns[0];
			var audioNames = columns[1];

			var audios = new List<Audio>();
			for (int i = 0; i < audioCodes.Length; i++)
			{
				if (String.IsNullOrEmpty(audioCodes[i]) || String.IsNullOrEmpty(audioNames[i])) continue;
				audios.Add(new Audio(audioCodes[i], audioNames[i]));
			}

			return audios;
		}

		public static List<Organization> GetOrganizations(this IActionableElement eurovision, IEngine engine)
		{
			var columns = eurovision.GetTable(engine, 5020, new string[] { "forcefulltable=true" });
			if (columns == null || columns.Count < 9)
				return null;

			var codes = columns[1];
			var names = columns[2];
			var countryCodes = columns[5];
			var cityCodes = columns[6];
			var supportsUNI = columns[7];
			var supportsOSS = columns[8];

			var organizations = new List<Organization>();
			for (int i = 0; i < codes.Length; i++)
			{
				organizations.Add(new Organization
				{
					Code = codes[i],
					Name = names[i],
					SupportsUNI = Convert.ToInt32(supportsUNI[i]) == 1,
					SupportsOSS = Convert.ToInt32(supportsOSS[i]) == 1
				});
			}

			return organizations;
		}

		public static List<BroadcastCenter> GetBroadcastCenters(this IActionableElement eurovision, IEngine engine)
		{
			var columns = eurovision.GetTable(engine, 8000, new string[] { "forcefulltable=true" });
			if (columns == null || columns.Count < 12)
				return null;

			var broadcastCenterIds = columns[0];
			var broadcastCenterCodes = columns[1];
			var broadcastCenterNames = columns[2];
			var broadcastCenterCityCodes = columns[7];
			var broadcastCenterSupportsUNI = columns[10];
			var broadcastCenterSupportsOSSUNI = columns[11];

			var broadcastCenters = new List<BroadcastCenter>();
			for (int i = 0; i < broadcastCenterIds.Length; i++)
			{
				var broadcastCenter = new BroadcastCenter
				{
					Id = broadcastCenterIds[i],
					Code = broadcastCenterCodes[i],
					Name = broadcastCenterNames[i],
					SupportsUNI = broadcastCenterSupportsUNI[i] == "1",
					SupportsOSSUNI = broadcastCenterSupportsOSSUNI[i] == "1"
				};

				broadcastCenters.Add(broadcastCenter);
			}

			return broadcastCenters;
		}

		public static List<City> GetCities(this IActionableElement eurovision, IEngine engine)
		{
			var columns = eurovision.GetTable(engine, 5010, new string[] { "forcefulltable=true" });
			if (columns == null || columns.Count < 3) return null;

			var cityCodes = columns[0];
			var cityNames = columns[1];
			var cityCountryCodes = columns[2];

			var cities = new List<City>();
			for (int i = 0; i < cityCodes.Length; i++)
				cities.Add(new City(cityCodes[i], cityNames[i], cityCountryCodes[i]));

			return cities;
		}

		public static List<Satellite> GetSatellites(this IActionableElement eurovision, IEngine engine)
		{
			var columns = eurovision.GetTable(engine, 3500, new string[] { "forcefulltable=true" });
			if (columns == null || columns.Count < 5)
				return null;

			var ids = columns[0];
			var codes = columns[1];
			var names = columns[2];
			var productCodes = columns[3];
			var families = columns[4];

			var satellites = new List<Satellite>();
			for (int i = 0; i < ids.Length; i++)
			{
				if (String.IsNullOrEmpty(names[i]))
					continue;

				satellites.Add(new Satellite(
					ids[i],
					codes[i],
					names[i],
					productCodes[i],
					families[i]));
			}

			return satellites;
		}

		public static List<Transportable> GetTransportables(this IActionableElement eurovision, IEngine engine)
		{
			var columns = eurovision.GetTable(engine, 3800, new string[] { "forcefulltable=true" });
			if (columns == null || columns.Count < 3)
				return null;

			var ids = columns[0];
			var codes = columns[1];
			var names = columns[2];

			var transportables = new List<Transportable>();
			for (int i = 0; i < ids.Length; i++)
			{
				transportables.Add(new Transportable(
					ids[i],
					codes[i],
					names[i]));
			}

			return transportables;
		}

		public static List<Contract> GetContracts(this IActionableElement eurovision, IEngine engine)
		{
			var columns = eurovision.GetTable(engine, 9000, new string[] { "forcefulltable=true" });
			if (columns == null || columns.Count < 3)
				return null;

			var ids = columns[0];
			var codes = columns[1];
			var names = columns[2];

			var contracts = new List<Contract>();
			for (int i = 0; i < ids.Length; i++)
			{
				if (String.IsNullOrEmpty(names[i]))
					continue;

				contracts.Add(new Contract(ids[i], codes[i], names[i]));
			}

			return contracts;
		}

		public static bool CheckForSuccessfulJsonResponse(this IActionableElement eurovision, string reference, out string requestId)
		{
			requestId = String.Empty;

			var remainingRetries = 100;
			var success = false;
			while (remainingRetries > 0 && !success)
			{
				var externalRequestKeys = eurovision.GetTablePrimaryKeys(20001);
				if (externalRequestKeys.Contains(reference))
				{
					var responseJson = (string)eurovision.GetParameterByPrimaryKey(20007, reference);
					if (String.IsNullOrEmpty(responseJson) || responseJson == "-1") // pending
					{
						remainingRetries--;
						Thread.Sleep(250);
						continue;
					}
					else if (responseJson == "-2") // timeout
					{
						success = false;
						break;
					}
					else if (!((string)eurovision.GetParameterByPrimaryKey(20006, reference)).Contains("200 OK"))
					{
						success = false;
						break;
					}
					else
					{
						try
						{
							var response = JsonConvert.DeserializeObject<CreateResponseJson>(responseJson);
							requestId = response.RequestId;
						}
						catch (Exception) { }

						success = true;
						break;
					}
				}
			}

			return success;
		}

		public static bool CheckForSuccessfulXmlResponse(this IActionableElement eurovision, IEngine engine, string reference, out string key)
		{
			key = "";

			var remainingRetries = 100;
			var success = false;
			while (remainingRetries > 0 && !success)
			{
				var externalRequestKeys = eurovision.GetTablePrimaryKeys(20001);
				if (!externalRequestKeys.Contains(reference))
				{
					remainingRetries--;
					Thread.Sleep(250);
					continue;
				}
							
				var responseXml = (string)eurovision.GetParameterByPrimaryKey(20007, reference);
				if (String.IsNullOrEmpty(responseXml) || responseXml == "-1") // pending
				{
					remainingRetries--;
					Thread.Sleep(250);
					continue;
				}
				else if (responseXml == "-2") // timeout
				{
					success = false;
					break;
				}
				else if (!((string)eurovision.GetParameterByPrimaryKey(20006, reference)).Contains("200 OK"))
				{
					success = false;
					break;
				}
				else
				{
					CreateResponseSuccessXml response;
					if (CreateResponseSuccessXml.TryParse(engine, responseXml, out response))
					{
						key = response.RequestId;
						success = true;
						break;
					}
					else
					{
						success = false;
						break;
					}
				}
				
			}

			return success;
		}
	}
}
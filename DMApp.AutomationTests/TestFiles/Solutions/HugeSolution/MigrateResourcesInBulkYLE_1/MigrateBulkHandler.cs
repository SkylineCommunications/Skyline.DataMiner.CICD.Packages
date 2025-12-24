namespace MigrateResourcesInBulkYLE_1
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Messages;

	public class MigrateBulkHandler
	{
		private readonly Helpers helpers;
		private readonly IDms dms;
		private readonly int currentDmaId;

		public MigrateBulkHandler(Helpers helpers, List<string> existingElementNames, string protocolName, string protocolVersion)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			dms = Skyline.DataMiner.Automation.Engine.SLNetRaw.GetDms();
			currentDmaId = (helpers.Engine.SendSLNetSingleResponseMessage(new GetInfoMessage(InfoType.LocalDataMinerInfo)) as GetDataMinerInfoResponseMessage).ID;
			
			ExistingElementNames = existingElementNames;
			ProtocolName = protocolName;
			ProtocolVersion = protocolVersion;
		}

		public List<string> ExistingElementNames { get; private set; }

		public string ProtocolName { get; set; }

		public string ProtocolVersion { get; set; }

		public void Migrate()
		{
			foreach (var existingElementName in ExistingElementNames)
			{
				IDmsElement existingElement = null;
				try
				{
					existingElement = dms.GetElement(existingElementName);
				}
				catch (Exception)
				{
					// No Action
				}

				if (existingElement == null) continue;

				var newElementName = existingElementName + "_2";
				var newElementId = CreateEricssonRX8200Element(newElementName, existingElementName);
				if (newElementId.ElementId == 0 || newElementId.AgentId == 0) continue;

				var newElement = WaitUntilNewElementIsAvailable(newElementName, newElementId);

				var elementResourceMigrationResult = ExecuteResourceMigrationScript(existingElement, newElementId);
				UpdateNewElementName(existingElementName, newElement, elementResourceMigrationResult);
			}
		}

		private void UpdateNewElementName(string existingElementName, IDmsElement newElement, ElementResourceMigrationResult elementResourceMigrationResult)
		{
			if (elementResourceMigrationResult.IsSuccessful)
			{
				helpers.Engine.Sleep(5000); // Need to wait an extra 5 seconds to make sure connection is fully established in the background for the newly created element

				newElement.Name = existingElementName;
				newElement.Update();
			}
		}

		private IDmsElement WaitUntilNewElementIsAvailable(string newElementName, DmsElementId newElementId)
		{
			bool isElementAvailable = Retry(() => dms.ElementExists(newElementName), TimeSpan.FromSeconds(10));
			if (!isElementAvailable)
			{
				throw new ElementNotActiveException($"Element {newElementName} is not available in time after creation");
			}

			var newElement = dms.GetElement(newElementId);

			bool isElementDataLoaded = Retry(() => newElement.GetTable(1600).RowExists("1"), TimeSpan.FromSeconds(120));
			if (!isElementDataLoaded)
			{
				throw new ElementNotActiveException($"Element {newElementName} doesn't have satellite input in time");
			}

			return newElement;
		}

		private ElementResourceMigrationResult ExecuteResourceMigrationScript(IDmsElement existingElement, DmsElementId newElementId)
		{
			var migrateResourcesSubScript = helpers.Engine.PrepareSubScript("MigrateResourcesYLE");
			migrateResourcesSubScript.Synchronous = true;
			migrateResourcesSubScript.SelectScriptParam("oldElementId", existingElement.DmsElementId.Value);
			migrateResourcesSubScript.SelectScriptParam("newElementId", newElementId.Value);
			migrateResourcesSubScript.StartScript();

			var result = migrateResourcesSubScript.GetScriptResult()["ElementResourceMigrationResult"];
			helpers.Engine.ClearScriptOutput("ElementResourceMigrationResult");

			helpers.Log(nameof(MigrateBulkHandler), nameof(ExecuteResourceMigrationScript), $"ElementResourceMigrationResult: {result}");

			var elementResourceMigrationResult = JsonConvert.DeserializeObject<ElementResourceMigrationResult>(result);
			return elementResourceMigrationResult;
		}

		private DmsElementId CreateEricssonRX8200Element(string newElementName, string existingElementName)
		{
			if (!IrdMapping.IrdMappings.TryGetValue(existingElementName, out var matchingIp))
			{
				helpers.Log(nameof(MigrateBulkHandler), nameof(CreateEricssonRX8200Element), $"No matching IP found for element: {existingElementName}");
				return new DmsElementId();
			}

			IDma agent = dms.GetAgent(currentDmaId);

			IDmsProtocol elementProtocol = dms.GetProtocol(ProtocolName, ProtocolVersion);

			IUdp portSnmp = new Udp(matchingIp, 161);
			ISnmpV2Connection mySnmpV2Connection = new SnmpV2Connection(portSnmp)
			{
				GetCommunityString = "public",
				SetCommunityString = "private",
				Retries = 3,
				Timeout = TimeSpan.FromMilliseconds(1500),
				ElementTimeout = null,
			};

			ITcp portHttp = new Tcp(matchingIp, 80);
			IHttpConnection myHttpConnection = new HttpConnection(portHttp)
			{
				Timeout = TimeSpan.FromMilliseconds(1500),
				Retries = 3,
				ElementTimeout = null
			};

			var configuration = new ElementConfiguration(
				dms,
				newElementName,
				elementProtocol,
				new List<IElementConnection> { mySnmpV2Connection, myHttpConnection });

			return agent.CreateElement(configuration);
		}

		/// <summary>
		/// Retry until success or until timeout. 
		/// </summary>
		/// <param name="func">Operation to retry.</param>
		/// <param name="timeout">Max TimeSpan during which the operation specified in <paramref name="func"/> can be retried.</param>
		/// <returns><c>true</c> if one of the retries succeeded within the specified <paramref name="timeout"/>. Otherwise <c>false</c>.</returns>
		private static bool Retry(Func<bool> func, TimeSpan timeout)
		{
			bool success = false;

			Stopwatch sw = new Stopwatch();
			sw.Start();

			do
			{
				success = func();
				if (!success)
				{
					System.Threading.Thread.Sleep(100);
				}
			}
			while (!success && sw.Elapsed <= timeout);

			return success;
		}
	}
}

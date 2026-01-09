namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.EVS
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Security;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.ConnectorAPI.EVS.IPD_VIA.Messages;
	using Skyline.DataMiner.ConnectorAPI.EVS.IPD_VIA.Model;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallBulk;
	using Skyline.DataMiner.Core.InterAppCalls.Common.Shared;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using ColumnFilter = Core.DataMinerSystem.Common.ColumnFilter;
	using Service = Service.Service;

	public class EvsManager
	{
		public static readonly string DefaultMessiLiveTarget = "Ingest to Nearline";

		private readonly Dictionary<Type, Type> executorMap = new Dictionary<Type, Type>
		{
			{ typeof(AddOrUpdateRecordingSessionResult), typeof(AddOrUpdateRecordingSessionExecutor) },
			{ typeof(DeleteRecordingSessionResult), typeof(DeleteRecordingSessionExecutor) },
		};

		private readonly List<Type> knownTypes = new List<Type>
		{
			typeof(AddOrUpdateRecordingSession),
			typeof(AddOrUpdateRecordingSessionResult),
			typeof(DeleteRecordingSession),
			typeof(DeleteRecordingSessionResult),
			typeof(RecordingSession),
			typeof(Metadata),
			typeof(List<Metadata>),
			typeof(Metadata[]),
			typeof(Dictionary<string, string>),
			typeof(ReturnAddress)
		};

		private readonly Helpers helpers;
		private readonly IDmsElement element;

		private IDictionary<string, object[]> targetsTable;
		private IDictionary<string, object[]> recodersTable;

		public EvsManager(Helpers helpers)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));

			var dms = Engine.SLNetRaw.GetDms();
			element = dms.GetElements().FirstOrDefault(x => x.Protocol.Name.Equals(EvsIpdViaProtocol.Name) && x.State == ElementState.Active) ?? throw new ElementByProtocolNotFoundException(EvsIpdViaProtocol.Name);
		}

		public List<Recorder> GetRecorderNames()
		{
			recodersTable = recodersTable ?? element.GetTable(EvsIpdViaProtocol.RecordersTable.TablePid)?.GetData();
			return recodersTable.Select(x => new Recorder { Instance = x.Key, Name = Convert.ToString(x.Value[EvsIpdViaProtocol.RecordersTable.Idx.RecordersName]) }).ToList();
		}

		public IEnumerable<string> GetTargetNames()
		{
			targetsTable = targetsTable ?? element.GetTable(EvsIpdViaProtocol.TargetsTable.TablePid).GetData();
			return targetsTable.Values.Select(x => Convert.ToString(x[EvsIpdViaProtocol.TargetsTable.Idx.TargetsName])).ToList();
		}

		public void AddOrUpdateRecordingSession(Service recordingService, RecordingSession recordingSession)
		{
			helpers.Log(nameof(EvsManager), nameof(AddOrUpdateRecordingSession), $"EVS Id for service: {recordingService.Name} => {(String.IsNullOrWhiteSpace(recordingService.EvsId) ? "No ID assigned" : recordingService.EvsId)}");
			helpers.Log(nameof(EvsManager), nameof(AddOrUpdateRecordingSession), $"Adding or updating recording session: {JsonConvert.SerializeObject(recordingSession)}");

			var commands = InterAppCallFactory.CreateNew();
			commands.ReturnAddress = new ReturnAddress(element.AgentId, element.Id, 9000001);
			commands.Messages.Add(new AddOrUpdateRecordingSession
			{
				RecordingSession = recordingSession
			});

			var responses = commands.Send(Engine.SLNetRaw, element.AgentId, element.Id, 9000000, TimeSpan.FromSeconds(30), knownTypes);
			foreach (var response in responses)
			{
				if (!response.TryExecute(helpers, recordingService, executorMap, out var responseMessage))
				{
					helpers.Log(nameof(EvsManager), nameof(AddOrUpdateRecordingSession), $"Handling response message failed");
				}
			}

			helpers.Log(nameof(EvsManager), nameof(AddOrUpdateRecordingSession), $"EVS Recording Session Id: {recordingService.EvsId}");
		}

		public bool DeleteRecordingSession(Service recordingService, string recordingSessionId)
		{
			try
			{
				var commands = InterAppCallFactory.CreateNew();
				commands.ReturnAddress = new ReturnAddress(element.AgentId, element.Id, 9000001);
				commands.Messages.Add(new DeleteRecordingSession
				{
					RecordingSessionsId = recordingSessionId
				});

				helpers.Log(nameof(EvsManager), nameof(DeleteRecordingSession), $"Deleting recording session {recordingSessionId}");

				commands.Send(Engine.SLNetRaw, element.AgentId, element.Id, 9000000, knownTypes);
				recordingService.EvsId = null;

				return true;
			}
			catch (Exception e)
			{
				helpers.Log(nameof(EvsManager), nameof(DeleteRecordingSession), $"Unable to delete Recording Session due to: {e}");
				return false;
			}
		}

		/// <summary>
		/// Retrieves the metadata labels from the Profile Fields table.
		/// </summary>
		/// <returns>All labels from the Profile Fields table.</returns>
		public List<Label> GetMetadataLabels()
		{
			var profileFieldsTable = element.GetTable(EvsIpdViaProtocol.ProfileFieldsTable.TablePid);

			return profileFieldsTable.GetData().Values.Select(x => new Label
			{
				Key = Convert.ToString(x[EvsIpdViaProtocol.ProfileFieldsTable.Idx.ProfileFieldsKey]),
				Name = Convert.ToString(x[EvsIpdViaProtocol.ProfileFieldsTable.Idx.ProfileFieldsLabel]),
				Type = Convert.ToString(x[EvsIpdViaProtocol.ProfileFieldsTable.Idx.ProfileFieldsType]),
				Required = Convert.ToBoolean(x[EvsIpdViaProtocol.ProfileFieldsTable.Idx.ProfileFieldsRequired]),
				ProfileFqn = Convert.ToString(x[EvsIpdViaProtocol.ProfileFieldsTable.Idx.ProfileFieldsProfileFqn]),
				ProfileName = Convert.ToString(x[EvsIpdViaProtocol.ProfileFieldsTable.Idx.ProfileFieldsProfileName])
			}).ToList();
		}

		public bool TryGetRecordingSession(string evsId, out RecordingSession recordingSession)
		{
			recordingSession = new RecordingSession();

			if (String.IsNullOrWhiteSpace(evsId)) return false;

			var recordingSessionsTable = element.GetTable(EvsIpdViaProtocol.RecordingSessionsTable.TablePid);
			if (!recordingSessionsTable.RowExists(evsId)) return false;

			var row = recordingSessionsTable.GetRow(evsId);
			if (row == null) return false;

			recordingSession.Id = Convert.ToString(row[EvsIpdViaProtocol.RecordingSessionsTable.Idx.RecordingSessionsInstanceIdx]);
			recordingSession.Name = Convert.ToString(row[EvsIpdViaProtocol.RecordingSessionsTable.Idx.RecordingSessionsNameIdx]);
			recordingSession.Start = DateTime.FromOADate(Convert.ToDouble(row[EvsIpdViaProtocol.RecordingSessionsTable.Idx.RecordingSessionsStartIdx]));
			recordingSession.End = DateTime.FromOADate(Convert.ToDouble(row[EvsIpdViaProtocol.RecordingSessionsTable.Idx.RecordingSessionsEndIdx]));
			recordingSession.Recorder = Convert.ToString(row[EvsIpdViaProtocol.RecordingSessionsTable.Idx.RecordingSessionsRecorderIdx]);

			// Get Targets
			var recordingSessionsTargetsTable = element.GetTable(EvsIpdViaProtocol.RecordingSessionsTargetsTable.TablePid);
			recordingSession.Targets = recordingSessionsTargetsTable.QueryData(new[]
			{
				new ColumnFilter
				{
					Pid = EvsIpdViaProtocol.RecordingSessionsTargetsTable.Pid.RecordingSessionsTargetsRecordingSessionInstance,
					ComparisonOperator = Core.DataMinerSystem.Common.ComparisonOperator.Equal,
					Value = evsId
				}
			}).Select(x => Convert.ToString(x[EvsIpdViaProtocol.RecordingSessionsTargetsTable.Idx.RecordingSessionsTargetsTarget])).ToArray();

			// Get MetaData
			var metaDataTable = element.GetTable(EvsIpdViaProtocol.RecordingSessionsMetadataValuesTable.TablePid);
			var metaDataEntries = metaDataTable.QueryData(new[]
			{
				new ColumnFilter
				{
					Pid = EvsIpdViaProtocol.RecordingSessionsMetadataValuesTable.Pid.RecordingSessionsMetadataValuesRecordingSessionId,
					ComparisonOperator = Core.DataMinerSystem.Common.ComparisonOperator.Equal,
					Value = evsId
				}
			});

			Dictionary<string, Metadata> metadataToStore = new Dictionary<string, Metadata>();
			foreach (var metaDataEntry in metaDataEntries)
			{
				string profileFqn = Convert.ToString(metaDataEntry[EvsIpdViaProtocol.RecordingSessionsMetadataValuesTable.Idx.RecordingSessionsMetadataValuesProfile]);
				string label = Convert.ToString(metaDataEntry[EvsIpdViaProtocol.RecordingSessionsMetadataValuesTable.Idx.RecordingSessionsMetadataValuesKey]);
				string value = Convert.ToString(metaDataEntry[EvsIpdViaProtocol.RecordingSessionsMetadataValuesTable.Idx.RecordingSessionsMetadataValuesValue]);

				if (metadataToStore.TryGetValue(profileFqn, out Metadata metadata))
				{
					metadata.Values[label] = value;
				}
				else
				{
					metadataToStore.Add(profileFqn, new Metadata
					{
						Profile = profileFqn,
						Values = new Dictionary<string, string>
						{
							{ label, value }
						}
					});
				}
			}

			recordingSession.Metadata = metadataToStore.Values;

			return true;
		}
	}

	public class Recorder
	{
		public string Instance { get; set; }

		public string Name { get; set; }
	}

	public class Label
	{
		public string Name { get; set; }

		public string ProfileName { get; set; }

		public string Key { get; set; }

		public string ProfileFqn { get; set; }

		public bool Required { get; set; }

		public string Type { get; set; }
	}
}
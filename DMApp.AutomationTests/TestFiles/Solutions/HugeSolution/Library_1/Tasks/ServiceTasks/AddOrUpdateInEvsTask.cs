namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.ConnectorAPI.EVS.IPD_VIA.Model;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.EVS;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.NetInsightNimbra.Manager;
	using Skyline.DataMiner.Library.Solutions.SRM.Logging.Orchestration;
	using Skyline.DataMiner.Utils.YLE.Integrations.Plasma.VCM;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Service.Service;

	public class AddOrUpdateInEvsTask : Task
	{
		private readonly Service service;
		private const string ProfileName_YleNameAndId = "yle-nameandid";
		private const string ProfileFieldKey_OrderName = "order-name";

		public AddOrUpdateInEvsTask(Helpers helpers, Service service) : base(helpers)
		{
			this.service = service;
			IsBlocking = false;
		}

		public override string Description => $"Registering {service.Name} in EVS";

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			bool isEligibleForEvsAutomation = IsEligibleForEvsAutomation();
			bool hasExistingEvsRecordingSession = helpers.EvsManager.TryGetRecordingSession(service.EvsId, out RecordingSession existingRecordingSession);

			if (!isEligibleForEvsAutomation)
			{
				// Delete existing EVS recording session if it is not recording yet
				if (hasExistingEvsRecordingSession && service.Start > DateTime.Now)
				{
					helpers.Log(nameof(AddOrUpdateInEvsTask), nameof(InternalExecute), "Removing existing EVS recording");
					helpers.EvsManager.DeleteRecordingSession(service, service.EvsId);
				}
			}
			else if (hasExistingEvsRecordingSession && existingRecordingSession.End <= DateTime.Now)
			{
				helpers.Log(nameof(AddOrUpdateInEvsTask), nameof(InternalExecute), "Unable to update EVS recording that already finished");
			}
			else if (hasExistingEvsRecordingSession && DateTime.Now <= existingRecordingSession.Start && DateTime.Now <= existingRecordingSession.End)
			{
				// Update ongoing recording session
				helpers.Log(nameof(AddOrUpdateInEvsTask), nameof(InternalExecute), $"Updating ongoing EVS recording: {JsonConvert.SerializeObject(existingRecordingSession, Formatting.Indented)}");

				existingRecordingSession.Name = service.OrderName;
				existingRecordingSession.End = service.End;

				helpers.EvsManager.AddOrUpdateRecordingSession(service, existingRecordingSession);
			}
			else
			{
				// Add or update EVS Recording session
				var recordingSession = new RecordingSession
				{
					Id = service.EvsId ?? Guid.NewGuid().ToString(),
					Name = service.OrderName,
					Start = service.Start,
					End = service.End,
					Recorder = GetRecorderName(),
					Targets = new[] { GetTarget() },
					Metadata = BuildMetaData()
				};

				helpers.EvsManager.AddOrUpdateRecordingSession(service, recordingSession);
			}
		}

		private string GetTarget()
		{
			if (service.Definition.Id == ServiceDefinitionGuids.RecordingMessiNews)
			{
				return service.RecordingConfiguration.EvsMessiNewsTarget;
			}
			else if (service.Definition.Id == ServiceDefinitionGuids.RecordingMessiLive)
			{
				return EvsManager.DefaultMessiLiveTarget;
			}
			else
			{
				return String.Empty;
			}
		}

		private string GetRecorderName()
		{
			return service.Functions.Single().ResourceName;
		}

		private bool IsEligibleForEvsAutomation()
		{
			if (service.Definition.VirtualPlatformServiceType != VirtualPlatformType.Recording)
			{
				Log(nameof(IsEligibleForEvsAutomation), $"Service {service.Name} is not a recording");
				return false;
			}

			var functionResource = service.Functions.Single().Resource;
			if (functionResource == null)
			{
				Log(nameof(AddOrUpdateInEvsTask), nameof(IsEligibleForEvsAutomation), $"Service {service.Name} does not have a resource assigned to it");
				return false;
			}

			if (!functionResource.GetResourcePropertyBooleanValue(ResourcePropertyNames.IsAutomated))
			{
				Log(nameof(AddOrUpdateInEvsTask), nameof(IsEligibleForEvsAutomation), $"Resource {functionResource.Name} doesn't have the IsAutomated property set to true");
				return false;
			}

			string recorderName = GetRecorderName();
			if (!helpers.EvsManager.GetRecorderNames().Select(x => x.Name).Contains(recorderName))
			{
				Log(nameof(AddOrUpdateInEvsTask), nameof(IsEligibleForEvsAutomation), $"Recorder {recorderName} is not known in EVS");
				return false;
			}

			string target = GetTarget();
			if (!helpers.EvsManager.GetTargetNames().Contains(target))
			{
				Log(nameof(AddOrUpdateInEvsTask), nameof(IsEligibleForEvsAutomation), $"Target {target} is not known in EVS");
				return false;
			}

			return true;
		}

		private IEnumerable<Metadata> BuildMetaData()
		{
			var labels = helpers.EvsManager.GetMetadataLabels();
			if (service.Definition.Id == ServiceDefinitionGuids.RecordingMessiNews)
			{
				// Only include Order Name for Messi News Recordings
				return BuildMetaData(labels.Where(x => x.ProfileFqn.Equals(ProfileName_YleNameAndId) && x.Key.Equals(ProfileFieldKey_OrderName)));
			}
			else if (service.Definition.Id == ServiceDefinitionGuids.RecordingMessiLive)
			{
				// Include all fields from yle-nameandid profile
				return BuildMetaData(labels.Where(x => x.ProfileFqn.Equals(ProfileName_YleNameAndId)));
			}
			else
			{
				return new Metadata[0];
			}
		}

		private IEnumerable<Metadata> BuildMetaData(IEnumerable<Label> labelsToInclude)
		{
			HashSet<string> profileFqns = new HashSet<string>();
			Dictionary<string, Dictionary<string, string>> metaDataProfiles = new Dictionary<string, Dictionary<string, string>>();

			// Only add metadata for the yle-nameandid profile
			foreach (var label in labelsToInclude)
			{
				string value;
				switch (label.Key)
				{
					case "plasma-id":
						value = service.RecordingConfiguration.PlasmaIdForArchive;
						break;
					case "order-name":
						value = service.OrderName;
						break;
					case "yle-id":
						value = String.Empty; // currently unsupported
						break;
					case "additional-information":
						value = service.Comments;
						break;
					case "finnish-main-title":
						value = service.RecordingConfiguration.RecordingName;
						break;
					case "dataminer-order-url":
						value = $@"https://slc-h42-g06.skyline.local/yle/order/{service.Id}";
						break;
					case "fast-rerun-copy":
						value = service.RecordingConfiguration.FastRerunCopy.ToString();
						break;
					case "areena-copy":
						value = service.RecordingConfiguration.FastAreenaCopy.ToString();
						break;
					case "subtitle-proxy":
						value = service.RecordingConfiguration.SubtitleProxy.ToString();
						break;
					case "audio-tracks-1-2":
						// unsupported atm
						continue;
					case "content-1-2":
						// unsupported atm
						continue;
					default:
						continue;
				}

				profileFqns.Add(label.ProfileFqn);

				if (metaDataProfiles.TryGetValue(label.ProfileFqn, out Dictionary<string, string> metaDataValues))
				{
					if (metaDataValues.ContainsKey(label.Key))
					{
						metaDataValues[label.Key] = value;
					}
					else
					{
						metaDataValues.Add(label.Key, value);
					}
				}
				else
				{
					metaDataProfiles.Add(label.ProfileFqn, new Dictionary<string, string>
					{
						{ label.Key, value }
					});
				}
			}

			var result = profileFqns.Select(x => new Metadata { Profile = x, Values = metaDataProfiles[x] }).ToList();

			helpers.Log(nameof(AddOrUpdateInEvsTask), nameof(BuildMetaData), $"Metadata: {JsonConvert.SerializeObject(result, Formatting.Indented)}");

			return result;
		}
	}
}
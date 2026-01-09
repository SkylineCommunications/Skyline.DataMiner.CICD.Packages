namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTaskCreators;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.Enums;
	using Skyline.DataMiner.Utils.YLE.Integrations.Plasma;
	using Service = Service.Service;

	public class RecordingUserTaskCreator : LiveUserTaskCreator
	{
		public RecordingUserTaskCreator(Helpers helpers, Service service, Guid ticketFieldResolverId, Order order)
			: base(helpers, service, order)
		{
			userTaskConstructors = new Dictionary<string, UserTask>
			{
				{ Descriptions.Recording.TvRecording, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.Recording.TvRecording, UserGroup.MediaOperator) },
				{ Descriptions.Recording.TvRecordingFileProcessing, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.Recording.TvRecordingFileProcessing, UserGroup.MediaOperator) },
				{ Descriptions.Recording.AreenaCopyRecording, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.Recording.AreenaCopyRecording, UserGroup.MediaOperator) },
				{ Descriptions.Recording.AreenaCopyRecordingFileProcessing, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.Recording.AreenaCopyRecordingFileProcessing, UserGroup.MediaOperator) },
				{ Descriptions.Recording.SubtitleProxyRecording, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.Recording.SubtitleProxyRecording, UserGroup.MediaOperator) },
				{ Descriptions.Recording.SubtitleProxyRecordingFileProcessing, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.Recording.SubtitleProxyRecordingFileProcessing, UserGroup.MediaOperator) },
				{ Descriptions.Recording.FastRerunRecording, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.Recording.FastRerunRecording, UserGroup.MediaOperator) },
				{ Descriptions.Recording.FastRerunRecordingFileProcessing, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.Recording.FastRerunRecordingFileProcessing, UserGroup.MediaOperator) },
				{ Descriptions.Recording.TvBackupRecording, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.Recording.TvBackupRecording, UserGroup.MediaOperator) },
				{ Descriptions.Recording.TvBackupRecordingFileProcessing, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.Recording.TvBackupRecordingFileProcessing, UserGroup.MediaOperator) },
                { Descriptions.Recording.NewsCleanRecording, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.Recording.NewsCleanRecording, UserGroup.MediaOperator) },
                { Descriptions.Recording.NewsPgmRecording, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.Recording.NewsPgmRecording, UserGroup.MediaOperator) },
				{ Descriptions.Recording.LiveSignalRecording, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.Recording.LiveSignalRecording, UserGroup.MediaOperator) },
				{ Descriptions.Recording.LiveSignalRecordingFileProcessing, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.Recording.LiveSignalRecordingFileProcessing, UserGroup.MediaOperator) }
			};

			userTaskConditions = new Dictionary<string, bool>
			{
				{ Descriptions.Recording.TvRecording, RequiresTvRecordingUserTask() },
				{ Descriptions.Recording.TvRecordingFileProcessing, RequiresTvRecordingFileProcessingUserTask() },
				{ Descriptions.Recording.AreenaCopyRecording, RequiresAreenaRecordingCopyUserTask() },
				{ Descriptions.Recording.AreenaCopyRecordingFileProcessing, RequiresAreenaCopyRecordingFileProcessingUserTask() },
				{ Descriptions.Recording.SubtitleProxyRecording, RequiresSubtitleProxyRecordingUserTask() },
				{ Descriptions.Recording.SubtitleProxyRecordingFileProcessing, RequiresSubtitleProxyRecordingFileProcessingUserTask() },
				{ Descriptions.Recording.FastRerunRecording, RequiresFastRerunRecordingUserTask() },
				{ Descriptions.Recording.FastRerunRecordingFileProcessing, RequiresFastRerunRecordingFileProcessingUserTask() },
				{ Descriptions.Recording.TvBackupRecording, RequiresTvBackupRecordingUserTask() },
				{ Descriptions.Recording.TvBackupRecordingFileProcessing, RequiresTvBackupRecordingFileProcessingUserTask() },
				{ Descriptions.Recording.NewsCleanRecording, RequiresNewsRecordingUserTask(NewsRecordingType.Clean) },
				{ Descriptions.Recording.NewsPgmRecording, RequiresNewsRecordingUserTask(NewsRecordingType.PGM) },
				{ Descriptions.Recording.LiveSignalRecording, RequiresLiveSignalRecordingUserTask() },
				{ Descriptions.Recording.LiveSignalRecordingFileProcessing, RequiresLiveSignalRecordingFileProcessingUserTask() }
			};

			Log("Constructor",$"User task conditions: {string.Join(";", userTaskConditions.Select(u => $"{u.Key}={(u.Value ? "Required" : "Not Required")}"))}",service.Name);

			// add the necessary sub recording user tasks
			if (service.RecordingConfiguration?.SubRecordings == null || !service.RecordingConfiguration.SubRecordings.Any()) return;

			for (int i = 0; i < service.RecordingConfiguration.SubRecordings.Count; i++)
			{
				var subRecordingDescription = $"{Descriptions.Recording.LiveSignalRecording} - Sub-Recording {i + 1}";
				userTaskConstructors.Add(subRecordingDescription, new LiveUserTask(helpers, ticketFieldResolverId, service, subRecordingDescription, UserGroup.MediaOperator));
				userTaskConditions.Add(subRecordingDescription, RequiresLiveSignalSubRecordingUserTask(i + 1));

				var subRecordingFileProcessingDescription = $"{subRecordingDescription} File Processing";
				userTaskConstructors.Add(subRecordingFileProcessingDescription, new LiveUserTask(helpers, ticketFieldResolverId, service, subRecordingFileProcessingDescription, UserGroup.MediaOperator));
				userTaskConditions.Add(subRecordingFileProcessingDescription, RequiresLiveSignalSubRecordingFileProcessingUserTask(i + 1));
			}
		}

        public bool FileProcessingUserTaskRequired()
        {
            bool requiresLiveSignalSubRecordingFileProcessingUserTask = false;
            for (int i = 0; i < service.RecordingConfiguration.SubRecordings.Count; i++)
            {
                if (RequiresLiveSignalSubRecordingFileProcessingUserTask(i + 1))
                {
                    requiresLiveSignalSubRecordingFileProcessingUserTask = true;
                    break;
                }
            }

            bool tvFileProcessingRequired = RequiresTvBackupRecordingFileProcessingUserTask() || RequiresTvRecordingFileProcessingUserTask()  || RequiresAreenaCopyRecordingFileProcessingUserTask();

			bool liveFileProcessingRequired = RequiresFastRerunRecordingFileProcessingUserTask() || RequiresLiveSignalRecordingFileProcessingUserTask() || requiresLiveSignalSubRecordingFileProcessingUserTask || RequiresSubtitleProxyRecordingFileProcessingUserTask();

			return tvFileProcessingRequired || liveFileProcessingRequired;
		}

        private bool RequiresTvRecordingUserTask()
		{
			bool isPlasmaRecording = service.IntegrationType == IntegrationType.Plasma;

			string serviceDefinitionDescription = service.Definition?.Description;
			bool isMessiLiveRecording = !String.IsNullOrEmpty(serviceDefinitionDescription) && serviceDefinitionDescription.Equals("Messi Live", StringComparison.InvariantCultureIgnoreCase);

			return isPlasmaRecording && isMessiLiveRecording;
		}

		private bool RequiresTvRecordingFileProcessingUserTask()
		{
			// only needed in case there was a TV Recording user task before
			var isTvRecording = RequiresTvRecordingUserTask();
			if (!isTvRecording) return false;

			if (service.UserTasks == null) return false;

			var tvRecordingUserTask = service.UserTasks.FirstOrDefault(u => u.Description == Descriptions.Recording.TvRecording);
			if (tvRecordingUserTask == null) return false;

			// the File Processing user task is only needed once the service is no longer Running
			var serviceStatus = service.Status;
			bool serviceContainsOneOfTheEndingStates = serviceStatus == YLE.Service.Status.PostRoll || serviceStatus == YLE.Service.Status.ServiceCompleted || serviceStatus == YLE.Service.Status.ServiceCompletedWithErrors || serviceStatus == YLE.Service.Status.FileProcessing;
			bool consideredAsEnded = service.End <= DateTime.Now;
			bool serviceIsNoLongerRunning = serviceContainsOneOfTheEndingStates || consideredAsEnded;

			Log(nameof(RequiresTvRecordingFileProcessingUserTask), $"Service is {(serviceIsNoLongerRunning ? "no longer" : "still")} running, user task {(serviceIsNoLongerRunning ? string.Empty : "not ")}required", service.Name);

			return serviceIsNoLongerRunning;
		}

		private bool RequiresAreenaRecordingCopyUserTask()
		{
			var isTvRecording = RequiresTvRecordingUserTask();

			Log(nameof(RequiresAreenaRecordingCopyUserTask), $"Service {(service.RecordingConfiguration?.FastAreenaCopy ?? false ? "requires" : "doesn't require")} a fast areena copy", service.Name);

			// only required in case Fast Areena Copy is enabled
			return isTvRecording && service.RecordingConfiguration != null && service.RecordingConfiguration.FastAreenaCopy;
		}

		private bool RequiresAreenaCopyRecordingFileProcessingUserTask()
		{
			// only needed in case there was an Areena Copy Recording user task before
			var isAreenaCopyRecording = RequiresAreenaRecordingCopyUserTask();
			if (!isAreenaCopyRecording) return false;

			if (service.UserTasks == null) return false;

			var areenaCopyRecordingUserTask = service.UserTasks.FirstOrDefault(u => u.Description == Descriptions.Recording.AreenaCopyRecording);
			if (areenaCopyRecordingUserTask == null) return false;

			// the File Processing user task is only needed once the service is no longer Running
			var serviceStatus = service.Status;
			bool serviceContainsOneOfTheEndingStates = serviceStatus == YLE.Service.Status.PostRoll || serviceStatus == YLE.Service.Status.ServiceCompleted || serviceStatus == YLE.Service.Status.ServiceCompletedWithErrors || serviceStatus == YLE.Service.Status.FileProcessing;
			bool consideredAsEnded = service.End <= DateTime.Now;
			bool serviceIsNoLongerRunning = serviceContainsOneOfTheEndingStates || consideredAsEnded;

			Log(nameof(RequiresAreenaCopyRecordingFileProcessingUserTask), $"Service is {(serviceIsNoLongerRunning ? "no longer" : "still")} running, user task {(serviceIsNoLongerRunning ? string.Empty : "not ")}required", service.Name);

			return serviceIsNoLongerRunning;
		}

		private bool RequiresSubtitleProxyRecordingUserTask()
		{
			var isTvRecording = RequiresTvRecordingUserTask();
			var isLiveSignalRecordingMessiLive = RequiresLiveSignalRecordingUserTask() && service.Definition?.Description == "Messi Live";

			// only needed in case there is a TV Recording user task
			// or a Live Signal Recording user task for Messi Live
			if (isTvRecording || isLiveSignalRecordingMessiLive)
			{
				// only required in case Subtitle Proxy is enabled
				return service.RecordingConfiguration != null && service.RecordingConfiguration.SubtitleProxy;
			}
			else
			{
				return false;
			}
		}

		private bool RequiresSubtitleProxyRecordingFileProcessingUserTask()
		{
			// only needed in case there was a Subtitle Proxy Recording user task before
			var isSubtitleProxyRecording = RequiresSubtitleProxyRecordingUserTask();
			if (!isSubtitleProxyRecording) return false;

			if (service.UserTasks == null) return false;

			var subtitleProxyRecordingUserTask = service.UserTasks.FirstOrDefault(u => u.Description == Descriptions.Recording.SubtitleProxyRecording);
			if (subtitleProxyRecordingUserTask == null) return false;

			// the File Processing user task is only needed once the service is no longer Running
			var serviceStatus = service.Status;

			bool serviceContainsOneOfTheEndingStates = serviceStatus == YLE.Service.Status.PostRoll || serviceStatus == YLE.Service.Status.ServiceCompleted || serviceStatus == YLE.Service.Status.ServiceCompletedWithErrors || serviceStatus == YLE.Service.Status.FileProcessing;
			bool consideredAsEnded = service.End <= DateTime.Now;
			bool serviceIsNoLongerRunning = serviceContainsOneOfTheEndingStates || consideredAsEnded;

			Log(nameof(RequiresSubtitleProxyRecordingFileProcessingUserTask), $"Service is {(serviceIsNoLongerRunning ? "no longer" : "still")} running, user task {(serviceIsNoLongerRunning ? string.Empty : "not ")}required", service.Name);

			return serviceIsNoLongerRunning;
		}

		private bool RequiresFastRerunRecordingUserTask()
		{
			var isTvRecording = RequiresTvRecordingUserTask();
			var isLiveSignalRecordingMessiLive = RequiresLiveSignalRecordingUserTask() && service.Definition?.Description == "Messi Live";

			// only needed in case there is a TV Recording user task
			// or a Live Signal Recording user task for Messi Live
			if (isTvRecording || isLiveSignalRecordingMessiLive)
			{
				// only required in case Subtitle Proxy is enabled
				return service.RecordingConfiguration != null && service.RecordingConfiguration.FastRerunCopy;
			}
			else
			{
				return false;
			}
		}

		private bool RequiresFastRerunRecordingFileProcessingUserTask()
		{
			// only needed in case there was a Fast Rerun Recording user task before
			var isFastRerunRecording = RequiresFastRerunRecordingUserTask();
			if (!isFastRerunRecording) return false;

			if (service.UserTasks == null) return false;

			var fastRerunRecordingUserTask = service.UserTasks.FirstOrDefault(u => u.Description == Descriptions.Recording.FastRerunRecording);
			if (fastRerunRecordingUserTask == null) return false;

			// the File Processing user task is only needed once the service is no longer Running
			var serviceStatus = service.Status;

			bool serviceContainsOneOfTheEndingStates = serviceStatus == YLE.Service.Status.PostRoll || serviceStatus == YLE.Service.Status.ServiceCompleted || serviceStatus == YLE.Service.Status.ServiceCompletedWithErrors || serviceStatus == YLE.Service.Status.FileProcessing;
			bool consideredAsEnded = service.End <= DateTime.Now;
			bool serviceIsNoLongerRunning = serviceContainsOneOfTheEndingStates || consideredAsEnded;

			Log(nameof(RequiresFastRerunRecordingFileProcessingUserTask), $"Service is {(serviceIsNoLongerRunning ? "no longer" : "still")} running, user task {(serviceIsNoLongerRunning ? string.Empty : "not ")}required", service.Name);

			return serviceIsNoLongerRunning;
		}

		private bool RequiresTvBackupRecordingUserTask()
		{
			var isPlasmaRecording = service.IntegrationType == IntegrationType.Plasma;
			var isMessiLiveBackupRecording = service.Definition?.Description == "Messi Live Backup";

			// only required in case of a Plasma Messi Live Backup Recording
			return isPlasmaRecording && isMessiLiveBackupRecording;
		}

		private bool RequiresTvBackupRecordingFileProcessingUserTask()
		{
			// only needed in case there was a TV Backup Recording user task before
			var isTvBackupRecording = RequiresTvBackupRecordingUserTask();
			if (!isTvBackupRecording) return false;

			if (service.UserTasks == null) return false;

			var tvBackupRecordingUserTask = service.UserTasks.FirstOrDefault(u => u.Description == Descriptions.Recording.TvBackupRecording);
			if (tvBackupRecordingUserTask == null) return false;

			// the TV Backup Recording user task should first be completed
			if (tvBackupRecordingUserTask.Status == UserTaskStatus.Incomplete) return false;

			// the File Processing user task is only needed once the service is no longer Running
			var serviceStatus = service.Status;
			bool serviceContainsOneOfTheEndingStates = serviceStatus == YLE.Service.Status.PostRoll || serviceStatus == YLE.Service.Status.ServiceCompleted || serviceStatus == YLE.Service.Status.ServiceCompletedWithErrors || serviceStatus == YLE.Service.Status.FileProcessing;
			bool consideredAsEnded = service.End <= DateTime.Now;
			bool serviceIsNoLongerRunning = serviceContainsOneOfTheEndingStates || consideredAsEnded;

			Log(nameof(RequiresTvBackupRecordingFileProcessingUserTask), $"Service is {(serviceIsNoLongerRunning ? "no longer" : "still")} running, user task {(serviceIsNoLongerRunning ? string.Empty : "not ")}required", service.Name);

			return serviceIsNoLongerRunning;
		}

        private bool RequiresNewsRecordingUserTask(NewsRecordingType newsRecordingType)
        {
            var isPlasmaRecording = service.IntegrationType == IntegrationType.Plasma;
            if (!isPlasmaRecording) return false;

            var isMessiNewsRecording = service.Definition?.Description == "Messi News";
            if (!isMessiNewsRecording) return false;

            var newsRecordingFunction = service.Functions.FirstOrDefault();
            if (newsRecordingFunction == null) return false;

            var feedTypeProfileParameter = newsRecordingFunction.Parameters.FirstOrDefault(p => p.Id == ProfileParameterGuids._FeedType);
            if (feedTypeProfileParameter == null) return false;

			bool feedTypeIsNewsRecording = feedTypeProfileParameter.StringValue == newsRecordingType.ToString();

			return feedTypeIsNewsRecording;
        }

		private bool RequiresLiveSignalRecordingUserTask()
		{
			// only required in case of a manual Recording
			return service.IntegrationType == IntegrationType.None;
		}

		private bool RequiresLiveSignalRecordingFileProcessingUserTask()
		{
			// only needed in case there was a Live Signal Recording user task before
			var isLiveSignalRecording = RequiresLiveSignalRecordingUserTask();
			if (!isLiveSignalRecording)
			{
				Log(nameof(RequiresLiveSignalRecordingFileProcessingUserTask), $"Originally no live recording user task required", service.Name);
				return false;
			}

			// Not required for Messi News Recordings
			var isMessiNewsRecording = service.Definition?.Description == "Messi News";
			if (isMessiNewsRecording) return false;

			if (service.UserTasks == null) return false;

			var liveSignalRecordingUserTask = service.UserTasks.FirstOrDefault(u => u.Description == Descriptions.Recording.LiveSignalRecording);
			if (liveSignalRecordingUserTask == null)
			{
				Log(nameof(RequiresLiveSignalRecordingFileProcessingUserTask), $"No existing live recording user task found with description {Descriptions.Recording.LiveSignalRecording}", service.Name);
				return false;
			}

			// the File Processing user task is only needed once the service is no longer Running
			var serviceStatus = service.Status;

			bool serviceContainsOneOfTheEndingStates = serviceStatus == YLE.Service.Status.PostRoll || serviceStatus == YLE.Service.Status.ServiceCompleted || serviceStatus == YLE.Service.Status.ServiceCompletedWithErrors || serviceStatus == YLE.Service.Status.FileProcessing;
			bool consideredAsEnded = service.End <= DateTime.Now;
			bool serviceIsNoLongerRunning = serviceContainsOneOfTheEndingStates || consideredAsEnded;

			Log(nameof(RequiresLiveSignalRecordingFileProcessingUserTask), $"Service is {(serviceIsNoLongerRunning ? "no longer" : "still")} running, user task {(serviceIsNoLongerRunning ? string.Empty : "not ")}required", service.Name);

			return serviceIsNoLongerRunning;
		}

		private bool RequiresLiveSignalSubRecordingUserTask(int index)
		{
			return RequiresLiveSignalRecordingUserTask();
		}

		private bool RequiresLiveSignalSubRecordingFileProcessingUserTask(int index)
		{
			// only needed in case there was a Live Signal Recording user task before
			var isLiveSignalRecording = RequiresLiveSignalRecordingUserTask();
			if (!isLiveSignalRecording) return false;

			if (service.UserTasks == null) return false;

			var subRecordingDescription = $"{Descriptions.Recording.LiveSignalRecording} - Sub-Recording {index}";
			var liveSignalSubRecordingUserTask = service.UserTasks.FirstOrDefault(u => u.Description == subRecordingDescription);
			if (liveSignalSubRecordingUserTask == null) return false;

			// the Live Signal Sub-Recording user task should first be completed
			if (liveSignalSubRecordingUserTask.Status == UserTaskStatus.Incomplete) return false;

			// the File Processing user task is only needed once the service is no longer Running
			var serviceStatus = service.Status;
			return serviceStatus == YLE.Service.Status.PostRoll || serviceStatus == YLE.Service.Status.ServiceCompleted || serviceStatus == YLE.Service.Status.ServiceCompletedWithErrors || serviceStatus == YLE.Service.Status.FileProcessing;
		}
	}
}
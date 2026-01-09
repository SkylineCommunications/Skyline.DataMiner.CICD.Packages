namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Statuses
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using System.Linq;

	public static class StatusManager
	{
		public static bool DoesOrderContainsFileProcessingServices(Helpers helpers, Service recordingService, Order order, bool updateOrderStatusAfterManualInteraction = false)
		{
			bool hasIncompleteFileProcessingUserTasks = recordingService.UserTasks != null && recordingService.UserTasks.Exists(t => t.Status == UserTaskStatus.Incomplete && t.Description.IndexOf("File Processing", StringComparison.OrdinalIgnoreCase) != -1);

			bool hasFileProcessingUserTasks = recordingService.UserTasks != null && recordingService.UserTasks.Exists(t => t.Description.IndexOf("File Processing", StringComparison.OrdinalIgnoreCase) != -1);

			var now = DateTime.Now;
			var recordingUserTaskCreator = helpers.UserTaskManager.GetUserTaskCreator(recordingService, order) as RecordingUserTaskCreator;

			bool fileProcessingServiceEndsBeforeOrder = recordingService.End <= now && recordingService.End < order.End;
			bool fileProcessingServiceEndsWithOrder = recordingService.End <= now && recordingService.End.Matches(order.End);
			bool shouldHaveIncompleteFileProcessingUserTasks = (fileProcessingServiceEndsBeforeOrder || fileProcessingServiceEndsWithOrder) && recordingUserTaskCreator.FileProcessingUserTaskRequired();

			if (AreOrderFileProcessingExclusionRulesApplied(helpers, recordingService, hasFileProcessingUserTasks, updateOrderStatusAfterManualInteraction)) return false;

			helpers.Log(nameof(StatusManager), nameof(DoesOrderContainsFileProcessingServices), $"{nameof(hasIncompleteFileProcessingUserTasks)}={hasIncompleteFileProcessingUserTasks}, {nameof(hasFileProcessingUserTasks)}={hasFileProcessingUserTasks}, {nameof(shouldHaveIncompleteFileProcessingUserTasks)}={shouldHaveIncompleteFileProcessingUserTasks}");

			return (hasIncompleteFileProcessingUserTasks || !hasFileProcessingUserTasks) && shouldHaveIncompleteFileProcessingUserTasks;
		}

		public static bool ServiceHasIncompleteFileProcessingUserTasks(Helpers helpers, Service recordingService, bool hasInCompleteDefaultUserTasks, Order order = null)
		{
			if (helpers == null) return false;

			bool hasFileProcessingUserTasks = recordingService.UserTasks != null && recordingService.UserTasks.Exists(userTask => userTask?.Description != null && userTask.Description.IndexOf("File Processing", StringComparison.OrdinalIgnoreCase) != -1);

			bool hasInCompletedFileProcessingUserTask = recordingService.UserTasks != null && recordingService.UserTasks.Exists(userTask => userTask?.Description != null && userTask.Status == UserTaskStatus.Incomplete && userTask.Description.IndexOf("File Processing", StringComparison.OrdinalIgnoreCase) != -1);

			var now = DateTime.Now;
			if (AreRecordingFileProcessingExclusionRulesApplied(helpers, recordingService, hasFileProcessingUserTasks, hasInCompleteDefaultUserTasks)) return false;

			order = order ?? helpers.OrderManager.GetOrder(recordingService.OrderReferences.FirstOrDefault(), false, true);

			var recordingUserTaskCreator = helpers.UserTaskManager.GetUserTaskCreator(recordingService, order) as RecordingUserTaskCreator;
			bool shouldHaveFileProcessingUserTasks = recordingService.End <= now && recordingUserTaskCreator.FileProcessingUserTaskRequired();

			helpers.Log(nameof(StatusManager), nameof(ServiceHasIncompleteFileProcessingUserTasks), $"{nameof(hasInCompletedFileProcessingUserTask)}={hasInCompletedFileProcessingUserTask}, {nameof(hasFileProcessingUserTasks)}={hasFileProcessingUserTasks}, {nameof(shouldHaveFileProcessingUserTasks)}={shouldHaveFileProcessingUserTasks}");

			return (hasInCompletedFileProcessingUserTask || !hasFileProcessingUserTasks) && shouldHaveFileProcessingUserTasks;
		}

		private static bool AreRecordingFileProcessingExclusionRulesApplied(Helpers helpers, Service recordingService, bool hasFileProcessingUserTasks, bool hasInCompleteDefaultUserTasks)
		{
			if (recordingService == null) throw new ArgumentNullException(nameof(recordingService));

			var now = DateTime.Now;

			bool recordingShouldHavePostRollOrCompletedWithErrorsStatus = recordingService.End <= now && !hasFileProcessingUserTasks && hasInCompleteDefaultUserTasks;

			bool recordingCompletedWithoutFileProcessingUserTasks = (recordingService.SavedStatus == YLE.Service.Status.ServiceCompletedWithErrors && recordingService.EndWithPostRoll > now) || recordingShouldHavePostRollOrCompletedWithErrorsStatus; // Check to avoid that end of order event will read this recording service as File Processing undesired.

			bool recordingCompletedDuringPostRollWithoutFileProcessingUserTasks = !hasFileProcessingUserTasks && !hasInCompleteDefaultUserTasks && recordingService.SavedStatus == YLE.Service.Status.PostRoll; // Completing the user tasks manually during post roll may not update the recording service to File Processing state

			helpers?.Log(nameof(StatusManager), nameof(AreRecordingFileProcessingExclusionRulesApplied), $"Exclusion rules on service file processing status: {nameof(recordingCompletedWithoutFileProcessingUserTasks)} = {recordingCompletedWithoutFileProcessingUserTasks}, {nameof(recordingCompletedDuringPostRollWithoutFileProcessingUserTasks)} = {recordingCompletedDuringPostRollWithoutFileProcessingUserTasks}");

			return recordingCompletedWithoutFileProcessingUserTasks || recordingCompletedDuringPostRollWithoutFileProcessingUserTasks;
		}

		private static bool AreOrderFileProcessingExclusionRulesApplied(Helpers helpers, Service recordingService, bool hasFileProcessingUserTasks, bool updateOrderStatusAfterManualInteraction)
		{
			if (recordingService == null) throw new ArgumentNullException(nameof(recordingService));

			var now = DateTime.Now;
			var recordingStatus = recordingService.Status;

			bool isRecordingAroundLiveEnd = recordingStatus == YLE.Service.Status.ServiceCompletedWithErrors || recordingStatus == YLE.Service.Status.ServiceCompleted || recordingStatus == YLE.Service.Status.PostRoll;
			bool alreadyCompletedByManualInteraction = updateOrderStatusAfterManualInteraction && !hasFileProcessingUserTasks && isRecordingAroundLiveEnd;

			bool recordingUserTasksCompletedDuringPostRoll = !hasFileProcessingUserTasks && recordingStatus == YLE.Service.Status.ServiceCompleted && recordingService.EndWithPostRoll <= now && recordingService.PostRoll != TimeSpan.Zero;

			bool wasRecordingAlreadyCompleted = recordingService.SavedStatus == YLE.Service.Status.ServiceCompletedWithErrors || alreadyCompletedByManualInteraction || recordingUserTasksCompletedDuringPostRoll;

			helpers?.Log(nameof(StatusManager), nameof(AreOrderFileProcessingExclusionRulesApplied), $"Exclusion rules on order file processing status: {nameof(alreadyCompletedByManualInteraction)} = {alreadyCompletedByManualInteraction}, {nameof(recordingUserTasksCompletedDuringPostRoll)} = {recordingUserTasksCompletedDuringPostRoll}, {nameof(wasRecordingAlreadyCompleted)} = {wasRecordingAlreadyCompleted}");

			return wasRecordingAlreadyCompleted;
		}
	}
}

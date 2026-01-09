namespace UpdateTicketStatus_4
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Messages;
	using UserGroup = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserGroup;
	using VirtualPlatform = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform;

	public static class UserTaskHandler
	{
		public static bool TryUpdateAllOrderUserTasks(Helpers helpers, Guid retrievedOrderId, List<UserGroup> userGroups)
		{
			var order = helpers.OrderManager.GetOrder(retrievedOrderId);
			if (order == null) return false;

			helpers.AddOrderReferencesForLogging(order.Id);

			var incompleteUserTasksForSpecificUserGroups = order.AllServices.Where(s=> s.Definition.VirtualPlatform != VirtualPlatform.ReceptionSatellite).SelectMany(s => s.UserTasks).Where(userTask => userTask.Status == UserTaskStatus.Incomplete && userGroups.Contains(userTask.UserGroup)).ToList();

			helpers.Log(nameof(UserTaskHandler), nameof(TryUpdateAllOrderUserTasks), $"All incomplete user tasks in order for user groups {string.Join(", ", userGroups)}: \n'{string.Join("\n", incompleteUserTasksForSpecificUserGroups.Select(s => $"{s.Name} for serivce {s.Service.Name}"))}'");

			foreach (var incompleteUserTask in incompleteUserTasksForSpecificUserGroups)
			{
				incompleteUserTask.SetToCompleteOrInComplete(order);
			}

			var incompleteUserTasksInOrder = order.AllServices.SelectMany(s => s.UserTasks).Where(ut => ut.Status == UserTaskStatus.Incomplete).ToList();
			if (!incompleteUserTasksInOrder.Any())
			{
				TryStartOrder(helpers, order);
			}

			AsynchronouslyUpdateOrder(helpers, order.AllServices[0].Id);

			return true;
		}

		public static bool TryUpdateAllServiceUserTasks(Helpers helpers, Guid retrievedServiceId, List<UserGroup> userGroups)
		{
			if (!helpers.ServiceManager.TryGetService(retrievedServiceId, out var service)) return false;

			bool serviceIsNewsRecording = service.Definition.VirtualPlatform == VirtualPlatform.Recording && service.Definition.Description.Contains("News");
			Order order = null;
			if (serviceIsNewsRecording)
			{
				ChangeUserTaskStatusOfNewsRecordingRoutingParent(helpers, service, userGroups, out order);

				// Re-assign the service variable using the service object that is part of the order object
				service = order.AllServices.SingleOrDefault(s => s.Name == service.Name) ?? throw new NotFoundException($"Unable to find service with name {service.Name} between services {string.Join(", ", order.AllServices.Select(s => s.Name))}");
			}

			helpers.AddOrderReferencesForLogging(order?.Id ?? service.OrderReferences.FirstOrDefault());

			var incompleteUserTasksForSpecificUserGroups = service.UserTasks.Where(userTask => userTask.Status == UserTaskStatus.Incomplete && userGroups.Contains(userTask.UserGroup)).ToList();

			helpers.Log(nameof(UserTaskHandler), nameof(TryUpdateAllServiceUserTasks), $"Service {service.Name} incomplete user tasks for user groups '{string.Join(";", userGroups.Select(userGroup => userGroup.GetDescription()))}': '{string.Join(", ", incompleteUserTasksForSpecificUserGroups.Select(u => u.Name))}'");

			var completeUserTasksForSpecificUserGroups = service.UserTasks.Where(userTask => userTask.Status == UserTaskStatus.Complete && userGroups.Contains(userTask.UserGroup)).ToList();

			helpers.Log(nameof(UserTaskHandler), nameof(TryUpdateAllServiceUserTasks), $"Service {service.Name} complete user tasks for user groups '{string.Join(";", userGroups.Select(userGroup => userGroup.GetDescription()))}': '{string.Join(", ", incompleteUserTasksForSpecificUserGroups.Select(u => u.Name))}'");	

			foreach (var userTask in incompleteUserTasksForSpecificUserGroups)
			{
				userTask.SetToCompleteOrInComplete(order);
			}

			foreach (var userTask in completeUserTasksForSpecificUserGroups)
			{
				userTask.SetToCompleteOrInComplete(order);
			}

			var incompleteUserTasksInService = service.UserTasks.Where(ut => ut.Status == UserTaskStatus.Incomplete).ToList();
			if (!incompleteUserTasksInService.Any())
			{
				TryStartLinkedOrders(helpers, service.OrderReferences);
			}

			AsynchronouslyUpdateOrder(helpers, service.Id);

			return true;
		}

		public static bool TryUpdateUserTask(Helpers helpers, string fullTicketId, Guid userTaskUniqueId)
		{
			UserTask userTask = null;
			if (userTaskUniqueId != Guid.Empty && helpers.UserTaskManager.TryGetUserTaskBasedOnUniqueId(userTaskUniqueId, out var userTaskFromUniqueId))
			{
				userTask = userTaskFromUniqueId;
			}
			else if (fullTicketId.Contains("/") && helpers.UserTaskManager.TryGetUserTask(fullTicketId, out var userTaskFromFullTicketId))
			{
				userTask = userTaskFromFullTicketId;
			}
			else
			{
				return false;
			}

			if (userTask is LiveUserTask liveUserTask)
			{
				Order order = null;

				var orderId = liveUserTask.Service?.OrderReferences?.FirstOrDefault() ?? Guid.Empty;

				helpers.AddOrderReferencesForLogging(orderId);

				var userGroups = Enum.GetValues(typeof(UserGroup)).Cast<UserGroup>().ToList();

				bool serviceIsNewsRecording = liveUserTask.Service.Definition.VirtualPlatform == VirtualPlatform.Recording && liveUserTask.Service.Definition.Description.Contains("News");
				if (serviceIsNewsRecording)
				{
					ChangeUserTaskStatusOfNewsRecordingRoutingParent(helpers, liveUserTask.Service, userGroups, out order);

					// Re-assign the liveUserTask variable using the user task object part of the service object part of the order object

					var service = order.AllServices.SingleOrDefault(s => s.Name == liveUserTask.Service.Name) ?? throw new NotFoundException($"Unable to find service with name {liveUserTask.Service.Name} between services {string.Join(", ", order.AllServices.Select(s => s.Name))}");

					liveUserTask = service.UserTasks.SingleOrDefault(ut => ut.ID == liveUserTask.ID) ?? throw new NotFoundException($"Unable to find user task with ID '{liveUserTask.ID}' between user tasks {string.Join(", ", service.UserTasks.Select(ut => ut.ID))}");
				}

				liveUserTask.SetToCompleteOrInComplete(order);

				TryStartLinkedOrders(helpers, liveUserTask.Service.OrderReferences);

				AsynchronouslyUpdateOrder(helpers, liveUserTask.Service.Id);
			}
			else if (userTask is NonLiveUserTask nonLiveUserTask)
			{
				nonLiveUserTask.SetToCompleteOrInComplete(helpers);
			}

			return true;
		}

		private static void TryStartLinkedOrders(Helpers helpers, IEnumerable<Guid> orderIds)
		{
			helpers.LogMethodStart(nameof(UserTaskHandler), nameof(TryStartLinkedOrders), out var stopWatch);

			foreach (var orderId in orderIds)
			{
				var liteOrder = helpers.OrderManager.GetLiteOrder(orderId);
				if (liteOrder.StartNow)
				{
					var order = helpers.OrderManager.GetOrder(orderId);

					helpers.OrderManager.TryStartOrderNow(helpers, order);
				}
			}

			helpers.LogMethodCompleted(nameof(UserTaskHandler), nameof(TryStartLinkedOrders), null, stopWatch);
		}

		private static void TryStartOrder(Helpers helpers, Order order)
		{
			helpers.LogMethodStart(nameof(UserTaskHandler), nameof(TryStartOrder), out var stopWatch);

			if (order.StartNow)
			{
				helpers.OrderManager.TryStartOrderNow(helpers, order);
			}

			helpers.LogMethodCompleted(nameof(UserTaskHandler), nameof(TryStartOrder), null, stopWatch);
		}


		private static void AsynchronouslyUpdateOrder(Helpers helpers, Guid serviceId)
		{
			helpers.LogMethodStart(nameof(UserTaskHandler), nameof(AsynchronouslyUpdateOrder), out var stopwatch);

			helpers.Log(nameof(UserTaskHandler), nameof(AsynchronouslyUpdateOrder), $"Launching script UpdateOrdersAfterUserTaskStatusChange");

			helpers.Engine.SendSLNetSingleResponseMessage(new ExecuteScriptMessage("UpdateOrdersAfterUserTaskStatusChange")
			{
				Options = new SA(new[]
				{
					$"PARAMETER:1:{serviceId}",
					"OPTIONS:0",
					"CHECKSETS:FALSE",
					"EXTENDED_ERROR_INFO",
					"DEFER:TRUE" // async execution
		        })
			});

			helpers.LogMethodCompleted(nameof(UserTaskHandler), nameof(AsynchronouslyUpdateOrder), null, stopwatch);
		}

		private static void ChangeUserTaskStatusOfNewsRecordingRoutingParent(Helpers helpers, Service newsRecording, List<UserGroup> userGroups, out Order order)
		{
			helpers.LogMethodStart(nameof(UserTask), nameof(ChangeUserTaskStatusOfNewsRecordingRoutingParent), out var stopwatch);

			helpers.Log(nameof(UserTask), nameof(ChangeUserTaskStatusOfNewsRecordingRoutingParent), $"Service {newsRecording.Name} is news recording, first completing user tasks for its routing parent");

			order = helpers.OrderManager.GetOrder(newsRecording.OrderReferences.Single(), false, true) ?? throw new OrderNotFoundException(newsRecording.OrderReferences.Single());

			var newsRouting = order.AllServices.SingleOrDefault(s => s.Children.Any(child => child.Id == newsRecording.Id)) ?? throw new ServiceNotFoundException($"Unable to find routing parent for {newsRecording.Name}", true);

			var inCompletedUserTasks = newsRouting.UserTasks.Where(userTask => userTask != null && userTask.Status == UserTaskStatus.Incomplete && userGroups.Contains(userTask.UserGroup)).ToList();

			helpers.Log(nameof(UserTask), nameof(ChangeUserTaskStatusOfNewsRecordingRoutingParent), $"Service {newsRouting.Name} incomplete user tasks for user groups '{string.Join(";", userGroups.Select(userGroup => userGroup.GetDescription()))}': '{string.Join(", ", inCompletedUserTasks.Select(u => u.Name))}'");

			foreach (var userTask in inCompletedUserTasks)
			{
				userTask.SetToCompleteOrInComplete(order);
				// fullComplete and executeStartNow set to false because we assume it will be set to true when completing the user tasks for the news recording itself
			}

			helpers.LogMethodCompleted(nameof(UserTask), nameof(ChangeUserTaskStatusOfNewsRecordingRoutingParent), null, stopwatch);
		}

	}
}
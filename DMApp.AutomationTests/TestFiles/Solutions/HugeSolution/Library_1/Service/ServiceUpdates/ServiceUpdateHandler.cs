namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceUpdates
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;

	using Status = Status;
	using Service = Service;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;

	public abstract class ServiceUpdateHandler
	{
		protected readonly List<Task> tasks = new List<Task>();
		protected readonly Order orderContainingService;
		protected readonly Service service;
		protected readonly Service existingService;

		protected bool isSuccessful = true;
		protected bool userTasksRequired;

		protected ServiceUpdateHandler(Helpers helpers, Order orderContainingService, Service service, Service existingService = null)
		{
			Helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			this.service = service ?? throw new ArgumentNullException(nameof(service));
			this.orderContainingService = orderContainingService ?? throw new ArgumentNullException(nameof(orderContainingService));
			this.existingService = existingService;
		}

		public Helpers Helpers { get; }

		public bool Execute(out List<Task> executedTasks, out bool createUserTasks)
		{
			CollectTasks();

			executedTasks = new List<Task>();

			foreach (var task in tasks.TakeWhile(t => isSuccessful))
			{
				Log(nameof(Execute), $"Executing task {task.Description}");

				executedTasks.Add(task);
				if (!task.Execute())
				{
					isSuccessful = false;
					createUserTasks = userTasksRequired;
					return isSuccessful;
				}

				if (task.Status == Tasks.Status.Fail)
				{
					Log(nameof(Execute), $"Nonblocking task {task.Description} failed: {task.Exception}");
				}
			}

			createUserTasks = userTasksRequired;
			return isSuccessful;
		}

		protected abstract void CollectTasks();

		protected void ServiceStatusActions()
		{
			if (orderContainingService.Status != YLE.Order.Status.Cancelled && service.Status == Status.Cancelled)
			{
				service.Uncancel(Helpers.Engine);
				userTasksRequired = true;
			}
			else if (orderContainingService.Status != YLE.Order.Status.Rejected && service.Status == Status.Preliminary)
			{
				service.IsPreliminary = false;
			}
		}

		protected bool ServiceAddOrUpdateRequired()
		{
			return (ServiceHasChanged() && service.BackupType != BackupType.Cold) || service.MajorTimeslotChange;
		}

		protected bool ServiceHasChanged()
		{
			Log(nameof(ServiceHasChanged), "Checking if service has changed");

			var serviceChange = service.ChangeTrackingStarted ? service.Change as ServiceChange : service.GetChangeComparedTo(null, existingService) as ServiceChange;

			return serviceChange.Summary.IsChanged;
		}

		protected void Log(string nameOfMethod, string message)
		{
			Helpers.Log(nameof(ServiceUpdateHandler), nameOfMethod, message, service.Name);
		}
	}
}

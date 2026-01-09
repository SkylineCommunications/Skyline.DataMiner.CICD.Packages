namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTaskCreators
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Service = Service.Service;

	public abstract class LiveUserTaskCreator : UserTaskCreator
	{
		protected readonly Service service;

		protected readonly Order order;

		protected readonly Guid serviceId;

		protected readonly OrderType orderType;

		protected readonly List<ProfileParameter> parameters;

		protected LiveUserTaskCreator(Helpers helpers, Service service, Order order) : base(helpers)
		{
			this.order = order;

			this.service = service ?? throw new ArgumentNullException(nameof(service));
			this.parameters = service.Functions.SelectMany(function => function.Parameters).ToList();
			this.serviceId = service.Id;
			this.orderType = service.OrderType;
		}

		protected override void UpdateUserTaskName(UserTask userTask, string additionalDescription)
		{
			userTask.Name = $"{service.Name}: {additionalDescription}";
		}

		/// <summary>
		/// Checks if a project of product task for the Event of which the service is part of in the Ceiton Element has an entry with the given resource within a certain timespan.
		/// </summary>
		protected bool EventHasCeitonResource(string resource)
		{
			try
			{
				var @event = order.Event;
				if (String.IsNullOrEmpty(@event.ProjectNumber) || !@event.ProductNumbers.Any()) return false;

				var ceitonElement = helpers.Engine.FindElementsByProtocol(CeitonProtocol.Name).FirstOrDefault();
				if (ceitonElement == null) throw new ElementNotFoundException(CeitonProtocol.Name);

				var filteredProjectTasksTable = ceitonElement.GetFilteredTable(
					(Engine)helpers.Engine,
					CeitonProtocol.ProjectTasksTable.PID,
					new[] { new ColumnFilter { Pid = CeitonProtocol.ProjectTasksTable.ProjectIdPid, Value = @event.ProjectNumber, ComparisonOperator = ComparisonOperator.Equal } });

				Log(nameof(EventHasCeitonResource), $"Project tasks table has {filteredProjectTasksTable.Count()} entries for project number {@event.ProjectNumber}");

				foreach (var row in filteredProjectTasksTable)
				{
					var activityTypeName = Convert.ToString(row[CeitonProtocol.ProjectTasksTable.ActivityTypeNameIdx]);
					var rowStartTime = DateTime.FromOADate(Convert.ToDouble(row[CeitonProtocol.ProjectTasksTable.StartTimeIdx]));
					var rowEndTime = DateTime.FromOADate(Convert.ToDouble(row[CeitonProtocol.ProjectTasksTable.EndTimeIdx]));

					if (activityTypeName == resource && rowStartTime <= service.Start && service.End <= rowEndTime)
					{
						Log(nameof(EventHasCeitonResource), $"Row with primary key {Convert.ToString(row[0])} has activity type name '{activityTypeName}', start {rowStartTime} and end {rowEndTime}. This matches resource {resource} and service timing {service.Start} - {service.End}", service.Name);
						return true;
					}
				}

				var filteredProductTasksTable = ceitonElement.GetFilteredTable((Engine)helpers.Engine, CeitonProtocol.ProductTasksTable.PID, new[] { new ColumnFilter { Pid = CeitonProtocol.ProductTasksTable.ProjectIdPid, Value = @event.ProjectNumber, ComparisonOperator = ComparisonOperator.Equal } });

				Log(nameof(EventHasCeitonResource), $"Product tasks table has {filteredProductTasksTable.Count()} entries for project number {@event.ProjectNumber}", service.Name);

				foreach (var row in filteredProductTasksTable)
				{
					var activityTypeName = Convert.ToString(row[CeitonProtocol.ProductTasksTable.ActivityTypeNameIdx]);
					var rowStartTime = DateTime.FromOADate(Convert.ToDouble(row[CeitonProtocol.ProductTasksTable.StartTimeIdx]));
					var rowEndTime = DateTime.FromOADate(Convert.ToDouble(row[CeitonProtocol.ProductTasksTable.EndTimeIdx]));

					if (activityTypeName == resource && rowStartTime <= service.Start && service.End <= rowEndTime)
					{
						Log(nameof(EventHasCeitonResource), $"Row with primary key {Convert.ToString(row[0])} has activity type name '{activityTypeName}', start {rowStartTime} and end {rowEndTime}. This matches resource {resource} and service timing {service.Start} - {service.End}", service.Name);
						return true;
					}
				}

				Log(nameof(EventHasCeitonResource), $"No rows found in Project tasks or Product tasks table that matches resource {resource} and service timing {service.Start} - {service.End}", service.Name);

				return false;
			}
			catch (Exception e)
			{
				Log(nameof(EventHasCeitonResource), $"Something went wrong: {e}", service.Name);
				return false;
			}
		}
	}
}

namespace ShowCeitonDetails_2.Ceiton
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using ColumnFilter = Skyline.DataMiner.Core.DataMinerSystem.Common.ColumnFilter;
	using ComparisonOperator = Skyline.DataMiner.Core.DataMinerSystem.Common.ComparisonOperator;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;

	public class CeitonManager
	{
		private readonly Helpers helpers;
		private readonly IDmsElement element;

		public static readonly int ProjectsTablePid = 1000;
		public static readonly int ProjectsTableIndexPid = 1001;
		public static readonly int ProductsTablePid = 1100;
		public static readonly int ProductsTableProjectIdPid = 1102;
		public static readonly int ProjectTasksTablePid = 1200;
		public static readonly int ProjectTasksTableProjectPid = 1202;
		public static readonly int ProductTasksTablePid = 1300;
		public static readonly int ProductTasksTableProductIdPid = 1302;
		public static readonly int AdHocTasksTablePid = 1400;
		public static readonly int AdHocTasksTableIndexPid = 1401;
		public static readonly int ProjectSubTasksTablePid = 1500;
		public static readonly int ProjectSubTasksMainTaskIdPid = 1507;
		public static readonly int ProductSubTasksTablePid = 1600;
		public static readonly int ProductSubTasksMainTaskIdPid = 1607;
		public static readonly int AdHocSubTasksTablePid = 1700;
		public static readonly int AdHocSubTasksTableIndexPid = 1701;

		public CeitonManager(Helpers helpers)
		{
			OrderManagerElement orderManager = new OrderManagerElement(helpers);
			this.element = orderManager.CeitonElement;
		}

		public bool ProjectExists(string projectNumber)
		{
			try
			{
				IDmsTable projectsTable = element.GetTable(ProjectsTablePid);

				string[] primaryKeys = projectsTable.GetPrimaryKeys();

				return primaryKeys.Contains(projectNumber);
			}
			catch (Exception e)
			{
				helpers.Log(nameof(CeitonManager), nameof(ProjectExists), "error while checking if project " + projectNumber + " exists: " + e);
				return false;
			}
		}

		public Project GetProject(string projectNumber)
		{
			if (!ProjectExists(projectNumber))
			{
				return null;
			}

			try
			{
				IDmsTable projectsTable = element.GetTable(ProjectsTablePid);
				object[] projectsRow = projectsTable.QueryData(new[] { new ColumnFilter { Pid = ProjectsTableIndexPid, Value = projectNumber, ComparisonOperator = ComparisonOperator.Equal } }).FirstOrDefault();
				if (projectsRow == null)
				{
					return null;
				}
				else
				{
					return new Project
					{
						Number = Convert.ToString(projectsRow[0]),
						CeitonId = Convert.ToString(projectsRow[1]),
						Name = Convert.ToString(projectsRow[2]),
						Description = Convert.ToString(projectsRow[3]),
						ContentOwnerName = Convert.ToString(projectsRow[4]),
						ContentClass = Convert.ToString(projectsRow[8])
					};
				}
			}
			catch (Exception e)
			{
				helpers.Log(nameof(CeitonManager), nameof(GetProject), "error while getting project " + projectNumber + ": " + e);
				return null;
			}
		}

		public IEnumerable<Product> GetProducts(string projectNumber)
		{
			List<Product> products = new List<Product>();

			if (!ProjectExists(projectNumber))
			{
				return products;
			}

			IDmsTable productsTable = element.GetTable(ProductsTablePid);
			IEnumerable<object[]> productsRows = productsTable.QueryData(new[] { new ColumnFilter { Pid = ProductsTableProjectIdPid, Value = projectNumber, ComparisonOperator = ComparisonOperator.Equal } });
			foreach (object[] row in productsRows)
			{
				products.Add(new Product
				{
					Number = Convert.ToString(row[0]),
					Name = Convert.ToString(row[3]),
					Description = Convert.ToString(row[4])
				});
			}

			return products;
		}

		public IEnumerable<Task> GetProjectTasks(string projectNumber)
		{
			List<Task> tasks = new List<Task>();

			if (!ProjectExists(projectNumber))
			{
				return tasks;
			}

			IDmsTable projectTasksTable = element.GetTable(ProjectTasksTablePid);
			IEnumerable<object[]> projectTasksTableRows = projectTasksTable.QueryData(new[] { new ColumnFilter { Pid = ProjectTasksTableProjectPid, Value = projectNumber, ComparisonOperator = ComparisonOperator.Equal } });
			foreach (object[] taskRow in projectTasksTableRows)
			{
				Task task = new Task
				{
					Status = EnumExtensions.GetDescriptionFromEnumValue((TaskStatus)Convert.ToInt32(taskRow[2])),
					IsCancelled = EnumExtensions.GetDescriptionFromEnumValue((IsCancelled)Convert.ToInt32(taskRow[3])),
					IsFlexible = EnumExtensions.GetDescriptionFromEnumValue((IsFlexible)Convert.ToInt32(taskRow[12])),
					Comment = Convert.ToString(taskRow[4]),
					StartTime = DateTime.FromOADate(Convert.ToDouble(taskRow[5])),
					EndTime = DateTime.FromOADate(Convert.ToDouble(taskRow[6])),
					Duration = TimeSpan.FromMinutes(Convert.ToDouble(taskRow[7])),
					ActivityType = Convert.ToString(taskRow[9]),
					ResourceType = "N/A",
					EquipmentName = "N/A",
					ResourceId = Convert.ToString(taskRow[10]),
					ResourceName = Convert.ToString(taskRow[11]),
					SubTasks = new List<SubTask>()
				};

				if (task.StartTime.Day != task.EndTime.Day)
				{
					task.SubTasks = CreateProjectSubTasks(Convert.ToString(taskRow[0]));
				}

				tasks.Add(task);
			}

			return tasks;
		}

		public IEnumerable<Task> GetProductTasks(string projectNumber)
		{
			List<Task> tasks = new List<Task>();
			if (!ProjectExists(projectNumber))
			{
				return tasks;
			}

			// Find product ids for given project
			IDmsTable productsTable = element.GetTable(ProductsTablePid);
			IEnumerable<object[]> productTableRows = productsTable.QueryData(new[] { new ColumnFilter { Pid = ProductsTableProjectIdPid, Value = projectNumber, ComparisonOperator = ComparisonOperator.Equal } });
			string[] productIds = productTableRows.Select(x => (string)x[0]).ToArray();

			// Find products tasks for products that are part of the given project
			IDmsTable productTasksTable = element.GetTable(ProductTasksTablePid);
			foreach (string productId in productIds)
			{
				foreach (object[] productTaskRow in productTasksTable.QueryData(new[] { new ColumnFilter { Pid = ProductTasksTableProductIdPid, Value = productId, ComparisonOperator = ComparisonOperator.Equal } }))
				{
					Task task = new Task
					{
						ProjectOrProductId = productId,
						Status = EnumExtensions.GetDescriptionFromEnumValue((TaskStatus)Convert.ToInt32(productTaskRow[2])),
						IsCancelled = EnumExtensions.GetDescriptionFromEnumValue((IsCancelled)Convert.ToInt32(productTaskRow[3])),
						IsFlexible = EnumExtensions.GetDescriptionFromEnumValue((IsFlexible)Convert.ToInt32(productTaskRow[12])),
						Comment = Convert.ToString(productTaskRow[4]),
						StartTime = DateTime.FromOADate(Convert.ToDouble(productTaskRow[5])),
						EndTime = DateTime.FromOADate(Convert.ToDouble(productTaskRow[6])),
						Duration = TimeSpan.FromMinutes(Convert.ToDouble(productTaskRow[7])),
						ActivityType = Convert.ToString(productTaskRow[9]),
						ResourceType = "N/A",
						EquipmentName = "N/A",
						ResourceId = Convert.ToString(productTaskRow[10]),
						ResourceName = Convert.ToString(productTaskRow[11]),
						SubTasks = new List<SubTask>()
					};

					if (task.StartTime.Day != task.EndTime.Day)
					{
						task.SubTasks = CreateProductSubTasks(Convert.ToString(productTaskRow[0]));
					}

					tasks.Add(task);
				}
			}

			return tasks;
		}

		private IEnumerable<SubTask> CreateProjectSubTasks(string mainTaskId)
		{
			IDmsTable projectSubTasksTable = element.GetTable(ProjectSubTasksTablePid);
			List<object[]> projectSubTaskRows = projectSubTasksTable.QueryData(new[] { new ColumnFilter { Pid = ProjectSubTasksMainTaskIdPid, Value = mainTaskId, ComparisonOperator = ComparisonOperator.Equal } }).ToList();

			List<SubTask> subTasks = new List<SubTask>();
			foreach (object[] subTaskRow in projectSubTaskRows)
			{
				subTasks.Add(new SubTask(subTaskRow));
			}

			return subTasks;
		}

		private IEnumerable<SubTask> CreateProductSubTasks(string mainTaskId)
		{
			IDmsTable productSubTasksTable = element.GetTable(ProductSubTasksTablePid);
			IEnumerable<object[]> productSubTaskRows = productSubTasksTable.QueryData(new[] { new ColumnFilter { Pid = ProductSubTasksMainTaskIdPid, Value = mainTaskId, ComparisonOperator = ComparisonOperator.Equal } });

			List<SubTask> subTasks = new List<SubTask>();
			foreach (object[] subTaskRow in productSubTaskRows)
			{
				subTasks.Add(new SubTask(subTaskRow));
			}

			return subTasks;
		}
	}
}
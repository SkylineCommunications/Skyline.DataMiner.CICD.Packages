using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using Skyline.DataMiner.Library.Exceptions;
using Skyline.DataMiner.Net.Exceptions;
using Skyline.DataMiner.Net.Messages.Advanced;
using SLDataGateway.API.Repositories.CustomDataTableConfiguration;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Threading;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Messages;
	using System.Linq;

	public static class ElementExtensions
	{
		/// <summary>
		/// Get Table Data from an Element using SLNet calls.
		/// </summary>
		/// <remarks>WARNING: This method is based on an identical method in IDmsElement and IDmsTable. But without checks and exception handling.</remarks>
		public static IDictionary<string, object[]> GetTable(this Element element, IEngine engine, int tablePid, int keyColumnIdx = 0)
		{
			if (tablePid <= 0) throw new ArgumentOutOfRangeException(nameof(tablePid), "tablePid is 0 or negative");
			if (keyColumnIdx < 0) throw new ArgumentOutOfRangeException(nameof(keyColumnIdx), "keyColumnIdx is negative");

			GetPartialTableMessage message = new GetPartialTableMessage(element.DmaId, element.ElementId, tablePid, new[] {"forceFullTable=true"});

			ParameterChangeEventMessage response = (ParameterChangeEventMessage) engine.SendSLNetSingleResponseMessage(message);
			if (response == null) return new Dictionary<string, object[]>();

			return BuildDictionary(response, keyColumnIdx);
		}

		public static List<string[]> GetTable(this IActionableElement element, IEngine engine, int tableId, string[] filter)
		{
			var columns = new List<string[]>();
			try
			{
				var table = (ParameterChangeEventMessage)engine.SendSLNetSingleResponseMessage(new GetPartialTableMessage(element.DmaId, element.ElementId, tableId, filter));
				if (table != null && table.NewValue != null && table.NewValue.ArrayValue != null)
				{
					columns = table.NewValue.ArrayValue.Where(av => av != null && av.ArrayValue != null)
						.Select(p => p.ArrayValue/*.Where(v => v != null && v.ArrayValue != null && v.ArrayValue.Length > 0)*/
							.Select(c => (c != null && c.ArrayValue != null && c.ArrayValue.Length > 0) ? (c.ArrayValue[0].IsDouble ? Convert.ToString(c.ArrayValue[0].DoubleValue, CultureInfo.InvariantCulture) : c.ArrayValue[0].StringValue) : null).ToArray()).ToList();
				}
			}
			catch (Exception e)
			{
				engine.Log("(GetTable) Error getting table: " + e.ToString());
				return null;
			}

			return columns;
		}

		public static bool TableContainsPrimaryKey(this Element element, int tablePid, string primaryKey, int totalRetries = 10, int timeBetweenRetriesInMs = 200)
		{
			var retries = 0;
			var primaryKeyFound = false;

			while (!primaryKeyFound && retries < totalRetries)
			{
				primaryKeyFound = element.GetTablePrimaryKeys(tablePid).Contains(primaryKey);
				if (!primaryKeyFound)
				{
					Thread.Sleep(timeBetweenRetriesInMs);
					retries++;
				}
			}

			return primaryKeyFound;
		}

		private static IDictionary<string, object[]> BuildDictionary(ParameterChangeEventMessage response, int keyColumnIndex)
		{
			if (response == null) throw new ArgumentNullException(nameof(response));

			var result = new Dictionary<string, object[]>();

			if (response.NewValue?.ArrayValue == null) return result;
			
			ParameterValue[] columns = response.NewValue.ArrayValue;

			if (keyColumnIndex >= columns.Length)
			{
				throw new ArgumentException("Invalid key column index.", nameof(keyColumnIndex));
			}

			// Dictionary used as a mapping from index to key.
			string[] keyMap = new string[columns[keyColumnIndex].ArrayValue.Length];

			int rowNumber = 0;

			foreach (ParameterValue keyCell in columns[keyColumnIndex].ArrayValue)
			{
				string primaryKey = Convert.ToString(keyCell.CellValue.InteropValue, CultureInfo.CurrentCulture);

				result[primaryKey] = new object[columns.Length];
				keyMap[rowNumber] = primaryKey;
				rowNumber++;
			}

			int columnNumber = 0;
			foreach (ParameterValue column in columns)
			{
				rowNumber = 0;

				foreach (ParameterValue cell in column.ArrayValue)
				{
					result[keyMap[rowNumber]][columnNumber] = cell.CellValue.ValueType == ParameterValueType.Empty ? null : cell.CellValue.InteropValue;
					rowNumber++;
				}

				columnNumber++;
			}

			return result;
		}

		public static IEnumerable<object[]> GetFilteredTable(this Element element, Engine engine , int tableId, IEnumerable<ColumnFilter> filters)
		{
			TableFilter tableFilter = new TableFilter(filters);

			while (tableFilter.PageId >= 0)
			{
				IDmsTableQueryResult result = QueryDataInternal(engine, element, tableId, tableFilter);

				foreach (object[] row in result.PageRows)
				{
					yield return row;
				}

				if (tableFilter.IsIncludeAllPages || tableFilter.PageId >= result.NextPageId)
				{
					break;
				}

				tableFilter.PageId = result.NextPageId;
			}
		}

		private static IDmsTableQueryResult QueryDataInternal(Engine engine, Element element, int tableId, TableFilter filters)
		{
			try
			{
				List<string> filterValues = new List<string>();

				foreach (var filterItem in filters.Filter)
				{
					string filterValue = GetFilterValue(filterItem);

					if (!String.IsNullOrWhiteSpace(filterValue))
					{
						filterValues.Add(filterValue);
					}
				}

				if (filterValues.Any())
				{
					filters.IsIncludeAllPages = true; // when there are filters defined then it means that the entire table needs to be constructed with filtered items and placed into new pages per request, this is too impacting so a force return in one result is called.
				}

				if (filters.PageId > 0 && !filters.IsIncludeAllPages)
				{
					filterValues.Add("page=" + filters.PageId);
				}

				if (filters.IsIncludeAllPages)
				{
					filterValues.Add("forceFullTable=true");
				}

				GetPartialTableMessage message = new GetPartialTableMessage(element.DmaId, element.ElementId, tableId, filterValues.ToArray());

				ParameterChangeEventMessage response = (ParameterChangeEventMessage) engine.SendSLNetSingleResponseMessage(message);
				if (response == null)
				{
					throw new Exception($"Unable to find parameter {tableId} in element {element.ElementId}");
				}

				IDmsTableQueryResult result = new DmsTableQueryResult(response);

				return result;
			}
			catch (DataMinerException e)
			{
				if (e.ErrorCode == -2147024891 && e.Message == "No such element.")
				{
					// 0x80070005: Access is denied.
					throw new Exception($"Access to element {element.ElementId} denied", e);
				}
				else if (e.ErrorCode == -2147220935)
				{
					// 0x80040239, SL_FAILED_NOT_FOUND, The object or file was not found.
					throw new Exception($"Unable to find object {tableId} in element {element.ElementId}", e);
				}
				else if (e.ErrorCode == -2147220916)
				{
					// 0x8004024C, SL_NO_SUCH_ELEMENT, "The element is unknown."
					throw new Exception($"Unknown element {element.ElementId}", e);
				}
				else
				{
					throw;
				}
			}
		}

		private static string GetFilterValue(ColumnFilter tableFilterItem)
		{
			if (tableFilterItem.Pid < 1 || System.String.IsNullOrWhiteSpace(tableFilterItem.Value))
			{
				return System.String.Empty;
			}

			string returnValue = "value=" + tableFilterItem.Pid;
			switch (tableFilterItem.ComparisonOperator)
			{
				case ComparisonOperator.GreaterThan:
					returnValue = returnValue + " > ";
					break;
				case ComparisonOperator.GreaterThanOrEqual:
					returnValue = returnValue + " >= ";
					break;
				case ComparisonOperator.LessThan:
					returnValue = returnValue + " < ";
					break;
				case ComparisonOperator.LessThanOrEqual:
					returnValue = returnValue + " <= ";
					break;
				case ComparisonOperator.NotEqual:
					returnValue = returnValue + " != ";
					break;
				default:
					returnValue = returnValue + " == ";
					break;
			}

			returnValue = returnValue + tableFilterItem.Value;
			return returnValue;
		}

	}

	internal interface IDmsTableQueryResult
	{
		/// <summary>
		/// Gets the total number of pages that are present in the table.
		/// </summary>
		int TotalPageCount { get; }

		/// <summary>
		/// Gets the current page ID that has been returned.
		/// </summary>
		int CurrentPageId { get; }

		/// <summary>
		/// Gets the next page ID to be requested. '-1' when there are no more pages to be retrieved.
		/// </summary>
		int NextPageId { get; }

		/// <summary>
		/// Gets the total number of rows that are present in the table.
		/// </summary>
		int TotalRowCount { get; }

		/// <summary>
		/// Gets the rows that are present in this table page.
		/// </summary>
		ICollection<object[]> PageRows { get; }
	}

	internal class DmsTableQueryResult : IDmsTableQueryResult
	{
		private readonly int totalPageCount;
		private readonly int currentPageId;
		private readonly int nextPageId;
		private readonly int totalRowCount;
		private readonly ICollection<object[]> pageRows;

		public DmsTableQueryResult(Skyline.DataMiner.Net.Messages.ParameterChangeEventMessage response)
		{
			if (response == null)
			{
				throw new System.ArgumentNullException(nameof(response));
			}

			if (response.NewValue == null || response.NewValue.ArrayValue == null)
			{
				totalPageCount = 0;
				currentPageId = 1;
				nextPageId = -1;
				totalRowCount = 0;
				pageRows = new List<object[]>();
				return;
			}

			var table = new Dictionary<int, object[]>();
			var columns = response.NewValue.ArrayValue;
			int columnNumber = 0;

			foreach (var column in columns)
			{
				int rowNumber = 0;

				foreach (var cell in column.ArrayValue)
				{
					object[] row;

					if (!table.TryGetValue(rowNumber, out row))
					{
						row = new object[columns.Length];
						table[rowNumber] = row;
					}

					row[columnNumber] = cell.CellValue.ValueType == ParameterValueType.Empty ? null : cell.CellValue.InteropValue;
					rowNumber++;
				}

				columnNumber++;
			}

			pageRows = table.Values;

			if (response.PartialDataInfo == null || response.PartialDataInfo.Pages == null || response.PartialDataInfo.CurrentTablePage == 0)
			{
				totalPageCount = 1;
				currentPageId = 1;
				nextPageId = -1;
				totalRowCount = pageRows.Count;
				return;
			}

			totalPageCount = response.PartialDataInfo.Pages.Length;
			currentPageId = response.PartialDataInfo.CurrentTablePage;
			totalRowCount = response.PartialDataInfo.TotalAmountRows;
			nextPageId = currentPageId >= totalPageCount ? -1 : (currentPageId + 1);
		}

		/// <summary>
		/// Gets the total number of pages that are present in the queried table.
		/// </summary>
		public int TotalPageCount
		{
			get { return totalPageCount; }
		}



		/// <summary>
		/// Gets the current page ID that has been returned.
		/// </summary>
		public int CurrentPageId
		{
			get { return currentPageId; }
		}



		/// <summary>
		/// Gets the next page ID to be requested. '-1' when there are no more pages to be retrieved.
		/// </summary>
		public int NextPageId
		{
			get { return nextPageId; }
		}



		/// <summary>
		/// Gets the total number of rows that are present in the table.
		/// </summary>
		public int TotalRowCount
		{
			get { return totalRowCount; }
		}



		/// <summary>
		/// Gets the rows that are present in this page.
		/// </summary>
		public ICollection<object[]> PageRows
		{
			get { return pageRows; }
		}
	}

	public enum ComparisonOperator
	{
		/// <summary>
		/// Specifies to compare if both values are equal.
		/// </summary>
		Equal = 0,

		/// <summary>
		/// Specifies to compare if both values are not equal.
		/// </summary>
		NotEqual = 1,

		/// <summary>
		/// Specifies to compare if one value is greater than the other value.
		/// </summary>
		GreaterThan = 2,

		/// <summary>
		/// Specifies to compare if one value is greater than or equal to the other value.
		/// </summary>
		GreaterThanOrEqual = 3,

		/// <summary>
		/// Specifies to compare if one value is less than the other value.
		/// </summary>
		LessThan = 4,

		/// <summary>
		/// Specifies to compare if one value is less than or equal to the other value.
		/// </summary>
		LessThanOrEqual = 6
	}


	public class ColumnFilter
	{
		/// <summary>
		/// Gets or sets the ID of the column parameter that will be used in the filter.
		/// </summary>
		public int Pid { get; set; }

		/// <summary>
		/// Gets or sets the value that will be used to compare against.
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Gets or sets how the comparison operator.
		/// </summary>
		public ComparisonOperator ComparisonOperator { get; set; }
	}

	/// <summary>
	/// Filter to be applied when querying the table.
	/// </summary>
	internal class TableFilter
	{
		private readonly ICollection<ColumnFilter> filter;

		/// <summary>
		/// Initializes a new instance of the <see cref="TableFilter"/> class.
		/// </summary>
		/// <param name="filterItems">Filter to be applied when executing the query.</param>
		public TableFilter(IEnumerable<ColumnFilter> filterItems)
		{
			PageId = 0;
			IsIncludeAllPages = false;
			filter = new List<ColumnFilter>(filterItems);
		}


		/// <summary>
		/// Gets or sets the id of the page of the table to be returned. This has only effect on partial tables. This setting has no effect when querying normal tables, where all rows will be returned.
		/// </summary>
		public int PageId { get; set; }


		/// <summary>
		/// Gets or sets the indication if all pages of a partial table should be returned. Warning: when setting to 'true' on a partial table, without extra filtering, could result in a large object being returned, this could have a large impact on SLElement, SLNet, SLNetCom and SLScripting. This setting has no effect when querying normal tables, where all rows will be returned. 
		/// </summary>
		public bool IsIncludeAllPages { get; set; }


		/// <summary>
		/// Gets the collection of filters that will be applied when querying the table. Every item in the filter will be combined as a logical AND.
		/// </summary>
		public System.Collections.Generic.ICollection<ColumnFilter> Filter
		{
			get { return filter; }
		}
	}
}
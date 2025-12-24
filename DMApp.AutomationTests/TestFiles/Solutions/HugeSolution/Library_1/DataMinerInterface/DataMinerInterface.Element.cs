namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface
{
    using System;
    using System.Collections.Generic;
	using System.Reflection;
	using System.Text;
	using Automation;
	using Library.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public static partial class DataMinerInterface
	{
		public static class Element
		{
			[WrappedMethod("Element", "SetParameter")]
			public static void SetParameter(Helpers helpers, Automation.Element element, int parameterId, object parameterValue)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, element.ElementName);

				element.SetParameter(parameterId, parameterValue);

				if (parameterValue is string stringValue)
				{
					Log(helpers, MethodBase.GetCurrentMethod(), $"Sent a string of {Encoding.ASCII.GetBytes(stringValue).Length} bytes to {element.Name} parameter {parameterId}");
				}

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);
			}

			[WrappedMethod("Element", "GetParameter")]
			public static object GetParameter(Helpers helpers, Automation.Element element, int parameterId)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, element.ElementName);

				var result = element.GetParameter(parameterId);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			[WrappedMethod("Element", "SetParameterByPrimaryKey")]
			public static void SetParameterByPrimaryKey(Helpers helpers, Automation.Element element, int parameterId, string primaryKey, object parameterValue)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, element.ElementName);

				element.SetParameterByPrimaryKey(parameterId, primaryKey, parameterValue);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);
			}

			[WrappedMethod("Element", "GetTablePrimaryKeys")]
			public static string[] GetTablePrimaryKeys(Helpers helpers, Automation.Element element, int tablePid)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, element.ElementName);

				var primaryKeys = element.GetTablePrimaryKeys(tablePid);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return primaryKeys;
			}

			[WrappedMethod("Element", "GetTable")]
			public static IDictionary<string, object[]> GetTable(Helpers helpers, Automation.Element element, int tablePid)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, element.ElementName);

				var result = element.GetTable(helpers.Engine, tablePid);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			[WrappedMethod("Element", "GetParameterByPrimaryKey")]
			public static object GetParameterByPrimaryKey(Helpers helpers, Automation.Element element, int columnPid, string primaryKey)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, element.ElementName);

				var value = element.GetParameterByPrimaryKey(columnPid, primaryKey);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return value;
			}

			[WrappedMethod("Element", "TableContainsPrimaryKey")]
			public static bool TableContainsPrimaryKey(Helpers helpers, Automation.Element element, int tablePid, string primaryKey)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, element.ElementName);

				var value = element.TableContainsPrimaryKey(tablePid, primaryKey);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return value;
			}

			[WrappedMethod("Element", "DeleteRow")]
			public static void DeleteRow(Helpers helpers, Automation.Element element, int tablePid, string primaryKey)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, element.ElementName);

				element.DeleteRow((Automation.Engine)helpers.Engine, tablePid, primaryKey);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);
			}
		}

		public static class IDmsElement
		{
			/// <summary>
			/// Sets the value of a parameter.
			/// </summary>
			/// <typeparam name="T">string, int?, double? or DateTime?</typeparam>
			/// <param name="helpers">Link with DM.</param>
			/// <param name="element">Element to set the parameter value on.</param>
			/// <param name="parameterId">ID of the parameter to set.</param>
			/// <param name="parameterValue">Value to set.</param>
			[WrappedMethod("IDmsElement", "SetParameter")]
			public static void SetParameter<T>(Helpers helpers, Core.DataMinerSystem.Common.IDmsElement element, int parameterId, T parameterValue)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, element.Name);

				element.GetStandaloneParameter<T>(parameterId).SetValue(parameterValue);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);
			}

			/// <summary>
			/// Retrieves the value of a parameter.
			/// </summary>
			/// <typeparam name="T">string, int?, double? or DateTime?</typeparam>
			/// <param name="helpers">Link with DM.</param>
			/// <param name="element">Element to get the parameter value from.</param>
			/// <param name="parameterId">ID of the parameter to retrieve the value from.</param>
			/// <returns></returns>
			[WrappedMethod("IDmsElement", "GetParameter")]
			public static T GetParameter<T>(Helpers helpers, Core.DataMinerSystem.Common.IDmsElement element, int parameterId)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, element.Name);

				T result = element.GetStandaloneParameter<T>(parameterId).GetValue();

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			/// <summary>
			/// Sets the value of a cell in a table.
			/// </summary>
			/// <typeparam name="T">string, int?, double? or DateTime?</typeparam>
			/// <param name="helpers"></param>
			/// <param name="element"></param>
			/// <param name="tablePid"></param>
			/// <param name="columnPid"></param>
			/// <param name="primaryKey"></param>
			/// <param name="parameterValue"></param>
			[WrappedMethod("IDmsElement", "SetParameterByPrimaryKey")]
			public static void SetParameterByPrimaryKey<T>(Helpers helpers, Core.DataMinerSystem.Common.IDmsElement element, int tablePid, int columnPid, string primaryKey, T parameterValue)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, element.Name);

				var table = element.GetTable(tablePid);
				var column = table.GetColumn<T>(columnPid);
				column.SetValue(primaryKey, parameterValue);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);
			}

			[WrappedMethod("IDmsElement", "GetTablePrimaryKeys")]
			public static string[] GetTablePrimaryKeys(Helpers helpers, Core.DataMinerSystem.Common.IDmsElement element, int tablePid)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, element.Name);

				string[] primaryKeys = element.GetTable(tablePid).GetPrimaryKeys();

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return primaryKeys;
			}

			[WrappedMethod("IDmsElement", "GetTable")]
			public static IDictionary<string, object[]> GetTable(Helpers helpers, Core.DataMinerSystem.Common.IDmsElement element, int tablePid)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, element.Name);

				var result = element.GetTable(tablePid).GetData();

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			/// <summary>
			/// Retrieves the value from a cell in a table.
			/// </summary>
			/// <typeparam name="T">string, int?, double? or DateTime?</typeparam>
			/// <param name="helpers">Link with DM.</param>
			/// <param name="element">Element to retrieve the table value from.</param>
			/// <param name="tablePid">ID of the table to retrieve the value from.</param>
			/// <param name="columnPid">ID of the column that holds the requested value.</param>
			/// <param name="primaryKey">Key of the row that holds the requested value.</param>
			/// <returns></returns>
			[WrappedMethod("IDmsElement", "GetParameterByPrimaryKey")]
			public static T GetParameterByPrimaryKey<T>(Helpers helpers, Core.DataMinerSystem.Common.IDmsElement element, int tablePid, int columnPid, string primaryKey)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, element.Name);

				var table = element.GetTable(tablePid);
				var column = table.GetColumn<T>(columnPid);
				var value = column.GetValue(primaryKey, Core.DataMinerSystem.Common.KeyType.PrimaryKey);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return value;
			}

			[WrappedMethod("IDmsElement", "TableContainsPrimaryKey")]
			public static bool TableContainsPrimaryKey(Helpers helpers, Core.DataMinerSystem.Common.IDmsElement element, int tablePid, string primaryKey)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, element.Name);

				var value = element.GetTable(tablePid).RowExists(primaryKey);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return value;
			}

			[WrappedMethod("IDmsElement", "DeleteRow")]
			public static void DeleteRow(Helpers helpers, Core.DataMinerSystem.Common.IDmsElement element, int tablePid, string primaryKey)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, element.Name);

				element.GetTable(tablePid).DeleteRow(primaryKey);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);
			}
		}
	}
}
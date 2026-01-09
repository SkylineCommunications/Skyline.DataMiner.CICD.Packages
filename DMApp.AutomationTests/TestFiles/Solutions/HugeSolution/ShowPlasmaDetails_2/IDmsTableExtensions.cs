namespace ShowPlasmaDetails_2
{
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Net.Messages;

	/// <summary>
	/// This class contains extension methods on the IDmsTable class.
	/// These methods were introduced as the default DmsTable methods were not compatible with replicated elements.
	/// </summary>
	public static class IDmsTableExtensions
	{
		public static bool RowExists_Ext(this IDmsTable table, string key)
		{
			return table.GetPrimaryKeys().Contains(key);
		}

		public static IDictionary<string, object[]> GetData_Ext(this IDmsTable table, IEngine engine)
		{
			var response = (GetParameterResponseMessage) engine.SendSLNetSingleResponseMessage(new GetParameterMessage
			{
				DataMinerID = table.Element.AgentId,
				ElId = table.Element.Id,
				ParameterId = table.Id,
				HostingDataMinerID = -1,
				TableIndex = "0"
			});

			object[] row;
			string key;
			IDictionary<string, object[]> data = new Dictionary<string, object[]>();
			var rowCount = response.Value.ArrayValue[0].ArrayValue.Length;
			for (var rowIdx = 0; rowIdx < rowCount; rowIdx++)
			{
				row = new object[response.Value.ArrayValue.Length];
				key = response.Value.ArrayValue[0].ArrayValue[rowIdx].CellValue.StringValue;

				for (var columnIdx = 0; columnIdx < response.Value.ArrayValue.Length; columnIdx++) row[columnIdx] = response.Value.ArrayValue[columnIdx].ArrayValue[rowIdx].CellValue.InteropValue;

				data.Add(key, row);
			}

			return data;
		}

		public static object[] GetRow_Ext(this IDmsTable table, IEngine engine, string key)
		{
			return table.QueryData(new ColumnFilter[]
				{
					new ColumnFilter
					{
						Pid = table.Id + 1, // This expects that the first column for each table is the Index column
						Value = key,
						ComparisonOperator = ComparisonOperator.Equal
					}
				})
				.FirstOrDefault();
		}

		//public static void SetRow??

		public static void SetCell(this IDmsTable table, Engine engine, string value, int columnPid, string primaryKey)
		{
			var response = (SetParameterResponseMessage) engine.SendSLNetSingleResponseMessage(new SetParameterMessage
			{
				DataMinerID = table.Element.AgentId,
				ElId = table.Element.Id,
				HostingDataMinerID = -1,
				ParameterId = columnPid,
				TableIndex = primaryKey,
				Value = new ParameterValue(value)
			});
		}
	}
}
namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Metrics
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.MethodCalls;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using System.IO;

	public class MetricsDialog : Dialog
	{
		private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
		{
			ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
			Formatting = Formatting.Indented
		};

		private const string MetricLoggingDirectory = @"C:\Skyline_Data\MetricLogging\";

		private readonly Helpers helpers;
		private readonly MetricAggregator metricAggregator;

		private readonly Label metricsPerOrderLabel = new Label("Metrics per Order") { Style = TextStyle.Heading };
		private readonly Label orderIdLabel = new Label("Order ID");
		private readonly TextBox orderIdTextBox = new TextBox();
		private readonly Button getMetricsButton = new Button("Get Metrics") { Width = 200 };
		private readonly Button enterCurrentIdButton = new Button("Enter Current ID") { Width = 250 };

		private readonly Label dataMinerInterfaceMetricsLabel = new Label("DataMiner Interface Metrics") { Style = TextStyle.Heading };
		private readonly Button getAllDataMinerInterfaceMetricsButton = new Button("Get All DataMiner Interface Metrics") { Width = 250 };
		
		private readonly Button getDataMinerInterfaceMethodCallMetricSummaryButton = new Button("Get Metric Summary for");
		private readonly DropDown dataMinerInterfaceMethodsDropDown = new DropDown(DataMinerInterface.GetWrappedMethods().Select(wm => wm.ToString()));

		private readonly Label performanceLabel = new Label("Performance") { Style= TextStyle.Heading };
		private readonly Button getPerformanceDropTimestampsButton = new Button("Get Performance Drop Timestamps");
		private readonly Button getScriptExecutionTimestampsButton = new Button("Get Script Execution Timestamps");

		private readonly Label filtersLabel = new Label("Filters") { Style = TextStyle.Heading };
		private readonly Label startLabel = new Label("Start");
		private readonly DateTimePicker startDateTimePicker = new DateTimePicker(DateTime.Now.AddDays(-1));
		private readonly Label endLabel = new Label("End");
		private readonly DateTimePicker endDateTimePicker = new DateTimePicker(DateTime.Now);
		private readonly CheckBox limitAmountOfFilesToProcessCheckBox = new CheckBox("Limit Amount of Files to Process to") { IsChecked = true };
		private readonly Numeric maxAmountOfFilesToProcessNumeric = new Numeric(50) { Minimum = 1, StepSize = 1, Decimals = 0 };
		private readonly Label totalAmountOfFilesLabel = new Label(String.Empty);

		private readonly List<RequestResultSection> responseSections = new List<RequestResultSection>();

		public MetricsDialog(Helpers helpers) : base(helpers.Engine)
		{
			this.helpers = helpers;
			this.metricAggregator = new MetricAggregator(helpers);

			Title = "Metrics";

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		private int MaxAmountOfFilesToProcess => limitAmountOfFilesToProcessCheckBox.IsChecked ? (int)maxAmountOfFilesToProcessNumeric.Value : Int32.MaxValue;

		private void Initialize()
		{
			var numberOfFiles = Directory.GetFiles(MetricLoggingDirectory).Length;
			totalAmountOfFilesLabel.Text = $"Out of {numberOfFiles} Files";

			limitAmountOfFilesToProcessCheckBox.Changed += (s, e) => maxAmountOfFilesToProcessNumeric.IsEnabled = limitAmountOfFilesToProcessCheckBox.IsChecked;

			getMetricsButton.Pressed += GetMetricsButtonPressed;

			enterCurrentIdButton.Pressed += (sender, args) => orderIdTextBox.Text = helpers.Engine.GetScriptParam(1)?.Value;

			getAllDataMinerInterfaceMetricsButton.Pressed += GetAllDataMinerInterfaceMetricsButton_Pressed;

			getDataMinerInterfaceMethodCallMetricSummaryButton.Pressed += GetDataMinerInterfaceMethodCallMetricSummaryButton_Pressed;

			getPerformanceDropTimestampsButton.Pressed += GetPerformanceDropTimestampsButton_Pressed;

			getScriptExecutionTimestampsButton.Pressed += GetScriptExecutionTimestampsButton_Pressed;
		}

		private void GetScriptExecutionTimestampsButton_Pressed(object sender, EventArgs e)
		{
			var summary = metricAggregator.GetMetricsSummary(MetricLoggingDirectory, out var filesToRemove, out var amountOfProcessedFiles, startDateTimePicker.DateTime, endDateTimePicker.DateTime, MaxAmountOfFilesToProcess);

			ShowRequestResult($"Script Metric Summaries (taken from {amountOfProcessedFiles} files)", string.Join("\n", summary.ScriptMetricSummaries.Select(x => JsonConvert.SerializeObject(x, JsonSerializerSettings))));
		}

		private void GetPerformanceDropTimestampsButton_Pressed(object sender, EventArgs e)
		{
			var timestamps = metricAggregator.GetPerformanceDropTimeStamps(MetricLoggingDirectory, out var processedFiles, MaxAmountOfFilesToProcess).OrderBy(x => x);

			ShowRequestResult($"Performance Drop Timestamps (taken from {processedFiles.Count} files)", string.Join("\n", timestamps));
		}

		private void GetDataMinerInterfaceMethodCallMetricSummaryButton_Pressed(object sender, EventArgs e)
		{
			var summary = metricAggregator.GetDataMinerInterfaceMethodCallSummary(MetricLoggingDirectory, dataMinerInterfaceMethodsDropDown.Selected, out var processedFiles, MaxAmountOfFilesToProcess);

			ShowRequestResult($"{dataMinerInterfaceMethodsDropDown.Selected} Summary (taken from {processedFiles.Count} files)", JsonConvert.SerializeObject(summary, JsonSerializerSettings));
		}

		private void GetAllDataMinerInterfaceMetricsButton_Pressed(object sender, EventArgs e)
		{
			var summary = metricAggregator.GetMetricsSummary(MetricLoggingDirectory, out var filesToRemove, out var amountOfProcessedFiles, startDateTimePicker.DateTime, endDateTimePicker.DateTime, MaxAmountOfFilesToProcess);

			ShowRequestResult($"All DataMiner Interface Summaries (taken from {amountOfProcessedFiles} files)", JsonConvert.SerializeObject(summary.DataMinerInterfaceMethodCallSummaries, JsonSerializerSettings));
		}

		private void GetMetricsButtonPressed(object sender, EventArgs e)
		{
			var scriptExecutionMetrics = metricAggregator.ReadMetricFile($"{MetricLoggingDirectory}{orderIdTextBox.Text}.txt");

			foreach (var scriptExecutionMetric in scriptExecutionMetrics)
			{
				ShowRequestResult($"Script Execution {scriptExecutionMetric.ScriptName} at {scriptExecutionMetric.StartTime} by {scriptExecutionMetric.UserDisplayName} on DMA {scriptExecutionMetric.DmaId}", JsonConvert.SerializeObject(scriptExecutionMetric, JsonSerializerSettings));
			}

			var scriptExecutionMetricSummaries = metricAggregator.GetScriptExecutionMetricSummaries(scriptExecutionMetrics.ToArray());

			foreach (var summary in scriptExecutionMetricSummaries)
			{
				ShowRequestResult($"Script Execution Summary {summary.ScriptName}", $@"Metrics are valid = {summary.ScriptMetricsAreValid}\nTotal Execution Time = {summary.ExecutionTime}\nMethod Call Summaries:\n{string.Join("\n", JsonConvert.SerializeObject(summary.MethodCallSummaries, JsonSerializerSettings))}");
			}

			var summaries = MetricAggregator.GetDataMinerInterfaceMetricSummaries(scriptExecutionMetricSummaries.ToArray());

			ShowRequestResult("DataMiner Interface Summaries", JsonConvert.SerializeObject(summaries, JsonSerializerSettings));
		}

		private void ShowRequestResult(string header, params string[] results)
		{
			responseSections.Add(new RequestResultSection(header, results));

			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddWidget(metricsPerOrderLabel, ++row, 0, 1, 5);
			AddWidget(orderIdLabel, ++row, 0, 1, 2);
			AddWidget(orderIdTextBox, row, 2, 1, 2);
			AddWidget(getMetricsButton, row, 4);
			AddWidget(enterCurrentIdButton, ++row, 2, 1, 2);

			AddWidget(new WhiteSpace(), ++row, 1);

			AddWidget(dataMinerInterfaceMetricsLabel, ++row, 0, 1, 5);
			AddWidget(getAllDataMinerInterfaceMetricsButton, ++row, 2, 1, 2);

			AddWidget(new WhiteSpace(), ++row, 1);

			AddWidget(getDataMinerInterfaceMethodCallMetricSummaryButton, ++row, 2);
			AddWidget(dataMinerInterfaceMethodsDropDown, row, 3);			

			AddWidget(new WhiteSpace(), ++row, 1);

			AddWidget(performanceLabel, ++row, 0, 1, 5);
			AddWidget(getPerformanceDropTimestampsButton, ++row, 2);
			AddWidget(getScriptExecutionTimestampsButton, ++row, 2);

			AddWidget(new WhiteSpace(), ++row, 1);

			AddWidget(filtersLabel, ++row, 0, 1, 5);
			AddWidget(limitAmountOfFilesToProcessCheckBox, ++row, 2);
			AddWidget(maxAmountOfFilesToProcessNumeric, row, 3);
			AddWidget(totalAmountOfFilesLabel, row, 4);
			AddWidget(startLabel, ++row, 0, 1, 2);
			AddWidget(startDateTimePicker, row, 2); 
			AddWidget(endLabel, ++row, 0, 1, 2);
			AddWidget(endDateTimePicker, row, 2);

			AddWidget(new WhiteSpace(), ++row, 1);

			row++;
			foreach (var responseSection in responseSections)
			{
				responseSection.Collapse();
				AddSection(responseSection, row, 0);
				row += responseSection.RowCount;
			}
		}
	}
}

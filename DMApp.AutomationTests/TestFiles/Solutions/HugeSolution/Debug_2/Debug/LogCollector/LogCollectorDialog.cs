namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.LogCollector
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public  class LogCollectorDialog : Dialog
	{
		private readonly Helpers helpers;

		private readonly Label runLogCollectorLabel = new Label("Run Log Collector") { Style = TextStyle.Heading };

		private readonly Button getMostRecentLogCollectionTimeStampButton = new Button("Get Most Recent Log Collection Timestamp");
		private readonly Label mostRecentLogCollectionTimestampLabel = new Label(String.Empty);

		private readonly Button runOnThisDmaButton = new Button("Run sync on this DMA");
		private readonly Button runAsyncOnMasterDmaButton = new Button("Run async on this and master DMA");

		private readonly List<RequestResultSection> responseSections = new List<RequestResultSection>();

		public LogCollectorDialog(Helpers helpers) : base(helpers.Engine)
		{
			Title = "Run Log Collector";

			this.helpers = helpers;

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		private void ShowRequestResult(string header, params string[] results)
		{
			responseSections.Add(new RequestResultSection(header, results));

			GenerateUi();
		}

		private void Initialize()
		{
			getMostRecentLogCollectionTimeStampButton.Pressed += (s, e) => mostRecentLogCollectionTimestampLabel.Text = $"{helpers.LogCollectorHelper.GetMostRecentLogCollectionTimeStamp()} on current DMA {Automation.Engine.SLNetRaw.ServerDetails.AgentID}";

			runOnThisDmaButton.Pressed += RunOnThisDmaButton_Pressed;

			runAsyncOnMasterDmaButton.Pressed += RunAsyncOnMasterDmaButton_Pressed;
		}

		private void RunAsyncOnMasterDmaButton_Pressed(object sender, EventArgs e)
		{
			helpers.LogCollectorHelper.RunLogCollectorAsync();

			ShowRequestResult($"Log Collection at {DateTime.Now}", "Triggered async");
		}

		private void RunOnThisDmaButton_Pressed(object sender, EventArgs e)
		{
			helpers.LogCollectorHelper.RunLogCollectorOnThisDma();

			ShowRequestResult($"Log Collection at {DateTime.Now}", "Succeeded");
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddWidget(runLogCollectorLabel, ++row, 0, 1, 5);

			AddWidget(getMostRecentLogCollectionTimeStampButton, ++row, 0);
			AddWidget(mostRecentLogCollectionTimestampLabel, row, 1, 1, 4);

			AddWidget(runOnThisDmaButton, ++row, 0);
			AddWidget(runAsyncOnMasterDmaButton, ++row, 0);

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

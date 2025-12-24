namespace SendOrderDebugReport_2
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Text;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class SendOrderDebugReportDialog : Dialog, IDisposable
	{
		private const string DebugLoggingDirectoryPartOne = @"C:\Skyline_Data\Logging\";
		private const string DebugLoggingDirectoryPartTwo = @"_Debug.txt";

		private readonly Label relatedBugLabel = new Label("Related Bug");
		private readonly TextBox relatedBugTextBox = new TextBox(string.Empty) { PlaceHolder = "bug number" };
		private readonly Label additionalCommentsLabel = new Label("Additional Comments");
		private readonly TextBox additionalCommentsTextBox = new TextBox(string.Empty) { IsMultiline = true, MinHeight = 100, MaxHeight = 250, MinWidth = 500 };
		private readonly Button sendReportButton = new Button("Send Report") { Width = 150, Style = ButtonStyle.CallToAction };

		private readonly LiteOrder order;
		private Helpers helpers;
		private bool disposedValue;

		public SendOrderDebugReportDialog(Helpers helpers) : base(helpers.Engine)
		{
			this.helpers = helpers;

			string orderId = helpers.Engine.GetScriptParam(1).Value;
			if (!Guid.TryParse(orderId, out Guid orderGuid)) throw new ArgumentException("Provided Order ID is not a Guid", "ScriptParam 1");

			this.order = helpers.OrderManager.GetLiteOrder(orderGuid);

			Title = order.Name + " Debug Report";

			sendReportButton.Pressed += SendReportButton_Pressed;

			GenerateUi();
		}

		private void SendReportButton_Pressed(object sender, EventArgs e)
		{
			string logging = GetLoggingInHtmlFormat();

			string title = "Bug " + relatedBugTextBox.Text + " - Order: " + order.Name + " [" + DateTime.Now.ToString(CultureInfo.InvariantCulture) + "]";

			string message = $"Order name: '{order.Name}'<br>Order ID: {order.Id}<br>Event: '{order.Event.Name}'<br>Report sent by: {Engine.UserLoginName}<br>Timestamp: {DateTime.Now.ToString(CultureInfo.InvariantCulture)}<br>Additional user comments: {additionalCommentsTextBox.Text}<br><br>Logging: <br>{logging}";

			NotificationManager.SendMailToSkylineDevelopers(helpers, title, message);

			Engine.ExitSuccess("Successfully sent report");
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(relatedBugLabel, new WidgetLayout(++row, 0));
			AddWidget(relatedBugTextBox, new WidgetLayout(row, 1));

			AddWidget(additionalCommentsLabel, new WidgetLayout(++row, 0));
			AddWidget(additionalCommentsTextBox, new WidgetLayout(row, 1));

			AddWidget(sendReportButton, new WidgetLayout(++row, 0, 1, 2));
		}

		private string GetLoggingInHtmlFormat()
		{
			var sb = new StringBuilder();

			try
			{
				using (var stream = File.Open(CreateLoggingFileDirectory(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					using (var streamReader = new StreamReader(stream))
					{
						string line;
						while ((line = streamReader.ReadLine()) != null)
						{
							sb.Append(line);
							sb.Append("<br>");
						}
					}
				}
			}
			catch (Exception e)
			{
				sb.Append("Exception occurred while getting logging: " + e);
			}

			return sb.ToString();
		}

		private string CreateLoggingFileDirectory()
		{
			return DebugLoggingDirectoryPartOne + order.Id.ToString() + DebugLoggingDirectoryPartTwo;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					helpers.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
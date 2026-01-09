namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations
{
	using System;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Skyline.DataMiner.Utils.YLE.Integrations.Eurovision;

	public class SendEurovisionIntegrationNotificationDialog : DebugDialog
	{
		private readonly Label eurovisionLabel = new Label("Eurovision") { Style = TextStyle.Heading };
		private readonly Label infoLabel = new Label("Clicking 'Send Notification' will immediately trigger the HIU script without passing the data to the Order Manager");

		private readonly Label transmissionNumberLabel = new Label("Transmission Number");
		private readonly TextBox transmissionNumberTextBox = new TextBox();
		private readonly Label synopsisXmlLabel = new Label("Synopsis XML");
		private readonly TextBox synopsisXmlTextBox = new TextBox { IsMultiline = true, Height = 400 };
		private readonly Button sendNotificationButton = new Button("Send Notification");

		public SendEurovisionIntegrationNotificationDialog(Utilities.Helpers helpers) : base(helpers)
		{
			Title = "Send Eurovision Integration Notification";

			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			sendNotificationButton.Pressed += (s, e) => SendNotification();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0);

			AddWidget(eurovisionLabel, ++row, 0, 1, 3);

			AddWidget(infoLabel, ++row, 0, 1, 2);

			AddWidget(transmissionNumberLabel, ++row, 0, verticalAlignment: VerticalAlignment.Top);
			AddWidget(transmissionNumberTextBox, row, 1);

			AddWidget(synopsisXmlLabel, ++row, 0, verticalAlignment: VerticalAlignment.Top);
			AddWidget(synopsisXmlTextBox, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(sendNotificationButton, ++row, 0, 1, 2);

			AddResponseSections(++row);
		}

		private string TransmissionNumber => transmissionNumberTextBox.Text;

		private string SynopsisXml => synopsisXmlTextBox.Text;

		private void SendNotification()
		{
			try
			{
				var parsedSynopsis = synopsis.Deserialize(SynopsisXml) ?? throw new InvalidOperationException("Unable to parse synopis");
				IntegrationRequest request = new IntegrationRequest(IntegrationType.Eurovision, TransmissionNumber, parsedSynopsis.Serialize());

				string serializedRequest = JsonConvert.SerializeObject(request);

				var scriptOptions = helpers.Engine.PrepareSubScript("HandleIntegrationUpdate");
				scriptOptions.Synchronous = true;
				scriptOptions.SelectScriptParam("update", serializedRequest);
				scriptOptions.StartScript();

				ShowRequestResult(TransmissionNumber, serializedRequest);
			}
			catch (Exception e)
			{
				ShowRequestResult($"{TransmissionNumber} [{DateTime.Now}]", $"Failed to send notification: {e}");
				GenerateUi();
			}
		}
	}
}

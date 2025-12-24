namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.JsonObjects;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.ParsedObjects;
	using Skyline.DataMiner.Utils.YLE.Integrations.Plasma;
	using Skyline.DataMiner.Utils.YLE.Integrations.Plasma.VCM;
	using Newtonsoft.Json;

	public class SendPlasmaIntegrationNotificationDialog : DebugDialog
	{
		private YleCollapseButton parsedProgramCollapseButton;
		private readonly Label parsedProgramHeaderLabel = new Label("Parsed Program") { Style = TextStyle.Heading };
		private readonly ParsedProgram parsedProgram = new ParsedProgram();
		private JsonObjectSection parsedProgramSection;
		private readonly Button sendParsedProgramNotificationButton = new Button("Send Parsed Program") { Width = 200 };

		private YleCollapseButton editorialObjectCollapseButton;
		private readonly Label editorialObjectHeaderLabel = new Label("Editorial Object") { Style = TextStyle.Heading };
		private readonly EditorialObject editorialObject = new EditorialObject();
		private JsonObjectSection editorialObjectSection;
		private readonly Button sendEditorialObjectNotificationButton = new Button("Send Editorial Object") { Width = 200 };

		private YleCollapseButton publicationEventCollapseButton;
		private readonly Label publicationEventHeaderLabel = new Label("Publication Event") { Style = TextStyle.Heading };
		private readonly PublicationEvent publicationEvent = new PublicationEvent();
		private JsonObjectSection publicationEventSection;
		private readonly Button sendPublicationEventNotificationButton = new Button("Send Publication Event") { Width = 200 };

		private YleCollapseButton parsedPublicationEventCollapseButton;
		private readonly Label parsedPublicationEventHeaderLabel = new Label("Parsed Publication Event") { Style = TextStyle.Heading };
		private readonly ParsedPublicationEvent parsedPublicationEvent = new ParsedPublicationEvent();
		private JsonObjectSection parsedPublicationEventSection;
		private readonly Button sendParsedPublicationEventNotificationButton = new Button("Send Parsed PublicationEvent") { Width = 200 };

		public SendPlasmaIntegrationNotificationDialog(Utilities.Helpers helpers) : base(helpers)
		{
			Title = "Send Plasma Integration Notification";

			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			editorialObjectSection = new JsonObjectSection(editorialObject);
			editorialObjectSection.RegenerateUi += RegenerateUi;
			sendEditorialObjectNotificationButton.Pressed += SendEditorialObjectNotificationButton_Pressed;
			editorialObjectCollapseButton = new YleCollapseButton(editorialObjectSection.Widgets.Concat(sendEditorialObjectNotificationButton.Yield()), true);

			publicationEventSection = new JsonObjectSection(publicationEvent);
			publicationEventSection.RegenerateUi += RegenerateUi;
			sendPublicationEventNotificationButton.Pressed += SendPublicationEventNotificationButton_Pressed;
			publicationEventCollapseButton = new YleCollapseButton(publicationEventSection.Widgets.Concat(sendPublicationEventNotificationButton.Yield()), true);

			parsedProgramSection = new JsonObjectSection(parsedProgram);
			parsedProgramSection.RegenerateUi += RegenerateUi;
			sendParsedProgramNotificationButton.Pressed += SendParsedProgramNotificationButton_Pressed;
			parsedProgramCollapseButton = new YleCollapseButton(parsedProgramSection.Widgets.Concat(sendParsedProgramNotificationButton.Yield()), true);

			parsedPublicationEventSection = new JsonObjectSection(parsedPublicationEvent);
			parsedPublicationEventSection.RegenerateUi += RegenerateUi;
			sendParsedPublicationEventNotificationButton.Pressed += SendParsedPublicationEventNotificationButton_Pressed;
			parsedPublicationEventCollapseButton = new YleCollapseButton(parsedPublicationEventSection.Widgets.Concat(sendParsedPublicationEventNotificationButton.Yield()), true);
		}

		private void SendParsedProgramNotificationButton_Pressed(object sender, EventArgs e)
		{
			parsedProgramSection.UpdateJsonObjectWithUiValues();

			helpers.Log(nameof(SendPlasmaIntegrationNotificationDialog), nameof(SendParsedProgramNotificationButton_Pressed), $"Parsed program: {JsonConvert.SerializeObject(parsedProgram)}");

			string notification = parsedProgram.ToNotification();

			DataMinerInterface.IDmsElement.SetParameter(helpers, helpers.OrderManagerElement.PlasmaElement, MediagenixWhatsOnProtocol.RabbitMqMessagePid, notification);

			ShowRequestResult($"Sent editorial object notification", notification);
			GenerateUi();
		}

		private void SendPublicationEventNotificationButton_Pressed(object sender, EventArgs e)
		{
			publicationEventSection.UpdateJsonObjectWithUiValues();

			string publicationEventNotification = publicationEvent.ToNotification();

			DataMinerInterface.IDmsElement.SetParameter(helpers, helpers.OrderManagerElement.PlasmaElement, MediagenixWhatsOnProtocol.RabbitMqMessagePid, publicationEventNotification);

			ShowRequestResult($"Sent publication event notification", publicationEventNotification);
			GenerateUi();
		}

		private void SendParsedPublicationEventNotificationButton_Pressed(object sender, EventArgs e)
		{
			parsedPublicationEventSection.UpdateJsonObjectWithUiValues();

			helpers.Log(nameof(SendPlasmaIntegrationNotificationDialog), nameof(SendParsedPublicationEventNotificationButton_Pressed), $"Parsed publication event: {JsonConvert.SerializeObject(parsedPublicationEvent)}");

			string publicationEventNotification = parsedPublicationEvent.ToNotification();

			DataMinerInterface.IDmsElement.SetParameter(helpers, helpers.OrderManagerElement.PlasmaElement, MediagenixWhatsOnProtocol.RabbitMqMessagePid, publicationEventNotification);

			ShowRequestResult($"Sent publication event notification", publicationEventNotification);
			GenerateUi();
		}

		private void RegenerateUi(object sender, EventArgs e)
		{
			GenerateUi();
		}

		private void SendEditorialObjectNotificationButton_Pressed(object sender, EventArgs e)
		{
			editorialObjectSection.UpdateJsonObjectWithUiValues();

			string programNotification = editorialObject.ToNotification();

			DataMinerInterface.IDmsElement.SetParameter(helpers, helpers.OrderManagerElement.PlasmaElement, MediagenixWhatsOnProtocol.RabbitMqMessagePid, programNotification);

			ShowRequestResult($"Sent editorial object notification", programNotification);
			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 5);

			AddWidget(parsedProgramCollapseButton, ++row, 0);
			AddWidget(parsedProgramHeaderLabel, row, 1, 1, 5);
			AddSection(parsedProgramSection, new SectionLayout(++row, 0));
			row += parsedProgramSection.RowCount;
			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(sendParsedProgramNotificationButton, ++row, 0, 1, 5);

			AddWidget(editorialObjectCollapseButton, ++row, 0);
			AddWidget(editorialObjectHeaderLabel, row, 1, 1, 5);
			AddSection(editorialObjectSection, new SectionLayout(++row, 0));
			row += editorialObjectSection.RowCount;
			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(sendEditorialObjectNotificationButton, ++row, 0, 1, 5);

			AddWidget(parsedPublicationEventCollapseButton, ++row, 0);
			AddWidget(parsedPublicationEventHeaderLabel, row, 1, 1, 5);
			AddSection(parsedPublicationEventSection, new SectionLayout(++row, 0));
			row += parsedPublicationEventSection.RowCount;
			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(sendParsedPublicationEventNotificationButton, ++row, 0, 1, 5);

			AddWidget(publicationEventCollapseButton, ++row, 0);
			AddWidget(publicationEventHeaderLabel, row, 1, 1, 5);
			AddSection(publicationEventSection, new SectionLayout(++row, 0));
			row += publicationEventSection.RowCount;
			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(sendPublicationEventNotificationButton, ++row, 0, 1, 5);

			AddResponseSections(++row);

			foreach (var yleWidget in Widgets.OfType<IYleWidget>())
			{
				// To enable logging of user input
				yleWidget.Helpers = helpers;
			}
		}
	}
}
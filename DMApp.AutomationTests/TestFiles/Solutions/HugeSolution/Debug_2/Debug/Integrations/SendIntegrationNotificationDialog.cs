namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations
{
	using System;
	using Debug_2.Debug.Integrations;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class SendIntegrationNotificationDialog : Dialog
	{
		public SendIntegrationNotificationDialog(Utilities.Helpers helpers) : base(helpers.Engine)
		{
			Title = "Send Integration Notification";

			Initialize(helpers);
			GenerateUi();
		}

		private void Initialize(Utilities.Helpers helpers)
		{
			FeenixNotificationDialog = new SendFeenixIntegrationNotificationDialog(helpers);
			CeitonNotificationDialog = new SendCeitonIntegrationNotificationDialog(helpers);
			PlasmaNotificationDialog = new SendPlasmaIntegrationNotificationDialog(helpers);
			PlasmaNotificationOldDialog = new SendPlasmaIntegrationNotificationOldDialog(helpers);
			EurovisionNotificationDialog = new SendEurovisionIntegrationNotificationDialog(helpers);
			PbsNotificationDialog = new SendPbsIntegrationNotificationDialog(helpers);

			try
			{
				EvsNotificationDialog = new SendEvsNotificationDialog(helpers);
			}
			catch(Exception e)
			{
				helpers.Log(nameof(SendIntegrationNotificationDialog), nameof(Initialize), $"Exception occurred: {e}");
				EvsButton.IsEnabled = false;
			}
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(FeenixButton, ++row, 0);
			AddWidget(PlasmaButton, ++row, 0);
			AddWidget(CeitonButton, ++row, 0);
			AddWidget(EurovisionButton, ++row, 0);
			AddWidget(PebbleBeachButton, ++row, 0);
			AddWidget(EvsButton, ++row, 0);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(PlasmaOldButton, ++row, 0);
		}

		public Button FeenixButton { get; } = new Button("Feenix...") { Width = 150 };

		public Button PlasmaButton { get; } = new Button("Plasma...") { Width = 150 };

		public Button PlasmaOldButton { get; } = new Button("Plasma Old...") { Width = 150 };

		public Button CeitonButton { get; } = new Button("Ceiton...") { Width = 150 };

		public Button EurovisionButton { get; } = new Button("Eurovision...") { Width = 150 };

		public Button PebbleBeachButton { get; } = new Button("Pebble Beach...") { Width = 150 };

		public Button EvsButton { get; } = new Button("EVS...") { Width = 150 };

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		public SendFeenixIntegrationNotificationDialog FeenixNotificationDialog { get; private set; }

		public SendCeitonIntegrationNotificationDialog CeitonNotificationDialog { get; private set; }

		public SendEurovisionIntegrationNotificationDialog EurovisionNotificationDialog { get; private set; }

		public SendPlasmaIntegrationNotificationDialog PlasmaNotificationDialog { get; private set; }

		public SendPlasmaIntegrationNotificationOldDialog PlasmaNotificationOldDialog { get; private set; }

		public SendEvsNotificationDialog EvsNotificationDialog { get; private set; }

		public SendPbsIntegrationNotificationDialog PbsNotificationDialog { get; private set; }
	}
}

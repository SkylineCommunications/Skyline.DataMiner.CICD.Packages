namespace ShowPlasmaDetails_2.Sections
{
	using System;
	using Plasma;
	using ShowPlasmaDetails_2.Plasma.Enums;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.ParsedObjects;

	public class PlasmaTransmissionGeneralSection : Section
	{
		private readonly Label titleLabel = new Label("GENERAL") {Style = TextStyle.Bold};
		private readonly Label liveLabel = new Label("TRANSMISSION LIVE INDICATOR");
		private readonly Label startLabel = new Label("TRANSMISSION START");
		private readonly Label linearBroadcastEndLabel = new Label("LINEAR BROADCAST END");
		private readonly Label originalTxLabel = new Label("ORIGINAL TX");
		private readonly Label channelLabel = new Label("CHANNEL");
		private readonly Label typeLabel = new Label("TYPE");
		private readonly Label sourceLabel = new Label("SOURCE");

		private readonly Label liveValue;
		private readonly Label startValue;
		private readonly Label linearBroadcastEndValue;
		private readonly Label originalTxValue;
		private readonly Label channelValue;
		private readonly Label typeValue;
		private readonly Label sourceValue;

		public PlasmaTransmissionGeneralSection(IEngine engine, ParsedPublicationEvent plasmaPublicationEvent)
		{
			liveValue = new UIDetailValueLabel(((Live)plasmaPublicationEvent.Live).GetDescription());
			startValue = new UIDetailValueLabel(plasmaPublicationEvent.Start.ToString());
			linearBroadcastEndValue = new UIDetailValueLabel(plasmaPublicationEvent.End.ToString());
			originalTxValue = new UIDetailValueLabel(plasmaPublicationEvent.OriginalTransmission);
			channelValue = new UIDetailValueLabel(plasmaPublicationEvent.Channel);
			typeValue = new UIDetailValueLabel(plasmaPublicationEvent.Type);
			sourceValue = new UIDetailValueLabel(plasmaPublicationEvent.Source);

			GenerateUI();
		}

		private void GenerateUI()
		{
			var row = 0;
			AddWidget(titleLabel, new WidgetLayout(row, 0, 1, 5));

			AddWidget(liveLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(liveValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(startLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(startValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(linearBroadcastEndLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(linearBroadcastEndValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(originalTxLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(originalTxValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(channelLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(channelValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(typeLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(typeValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(sourceLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(sourceValue, new WidgetLayout(row, 5, 1, 1));
		}
	}
}
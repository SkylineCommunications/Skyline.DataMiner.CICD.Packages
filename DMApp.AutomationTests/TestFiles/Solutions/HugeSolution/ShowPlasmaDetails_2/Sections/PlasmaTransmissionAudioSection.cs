namespace ShowPlasmaDetails_2.Sections
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Plasma;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.ParsedObjects;

	public class PlasmaTransmissionAudioSection : Section
	{
		private readonly Widget[][] transmissionAudioLabelsArr;

		private readonly CollapseButton collapseButton;
		private readonly Label titleLabel = new Label("Audio details") {Style = TextStyle.Bold};

		private readonly Label channelNumberLabel = new Label("CHANNEL NUMBER") {Style = TextStyle.Heading};
		private readonly Label audioTypeLabel = new Label("AUDIO TYPE") {Style = TextStyle.Heading};
		private readonly Label languageLabel = new Label("LANGUAGE") {Style = TextStyle.Heading};

		public PlasmaTransmissionAudioSection(IEngine engine, List<ParsedAudioResource> plasmaAudioResources)
		{
			if (plasmaAudioResources.Any())
			{
				IList<Widget> tableContentWidgetList = new List<Widget>();
				transmissionAudioLabelsArr = new Widget[plasmaAudioResources.Count][];
				var counter = 0;
				foreach (var plasmaAudioResource in plasmaAudioResources)
				{
					Label channelNumberValueLabel = new UIDetailValueLabel(Convert.ToString(plasmaAudioResource.ChannelNumber));
					Label audioTypeValueLabel = new UIDetailValueLabel(plasmaAudioResource.AudioType);
					Label languageValueLabel = new UIDetailValueLabel(plasmaAudioResource.Language);

					tableContentWidgetList.Add(channelNumberValueLabel);
					tableContentWidgetList.Add(audioTypeValueLabel);
					tableContentWidgetList.Add(languageValueLabel);

					Widget[] labelArr = new[] {channelNumberValueLabel, audioTypeValueLabel, languageValueLabel};
					transmissionAudioLabelsArr[counter++] = labelArr;
				}

				var widgetsToHide = Enumerable.Concat(new[] {channelNumberLabel, audioTypeLabel, languageLabel}, tableContentWidgetList);

				collapseButton = new CollapseButton(widgetsToHide, true) {CollapseText = "-", ExpandText = "+", Width = 44};
				GenerateUI();
			}
		}

		private void GenerateUI()
		{
			var row = 0;
			AddWidget(collapseButton, new WidgetLayout(row, 0));
			AddWidget(titleLabel, new WidgetLayout(row, 1, 1, 5));

			AddWidget(channelNumberLabel, new WidgetLayout(++row, 1, 1, 2));
			AddWidget(audioTypeLabel, new WidgetLayout(row, 3, 1, 1));
			AddWidget(languageLabel, new WidgetLayout(row, 4, 1, 1));

			foreach (var w in transmissionAudioLabelsArr)
			{
				AddWidget(w[0], new WidgetLayout(++row, 1, 1, 2));
				AddWidget(w[1], new WidgetLayout(row, 3, 1, 1));
				AddWidget(w[2], new WidgetLayout(row, 4, 1, 1));
			}
		}
	}
}
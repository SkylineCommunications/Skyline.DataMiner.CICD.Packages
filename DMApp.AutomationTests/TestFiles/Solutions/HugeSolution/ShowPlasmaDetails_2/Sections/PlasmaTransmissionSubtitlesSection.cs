namespace ShowPlasmaDetails_2.Sections
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Plasma;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.ParsedObjects;

	public class PlasmaTransmissionSubtitlesSection : Section
	{
		private readonly Widget[][] transmissionSubsLabelsArr;
		private readonly CollapseButton collapseButton;
		private readonly Label titleLabel = new Label("Subtitle details") {Style = TextStyle.Bold};

		private readonly Label languageLabel = new Label("LANGUAGE") {Style = TextStyle.Heading};
		private readonly Label mediaIDLabel = new Label("MEDIA IDENTIFIER") {Style = TextStyle.Heading};
		private readonly Label typeLabel = new Label("TYPE") {Style = TextStyle.Heading};
		private readonly Label codeLabel = new Label("CODE") {Style = TextStyle.Heading};
		private readonly Label subtitlingModeResponsibleLabel = new Label("SUBTITLING MODE RESPONSIBLE") {Style = TextStyle.Heading};
		private readonly Label subtitlingCodeResponsibleLabel = new Label("SUBTITLING CODE RESPONSIBLE") {Style = TextStyle.Heading};

		public PlasmaTransmissionSubtitlesSection(IEngine engine, List<ParsedSubtitleResource> plasmaSubtitleResources)
		{
			if (plasmaSubtitleResources.Any())
			{
				IList<Widget> tableContentWidgetList = new List<Widget>();
				transmissionSubsLabelsArr = new Widget[plasmaSubtitleResources.Count][];

				var counter = 0;
				foreach (var plasmaSubtitleResource in plasmaSubtitleResources)
				{
					Label languageValueLabel = new UIDetailValueLabel(Convert.ToString(plasmaSubtitleResource.Language));
					Label mediaIdValueLabel = new UIDetailValueLabel(plasmaSubtitleResource.MediaId);
					Label typeValueLabel = new UIDetailValueLabel(plasmaSubtitleResource.Type);
					Label codeValueLabel = new UIDetailValueLabel(plasmaSubtitleResource.Code);
					Label subtitlingModeResponsibleValueLabel = new UIDetailValueLabel(plasmaSubtitleResource.SubtitlingModeResponsible);
					Label subtitlingCodeResonsibleValueLabel = new UIDetailValueLabel(plasmaSubtitleResource.SubtitlingCodeResponsible);


					tableContentWidgetList.Add(languageValueLabel);
					tableContentWidgetList.Add(mediaIdValueLabel);
					tableContentWidgetList.Add(typeValueLabel);
					tableContentWidgetList.Add(codeValueLabel);
					tableContentWidgetList.Add(subtitlingModeResponsibleValueLabel);
					tableContentWidgetList.Add(subtitlingCodeResonsibleValueLabel);

					Widget[] labelArr = new[] {languageValueLabel, mediaIdValueLabel, typeValueLabel, codeValueLabel, subtitlingModeResponsibleValueLabel, subtitlingCodeResonsibleValueLabel};
					transmissionSubsLabelsArr[counter++] = labelArr;
				}

				var widgetsToHide = Enumerable.Concat(new[] {languageLabel, mediaIDLabel, typeLabel, codeLabel, subtitlingCodeResponsibleLabel, subtitlingModeResponsibleLabel}, tableContentWidgetList);
				collapseButton = new CollapseButton(widgetsToHide, true) {CollapseText = "-", ExpandText = "+", Width = 44};

				GenerateUI();
			}
		}

		private void GenerateUI()
		{
			var row = 0;

			AddWidget(collapseButton, new WidgetLayout(row, 0));
			AddWidget(titleLabel, new WidgetLayout(row, 1, 1, 5));

			AddWidget(languageLabel, new WidgetLayout(++row, 1, 1, 1));
			AddWidget(mediaIDLabel, new WidgetLayout(row, 2, 1, 1));
			AddWidget(typeLabel, new WidgetLayout(row, 3, 1, 1));
			AddWidget(codeLabel, new WidgetLayout(row, 4, 1, 1));
			AddWidget(subtitlingModeResponsibleLabel, new WidgetLayout(row, 5, 1, 2));
			AddWidget(subtitlingCodeResponsibleLabel, new WidgetLayout(row, 7, 1, 1));

			foreach (var w in transmissionSubsLabelsArr)
			{
				row += 1;

				AddWidget(w[0], new WidgetLayout(row, 1, 1, 1));
				AddWidget(w[1], new WidgetLayout(row, 2, 1, 1));
				AddWidget(w[2], new WidgetLayout(row, 3, 1, 1));
				AddWidget(w[3], new WidgetLayout(row, 4, 1, 1));
				AddWidget(w[4], new WidgetLayout(row, 5, 1, 2));
				AddWidget(w[5], new WidgetLayout(row, 7, 1, 1));
			}
		}
	}
}
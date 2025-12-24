namespace ShowFeenixDetails_2.Sections
{
	using Feenix;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class VersionSection : Section
	{
		private readonly CollapseButton versionCollapseButton;
		private readonly Label versionLabel = new Label("VERSION") { Style = TextStyle.Bold };

		private readonly CollapseButton mediaResourceCollapseButton;
		private readonly Label mediaResourceLabel = new Label("MEDIA RESOURCE") { Style = TextStyle.Bold };

		private readonly Label sourceNameLabel = new Label("SOURCE NAME");
		private readonly Label sourceTypeLabel = new Label("SOURCE TYPE");
		private readonly Label sourceLabelLabel = new Label("SOURCE LABEL");
		private readonly Label contactNameLabel = new Label("CONTACT NAME");
		private readonly Label formatModeLabel = new Label("FORMAT MODE");
		private readonly Label formatFramerateLabel = new Label("FORMAT FRAMERATE");
		private readonly Label trackDurationLabel = new Label("TRACK DURATION");
		private readonly Label languageLabel = new Label("LANGUAGE");
		private readonly Label versionMediaResourceID = new Label("MEDIA RESOURCE ID");

		private readonly Label sourceNameValue;
		private readonly Label sourceTypeValue;
		private readonly Label sourceLabelValue;
		private readonly Label contactNameValue;
		private readonly Label formatModeValue;
		private readonly Label formatFramerateValue;
		private readonly Label trackDurationValue;
		private readonly Label languageValue;
		private readonly Label versionMediaResourceIDValue;

		private readonly CollapseButton publicationEventCollapseButton;
		private readonly Label publicationEventLabel = new Label("PUBLICATION EVENT") { Style = TextStyle.Bold };

		private readonly Label serviceIDLabel = new Label("SERVICE ID");
		private readonly Label serviceKindLabel = new Label("SERVICE KIND");
		private readonly Label publisherIDLabel = new Label("PUBLISHER ID");
		private readonly Label regionLabel = new Label("REGION");
		private readonly Label startTimeLabel = new Label("START TIME");
		private readonly Label endTimeLabel = new Label("END TIME");

		private readonly Label serviceIDValue;
		private readonly Label serviceKindValue;
		private readonly Label publisherIDValue;
		private readonly Label regionValue;
		private readonly Label startTimeValue;
		private readonly Label endTimeValue;

		public VersionSection(OrderVersion orderVersion)
		{
			sourceNameValue = new UIDetailValueLabel(orderVersion.VersionMediaResourceHasSourceName);
			sourceTypeValue = new UIDetailValueLabel(orderVersion.VersionMediaResourceHasSourceType);
			sourceLabelValue = new UIDetailValueLabel(orderVersion.VersionMediaResourceHasSourceLabel);
			contactNameValue = new UIDetailValueLabel(orderVersion.VersionMediaResourceContactName);
			formatModeValue = new UIDetailValueLabel(orderVersion.VersionMediaResourceFormatMode);
			formatFramerateValue = new UIDetailValueLabel(orderVersion.VersionMediaResourceFormatFrameRate);
			trackDurationValue = new UIDetailValueLabel(orderVersion.VersionPublicationEventDuration);
			languageValue = new UIDetailValueLabel(orderVersion.VersionMediaResourceLanguage);
			versionMediaResourceIDValue = new UIDetailValueLabel(orderVersion.VersionMediaResourceID);

			serviceIDValue = new UIDetailValueLabel(orderVersion.VersionPublicationEventServiceID);
			serviceKindValue = new UIDetailValueLabel(orderVersion.VersionPublicationEventServiceKind);
			publisherIDValue = new UIDetailValueLabel(orderVersion.VersionPublicationEventPublisherID);
			regionValue = new UIDetailValueLabel(orderVersion.VersionPublicationEventRegion);
			startTimeValue = new UIDetailValueLabel(orderVersion.VersionPublicationEventStartTime.HasValue ? orderVersion.VersionPublicationEventStartTime.ToString() : "Not available");
			endTimeValue = new UIDetailValueLabel(orderVersion.VersionPublicationEventEndTime.HasValue ? orderVersion.VersionPublicationEventEndTime.ToString() : "Not available");

			this.mediaResourceCollapseButton = new CollapseButton(new Widget[] { versionMediaResourceID, versionMediaResourceIDValue, sourceNameLabel, sourceNameValue, sourceTypeLabel, sourceTypeValue, sourceLabelLabel, sourceLabelValue, contactNameLabel, contactNameValue, formatModeLabel, formatModeValue, formatFramerateLabel, formatFramerateValue, trackDurationLabel, trackDurationValue, languageLabel, languageValue }, isCollapsed: true) { CollapseText = "-", ExpandText = "+", Width = 44 };
			this.publicationEventCollapseButton = new CollapseButton(new Widget[] { serviceIDLabel, serviceIDValue, serviceKindLabel, serviceKindValue, publisherIDLabel, publisherIDValue, regionLabel, regionValue, startTimeLabel, startTimeValue, endTimeLabel, endTimeValue }, isCollapsed: true) { CollapseText = "-", ExpandText = "+", Width = 44 };

			this.versionCollapseButton = new CollapseButton(new Widget[] { mediaResourceCollapseButton, mediaResourceLabel, publicationEventCollapseButton, publicationEventLabel }, isCollapsed: true) { CollapseText = "-", ExpandText = "+", Width = 44 };

			GenerateUI();
		}

		private void GenerateUI()
		{
			int row = 0;

			AddWidget(versionCollapseButton, new WidgetLayout(row, 0, 1, 1));
			AddWidget(versionLabel, new WidgetLayout(row, 1, 1, 4));

			AddWidget(mediaResourceCollapseButton, new WidgetLayout(++row, 1, 1, 1));
			AddWidget(mediaResourceLabel, new WidgetLayout(row, 2, 1, 3));

			AddWidget(versionMediaResourceID, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(versionMediaResourceIDValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(sourceNameLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(sourceNameValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(sourceTypeLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(sourceTypeValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(sourceLabelLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(sourceLabelValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(contactNameLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(contactNameValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(formatModeLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(formatModeValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(formatFramerateLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(formatFramerateValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(trackDurationLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(trackDurationValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(languageLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(languageValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(publicationEventCollapseButton, new WidgetLayout(++row, 1, 1, 1));
			AddWidget(publicationEventLabel, new WidgetLayout(row, 2, 1, 3));

			AddWidget(serviceIDLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(serviceIDValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(serviceKindLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(serviceKindValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(publisherIDLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(publisherIDValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(regionLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(regionValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(startTimeLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(startTimeValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(endTimeLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(endTimeValue, new WidgetLayout(row, 5, 1, 2));
		}
	}

}
namespace ShowFeenixDetails_2.Sections
{
	using Feenix;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class MetadataSection : Section
	{
		private readonly CollapseButton metadataSectionCollapseButton;
		private readonly Label metadataSectionLabel = new Label("METADATA") { Style = TextStyle.Bold };

		private readonly CollapseButton relationCollapseButton;
		private readonly Label relationLabel = new Label("RELATION") { Style = TextStyle.Bold };

		private readonly CollapseButton mainClassButton;
		private readonly Label mainClassLabel = new Label("MAIN CLASS") { Style = TextStyle.Bold };

		private readonly Label mainClassIDLabel = new Label("MAIN CLASS ID");
		private readonly Label mainClassFinnishTitleLabel = new Label("FINNISH TITLE");
		private readonly Label mainClassSwedishTitleLabel = new Label("SWEDISH TITLE");

		private readonly Label mainClassIDValue;
		private readonly Label mainClassFinnishTitleValue;
		private readonly Label mainClassSwedishTitleValue;

		private readonly CollapseButton subClassButton;
		private readonly Label subClassLabel = new Label("SUB CLASS") { Style = TextStyle.Bold };

		private readonly Label subClassIDLabel = new Label("SUB CLASS ID");
		private readonly Label subClassFinnishTitleLabel = new Label("FINNISH TITLE");
		private readonly Label subClassSwedishTitleLabel = new Label("SWEDISH TITLE");

		private readonly Label subClassIDValue;
		private readonly Label subClassFinnishTitleValue;
		private readonly Label subClassSwedishTitleValue;

		private readonly CollapseButton contentClassButton;
		private readonly Label contentClassLabel = new Label("CONTENT CLASS") { Style = TextStyle.Bold };

		private readonly Label contentClassIDLabel = new Label("CONTENT CLASS ID");
		private readonly Label contentClassFinnishTitleLabel = new Label("FINNISH TITLE");
		private readonly Label contentClassSwedishTitleLabel = new Label("SWEDISH TITLE");

		private readonly Label contentClassIDValue;
		private readonly Label contentClassFinnishTitleValue;
		private readonly Label contentClassSwedishTitleValue;

		private readonly CollapseButton reportingClassButton;
		private readonly Label reportingClassLabel = new Label("REPORTING CLASS") { Style = TextStyle.Bold };

		private readonly Label reportingClassIDLabel = new Label("REPORTING CLASS ID");
		private readonly Label reportingClassFinnishTitleLabel = new Label("FINNISH TITLE");
		private readonly Label reportingClassSwedishTitleLabel = new Label("SWEDISH TITLE");

		private readonly Label reportingClassIDValue;
		private readonly Label reportingClassFinnishTitleValue;
		private readonly Label reportingClassSwedishTitleValue;

		private readonly CollapseButton modificationCollapseButton;
		private readonly Label modificationLabel = new Label("MODIFICATION") { Style = TextStyle.Bold };

		private readonly Label createdLabel = new Label("CREATED");
		private readonly Label modifiedLabel = new Label("MODIFIED");
		private readonly Label deletedLabel = new Label("DELETED");

		private readonly Label createdValue;
		private readonly Label modifiedValue;
		private readonly Label deletedValue;

		public MetadataSection(OrderMetadata orderMetadata)
		{
			mainClassIDValue = new UIDetailValueLabel(orderMetadata.MetaDataRelationMainClassId);
			mainClassFinnishTitleValue = new UIDetailValueLabel(orderMetadata.MetaDataRelationMainClassFinnishTitle);
			mainClassSwedishTitleValue = new UIDetailValueLabel(orderMetadata.MetaDataRelationMainClassSwedishTitle);

			subClassIDValue = new UIDetailValueLabel(orderMetadata.MetaDataRelationSubClassId);
			subClassFinnishTitleValue = new UIDetailValueLabel(orderMetadata.MetaDataRelationSubClassFinnishTitle);
			subClassSwedishTitleValue = new UIDetailValueLabel(orderMetadata.MetaDataRelationSubClassSwedishTitle);

			contentClassIDValue = new UIDetailValueLabel(orderMetadata.MetaDataRelationContentClassId);
			contentClassFinnishTitleValue = new UIDetailValueLabel(orderMetadata.MetaDataRelationContentClassFinnishTitle);
			contentClassSwedishTitleValue = new UIDetailValueLabel(orderMetadata.MetaDataRelationContentClassSwedishTitle);

			reportingClassIDValue = new UIDetailValueLabel(orderMetadata.MetaDataRelationReportingClassId);
			reportingClassFinnishTitleValue = new UIDetailValueLabel(orderMetadata.MetaDataRelationReportingClassFinnishTitle);
			reportingClassSwedishTitleValue = new UIDetailValueLabel(orderMetadata.MetaDataRelationReportingClassSwedishTitle);

			mainClassButton = new CollapseButton(new Widget[] { mainClassIDLabel, mainClassIDValue, mainClassFinnishTitleLabel, mainClassFinnishTitleValue, mainClassSwedishTitleLabel, mainClassSwedishTitleValue }, isCollapsed: true) { CollapseText = "-", ExpandText = "+", Width = 44 };
			subClassButton = new CollapseButton(new Widget[] { subClassIDLabel, subClassIDValue, subClassFinnishTitleLabel, subClassFinnishTitleValue, subClassSwedishTitleLabel, subClassSwedishTitleValue }, isCollapsed: true) { CollapseText = "-", ExpandText = "+", Width = 44 };
			contentClassButton = new CollapseButton(new Widget[] { contentClassIDLabel, contentClassIDValue, contentClassFinnishTitleLabel, contentClassFinnishTitleValue, contentClassSwedishTitleLabel, contentClassSwedishTitleValue }, isCollapsed: true) { CollapseText = "-", ExpandText = "+", Width = 44 };
			reportingClassButton = new CollapseButton(new Widget[] { reportingClassIDLabel, reportingClassIDValue, reportingClassFinnishTitleLabel, reportingClassFinnishTitleValue, reportingClassSwedishTitleLabel, reportingClassSwedishTitleValue }, isCollapsed: true) { CollapseText = "-", ExpandText = "+", Width = 44 };
			relationCollapseButton = new CollapseButton(new Widget[] { mainClassButton, mainClassLabel, subClassButton, subClassLabel, contentClassButton, contentClassLabel, reportingClassButton, reportingClassLabel }, isCollapsed: true) { CollapseText = "-", ExpandText = "+", Width = 44 };

			createdValue = new Label(orderMetadata.MetadataModificationCreated.HasValue ? orderMetadata.MetadataModificationCreated.ToString() : "Not available");
			modifiedValue = new Label(orderMetadata.MetadataModificationModified.HasValue ? orderMetadata.MetadataModificationModified.ToString() : "Not available");
			deletedValue = new Label(orderMetadata.MetadataModificationDeleted.HasValue ? orderMetadata.MetadataModificationDeleted.ToString() : "Not available");
			modificationCollapseButton = new CollapseButton(new Widget[] { createdLabel, createdValue, modifiedLabel, modifiedValue, deletedLabel, deletedValue }, isCollapsed: true) { CollapseText = "-", ExpandText = "+", Width = 44 };

			metadataSectionCollapseButton = new CollapseButton(new Widget[] { relationCollapseButton, relationLabel, modificationCollapseButton, modificationLabel }, isCollapsed: true) { CollapseText = "-", ExpandText = "+", Width = 44 };

			GenerateUI();
		}

		private void GenerateUI()
		{
			int row = 0;
			AddWidget(metadataSectionCollapseButton, new WidgetLayout(row, 0, 1, 1));
			AddWidget(metadataSectionLabel, new WidgetLayout(row, 1, 1, 2));

			AddWidget(relationCollapseButton, new WidgetLayout(++row, 1, 1, 1));
			AddWidget(relationLabel, new WidgetLayout(row, 2, 1, 2));

			AddWidget(mainClassButton, new WidgetLayout(++row, 2, 1, 1));
			AddWidget(mainClassLabel, new WidgetLayout(row, 3, 1, 2));

			AddWidget(mainClassIDLabel, new WidgetLayout(++row, 3, 1, 2));
			AddWidget(mainClassIDValue, new WidgetLayout(row, 5, 1, 2));
			AddWidget(mainClassFinnishTitleLabel, new WidgetLayout(++row, 3, 1, 2));
			AddWidget(mainClassFinnishTitleValue, new WidgetLayout(row, 5, 1, 2));
			AddWidget(mainClassSwedishTitleLabel, new WidgetLayout(++row, 3, 1, 2));
			AddWidget(mainClassSwedishTitleValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(subClassButton, new WidgetLayout(++row, 2, 1, 1));
			AddWidget(subClassLabel, new WidgetLayout(row, 3, 1, 2));

			AddWidget(subClassIDLabel, new WidgetLayout(++row, 3, 1, 1));
			AddWidget(subClassIDValue, new WidgetLayout(row, 5, 1, 2));
			AddWidget(subClassFinnishTitleLabel, new WidgetLayout(++row, 3, 1, 2));
			AddWidget(subClassFinnishTitleValue, new WidgetLayout(row, 5, 1, 2));
			AddWidget(subClassSwedishTitleLabel, new WidgetLayout(++row, 3, 1, 2));
			AddWidget(subClassSwedishTitleValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(contentClassButton, new WidgetLayout(++row, 2, 1, 1));
			AddWidget(contentClassLabel, new WidgetLayout(row, 3, 1, 2));

			AddWidget(contentClassIDLabel, new WidgetLayout(++row, 3, 1, 2));
			AddWidget(contentClassIDValue, new WidgetLayout(row, 5, 1, 2));
			AddWidget(contentClassFinnishTitleLabel, new WidgetLayout(++row, 3, 1, 2));
			AddWidget(contentClassFinnishTitleValue, new WidgetLayout(row, 5, 1, 2));
			AddWidget(contentClassSwedishTitleLabel, new WidgetLayout(++row, 3, 1, 2));
			AddWidget(contentClassSwedishTitleValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(reportingClassButton, new WidgetLayout(++row, 2, 1, 1));
			AddWidget(reportingClassLabel, new WidgetLayout(row, 3, 1, 2));

			AddWidget(reportingClassIDLabel, new WidgetLayout(++row, 3, 1, 2));
			AddWidget(reportingClassIDValue, new WidgetLayout(row, 5, 1, 2));
			AddWidget(reportingClassFinnishTitleLabel, new WidgetLayout(++row, 3, 1, 2));
			AddWidget(reportingClassFinnishTitleValue, new WidgetLayout(row, 5, 1, 2));
			AddWidget(reportingClassSwedishTitleLabel, new WidgetLayout(++row, 3, 1, 2));
			AddWidget(reportingClassSwedishTitleValue, new WidgetLayout(row, 5, 1, 2));

			AddWidget(modificationCollapseButton, new WidgetLayout(++row, 1, 1, 1));
			AddWidget(modificationLabel, new WidgetLayout(row, 2, 1, 2));

			AddWidget(createdLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(createdValue, new WidgetLayout(row, 5, 1, 5));

			AddWidget(modifiedLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(modifiedValue, new WidgetLayout(row, 5, 1, 5));

			AddWidget(deletedLabel, new WidgetLayout(++row, 2, 1, 3));
			AddWidget(deletedValue, new WidgetLayout(row, 5, 1, 5));
		}
	}

}
namespace ShowPlasmaDetails_2.Sections
{
	using Plasma;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.ParsedObjects;

	public class PlasmaProgramCompanyAndPeopleSection : Section
	{
		private readonly CollapseButton companyAndPeopleSectionButton;
		private readonly Label titleLabel = new Label("COMPANY AND PEOPLE") {Style = TextStyle.Bold};

		private readonly Label contactFullNameLabel = new Label("CONTACT FULL NAME");
		private readonly Label contactAdNameLabel = new Label("CONTACT AD NAME");
		private readonly Label contactSubinformationLabel = new Label("CONTACT SUBINFORMATION");
		private readonly Label vendorCompanyLabel = new Label("VENDOR COMPANY");
		private readonly Label productionCompanyLabel = new Label("PRODUCTION COMPANY");
		private readonly Label programInternalDescriptionLabel = new Label("PROGRAM INTERNAL DESCRIPTION");
		private readonly Label remarksLabel = new Label("REMARKS");
		private readonly Label subtitlingCoordinatorLabel = new Label("SUBTITLING COORDINATOR");
		private readonly Label productionCoordinatorLabel = new Label("PRODUCTION COORDINATOR");
		private readonly Label assistantProducerLabel = new Label("ASSISTANT PRODUCER");
		private readonly Label localizationCoordinatorLabel = new Label("LOCALIZATION COORDINATOR");

		private readonly Label contactFullNameValue;
		private readonly Label contactAdNameValue;
		private readonly Label contactSubinformationValue;
		private readonly Label vendorCompanyValue;
		private readonly Label productionCompanyValue;
		private readonly Label programInternalDescriptionValue;
		private readonly Label remarksValue;
		private readonly Label subtitlingCoordinatorValue;
		private readonly Label productionCoordinatorValue;
		private readonly Label assistantProducerValue;
		private readonly Label localizationCoordinatorValue;

		public PlasmaProgramCompanyAndPeopleSection(Engine engine, ParsedPlasmaOrder plasmaOrder, int columnOffset = 0)
		{
			contactFullNameValue = new UIDetailValueLabel(plasmaOrder.Program.ContactPersonFullName);
			contactAdNameValue = new UIDetailValueLabel(plasmaOrder.Program.ContactPersonAdUserName);
			contactSubinformationValue = new UIDetailValueLabel(Constants.NotApplicable);
			vendorCompanyValue = new UIDetailValueLabel(plasmaOrder.Program.VendorCompany);
			productionCompanyValue = new UIDetailValueLabel(plasmaOrder.Program.ProductionCompany);
			programInternalDescriptionValue = new UIDetailValueLabel(plasmaOrder.Program.InternalDescription);
			remarksValue = new UIDetailValueLabel(plasmaOrder.Program.Remarks);
			subtitlingCoordinatorValue = new UIDetailValueLabel(plasmaOrder.Program.SubtitlingCoordinator);
			productionCoordinatorValue = new UIDetailValueLabel(Constants.NotApplicable);
			assistantProducerValue = new UIDetailValueLabel(Constants.NotApplicable);
			localizationCoordinatorValue = new UIDetailValueLabel(plasmaOrder.Program.LocalizationCoordinator);

			companyAndPeopleSectionButton = new CollapseButton(new[] {contactFullNameLabel, contactAdNameLabel, contactSubinformationLabel, vendorCompanyLabel, productionCompanyLabel, programInternalDescriptionLabel, remarksLabel, subtitlingCoordinatorLabel, productionCoordinatorLabel, assistantProducerLabel, localizationCoordinatorLabel, contactFullNameValue, contactAdNameValue, contactSubinformationValue, vendorCompanyValue, productionCompanyValue, programInternalDescriptionValue, remarksValue, subtitlingCoordinatorValue, productionCoordinatorValue, assistantProducerValue, localizationCoordinatorValue}, true) {CollapseText = "-", ExpandText = "+", Width = 44};

			GenerateUI(columnOffset);
		}

		private void GenerateUI(int columnOffset)
		{
			var row = 0;

			AddWidget(companyAndPeopleSectionButton, new WidgetLayout(row, 0));
			AddWidget(titleLabel, new WidgetLayout(row, 1, 1, 5 + columnOffset));

			AddWidget(contactFullNameLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(contactFullNameValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(contactAdNameLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(contactAdNameValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(contactSubinformationLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(contactSubinformationValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(vendorCompanyLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(vendorCompanyValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(productionCompanyLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(productionCompanyValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(programInternalDescriptionLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(programInternalDescriptionValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(remarksLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(remarksValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(subtitlingCoordinatorLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(subtitlingCoordinatorValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(productionCoordinatorLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(productionCoordinatorValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(assistantProducerLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(assistantProducerValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(localizationCoordinatorLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(localizationCoordinatorValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));
		}

		public Label Title => titleLabel;

		public CollapseButton CollapseButton => companyAndPeopleSectionButton;
	}
}
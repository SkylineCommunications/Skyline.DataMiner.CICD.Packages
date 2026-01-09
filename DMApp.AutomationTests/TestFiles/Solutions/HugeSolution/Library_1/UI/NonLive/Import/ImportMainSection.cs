namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.Import
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Library_1.UI.NonLive.Import;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ImportMainSection : MainSection
	{
		private readonly Label materialIngestRequestsTitleLabel = new Label("Material Import Details") { Style = TextStyle.Bold };
		private readonly Button materialIngestRequestAddButton = new Button("New material Import request");

		private readonly Label additionalInformationTitleLabel = new Label("Additional Information") { Style = TextStyle.Bold };
		private readonly Label additionalCustomerInfoLabel = new Label("Additional customer information");
		private readonly YleTextBox additionalCustomerInfoTextBox = new YleTextBox { IsMultiline = true, Height = 200, PlaceHolder = "Please provide additional information for the whole order: notes to media operator, names for the material folders, sorting of clips, etc." };

		private readonly Label ingestDestinationLabel = new Label("Import destination");
		private readonly DropDown ingestDestinationDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<InterplayPamElements>().OrderBy(x => x), InterplayPamElements.Helsinki.GetDescription());

		private readonly NotificationSection notificationSection;

		private readonly Label isilonBackupFileLocationLabel = new Label("Isilon Backup File Location");
		private readonly YleTextBox isilonBackupFileLocationTextBox = new YleTextBox { IsMultiline = true, Height = 50 };
		private readonly ImportSectionConfiguration configuration;

		public ImportMainSection(Helpers helpers, Ingest ingest, ImportSectionConfiguration configuration) : base(helpers)
        {
			this.configuration = configuration;

			notificationSection = new NotificationSection(helpers, configuration, ingest);
            Initialize(helpers, ingest);

            materialIngestRequestAddButton.Pressed += MaterialIngestRequestAddButton_Pressed;
            ingestDestinationDropDown.Changed += IngestDestinationDropDown_Changed;
            ingestDestinationDropDown.Changed += IngestDestinationSection.IngestDestinationDropDown_Changed;

			GenerateUi(out int row);
		}

		public ImportGeneralInfoSection GeneralInfoSection { get; private set; }

		public ImportDestinationSection IngestDestinationSection { get; private set; }

		public List<MaterialImportRequestSection> MaterialIngestRequestSections { get; private set; }

		public MaterialDestinationDetailsSection MaterialDestinationDetailsSection { get; private set; }

		public MaterialBackupDetailsSection MaterialBackupDetailsSection { get; private set; }

		public InterplayPamElements IngestDestination { get => EnumExtensions.GetEnumValueFromDescription<InterplayPamElements>(IngestDestinationDropDown.Selected); }

		public DropDown IngestDestinationDropDown => ingestDestinationDropDown;

		public string IngestDestinationSelection
		{
			get
			{
				return IngestDestinationDropDown.Selected;
			}
			set
			{
				IngestDestinationDropDown.Selected = value;
			}
		}

		public string IsilonBackupFileLocation
		{
			get => isilonBackupFileLocationTextBox.Text;
			private set => isilonBackupFileLocationTextBox.Text = value;
		}

		public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections
		{
			get 
			{
				List<NonLiveManagerTreeViewSection> treeViewSections = new List<NonLiveManagerTreeViewSection>();
				treeViewSections.AddRange(IngestDestinationSection.TreeViewSections);
				foreach(var subSection in MaterialIngestRequestSections) treeViewSections.AddRange(subSection.TreeViewSections);
				treeViewSections.AddRange(MaterialDestinationDetailsSection.TreeViewSections);
				treeViewSections.AddRange(MaterialBackupDetailsSection.TreeViewSections);
				return treeViewSections;
			} 
		}

		public override bool IsValid(OrderAction action)
		{
			if (action == OrderAction.Save) return GeneralInfoSection.IsValid(action);

			bool isValid = GeneralInfoSection.IsValid(action) && IngestDestinationSection.IsValid() && MaterialDestinationDetailsSection.IsValid() && MaterialBackupDetailsSection.IsValid();
            foreach (var materialIngestRequestSection in MaterialIngestRequestSections)
            {
                isValid &= materialIngestRequestSection.IsValid();
            }

            return isValid;
        }

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(ingestDestinationLabel, ++row, 0);
			AddWidget(IngestDestinationDropDown, row, 1, 1, 2);

			AddSection(GeneralInfoSection, new SectionLayout(++row, 0));
			row += GeneralInfoSection.RowCount;

			AddSection(IngestDestinationSection, new SectionLayout(++row, 0));
			row += IngestDestinationSection.RowCount;

			AddWidget(materialIngestRequestsTitleLabel, ++row, 0);

			foreach (var materialIngestRequestSection in MaterialIngestRequestSections)
			{
				AddSection(materialIngestRequestSection, new SectionLayout(++row, 0));
				row += materialIngestRequestSection.RowCount;
			}

			AddWidget(materialIngestRequestAddButton, ++row, 0);

			if (IngestDestination != InterplayPamElements.UA)
			{
				AddSection(MaterialDestinationDetailsSection, new SectionLayout(++row, 0));
				row += MaterialDestinationDetailsSection.RowCount;

				AddSection(MaterialBackupDetailsSection, new SectionLayout(++row, 0));
				row += MaterialBackupDetailsSection.RowCount;
			}

			AddWidget(additionalInformationTitleLabel, new WidgetLayout(++row, 0));
			AddWidget(additionalCustomerInfoLabel, ++row, 0, HorizontalAlignment.Left, VerticalAlignment.Top);
			AddWidget(additionalCustomerInfoTextBox, row, 1, 1, 2);

			AddSection(notificationSection , new SectionLayout(++row, 0));

			AddWidget(isilonBackupFileLocationLabel, ++row, 0);
			AddWidget(isilonBackupFileLocationTextBox, row, 1, 1, 2);

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
		}

		public override void UpdateNonLiveOrder(NonLiveOrder nonLiveOrder)
		{
			if (nonLiveOrder.OrderType != IngestExport.Type.Import) return;

			Ingest ingestOrder = (Ingest)nonLiveOrder;

            GeneralInfoSection.UpdateIngest(ingestOrder);
			IngestDestinationSection.UpdateIngest(ingestOrder);
			if (IngestDestination != InterplayPamElements.UA) MaterialDestinationDetailsSection.UpdateIngest(ingestOrder);
			MaterialBackupDetailsSection.UpdateIngest(ingestOrder);
			ingestOrder.EmailReceivers = notificationSection.GetEmails();
			ingestOrder.IsilonBackupFileLocation = this.IsilonBackupFileLocation;

			// Will reset all saved content to avoid duplicate sections
			ingestOrder.FileImportDetails = new List<FileIngestDetails>();
			ingestOrder.HddImportDetails = new List<HddIngestDetails>();
			ingestOrder.CardImportDetails = new List<CardIngestDetails>();
			ingestOrder.MetroMamImportDetails = new List<MetroMamImportDetails>();

			foreach (MaterialImportRequestSection materialIngestRequest in MaterialIngestRequestSections)
			{
				materialIngestRequest.UpdateIngest(ingestOrder);
			}

			ingestOrder.AdditionalInformation = additionalCustomerInfoTextBox.Text;
		}

		public void IngestDestinationDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				if (IngestDestination == InterplayPamElements.UA)
				{
					MaterialIngestRequestSections.RemoveAll(x => x.MaterialType == MaterialTypes.FILE);
				}
				else if (IngestDestination != InterplayPamElements.Vaasa)
				{
					MaterialIngestRequestSections.RemoveAll(x => x.MaterialType == MaterialTypes.MetroMam);
				}

				if (!MaterialIngestRequestSections.Any())
				{
					var newMaterialIngestRequestSection = new MaterialImportRequestSection(helpers, configuration, this);

					newMaterialIngestRequestSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
					newMaterialIngestRequestSection.RegenerateUiRequired += HandleRegenerateUiRequired;

					MaterialIngestRequestSections.Add(newMaterialIngestRequestSection);
				}

				foreach (MaterialImportRequestSection ingestRequestSection in MaterialIngestRequestSections)
				{
					if (ingestRequestSection.MaterialIngestDetails is FileImportDetailsSection fileIngestDetailsSection)
					{
						fileIngestDetailsSection.InitializeFileMaterialTypeDropDown();
					}

					if (ingestRequestSection.MaterialIngestDetails is CardImportDetailsSection cardIngestDetailsSection)
					{
						cardIngestDetailsSection.InitializeCameraOrAudioTypeDropdown();
					}
				}

				InvokeRegenerateUi();
				IsValid(OrderAction.Book);
			}
		}

		public void MaterialIngestRequestDeleteButton_Pressed(object sender, EventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				MaterialIngestRequestSections.Remove(MaterialIngestRequestSections.First(x => x.DeleteButton == (Button)sender));

				InvokeRegenerateUi();
			}
		}

		private void Initialize(Helpers helpers, Ingest ingest)
		{
			GeneralInfoSection = new ImportGeneralInfoSection(helpers, configuration, this, ingest);
			GeneralInfoSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			GeneralInfoSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			IngestDestinationSection = new ImportDestinationSection(helpers, configuration, this, ingest);
			IngestDestinationSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			IngestDestinationSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			MaterialDestinationDetailsSection = new MaterialDestinationDetailsSection(helpers, configuration, this, ingest);
			MaterialDestinationDetailsSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			MaterialDestinationDetailsSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			MaterialBackupDetailsSection = new MaterialBackupDetailsSection(helpers, configuration, this, ingest);
			MaterialBackupDetailsSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			MaterialBackupDetailsSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			MaterialIngestRequestSections = new List<MaterialImportRequestSection>();

			if (ingest != null)
			{
				if (ingest.CardImportDetails != null) MaterialIngestRequestSections.AddRange(ingest.CardImportDetails.Select(x => new MaterialImportRequestSection(helpers, configuration, this, x)).ToArray());
				if (ingest.FileImportDetails != null) MaterialIngestRequestSections.AddRange(ingest.FileImportDetails.Select(x => new MaterialImportRequestSection(helpers, configuration, this, x)).ToArray());
				if (ingest.HddImportDetails != null) MaterialIngestRequestSections.AddRange(ingest.HddImportDetails.Select(x => new MaterialImportRequestSection(helpers, configuration, this, x)).ToArray());
				if (ingest.MetroMamImportDetails != null) MaterialIngestRequestSections.AddRange(ingest.MetroMamImportDetails.Select(x => new MaterialImportRequestSection(helpers, configuration, this, x)).ToArray());
				additionalCustomerInfoTextBox.Text = ingest.AdditionalInformation;
			}
			else
			{
				var newMaterialIngestRequestSection = new MaterialImportRequestSection(helpers, configuration, this);
				newMaterialIngestRequestSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
				newMaterialIngestRequestSection.RegenerateUiRequired += HandleRegenerateUiRequired;

				MaterialIngestRequestSections.Add(newMaterialIngestRequestSection);
			}
			IsilonBackupFileLocation = ingest?.IsilonBackupFileLocation ?? String.Empty;
		}

		private void MaterialIngestRequestAddButton_Pressed(object sender, EventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				var newMaterialIngestRequestSection = new MaterialImportRequestSection(helpers, configuration, this);
				newMaterialIngestRequestSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
				newMaterialIngestRequestSection.RegenerateUiRequired += HandleRegenerateUiRequired;

				MaterialIngestRequestSections.Add(newMaterialIngestRequestSection);

				InvokeRegenerateUi();
			}
		}

        protected override void HandleVisibilityAndEnabledUpdate()
        {
			ingestDestinationLabel.IsVisible = IsVisible;
			IngestDestinationDropDown.IsVisible = IsVisible;
			IngestDestinationDropDown.IsEnabled = IsEnabled;

            GeneralInfoSection.IsVisible = IsVisible;
            GeneralInfoSection.IsEnabled = IsEnabled;

            IngestDestinationSection.IsVisible = IsVisible;
			IngestDestinationSection.IsEnabled = IsEnabled;

			materialIngestRequestsTitleLabel.IsVisible = IsVisible;

			MaterialIngestRequestSections.ForEach(x =>
			{
				x.IsVisible = IsVisible;
				x.IsEnabled = IsEnabled;
			});

			materialIngestRequestAddButton.IsVisible = IsVisible;
			materialIngestRequestAddButton.IsEnabled = IsEnabled;

			MaterialDestinationDetailsSection.IsVisible = IsVisible;
			MaterialDestinationDetailsSection.IsEnabled = IsEnabled;

            MaterialBackupDetailsSection.IsVisible = IsVisible;
			MaterialBackupDetailsSection.IsEnabled = IsEnabled;

			additionalInformationTitleLabel.IsVisible = IsVisible;
			additionalCustomerInfoLabel.IsVisible = IsVisible;
			additionalCustomerInfoTextBox.IsVisible = IsVisible;
			additionalCustomerInfoTextBox.IsEnabled = IsEnabled;

			notificationSection.IsVisible = IsVisible;
			notificationSection.IsEnabled = IsEnabled;

			isilonBackupFileLocationLabel.IsVisible = IsVisible && configuration.IsilonBackupFileLocationIsVisible;
			isilonBackupFileLocationTextBox.IsVisible = IsVisible && configuration.IsilonBackupFileLocationIsVisible;
			isilonBackupFileLocationTextBox.IsEnabled = IsEnabled && configuration.IsilonBackupFileLocationIsEnabled;

			ToolTipHandler.SetTooltipVisibility(this);
        }

		public override void RegenerateUi()
		{
			GeneralInfoSection.RegenerateUi();
			IngestDestinationSection.RegenerateUi();
			MaterialIngestRequestSections.ForEach(section => section.RegenerateUi());
			MaterialDestinationDetailsSection.RegenerateUi();
			MaterialBackupDetailsSection.RegenerateUi();
			GenerateUi(out int row);
		}
	}
}

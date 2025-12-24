namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.Import
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class MaterialImportRequestSection : ImportSubSection
	{
		private const string titlePart1 = "New material Import - ";

		private readonly Label title = new Label { Style = TextStyle.Heading };
		private Button deleteButton = new Button("Delete") { Width = 150 };

		private readonly Label materialIngestTypeLabel = new Label("Import material type");
		private readonly DropDown materialIngestTypeDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<MaterialTypes>().OrderBy(x => x), EnumExtensions.GetDescriptionFromEnumValue(MaterialTypes.CARD));

		private MaterialTypes selectedMaterialIngestType = MaterialTypes.CARD;

		public MaterialImportRequestSection(Helpers helpers, ISectionConfiguration configuration, ImportMainSection section, MaterialIngestDetails materialIngestDetails = null) : base(helpers, configuration, section, null)
		{
			materialIngestTypeDropDown.Changed += MaterialIngestTypeDropDown_Changed;
			deleteButton.Pressed += section.MaterialIngestRequestDeleteButton_Pressed;
			
			if (materialIngestDetails != null)
			{
				selectedMaterialIngestType = EnumExtensions.GetEnumValueFromDescription<MaterialTypes>(materialIngestDetails.Type);
				switch (selectedMaterialIngestType)
				{
					case MaterialTypes.CARD:
						MaterialIngestDetails = new CardImportDetailsSection(helpers, configuration, section, (CardIngestDetails)materialIngestDetails);
						break;
					case MaterialTypes.HDD:
						MaterialIngestDetails = new HddImportDetailsSection(helpers, configuration, section, (HddIngestDetails)materialIngestDetails);
						break;
					case MaterialTypes.FILE:
						MaterialIngestDetails = new FileImportDetailsSection(helpers, configuration, section, (FileIngestDetails)materialIngestDetails);
						break;
					case MaterialTypes.MetroMam:
						MaterialIngestDetails = new MetroMamImportDetailsSection(helpers, configuration, section, (MetroMamImportDetails)materialIngestDetails);
						break;
					default:
						MaterialIngestDetails = null;
						break;
				}

				materialIngestTypeDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(selectedMaterialIngestType);
            }
			else
			{
				MaterialIngestDetails = new CardImportDetailsSection(helpers, configuration, section);
			}

			if (MaterialIngestDetails != null)
			{
				MaterialIngestDetails.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
				MaterialIngestDetails.RegenerateUiRequired += HandleRegenerateUiRequired;
			}

			section.IngestDestinationDropDown.Changed += IngestDestinationDropDown_Changed;

			SetOptionsToIngestMaterialTypeDropdown();

			GenerateUi(out int row);
		}

		private void IngestDestinationDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			SetOptionsToIngestMaterialTypeDropdown();
		}

		private void SetOptionsToIngestMaterialTypeDropdown()
		{
			var materialImportTypeDropDownOptions = EnumExtensions.GetEnumDescriptions<MaterialTypes>().OrderBy(x => x).ToList();
			if (ingestMainSection.IngestDestination == InterplayPamElements.UA)
			{
				materialImportTypeDropDownOptions = EnumExtensions.GetEnumDescriptions<MaterialTypes>().Where(x => !x.Equals(EnumExtensions.GetDescriptionFromEnumValue(MaterialTypes.FILE))).OrderBy(x => x).ToList();
			}
			else if (ingestMainSection.IngestDestination != InterplayPamElements.Vaasa)
			{
				materialImportTypeDropDownOptions = EnumExtensions.GetEnumDescriptions<MaterialTypes>().Where(x => !x.Equals(EnumExtensions.GetDescriptionFromEnumValue(MaterialTypes.MetroMam))).OrderBy(x => x).ToList();
			}

			materialIngestTypeDropDown.Options = materialImportTypeDropDownOptions;
		}

		public MaterialTypes MaterialType
		{
			get
			{
				return selectedMaterialIngestType;
			}
			private set
			{
				selectedMaterialIngestType = value;
				materialIngestTypeDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(selectedMaterialIngestType);

				switch (selectedMaterialIngestType)
				{
					case MaterialTypes.CARD:
						MaterialIngestDetails = new CardImportDetailsSection(helpers, configuration, ingestMainSection);
						break;
					case MaterialTypes.HDD:
						MaterialIngestDetails = new HddImportDetailsSection(helpers, configuration, ingestMainSection);
						break;
					case MaterialTypes.FILE:
						MaterialIngestDetails = new FileImportDetailsSection(helpers, configuration, ingestMainSection);
						break;
					case MaterialTypes.MetroMam:
						MaterialIngestDetails = new MetroMamImportDetailsSection(helpers, configuration, ingestMainSection);
						break;
					default:
						MaterialIngestDetails = null;
						return;
				}

				if (MaterialIngestDetails != null)
				{
					MaterialIngestDetails.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
					MaterialIngestDetails.RegenerateUiRequired += HandleRegenerateUiRequired;
				}

				title.Text = titlePart1 + EnumExtensions.GetDescriptionFromEnumValue(MaterialType);
			}
		}

		public ImportSubSection MaterialIngestDetails { get; private set; }

		public Button DeleteButton
		{
			get { return deleteButton; }
			set { deleteButton = value; }
		}

		public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections
		{
			get
			{
				return MaterialIngestDetails == null ? new NonLiveManagerTreeViewSection[0] : MaterialIngestDetails.TreeViewSections;
			}
		}

		public override bool IsValid()
		{
			return MaterialIngestDetails.IsValid();
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(materialIngestTypeLabel, ++row, 0);
			AddWidget(materialIngestTypeDropDown, row, 1, 1, 2);

			AddSection(MaterialIngestDetails, new SectionLayout(++row, 0));
			row += MaterialIngestDetails.RowCount;

			AddWidget(deleteButton, row + 1, 1, 1, 2);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			title.IsVisible = IsVisible;

			materialIngestTypeLabel.IsVisible = IsVisible;
			materialIngestTypeDropDown.IsVisible = IsVisible;
			materialIngestTypeDropDown.IsEnabled = IsEnabled;

			MaterialIngestDetails.IsVisible = IsVisible;
			MaterialIngestDetails.IsEnabled = IsEnabled;

			deleteButton.IsVisible = IsVisible && ingestMainSection.MaterialIngestRequestSections.Count > 1;
			deleteButton.IsEnabled = IsEnabled;

            ToolTipHandler.SetTooltipVisibility(this);
		}

		public override void UpdateIngest(Ingest ingest)
		{
			MaterialIngestDetails.UpdateIngest(ingest);
		}

		private void MaterialIngestTypeDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				if (String.IsNullOrEmpty(e.Selected))
				{
					return;
				}

				MaterialType = EnumExtensions.GetEnumValueFromDescription<MaterialTypes>(e.Selected);

				InvokeRegenerateUi();
				IsValid();
			}
		}

		public override void RegenerateUi()
		{
			MaterialIngestDetails?.RegenerateUi();
			GenerateUi(out int row);
		}
	}

}

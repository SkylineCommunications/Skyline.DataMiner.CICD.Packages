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

	public class MaterialDestinationDetailsSection : ImportSubSection
	{
		private readonly Label materialDestinationTitleLabel = new Label("Material Destination Details") { Style = TextStyle.Bold };

		private readonly Label materialToBeRelinkedLabel = new Label("Material to be relinked to original materials in color post production?");
		private readonly CheckBox materialToBeRelinkedCheckBox = new CheckBox { IsChecked = false };

		private readonly Label InterplayFormatLabel = new Label("Interplay Format");
		private readonly DropDown InterplayFormatDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<HkiInterplayformat>(), EnumExtensions.GetDescriptionFromEnumValue(HkiInterplayformat.AVC_INTRA100));

		private readonly Label multiCameraMaterialLabel = new Label("Multi camera material");
		private readonly CheckBox multiCameraMaterialCheckBox = new CheckBox { IsChecked = false };

		public MaterialDestinationDetailsSection(Helpers helpers, ISectionConfiguration configuration, ImportMainSection section, Ingest ingest = null) : base(helpers, configuration, section, ingest)
		{
			materialToBeRelinkedCheckBox.Changed += (sender, e) => HandleVisibilityAndEnabledUpdate();

			InterplayFormatDropDown.Changed += (sender, e) => HandleVisibilityAndEnabledUpdate();

			if (ingest != null)
			{
				MaterialToBeRelinked = ingest.MaterialToBeRelinkedToOriginalMaterialsInColorPostProduction;

				List<string> options = new List<string>();
				InterplayPamElements selectedElement = EnumExtensions.GetEnumValueFromDescription<InterplayPamElements>(ingest.IngestDestination.Destination);
				if (selectedElement == InterplayPamElements.Helsinki)
				{
					options = EnumExtensions.GetEnumDescriptions<HkiInterplayformat>().Where(x => !x.Equals(EnumExtensions.GetDescriptionFromEnumValue(HkiInterplayformat.DNXHD_LB))).ToList();
				}
				else if (selectedElement == InterplayPamElements.Tampere) options = EnumExtensions.GetEnumDescriptions<TreInterplayformat>().Where(x => !x.Equals(EnumExtensions.GetDescriptionFromEnumValue(TreInterplayformat.DNXHD_LB))).ToList();
				else if (selectedElement == InterplayPamElements.Vaasa) options = EnumExtensions.GetEnumDescriptions<VsaInterplayformat>().ToList();

				if (materialToBeRelinkedCheckBox.IsChecked && section.IngestDestination != InterplayPamElements.Vaasa) options.Add(EnumExtensions.GetDescriptionFromEnumValue(HkiInterplayformat.DNXHD_LB));

				InterplayFormatDropDown.Options = options;
				InterplayFormatDropDown.Selected = ingest.InterplayFormat;

				MultiCameraMaterial = ingest.MultiCameraMaterial;
			}

			section.IngestDestinationDropDown.Changed += IngestDestinationDropDown_Changed;

			InitializeInterplayFormatDropDown();

			GenerateUi(out int row);
		}

		private void IngestDestinationDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			InitializeInterplayFormatDropDown();
		}

		private void InitializeInterplayFormatDropDown()
		{
			var options = new List<string>();
			if (ingestMainSection.IngestDestination == InterplayPamElements.Helsinki)
			{
				options = EnumExtensions.GetEnumDescriptions<HkiInterplayformat>().Where(x => !x.Equals(EnumExtensions.GetDescriptionFromEnumValue(HkiInterplayformat.DNXHD_LB))).ToList();
			}
			else if (ingestMainSection.IngestDestination == InterplayPamElements.Tampere)
			{
				options = EnumExtensions.GetEnumDescriptions<TreInterplayformat>().Where(x => !x.Equals(EnumExtensions.GetDescriptionFromEnumValue(TreInterplayformat.DNXHD_LB))).ToList();
			}
			else if (ingestMainSection.IngestDestination == InterplayPamElements.Vaasa)
			{
				options = EnumExtensions.GetEnumDescriptions<VsaInterplayformat>().ToList();
			}

			if (materialToBeRelinkedCheckBox.IsChecked && ingestMainSection.IngestDestination != InterplayPamElements.Vaasa)
			{
				options.Add(EnumExtensions.GetDescriptionFromEnumValue(HkiInterplayformat.DNXHD_LB));
			}

			InterplayFormatDropDown.Options = options;
		}

		public bool MaterialToBeRelinked
		{
			get
			{
				return materialToBeRelinkedCheckBox.IsChecked;
			}
			private set
			{
				materialToBeRelinkedCheckBox.IsChecked = value;
			}
		}

		public bool MultiCameraMaterial
		{
			get
			{
				return multiCameraMaterialCheckBox.IsChecked;
			}
			private set
			{
				multiCameraMaterialCheckBox.IsChecked = value;
			}
		}

		public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections
		{
			get
			{
				return new NonLiveManagerTreeViewSection[0];
			}
		}

		public override bool IsValid() => true;

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(materialDestinationTitleLabel, new WidgetLayout(++row, 0));

			AddWidget(materialToBeRelinkedLabel, ++row, 0);
			AddWidget(materialToBeRelinkedCheckBox, row, 1, 1, 2);

			AddWidget(InterplayFormatLabel, ++row, 0);
			AddWidget(InterplayFormatDropDown, row, 1, 1, 2);

			AddWidget(multiCameraMaterialLabel, ++row, 0);
			AddWidget(multiCameraMaterialCheckBox, row, 1, 1, 2);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

		public override void UpdateIngest(Ingest ingest)
		{
			ingest.MaterialToBeRelinkedToOriginalMaterialsInColorPostProduction = this.MaterialToBeRelinked;

			ingest.InterplayFormat = InterplayFormatDropDown.Selected;

			ingest.MultiCameraMaterial = this.MultiCameraMaterial;
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			materialDestinationTitleLabel.IsVisible = IsVisible;

			materialToBeRelinkedLabel.IsVisible = IsVisible;
			materialToBeRelinkedCheckBox.IsVisible = IsVisible;
			materialToBeRelinkedCheckBox.IsEnabled = IsEnabled;

			InterplayFormatLabel.IsVisible = IsVisible && ingestMainSection.IngestDestination != InterplayPamElements.UA;
			InterplayFormatDropDown.IsVisible = InterplayFormatLabel.IsVisible;
			InterplayFormatDropDown.IsEnabled = IsEnabled;

			multiCameraMaterialLabel.IsVisible= IsVisible;
			multiCameraMaterialCheckBox.IsVisible = IsVisible;
			multiCameraMaterialCheckBox.IsEnabled = IsEnabled;

			InitializeInterplayFormatDropDown();

			ToolTipHandler.SetTooltipVisibility(this);
		}

		public override void RegenerateUi()
		{
			GenerateUi(out int row);
		}
	}
}

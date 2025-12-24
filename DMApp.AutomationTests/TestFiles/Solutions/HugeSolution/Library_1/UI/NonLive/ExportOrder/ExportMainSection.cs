namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.ExportOrder
{
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ExportMainSection : MainSection
	{
		private readonly Export export;

		private readonly Element mediaParkkiElement;

		private Sources materialExportSourceType = Sources.INTERPLAY_PAM;
		private readonly Label materialExportSourceLabel = new Label("Material Source for Export");
		private readonly DropDown materialExportSourceDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<Sources>(), EnumExtensions.GetDescriptionFromEnumValue(Sources.INTERPLAY_PAM));

		private readonly ExportGeneralInfoSection generalInfoSection;
		private readonly NotificationSection notificationSection;
		private readonly ISectionConfiguration configuration = new NonLiveOrderConfiguration();

		public ExportMainSection(Helpers helpers, Export export) : base(helpers)
		{
			this.export = export;

			mediaParkkiElement = helpers.Engine.FindElementsByProtocol(MediaParkkiProtocolName).FirstOrDefault();

			if (export != null)
			{
				// this will make sure the material source object is initialized with all export data we retrieved
				MaterialExportSourceType = export.MaterialSource;
			}
			else
			{
				ExportSourceSection = new InterplayPamMaterialExportSource(helpers, configuration, this, export);
				ExportSourceSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
				ExportSourceSection.RegenerateUiRequired += HandleRegenerateUiRequired;
			}

			generalInfoSection = new ExportGeneralInfoSection(helpers, configuration, export);
			generalInfoSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			generalInfoSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			ExportInformationSection = new ExportInformationSection(helpers, configuration, this, export);
			ExportInformationSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			ExportInformationSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			notificationSection = new NotificationSection(helpers, configuration, export);

			materialExportSourceDropDown.Changed += MaterialExportSourceDropdown_Changed;

			GenerateUi(out int row);
		}

		public Sources MaterialExportSourceType
		{
			get
			{
				return materialExportSourceType;
			}
			private set
			{
				materialExportSourceType = value;
				materialExportSourceDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(materialExportSourceType);

                switch (value)
                {
                    case Sources.INTERPLAY_PAM:
                        ExportSourceSection = new InterplayPamMaterialExportSource(helpers, configuration, this, export);
                        break;
                    case Sources.MEDIAPARKKI:
                        ExportSourceSection = new MediaParkkiMaterialExportSource(helpers, configuration, mediaParkkiElement, this, export);
                        break;
                    case Sources.ISILON_BU:
                        ExportSourceSection = new IsilonBuMaterialExportSource(helpers, configuration, this, export);
                        break;
                    case Sources.MAM:
                        ExportSourceSection = new MamMetroMaterialExportSource(helpers, configuration, this, export);
                        break;
                    case Sources.OTHER:
                        ExportSourceSection = new OtherMaterialExportSource(helpers, configuration, this, export);
                        break;
                    default:
                        // Do nothing
                        break;
                }

				ExportSourceSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
				ExportSourceSection.RegenerateUiRequired += HandleRegenerateUiRequired;
			}
        }

		public ExportSourceSection ExportSourceSection { get; private set; }

		public ExportInformationSection ExportInformationSection { get; private set; }

		public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections 
		{
			get
			{
				List<NonLiveManagerTreeViewSection> treeViewSections = new List<NonLiveManagerTreeViewSection>();
				treeViewSections.AddRange(ExportInformationSection.TreeViewSections);
				treeViewSections.AddRange(ExportSourceSection.TreeViewSections);
				return treeViewSections;
			}
		}

		public override bool IsValid(OrderAction action)
		{
			bool isGeneralInfoSectionValid = generalInfoSection.IsValid(action);
			bool isExportSourceSectionValid = ExportSourceSection.IsValid(action); // separate boolean to make sure the logic in IsValid property is executed
			bool isExportInformationSectionValid = ExportInformationSection.IsValid(action); // separate boolean to make sure the logic in IsValid property is executed

			return isGeneralInfoSectionValid
				&& isExportSourceSectionValid
				&& isExportInformationSectionValid;
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddSection(generalInfoSection, new SectionLayout(++row, 0));
			row += generalInfoSection.RowCount;

			AddWidget(materialExportSourceLabel, ++row, 0);
			AddWidget(materialExportSourceDropDown, row, 1, 1, 2);

			AddSection(ExportSourceSection, new SectionLayout(++row, 0));
			row += ExportSourceSection.RowCount;

			AddSection(ExportInformationSection, new SectionLayout(row + 1, 0));		
			row += ExportInformationSection.RowCount;

			AddSection(notificationSection, new SectionLayout(row + 1, 0));

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

		public override void UpdateNonLiveOrder(NonLiveOrder nonLiveOrder)
		{
			if (nonLiveOrder.OrderType != Type.Export) return;

			Export exportOrder = (Export)nonLiveOrder;

			generalInfoSection.UpdateNonLiveOrder(exportOrder);
			exportOrder.MaterialSource = MaterialExportSourceType;
			ExportSourceSection.UpdateExport(exportOrder);
			ExportInformationSection.UpdateExport(exportOrder);
			exportOrder.EmailReceivers = notificationSection.GetEmails();
		}

		private void MaterialExportSourceDropdown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				MaterialExportSourceType = EnumExtensions.GetEnumValueFromDescription<Sources>(e.Selected);

				InvokeRegenerateUi();
				IsValid(OrderAction.Book);
			}
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			generalInfoSection.IsVisible = IsVisible;
			generalInfoSection.IsEnabled = IsEnabled;

			ExportSourceSection.IsEnabled = IsEnabled;
			ExportSourceSection.IsVisible = IsVisible;

			ExportInformationSection.IsEnabled = IsEnabled;

			bool hidedExportInformationSection = ExportSourceSection is InterplayPamMaterialExportSource interplayPamMaterialExportSource && interplayPamMaterialExportSource.CurrentSelectedExportFileType == InterplayPamExportFileTypes.MetroTransfer.GetDescription();

			ExportInformationSection.IsVisible = IsVisible && !hidedExportInformationSection;

			notificationSection.IsEnabled = IsEnabled;
			notificationSection.IsVisible = IsVisible;
		}

		public override void RegenerateUi()
		{
			generalInfoSection.RegenerateUi();
			ExportSourceSection.RegenerateUi();
			ExportInformationSection.RegenerateUi();
			GenerateUi(out int row);
		}
	}
}

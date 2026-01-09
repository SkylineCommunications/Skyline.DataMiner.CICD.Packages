namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.Aspera
{
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Aspera;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class AsperaMainSection : MainSection
    {
        private readonly Label asperaTypeLabel = new Label("Aspera Type");
        private readonly DropDown asperaTypeDropdown = new DropDown(EnumExtensions.GetEnumDescriptions<AsperaType>().OrderBy(x => x), AsperaType.Faspex.GetDescription());

        private readonly Label additionalInfoTitleLabel = new Label("Additional information") { Style = TextStyle.Bold };
        private readonly Label additionalInfoLabel = new Label("Additional information");
        private readonly TextBox additionalInfoTextBox = new TextBox { IsMultiline = true, Height = 200 };

        private NotificationSection notificationSection;

		private readonly ISectionConfiguration configuration = new NonLiveOrderConfiguration();

        public AsperaMainSection(Helpers helpers, Aspera aspera) : base(helpers)
        {
            if (aspera != null)
            {
                AsperaOrderType = aspera.AsperaType.GetEnumValue<AsperaType>();
            }
            else
            {
                AsperaOrderType = AsperaType.Faspex;
            }

            Initialize(helpers, aspera);

            GenerateUi(out int row);
        }

        public AsperaGeneralInfoSection GeneralInfoSection { get; private set; }

        public AsperaFaspexSection AsperaFaspexSection { get; private set; }

        public AsperaSharesSection AsperaSharesSection { get; private set; }

        private void AsperaTypeDropdown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
        {
			using (UiDisabler.StartNew(this))
			{
				AsperaOrderType = e.Selected.GetEnumValue<AsperaType>();
				HandleVisibilityAndEnabledUpdate();
			}
        }

        public AsperaType AsperaOrderType
        {
            get => AsperaTypeDropdown.Selected.GetEnumValue<AsperaType>();
            private set => AsperaTypeDropdown.Selected = value.GetDescription();      
        }

        public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections => new List<NonLiveManagerTreeViewSection>();

		public DropDown AsperaTypeDropdown => asperaTypeDropdown;

		private void Initialize(Helpers helpers, Aspera asperaOrder)
        {
            GeneralInfoSection = new AsperaGeneralInfoSection(helpers, configuration, asperaOrder);
			GeneralInfoSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;

            AsperaFaspexSection = new AsperaFaspexSection(helpers, configuration, asperaOrder);
			AsperaFaspexSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			AsperaFaspexSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			AsperaSharesSection = new AsperaSharesSection(helpers, configuration, asperaOrder);
			AsperaSharesSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			AsperaSharesSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			notificationSection = new NotificationSection(helpers, configuration, asperaOrder);

			additionalInfoTextBox.Text = asperaOrder?.AdditionalInfo;


			AsperaTypeDropdown.Changed += AsperaTypeDropdown_Changed;

			ToolTipHandler.SetTooltipVisibility(this);
        }

		public override bool IsValid(OrderAction action)
        {
            bool isGeneralInfoSectionValid = GeneralInfoSection.IsValid(action);
            bool isAsperaFaspexSectionValid = AsperaOrderType == AsperaType.Shares || AsperaFaspexSection.IsValid(action);
            bool isAsperaSharesSectionValid = AsperaOrderType == AsperaType.Faspex || AsperaSharesSection.IsValid(action);

            return isGeneralInfoSectionValid
                && isAsperaFaspexSectionValid
                && isAsperaSharesSectionValid;
        }

        public override void UpdateNonLiveOrder(NonLiveOrder nonLiveOrder)
        {
            Aspera asperaOrder = (Aspera)nonLiveOrder;
            asperaOrder.AsperaType = AsperaOrderType.GetDescription();
            asperaOrder.AdditionalInfo = additionalInfoTextBox.Text;

            GeneralInfoSection.UpdateAsperaOrder(asperaOrder);
            AsperaFaspexSection.UpdateNonLiveOrder(asperaOrder);
            AsperaSharesSection.UpdateNonLiveOrder(asperaOrder);

            // Update non - live teams filtering
            bool isTeamMgmtFaspexSelectionAllowed = (AsperaFaspexSection.Workgroup == AsperaWorkgroup.Mediaputiikki_TREIPLAY || AsperaFaspexSection.Workgroup == AsperaWorkgroup.Mediamylly_VSAIPLAY) && AsperaOrderType == AsperaType.Faspex;
            bool isTeamMgmtSharesSelectionAllowed = (AsperaSharesSection.ImportDepartment == ImportDepartment.Messi_HELIPLAY || AsperaSharesSection.ImportDepartment == ImportDepartment.Mediaputiikki_TREIPLAY || AsperaSharesSection.ImportDepartment == ImportDepartment.Mediamylly_VSAIPLAY) && AsperaOrderType == AsperaType.Shares;
            
            asperaOrder.EmailReceivers = notificationSection.GetEmails();
            asperaOrder.TeamHki = AsperaFaspexSection.Workgroup == AsperaWorkgroup.Messi_HELIPLAY && AsperaOrderType == AsperaType.Faspex;
            asperaOrder.TeamMgmt = isTeamMgmtFaspexSelectionAllowed || isTeamMgmtSharesSelectionAllowed;
        }

        protected override void GenerateUi(out int row)
        {
            base.GenerateUi(out row);

            AddSection(GeneralInfoSection, new SectionLayout(++row, 0));
            row += GeneralInfoSection.RowCount;

            AddWidget(asperaTypeLabel, ++row, 0);
            AddWidget(AsperaTypeDropdown, row, 1, 1, 2);

            AddSection(AsperaFaspexSection, new SectionLayout(++row, 0));
            row += AsperaFaspexSection.RowCount;
          
            AddSection(AsperaSharesSection, new SectionLayout(++row, 0));
            row += AsperaSharesSection.RowCount;       

            AddWidget(additionalInfoTitleLabel, new WidgetLayout(++row, 0));
            AddWidget(additionalInfoLabel, ++row, 0, HorizontalAlignment.Left, VerticalAlignment.Top);
            AddWidget(additionalInfoTextBox, row, 1, 1, 2);

            AddSection(notificationSection, new SectionLayout(row + 1, 0));

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			GeneralInfoSection.IsEnabled = IsEnabled;
			GeneralInfoSection.IsVisible = IsVisible;

			asperaTypeLabel.IsVisible = IsVisible;
			AsperaTypeDropdown.IsVisible = IsVisible;
			AsperaTypeDropdown.IsEnabled = IsEnabled;

			AsperaFaspexSection.IsEnabled = IsEnabled;
			AsperaFaspexSection.IsVisible = IsVisible && AsperaOrderType == AsperaType.Faspex;

			AsperaSharesSection.IsEnabled = IsEnabled;
			AsperaSharesSection.IsVisible = IsVisible && AsperaOrderType == AsperaType.Shares;

			TreeViewSections.ForEach(tvs =>
			{
				tvs.IsEnabled = IsEnabled;
				tvs.IsVisible = IsVisible;
			});	

			additionalInfoTitleLabel.IsVisible = IsVisible;
			additionalInfoLabel.IsVisible = IsVisible;
			additionalInfoTextBox.IsVisible = IsVisible;
			additionalInfoTextBox.IsEnabled = IsEnabled;

			notificationSection.IsVisible = IsVisible;
			notificationSection.IsEnabled = IsEnabled;

			ToolTipHandler.SetTooltipVisibility(this);
		}

		public override void RegenerateUi()
		{
			GeneralInfoSection.RegenerateUi();
			AsperaFaspexSection.RegenerateUi();
			AsperaSharesSection.RegenerateUi();
			GenerateUi(out int row);
		}
	}
}
namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.Aspera
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Aspera;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public sealed class AsperaSharesSection : YleSection
    {
        private readonly ISectionConfiguration configuration;

        private readonly Label asperSharesTitleLabel = new Label("ASPERA SHARE") { Style = TextStyle.Bold };

        private readonly Label importDepartmentLabel = new Label("Import Department");
        private readonly DropDown importDepartmentDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<ImportDepartment>().OrderBy(x => x), ImportDepartment.Messi_HELIPLAY.GetDescription());

        private readonly Label nameOfShareLabel = new Label("Name of the Share");
        private readonly TextBox nameOfShareTextBox = new TextBox();

        private readonly Label purposeUsageLabel = new Label("Purpose and usage of the Share");
        private readonly TextBox purposeUsageTextBox = new TextBox { IsMultiline = true, Height = 200, Tooltip = "Describe how the share is to be used. What kind of folders are required?" };

        public AsperaSharesSection(Helpers helpers, ISectionConfiguration configuration, Aspera aspera) : base(helpers)
        {
            this.configuration = configuration?? throw new ArgumentNullException(nameof(configuration));

            Initialize(aspera);
            
            AddParticipantSectionButton.Pressed += AddParticipantButton_Pressed;
            DeleteParticipantSectionButton.Pressed += RemoveParticipantButton_Pressed;

            GenerateUi(out int row);
        }

        public ImportDepartment ImportDepartment
        {
            get => EnumExtensions.GetEnumValueFromDescription<ImportDepartment>(importDepartmentDropDown.Selected);

            private set
            {
                importDepartmentDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(value);
            }
        }

        public string NameOfTheShare
        {
            get { return nameOfShareTextBox.Text; }
            private set
            {
                nameOfShareTextBox.Text = value;
            }
        }

        public string PurposeAndUsage
        {
            get { return purposeUsageTextBox.Text; }
            private set { purposeUsageTextBox.Text = value; }
        }

        public List<AsperaEmailSection> EmailSections { get; private set; }

        public Button AddParticipantSectionButton { get; private set; }

        public Button DeleteParticipantSectionButton { get; private set; }

        protected override void GenerateUi(out int row)
        {
            base.GenerateUi(out row);

            AddWidget(asperSharesTitleLabel, ++row, 0);

            AddWidget(importDepartmentLabel, ++row, 0);
            AddWidget(importDepartmentDropDown, row, 1, 1, 2);

            AddWidget(nameOfShareLabel, ++row, 0);
            AddWidget(nameOfShareTextBox, row, 1, 1, 2);

            AddWidget(purposeUsageLabel, ++row, 0);
            AddWidget(purposeUsageTextBox, row, 1, 1, 2);

            foreach (var emailSection in EmailSections)
            {
                AddSection(emailSection, new SectionLayout(++row, 0));
                row += emailSection.RowCount;
            }

            AddWidget(AddParticipantSectionButton, ++row, 0);
            AddWidget(DeleteParticipantSectionButton, ++row, 0);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

        private void Initialize(Aspera aspera)
        {
            EmailSections = new List<AsperaEmailSection> { new AsperaEmailSection(AsperaType.Shares, helpers, configuration) };
            AddParticipantSectionButton = new Button("Add Another Participant") { Width = 200 };
            DeleteParticipantSectionButton = new Button("Delete") { Width = 200 };

            if (aspera?.ParticipantsEmailAddress == null || aspera.ParticipantsEmailAddress.Length == 0) return;

            importDepartmentDropDown.Selected = aspera.ImportDepartment;
            NameOfTheShare = aspera.NameofTheShare;
            PurposeAndUsage = aspera.PurposeAndUsage;
            EmailSections = aspera.ParticipantsEmailAddress.Select(p => new AsperaEmailSection(AsperaType.Shares, helpers, configuration, p)).ToList();
        }

        private void AddParticipantButton_Pressed(object sender, EventArgs e)
        {
            EmailSections.Add(new AsperaEmailSection(AsperaType.Shares, helpers, configuration));

			InvokeRegenerateUi();
        }

        private void RemoveParticipantButton_Pressed(object sender, EventArgs e)
        {
            int lastItemIndex = EmailSections.Count - 1;
            if (lastItemIndex <= 0) return;

            EmailSections.RemoveAt(lastItemIndex);

			InvokeRegenerateUi();
        }

        public bool IsValid(OrderAction action)
        {
            bool areEmailSectionsValid = EmailSections.All(section => section.IsValid());

            bool nameOfTheShareIsValid = !string.IsNullOrWhiteSpace(NameOfTheShare);
            nameOfShareTextBox.ValidationState = nameOfTheShareIsValid ? Automation.UIValidationState.Valid : Automation.UIValidationState.Invalid;
            nameOfShareTextBox.ValidationText = "Please provide any information";

            bool purposeAndUsageOfShareIsValid = !string.IsNullOrWhiteSpace(PurposeAndUsage);
            purposeUsageTextBox.ValidationState = purposeAndUsageOfShareIsValid ? Automation.UIValidationState.Valid : Automation.UIValidationState.Invalid;
            purposeUsageTextBox.ValidationText = "Please provide any information";

            return areEmailSectionsValid && nameOfTheShareIsValid && purposeAndUsageOfShareIsValid;
        }

        public void UpdateNonLiveOrder(Aspera asperaOrder)
        {
            asperaOrder.ImportDepartment = ImportDepartment.GetDescription();
            asperaOrder.NameofTheShare = NameOfTheShare;
            asperaOrder.PurposeAndUsage = PurposeAndUsage;
            asperaOrder.ParticipantsEmailAddress = EmailSections.Select(section => section.EmailAddress).ToArray();
        }

		public override void RegenerateUi()
		{
			EmailSections.ForEach(section => section.RegenerateUi());
			GenerateUi(out int row);
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			asperSharesTitleLabel.IsVisible = IsVisible;

			importDepartmentLabel.IsVisible = IsVisible;
			importDepartmentDropDown.IsVisible = IsVisible;
			importDepartmentDropDown.IsEnabled = IsEnabled;

			nameOfShareLabel.IsVisible = IsVisible;
			nameOfShareTextBox.IsVisible = IsVisible;
			nameOfShareTextBox.IsEnabled = IsEnabled;

			purposeUsageLabel.IsVisible = IsVisible;
			purposeUsageTextBox.IsVisible = IsVisible;
			purposeUsageTextBox.IsEnabled = IsEnabled;

			EmailSections.ForEach(section =>
			{
				section.IsVisible = IsVisible;
				section.IsEnabled = IsEnabled;
			});

			AddParticipantSectionButton.IsVisible = IsVisible;
			AddParticipantSectionButton.IsEnabled = IsEnabled;

			DeleteParticipantSectionButton.IsVisible = IsVisible;
			DeleteParticipantSectionButton.IsEnabled = IsEnabled;

			ToolTipHandler.SetTooltipVisibility(this);
		}
	}
}
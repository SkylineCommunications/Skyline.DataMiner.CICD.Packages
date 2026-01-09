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

	public class AsperaFaspexSection : YleSection
	{
		private readonly Label asperafaspexTitleLabel = new Label("ASPERA FASPEX") { Style = TextStyle.Bold };

		private readonly Label asperaWorkgroupLabel = new Label("Aspera Workgroup");
		private readonly DropDown asperaWorkgroupDropdown = new DropDown(EnumExtensions.GetEnumDescriptions<AsperaWorkgroup>().OrderBy(x => x), AsperaWorkgroup.Messi_HELIPLAY.GetDescription());

		private readonly Label validDaysLabel = new Label("How many days is the link valid?");
		private readonly Numeric validDaysNumeric = new Numeric(7);

		private readonly ISectionConfiguration configuration;

		public AsperaFaspexSection(Helpers helpers, ISectionConfiguration configuration, Aspera aspera) : base(helpers)
		{
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

			Initialize(aspera);

			AddSenderButton.Pressed += AddSenderButton_Pressed;
			DeleteSenderSectionButton.Pressed += RemoveSenderButton_Pressed;
			asperaWorkgroupDropdown.Changed += AsperaWorkgroupDropdown_Changed;

			GenerateUi(out int row);
		}

		public AsperaWorkgroup Workgroup
		{
			get => EnumExtensions.GetEnumValueFromDescription<AsperaWorkgroup>(asperaWorkgroupDropdown.Selected);

			private set
			{
				asperaWorkgroupDropdown.Selected = EnumExtensions.GetDescriptionFromEnumValue(value);
			}
		}

		public double ValidDays
		{
			get { return validDaysNumeric.Value; }
			private set
			{
				validDaysNumeric.Value = value;
			}
		}

		public List<AsperaEmailSection> EmailSections { get; private set; }

		public Button AddSenderButton { get; private set; } = new Button("Add Another Sender") { Width = 200 };

		public Button DeleteSenderSectionButton { get; private set; } = new Button("Delete") { Width = 200 };

		private void Initialize(Aspera aspera)
		{
			EmailSections = new List<AsperaEmailSection> { new AsperaEmailSection(AsperaType.Faspex, helpers, configuration) };

			if (aspera?.ParticipantsEmailAddress == null || aspera.ParticipantsEmailAddress.Length == 0) return;

			asperaWorkgroupDropdown.Selected = aspera.Workgroup;
			validDaysNumeric.Value = aspera.ValidDays;
			EmailSections = aspera.SendersEmails.Select(s => new AsperaEmailSection(AsperaType.Faspex, helpers, configuration, s)).ToList();
		}

		private void AsperaWorkgroupDropdown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			Workgroup = EnumExtensions.GetEnumValueFromDescription<AsperaWorkgroup>(e.Selected);
		}

		private void AddSenderButton_Pressed(object sender, EventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				EmailSections.Add(new AsperaEmailSection(AsperaType.Faspex, helpers, configuration));

				InvokeRegenerateUi();

				IsValid(OrderAction.Book);
			}
		}

		private void RemoveSenderButton_Pressed(object sender, EventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				int lastItemIndex = EmailSections.Count - 1;
				if (lastItemIndex <= 0) return;

				EmailSections.RemoveAt(lastItemIndex);

				InvokeRegenerateUi();

				IsValid(OrderAction.Book);
			}
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(asperafaspexTitleLabel, ++row, 0);

			AddWidget(asperaWorkgroupLabel, ++row, 0);
			AddWidget(asperaWorkgroupDropdown, row, 1, 1, 2);

			foreach (var emailSection in EmailSections)
			{
				AddSection(emailSection, new SectionLayout(++row, 0));
				row += emailSection.RowCount;
			}

			AddWidget(AddSenderButton, ++row, 0);
			AddWidget(DeleteSenderSectionButton, ++row, 0);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(validDaysLabel, ++row, 0);
			AddWidget(validDaysNumeric, row, 1, 1, 2);

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
		}

		public bool IsValid(OrderAction action)
		{
			bool emailSectionsAreValid = EmailSections.All(section => section.IsValid());
			bool areLinkDaysValid = ValidDays > 0;
			validDaysNumeric.ValidationState = areLinkDaysValid ? Automation.UIValidationState.Valid : Automation.UIValidationState.Invalid;
			validDaysNumeric.ValidationText = "Please provide a valid amount of days";

			return emailSectionsAreValid;
		}

		public void UpdateNonLiveOrder(Aspera asperaOrder)
		{
			asperaOrder.Workgroup = Workgroup.GetDescription();
			asperaOrder.ValidDays = ValidDays;
			asperaOrder.SendersEmails = EmailSections.Select(section => section.EmailAddress).ToArray();
		}

		public override void RegenerateUi()
		{
			EmailSections.ForEach(x => x.RegenerateUi());
			GenerateUi(out int row);
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			asperafaspexTitleLabel.IsVisible = IsVisible;

			asperaWorkgroupLabel.IsVisible = IsVisible;
			asperaWorkgroupDropdown.IsVisible = IsVisible;
			asperaWorkgroupDropdown.IsEnabled = IsEnabled;

			EmailSections.ForEach(x =>
			{
				x.IsVisible = IsVisible;
				x.IsEnabled = IsEnabled;
			});

			AddSenderButton.IsVisible = IsVisible;
			AddSenderButton.IsEnabled = IsEnabled;

			DeleteSenderSectionButton.IsVisible = IsVisible;
			DeleteSenderSectionButton.IsEnabled = IsEnabled;

			validDaysLabel.IsVisible = IsVisible;
			validDaysNumeric.IsVisible = IsVisible;
			validDaysNumeric.IsEnabled = IsEnabled;

			ToolTipHandler.SetTooltipVisibility(this);
		}
	}
}

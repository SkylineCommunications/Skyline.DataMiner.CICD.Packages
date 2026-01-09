namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.ExportOrder
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class IsilonBuMaterialExportSource : ExportSourceSection
	{
		private const string None = "None";

		private BackupOrigins selectedBackupOrigin = BackupOrigins.HELSINKI;

		private readonly Label title = new Label("Isilon BU") { Style = TextStyle.Bold };

		private readonly Label backupOriginLabel = new Label("Backup origin");
		private readonly DropDown backupOriginDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<BackupOrigins>(), EnumExtensions.GetDescriptionFromEnumValue(BackupOrigins.HELSINKI));

		private readonly Label productionDepartmentLabel = new Label("Production Department");
		private readonly DropDown productionDepartmentDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<HelsinkiProductionDepartmentNames>().Concat(new[] { None }).OrderBy(x => x), None);

		private readonly Label programNameLabel = new Label("Program name");
		private readonly YleTextBox programNameTextBox = new YleTextBox();

		private readonly Label vsaEpisodeNameLabel = new Label("Episode name");
		private readonly YleTextBox vsaEpisodeNameTextBox = new YleTextBox();

		private readonly Label backupStartLabel = new Label("Backup start");
		private readonly DateTimePicker backupStartDateTimePicker = new DateTimePicker(DateTime.Now.AddDays(1));

		private readonly Label backupEndLabel = new Label("Backup end");
		private readonly DateTimePicker backupEndDateTimePicker = new DateTimePicker(DateTime.Now.AddDays(1).AddMinutes(10));

		private readonly Label additionalInformationLabel = new Label("Additional Information");
		private readonly YleTextBox additionalInformationTextBox = new YleTextBox { IsMultiline = true, Height = 200 };

		public IsilonBuMaterialExportSource(Helpers helpers, ISectionConfiguration configuration, ExportMainSection section, Export export) : base(helpers, configuration, section, export)
		{
			backupOriginDropDown.Changed += BackupOriginDropDown_Changed;

            InitializeExport(export);

			GenerateUi(out int row);
		}

		public BackupOrigins BackupOrigin
		{
			get
			{
				return selectedBackupOrigin;
			}
			private set
			{
				selectedBackupOrigin = value;
				backupOriginDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(selectedBackupOrigin);
			}
		}

		public string ProgramName
		{
			get
			{
				return programNameTextBox.Text;
			}
			private set
			{
				programNameTextBox.Text = value;
			}
		}

		public string VsaEpisodeName
		{
			get
			{
				return vsaEpisodeNameTextBox.Text;
			}
			private set
			{
				vsaEpisodeNameTextBox.Text = value;
			}
		}

		public DateTime BackupStart
		{
			get
			{
				return backupStartDateTimePicker.DateTime;
			}
			private set
			{
				backupStartDateTimePicker.DateTime = value;
			}
		}

		public DateTime BackupEnd
		{
			get
			{
				return backupEndDateTimePicker.DateTime;
			}
			private set
			{
				backupEndDateTimePicker.DateTime = value;
			}
		}

		public string AdditionalInformation
		{
			get
			{
				return additionalInformationTextBox.Text;
			}
			private set
			{
				additionalInformationTextBox.Text = value;
			}
		}

		public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections
		{
			get
			{
				return new NonLiveManagerTreeViewSection[0];
			}
		}

		public override bool IsValid(OrderAction action)
		{
			if (action == OrderAction.Save) return true;

			bool isProgramNameValid = !String.IsNullOrWhiteSpace(ProgramName);
			programNameTextBox.ValidationState = isProgramNameValid ? UIValidationState.Valid : UIValidationState.Invalid;
			programNameTextBox.ValidationText = "Provide a program name";

			bool isVsaEpisodeNameValid = BackupOrigin != BackupOrigins.VAASA || !String.IsNullOrEmpty(VsaEpisodeName);
			vsaEpisodeNameTextBox.ValidationState = isVsaEpisodeNameValid ? UIValidationState.Valid : UIValidationState.Invalid;
			vsaEpisodeNameTextBox.ValidationText = "Provide an episode name";

			bool isProductionNameDepartmentValid = productionDepartmentDropDown.Selected != None;
			productionDepartmentDropDown.ValidationState = isProductionNameDepartmentValid ? UIValidationState.Valid : UIValidationState.Invalid;
			productionDepartmentDropDown.ValidationText = "Select production department";

			bool isIsilonBackUpTimeValid = backupStartDateTimePicker.DateTime < backupEndDateTimePicker.DateTime || BackupOrigin == BackupOrigins.VAASA;
			backupEndDateTimePicker.ValidationState = isIsilonBackUpTimeValid ? UIValidationState.Valid : UIValidationState.Invalid;
			backupEndDateTimePicker.ValidationText = "End date must be after start date";

			return isProgramNameValid
				&& isProductionNameDepartmentValid
				&& isVsaEpisodeNameValid
				&& isIsilonBackUpTimeValid;
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(title, ++row, 0);

			AddWidget(backupOriginLabel, ++row, 0);
			AddWidget(backupOriginDropDown, row, 1, 1, 2);

			AddWidget(productionDepartmentLabel, ++row, 0);
			AddWidget(productionDepartmentDropDown, row, 1, 1, 2);

			AddWidget(programNameLabel, ++row, 0);
			AddWidget(programNameTextBox, row, 1, 1, 2);

			AddWidget(vsaEpisodeNameLabel, ++row, 0);
			AddWidget(vsaEpisodeNameTextBox, row, 1, 1, 2);

			AddWidget(backupStartLabel, ++row, 0);
			AddWidget(backupStartDateTimePicker, row, 1, 1, 2);

			AddWidget(backupEndLabel, ++row, 0);
			AddWidget(backupEndDateTimePicker, row, 1, 1, 2);

			AddWidget(additionalInformationLabel, ++row, 0);
			AddWidget(additionalInformationTextBox, row, 1, 1, 2);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

		public override void UpdateExport(Export export)
		{
			export.IsilonBuExport = new IsilonBuExport
			{
				BackupOrigin = BackupOrigin,
				ProgramName = ProgramName,
				AdditionalInformation = AdditionalInformation,
				ProductionDepartmentName = productionDepartmentDropDown.Selected
			};

			if (BackupOrigin == BackupOrigins.VAASA)
			{
				export.IsilonBuExport.VsaEpisodeName = vsaEpisodeNameTextBox.Text;
			}
			else
			{
				export.IsilonBuExport.BackupStart = BackupStart;
				export.IsilonBuExport.BackupEnd = BackupEnd;
			}
		}

		public override void InitializeExport(Export export)
		{
			if (export == null || export.IsilonBuExport == null)
			{
				return;
			}

			if (export.IsilonBuExport.BackupOrigin.HasValue)
			{
				BackupOrigin = export.IsilonBuExport.BackupOrigin.Value;

				if (BackupOrigin == BackupOrigins.HELSINKI)
				{
					productionDepartmentDropDown.Options = EnumExtensions.GetEnumDescriptions<HelsinkiProductionDepartmentNames>().OrderBy(x => x);
				}
				else if (BackupOrigin == BackupOrigins.VAASA)
				{
					productionDepartmentDropDown.Options = EnumExtensions.GetEnumDescriptions<VaasaProductionDepartmentNames>().OrderBy(x => x);
				}
				else if (BackupOrigin == BackupOrigins.TAMPERE)
				{
					productionDepartmentDropDown.Options = EnumExtensions.GetEnumDescriptions<TampereProductionDepartmentNames>().OrderBy(x => x);
				}
				productionDepartmentDropDown.Selected = export.IsilonBuExport.ProductionDepartmentName;
			}

			ProgramName = export.IsilonBuExport.ProgramName;
			VsaEpisodeName = export.IsilonBuExport.VsaEpisodeName;
			BackupStart = export.IsilonBuExport.BackupStart;
			BackupEnd = export.IsilonBuExport.BackupEnd;
			AdditionalInformation = export.IsilonBuExport.AdditionalInformation;
		}

		private void BackupOriginDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				BackupOrigin = EnumExtensions.GetEnumValueFromDescription<BackupOrigins>(e.Selected);

				if (BackupOrigin == BackupOrigins.HELSINKI)
				{
					productionDepartmentDropDown.Options = EnumExtensions.GetEnumDescriptions<HelsinkiProductionDepartmentNames>().Concat(new[] { None }).OrderBy(x => x);
					productionDepartmentDropDown.Selected = None;
				}
				else if (BackupOrigin == BackupOrigins.VAASA)
				{
					productionDepartmentDropDown.Options = EnumExtensions.GetEnumDescriptions<VaasaProductionDepartmentNames>().Concat(new[] { None }).OrderBy(x => x);
					productionDepartmentDropDown.Selected = None;
				}
				else if (BackupOrigin == BackupOrigins.TAMPERE)
				{
					productionDepartmentDropDown.Options = EnumExtensions.GetEnumDescriptions<TampereProductionDepartmentNames>().Concat(new[] { None }).OrderBy(x => x);
					productionDepartmentDropDown.Selected = None;
				}

				VsaEpisodeName = String.Empty;
				HandleVisibilityAndEnabledUpdate();
			}
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			title.IsVisible = IsVisible;

			backupOriginLabel.IsVisible = IsVisible;
			backupOriginDropDown.IsVisible = IsVisible;
			backupOriginDropDown.IsEnabled = IsEnabled;

			productionDepartmentLabel.IsVisible = IsVisible;
			productionDepartmentDropDown.IsVisible = IsEnabled;
			productionDepartmentDropDown.IsEnabled = IsEnabled;

			programNameLabel.IsVisible = IsVisible;
			programNameTextBox.IsVisible = IsVisible;
			programNameTextBox.IsEnabled = IsEnabled;

			vsaEpisodeNameLabel.IsVisible = IsVisible && BackupOrigin == BackupOrigins.VAASA;
			vsaEpisodeNameTextBox.IsVisible = vsaEpisodeNameLabel.IsVisible;
			vsaEpisodeNameTextBox.IsEnabled = IsEnabled;

			backupStartLabel.IsVisible = IsVisible && BackupOrigin != BackupOrigins.VAASA;
			backupStartDateTimePicker.IsVisible = backupStartLabel.IsVisible;
			backupStartDateTimePicker.IsEnabled = IsEnabled;

			backupEndLabel.IsVisible = IsVisible && BackupOrigin != BackupOrigins.VAASA;
			backupEndDateTimePicker.IsVisible = backupEndLabel.IsVisible;
			backupEndDateTimePicker.IsEnabled = IsEnabled;

			additionalInformationLabel.IsVisible = IsVisible;
			additionalInformationTextBox.IsVisible = IsVisible;
			additionalInformationTextBox.IsEnabled = IsEnabled;

            ToolTipHandler.SetTooltipVisibility(this);
        }

		public override void RegenerateUi()
		{
			GenerateUi(out int row);
		}
	}
}

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.Import
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    public class MaterialBackupDetailsSection : ImportSubSection
	{
		private readonly Label materialBackupTitleLabel = new Label("Material Backup Details") { Style = TextStyle.Bold };

		private readonly Label backUpsLongerStoredLabel = new Label("Do you want to store your backups longer than one year?");
		private readonly CheckBox backUpsLongerStoredCheckBox = new CheckBox { IsChecked = false };

		private readonly Label backUpDeletionDateLabel = new Label("Backup deletion date");
		private readonly YleDateTimePicker backUpDeletionDatePicker;

		private readonly Label whyBackUpLongerStoredLabel = new Label("Why must the backup be stored longer?");
		private readonly YleTextBox whyBackUpLongerStoredTextBox = new YleTextBox { IsMultiline = true, Height = 50 };

		public MaterialBackupDetailsSection(Helpers helpers, ISectionConfiguration configuration, ImportMainSection section, Ingest ingest = null) : base(helpers, configuration, section, ingest)
		{
			var nowPlusOneYear = DateTime.Now.AddYears(1);
			backUpDeletionDatePicker = new YleDateTimePicker(new DateTime(nowPlusOneYear.Year, nowPlusOneYear.Month, nowPlusOneYear.Day)) { DateTimeFormat = DateTimeFormat.ShortDate };

			if (ingest != null)
			{
				BackUpsLongerStored = ingest.BackUpsLongerStored ?? this.BackUpsLongerStored;
				BackupDeletionDate = ingest.BackupDeletionDate;
				WhyBackUpLongerStored = ingest.WhyBackUpLongerStored ?? this.WhyBackUpLongerStored;
			}

            backUpsLongerStoredCheckBox.Changed += BackUpsLongerStoredCheckBox_Changed;

            GenerateUi(out int row);
		}

		public DateTime BackupDeletionDate
		{
			get => backUpDeletionDatePicker.DateTime;
			private set => backUpDeletionDatePicker.DateTime = value;
		}

		public bool? BackUpsLongerStored
		{
			get => backUpsLongerStoredCheckBox.IsChecked;
			private set => backUpsLongerStoredCheckBox.IsChecked = (bool)value;
		}

		public string WhyBackUpLongerStored
		{
			get => whyBackUpLongerStoredTextBox.Text;
			private set => whyBackUpLongerStoredTextBox.Text = value;
		}

		public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections => new NonLiveManagerTreeViewSection[0];

		public override bool IsValid()
		{
			bool isWhyBackUpLongerStoredValid = !backUpsLongerStoredCheckBox.IsChecked || !string.IsNullOrEmpty(WhyBackUpLongerStored);
			whyBackUpLongerStoredTextBox.ValidationState = isWhyBackUpLongerStoredValid ? UIValidationState.Valid : UIValidationState.Invalid;
			whyBackUpLongerStoredTextBox.ValidationText = "Provide a reason that the backup need to be stored longer";

			return isWhyBackUpLongerStoredValid;
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(materialBackupTitleLabel, new WidgetLayout(++row, 0));

			AddWidget(backUpsLongerStoredLabel, ++row, 0);
			AddWidget(backUpsLongerStoredCheckBox, row, 1, 1, 2);

			AddWidget(backUpDeletionDateLabel, ++row, 0);
			AddWidget(backUpDeletionDatePicker, row, 1);

			AddWidget(whyBackUpLongerStoredLabel, ++row, 0);
			AddWidget(whyBackUpLongerStoredTextBox, row, 1, 1, 2);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

		public override void UpdateIngest(Ingest ingest)
		{
			if (ingest != null)
			{
				ingest.BackUpsLongerStored = this.BackUpsLongerStored;
				ingest.BackupDeletionDate = this.BackupDeletionDate;
				ingest.WhyBackUpLongerStored = this.WhyBackUpLongerStored;
			}
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			materialBackupTitleLabel.IsVisible = IsVisible;

			backUpsLongerStoredLabel.IsVisible = IsVisible;
			backUpsLongerStoredCheckBox.IsVisible = IsVisible;
			backUpsLongerStoredCheckBox.IsEnabled = IsEnabled;

			backUpDeletionDateLabel.IsVisible = IsVisible && backUpsLongerStoredCheckBox.IsChecked;
			backUpDeletionDatePicker.IsVisible = IsVisible && backUpDeletionDateLabel.IsVisible;
			backUpDeletionDatePicker.IsEnabled = IsEnabled;

			whyBackUpLongerStoredLabel.IsVisible = IsVisible && backUpsLongerStoredCheckBox.IsChecked;
			whyBackUpLongerStoredTextBox.IsVisible = IsVisible && whyBackUpLongerStoredLabel.IsVisible;
			whyBackUpLongerStoredTextBox.IsEnabled = IsEnabled;

			ToolTipHandler.SetTooltipVisibility(this);
		}

		private void BackUpsLongerStoredCheckBox_Changed(object sender, CheckBox.CheckBoxChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				if (!e.IsChecked)
				{
					whyBackUpLongerStoredTextBox.Text = string.Empty;
					var nowPlusOneYear = DateTime.Now.AddYears(1);
					backUpDeletionDatePicker.DateTime = new DateTime(nowPlusOneYear.Year, nowPlusOneYear.Month, nowPlusOneYear.Day);
				}

				HandleVisibilityAndEnabledUpdate();
				IsValid();
			}
		}

		public override void RegenerateUi()
		{
			GenerateUi(out int row);
		}
	}
}

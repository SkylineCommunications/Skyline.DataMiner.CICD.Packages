namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.IplayFolderCreation
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class NewEpisodeFolderRequestSection : FolderCreationSubSection
	{
		private readonly Label title = new Label("New episode folder request") { Style = TextStyle.Heading };

		private readonly Label deleteDateLabel = new Label("Delete date");
		private readonly DateTimePicker deleteDateDatePicker;

		private readonly Label producerNameLabel = new Label("Producer name");
		private readonly YleTextBox producerNameTextBox = new YleTextBox();

		private readonly Label producerEmailLabel = new Label("Producer Email");
		private readonly YleTextBox producerEmailTextBox = new YleTextBox();

		private readonly Label mediaManagerNameLabel = new Label("Media manager name");
		private readonly YleTextBox mediaManagerNameTextBox = new YleTextBox();

		private readonly Label mediaManagerEmailLabel = new Label("Media manager email");
		private readonly YleTextBox mediaManagerEmailTextBox = new YleTextBox();

		private readonly Label tuotantonumeroLabel = new Label("Product or production number");
		private readonly YleTextBox tuotantonumeroTextBox = new YleTextBox();

		private readonly Label episodeNumberOrNameLabel = new Label("Episode number or name");
		private readonly YleTextBox episodeNumberOrNameTextBox = new YleTextBox();

		private readonly Button deleteButton = new Button("Delete") { Width = 150 };

		public NewEpisodeFolderRequestSection(Helpers helpers, ISectionConfiguration configuration, FolderCreationSection section, NewEpisodeFolderRequestDetails details = null) : base(helpers, configuration, section)
		{
			DateTime nowPlusTwoMonths = DateTime.Now.AddMonths(2);
			deleteDateDatePicker = new YleDateTimePicker(new DateTime(nowPlusTwoMonths.Year, nowPlusTwoMonths.Month, nowPlusTwoMonths.Day)) { DateTimeFormat = DateTimeFormat.ShortDate };
            deleteDateDatePicker.Changed += DeleteDateDatePicker_Changed;

			if (details != null)
			{
				DeleteDate = details.DeleteDate;
				ProducerName = details.ProducerName;
				ProducerEmail = details.ProducerEmail;
                MediaManagerName = details.MediaManagerName;
				MediaManagerEmail = details.MediaManagerEmail;
				Tuotantonumero = details.ProductOrProductionName;
				EpisodeNumberOrName = details.EpisodeNumberOrName;
			}

            deleteButton.Pressed += section.DeleteButton_Pressed;

            GenerateUi(out int row);
            IsValid();
		}

        [Copy]
		public DateTime DeleteDate
		{
			get
			{
				return deleteDateDatePicker.DateTime;
			}
			private set
			{
				deleteDateDatePicker.DateTime = value;
			}
		}

		[Copy]
		public string ProducerName
		{
			get
			{
				return producerNameTextBox.Text;
			}
			private set
			{
				producerNameTextBox.Text = value;
			}
		}

		[Copy]
		public string ProducerEmail
		{
			get
			{
				return producerEmailTextBox.Text;
			}
			private set
			{
				producerEmailTextBox.Text = value;
			}
		}

		[Copy]
		public string MediaManagerName
		{
			get
			{
				return mediaManagerNameTextBox.Text;
			}
			private set
			{
				mediaManagerNameTextBox.Text = value;
			}
		}

		[Copy]
		public string MediaManagerEmail
		{
			get
			{
				return mediaManagerEmailTextBox.Text;
			}
			private set
			{
				mediaManagerEmailTextBox.Text = value;
			}
		}

		public string Tuotantonumero
		{
			get
			{
				return tuotantonumeroTextBox.Text;
			}
			private set
			{
				tuotantonumeroTextBox.Text = value;
			}
		}

		public string EpisodeNumberOrName
		{
			get
			{
				return episodeNumberOrNameTextBox.Text;
			}
			private set
			{
				episodeNumberOrNameTextBox.Text = value;
			}
		}

		public Button DeleteButton
		{
			get
			{
				return deleteButton;
			}
		}

		public override bool IsValid()
		{
			bool isProducerEmailValid = !String.IsNullOrWhiteSpace(ProducerEmail);
			producerEmailTextBox.ValidationState = isProducerEmailValid ? UIValidationState.Valid : UIValidationState.Invalid;
			producerEmailTextBox.ValidationText = "Provide the email of the producer";

			bool isMediaManagerEmailValid = !String.IsNullOrWhiteSpace(MediaManagerEmail);
			mediaManagerEmailTextBox.ValidationState = isMediaManagerEmailValid ? UIValidationState.Valid : UIValidationState.Invalid;
			mediaManagerEmailTextBox.ValidationText = "Provide the email of the media manager";

			bool isTuotantonumeroOrProductNumberValid = !String.IsNullOrWhiteSpace(Tuotantonumero);
			tuotantonumeroTextBox.ValidationState = isTuotantonumeroOrProductNumberValid ? UIValidationState.Valid : UIValidationState.Invalid;
			tuotantonumeroTextBox.ValidationText = "Provide a tuotantonumero/production number";

			bool isEpisodeNumberValid = !String.IsNullOrWhiteSpace(EpisodeNumberOrName);
			episodeNumberOrNameTextBox.ValidationState = isEpisodeNumberValid ? UIValidationState.Valid : UIValidationState.Invalid;
			episodeNumberOrNameTextBox.ValidationText = "Provide a episode number";

			return isProducerEmailValid
				&& isMediaManagerEmailValid
				&& isTuotantonumeroOrProductNumberValid
				&& isEpisodeNumberValid;
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(title, ++row, 0);

			AddWidget(deleteDateLabel, ++row, 0);
			AddWidget(deleteDateDatePicker, row, 1);

			AddWidget(producerNameLabel, ++row, 0);
			AddWidget(producerNameTextBox, row, 1, 1, 2);

			AddWidget(producerEmailLabel, ++row, 0);
			AddWidget(producerEmailTextBox, row, 1, 1, 2);

			AddWidget(mediaManagerNameLabel, ++row, 0);
			AddWidget(mediaManagerNameTextBox, row, 1, 1, 2);

			AddWidget(mediaManagerEmailLabel, ++row, 0);
			AddWidget(mediaManagerEmailTextBox, row, 1, 1, 2);

			AddWidget(tuotantonumeroLabel, ++row, 0);
			AddWidget(tuotantonumeroTextBox, row, 1, 1, 2);

			AddWidget(episodeNumberOrNameLabel, ++row, 0);
			AddWidget(episodeNumberOrNameTextBox, row, 1, 1, 2);

			AddWidget(deleteButton, row + 1, 1);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

		public override void UpdateFolderCreation(FolderCreation folderCreation)
		{
            folderCreation.NewEpisodeFolderRequestDetails?.Add(new NewEpisodeFolderRequestDetails
            {
                DeleteDate = section.GeneralInfoSection.Destination != AvidInterplayPAM.InterplayPamElements.Vaasa ? deleteDateDatePicker.DateTime : default,
				ProducerName = ProducerName,
				ProducerEmail = ProducerEmail,
				MediaManagerName = MediaManagerName,
				MediaManagerEmail = MediaManagerEmail,
				ProductOrProductionName = Tuotantonumero,
				EpisodeNumberOrName = EpisodeNumberOrName
			});
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			title.IsVisible = IsVisible;

			deleteDateLabel.IsVisible = section.GeneralInfoSection.Destination != AvidInterplayPAM.InterplayPamElements.Vaasa;
			deleteDateDatePicker.IsVisible = deleteDateLabel.IsVisible;
			deleteDateDatePicker.IsEnabled = IsEnabled;

			producerNameLabel.IsVisible = IsVisible;
			producerNameTextBox.IsVisible = IsVisible;
			producerNameTextBox.IsEnabled = IsEnabled;

			producerEmailLabel.IsVisible = IsVisible;
			producerEmailTextBox.IsEnabled = IsEnabled;
			producerEmailTextBox.IsVisible = IsVisible;

			mediaManagerNameLabel.IsVisible = IsVisible;
			mediaManagerNameTextBox.IsVisible = IsVisible;
			mediaManagerNameTextBox.IsEnabled = IsEnabled;

			mediaManagerEmailLabel.IsVisible = IsVisible;
			mediaManagerEmailTextBox.IsEnabled = IsEnabled;
			mediaManagerEmailTextBox.IsVisible = IsVisible;

			tuotantonumeroLabel.IsVisible = IsVisible;
			tuotantonumeroTextBox.IsVisible = IsVisible;
			tuotantonumeroTextBox.IsEnabled = IsEnabled;

			episodeNumberOrNameLabel.IsVisible = IsVisible;
			episodeNumberOrNameTextBox.IsVisible = IsVisible;
			episodeNumberOrNameTextBox.IsEnabled = IsEnabled;

			deleteButton.IsVisible = section.NewProgramFolderRequestSection != null || section.NewEpisodeFolderRequestSections.Count > 1;
			deleteButton.IsEnabled = IsEnabled;

            ToolTipHandler.SetTooltipVisibility(this);
        }

		private void DeleteDateDatePicker_Changed(object sender, DateTimePicker.DateTimePickerChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				if (section.NewProgramFolderRequestSection != null)
				{
					var allEpisodeDeleteDates = section.NewEpisodeFolderRequestSections.Select(x => x.DeleteDate).ToList();

					section.NewProgramFolderRequestSection.DeleteDate = allEpisodeDeleteDates.Max();
				}
			}
		}

		public override void RegenerateUi()
		{
			GenerateUi(out int row);
		}
	}
}

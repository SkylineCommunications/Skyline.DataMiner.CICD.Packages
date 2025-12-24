namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.IplayFolderCreation
{
	using System;
    using System.Linq;
    using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    public class NewProgramFolderRequestSection : FolderCreationSubSection
	{
		private readonly Label title = new Label("New Program Folder Request") { Style = TextStyle.Heading };

		private readonly Label programNameLabel = new Label("Program name");
		private readonly YleTextBox programNameTextBox = new YleTextBox();

		private readonly Label programShortNameLabel = new Label("Program short name");
		private readonly YleTextBox programShortNameTextBox = new YleTextBox();

		private readonly Label deleteDateLabel = new Label("Delete date");
		private readonly CheckBox deleteDateCheckBox = new CheckBox("unknown");
		private readonly DateTimePicker deleteDateDatePicker;

		private readonly Label producerNameLabel = new Label("Producer name");
		private readonly YleTextBox producerNameTextBox = new YleTextBox();

		private readonly Label producerEmailLabel = new Label("Producer Email");
		private readonly YleTextBox producerEmailTextBox = new YleTextBox();

		private readonly Label mediaManagerNameLabel = new Label("Media manager name");
		private readonly YleTextBox mediaManagerNameTextBox = new YleTextBox();

		private readonly Label mediaManagerEmailLabel = new Label("Media manager email");
		private readonly YleTextBox mediaManagerEmailTextBox = new YleTextBox();

		private readonly Label productNumberLabel = new Label("Product number");
		private readonly YleTextBox productNumberTextBox = new YleTextBox();

		public NewProgramFolderRequestSection(Helpers helpers, ISectionConfiguration configuration, FolderCreationSection section, NewProgramFolderRequestDetails details = null) : base(helpers, configuration, section)
		{
			DateTime twoMonthsFromNow = DateTime.Now.AddMonths(2);
			deleteDateDatePicker = new YleDateTimePicker(new DateTime(twoMonthsFromNow.Year, twoMonthsFromNow.Month, twoMonthsFromNow.Day)) { DateTimeFormat = DateTimeFormat.ShortDate };

            deleteDateDatePicker.Changed += (o, e) => IsValid();
			deleteDateCheckBox.Changed += (o, e) => HandleVisibilityAndEnabledUpdate();

			if (details != null)
			{
				ProgramName = details.ProgramName;
				ProgramShortName = details.ProgramShortName;
				deleteDateDatePicker.DateTime = details.DeleteDate != default ? details.DeleteDate : twoMonthsFromNow;
				ProducerName = details.ProducerName;
				ProducerEmail = details.ProducerEmail;
				MediaResponsibleName = details.MediaManagerName;
				MediaResponsibleEmail = details.MediaManagerEmail;
				ProductNumber = details.ProductNumber;
                deleteDateCheckBox.IsChecked = details.IsDeleteDateUnknown;
            }

            GenerateUi(out int row);
			IsValid();
		}

        public new bool IsEnabled
		{
			get => base.IsEnabled;
			set
			{
				base.IsEnabled = value;
				HandleVisibilityAndEnabledUpdate();
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

		public string ProgramShortName
		{
			get
			{
				return programShortNameTextBox.Text;
			}
			private set
			{
				programShortNameTextBox.Text = value;
			}
		}

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

		public string MediaResponsibleName
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

		public string MediaResponsibleEmail
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

		public string ProductNumber
		{
			get
			{
				return productNumberTextBox.Text;
			}
			private set
			{
				productNumberTextBox.Text = value;
			}
		}

		public DateTime DeleteDate
		{
			get
			{
				return deleteDateDatePicker.DateTime;
			}
			internal set
			{
				deleteDateDatePicker.DateTime = value;
			}
		}

		public override bool IsValid()
		{
			bool isProgramNameValid = !String.IsNullOrWhiteSpace(ProgramName);
			programNameTextBox.ValidationState = isProgramNameValid ? UIValidationState.Valid : UIValidationState.Invalid;
			programNameTextBox.ValidationText = "Provide a program name";

			bool isProgramShortNameValid = programShortNameTextBox.Text.Length != 0 && programShortNameTextBox.Text.Length == 3;
			programShortNameTextBox.ValidationState = isProgramShortNameValid ? UIValidationState.Valid : UIValidationState.Invalid;
			programShortNameTextBox.ValidationText = "Provide a program short name with 3 characters";

			bool isNamingValid = isProgramNameValid && isProgramShortNameValid;

			bool isProducerEmailValid = !String.IsNullOrWhiteSpace(ProducerEmail);
			producerEmailTextBox.ValidationState = isProducerEmailValid ? UIValidationState.Valid : UIValidationState.Invalid;
			producerEmailTextBox.ValidationText = "Provide the email of the producer";

			bool isMediaResponsibleEmailValid = !String.IsNullOrWhiteSpace(MediaResponsibleEmail);
			mediaManagerEmailTextBox.ValidationState = isMediaResponsibleEmailValid ? UIValidationState.Valid : UIValidationState.Invalid;
			mediaManagerEmailTextBox.ValidationText = "Provide the email of the media responsible";

			bool isProductNumberValid = !String.IsNullOrWhiteSpace(ProductNumber);
			productNumberTextBox.ValidationState = isProductNumberValid ? UIValidationState.Valid : UIValidationState.Invalid;
			productNumberTextBox.ValidationText = "Provide the product number";

			bool isTechnicalInformationValid = isProducerEmailValid && isMediaResponsibleEmailValid && isProductNumberValid;

			bool isDeleteDateValid = !section.NewEpisodeFolderRequestSections.Any() || section.NewEpisodeFolderRequestSections.Select(episodeSection => episodeSection.DeleteDate).Max() <= DeleteDate;
			deleteDateDatePicker.ValidationState = isDeleteDateValid ? UIValidationState.Valid : UIValidationState.Invalid;
			deleteDateDatePicker.ValidationText = "Delete date must be at least the same as the most future episode delete date";

			return isNamingValid
				&& isTechnicalInformationValid
				&& isDeleteDateValid;
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(title, ++row, 0);

			AddWidget(programNameLabel, ++row, 0);
			AddWidget(programNameTextBox, row, 1, 1, 2);

			AddWidget(programShortNameLabel, ++row, 0);
			AddWidget(programShortNameTextBox, row, 1, 1, 2);

			AddWidget(deleteDateLabel, ++row, 0);
			AddWidget(deleteDateDatePicker, row, 1);
			AddWidget(deleteDateCheckBox, row, 2);

			AddWidget(producerNameLabel, ++row, 0);
			AddWidget(producerNameTextBox, row, 1, 1, 2);

			AddWidget(producerEmailLabel, ++row, 0);
			AddWidget(producerEmailTextBox, row, 1, 1, 2);

			AddWidget(mediaManagerNameLabel, ++row, 0);
			AddWidget(mediaManagerNameTextBox, row, 1, 1, 2);

			AddWidget(mediaManagerEmailLabel, ++row, 0);
			AddWidget(mediaManagerEmailTextBox, row, 1, 1, 2);

			AddWidget(productNumberLabel, ++row, 0);
			AddWidget(productNumberTextBox, row, 1, 1, 2);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
		}

		public override void UpdateFolderCreation(FolderCreation folderCreation)
		{
            folderCreation.NewProgramFolderRequestDetails = new NewProgramFolderRequestDetails
            {
                ProgramName = ProgramName,
                ProgramShortName = ProgramShortName,
                DeleteDate = !deleteDateCheckBox.IsChecked && section.GeneralInfoSection.Destination != AvidInterplayPAM.InterplayPamElements.Vaasa ? deleteDateDatePicker.DateTime : default,
                ProducerName = ProducerName,
                ProducerEmail = ProducerEmail,
                MediaManagerName = MediaResponsibleName,
                MediaManagerEmail = MediaResponsibleEmail,
                ProductNumber = ProductNumber,
                IsDeleteDateUnknown = deleteDateCheckBox.IsChecked
			};
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			title.IsVisible = IsVisible;

			programNameLabel.IsVisible = IsVisible;
			programNameTextBox.IsVisible = IsVisible;
			productNumberTextBox.IsEnabled = IsEnabled;

			programShortNameLabel.IsVisible = IsVisible;
			programShortNameTextBox.IsVisible = IsVisible;
			programShortNameTextBox.IsEnabled = IsEnabled;

			deleteDateLabel.IsVisible = section.GeneralInfoSection.Destination != AvidInterplayPAM.InterplayPamElements.Vaasa;
			deleteDateDatePicker.IsVisible = deleteDateLabel.IsVisible;
			deleteDateDatePicker.IsEnabled = !deleteDateCheckBox.IsChecked;
			deleteDateCheckBox.IsVisible = deleteDateLabel.IsVisible;
			deleteDateCheckBox.IsEnabled = IsEnabled;

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

			productNumberLabel.IsVisible = IsVisible;
			productNumberTextBox.IsVisible = IsVisible;
			productNumberTextBox.IsEnabled = IsEnabled;

			ToolTipHandler.SetTooltipVisibility(this);		
        }

		public override void RegenerateUi()
		{
			GenerateUi(out int row);
		}
	}
}

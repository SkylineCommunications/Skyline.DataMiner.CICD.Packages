namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.Import
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.Net;

	public class CardImportDetailsSection : ImportSubSection
	{
		private CameraOrAudioTypes selectedCameraType = CameraOrAudioTypes.SonyFS;

		private readonly Label cameraOrAudioTypeLabel = new Label("Camera or Audio source");
		private readonly DropDown cameraOrAudioTypeDropDown = new DropDown();

		private readonly Label customCameraTypeLabel = new Label("Custom camera field");
		private readonly YleTextBox customCameraTypeTextBox = new YleTextBox { IsMultiline = true, Height = 50 };

		private readonly Label materialIncludesHighFrameRateLabel = new Label("Material includes high frame rate");
		private readonly CheckBox materialIncludesHighFrameRateCheckBox = new CheckBox { IsChecked = false };

		private readonly Label additionalInfoAboutHighFrameRateLabel = new Label("Additional info about high frame rate footage");
		private readonly YleTextBox additionalInfoAboutHighFrameRateTextBox = new YleTextBox { IsMultiline = true, Height = 200, PlaceHolder = "Please give the names of the clips that have special frame rate and the purpose for it (are supposed to be in slow motion or fast motion after the import)." };

		private readonly Label numberOfSimilarCardsLabel = new Label("Number of similar cards");
		private readonly Numeric numberOfSimilarCardsNumeric = new Numeric { Decimals = 0, Minimum = 0, Value = 1 };

		private readonly Label cardCanBeReusedLabel = new Label("Cards can be returned to kalustovarasto");
		private readonly CheckBox cardCanBeReusedCheckBox = new CheckBox { IsChecked = true };

		private readonly Label cardsToBeReturnedLabel = new Label("Which cards are to be returned to orderer?");
		private readonly YleTextBox cardsToBeReturnedTextBox = new YleTextBox { IsMultiline = true, Height = 100 };

        private readonly Label locationOfCardsToBeReturnedLabel = new Label("Where should the cards be returned to?");
        private readonly DropDown locationOfCardsToBeReturnedDropdown = new DropDown();

        private readonly Label nameOfRecipientLabel = new Label("Name of the recipient");
        private readonly YleTextBox nameOfTheRecipientTextbox = new YleTextBox();

        private readonly Label plNumberForCardsreturnLabel = new Label("PL number");
        private readonly YleTextBox plNumberForCardsreturnTextbox = new YleTextBox();

		private readonly Label additionalInformationLabel = new Label("Additional Information");
		private readonly YleTextBox additionalInformationTextBox = new YleTextBox { IsMultiline = true, Height = 100, PlaceHolder = "Provide some additional information about the request." };

		public CardImportDetailsSection(Helpers helpers, ISectionConfiguration configuration, ImportMainSection section, CardIngestDetails cardIngestDetail = null) : base(helpers, configuration, section, null)
		{
            CardIngestDetail = cardIngestDetail;

			cameraOrAudioTypeDropDown.Changed += CameraOrAudioTypeDropDown_Changed;
			materialIncludesHighFrameRateCheckBox.Changed += MaterialIncludesHighFrameRateCheckBox_Changed;
			cardCanBeReusedCheckBox.Changed += CardCanBeReusedCheckBox_Changed;
            locationOfCardsToBeReturnedDropdown.Changed += (o, e) => HandleVisibilityAndEnabledUpdate();

            InitializeCardIngestDetails();

			GenerateUi(out int row);
		}

		public CameraOrAudioTypes CameraOrAudioType
		{
			get
			{
				return selectedCameraType;
			}

			private set
			{
				selectedCameraType = value;
				cameraOrAudioTypeDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(selectedCameraType);
			}
		}

		public string CustomCameraType
		{
			get
			{
				return customCameraTypeTextBox.Text;
			}

			private set
			{
				customCameraTypeTextBox.Text = value;
			}
		}

		public bool MaterialIncludesHighFrameRateFootage
		{
			get
			{
				return materialIncludesHighFrameRateCheckBox.IsChecked;
			}

			private set
			{
				materialIncludesHighFrameRateCheckBox.IsChecked = value;
			}
		}

		public string AdditionalInfoAboutHighFrameRateFootage
		{
			get
			{
				return additionalInfoAboutHighFrameRateTextBox.Text;
			}

			private set
			{
				additionalInfoAboutHighFrameRateTextBox.Text = value;
			}
		}

		public int NumberOfSimilarCards
		{
			get
			{
				return Convert.ToInt32(numberOfSimilarCardsNumeric.Value);
			}

			private set
			{
				numberOfSimilarCardsNumeric.Value = value;
			}
		}

		public bool CardCanBeReused
		{
			get
			{
				return cardCanBeReusedCheckBox.IsChecked;
			}

			private set
			{
				cardCanBeReusedCheckBox.IsChecked = value;
			}
		}

		public string CardsToBeReturned
		{
			get
			{
				return cardsToBeReturnedTextBox.Text;
			}

			private set
			{
				cardsToBeReturnedTextBox.Text = value;
			}
		}

        internal CardIngestDetails CardIngestDetail { get; private set; }

        public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections
		{
			get
			{
				return new NonLiveManagerTreeViewSection[0];
			}
		}

		public override bool IsValid()
        { 
			bool cameraTypeIsValid = true;

			if (CameraOrAudioType == CameraOrAudioTypes.OTHER)
			{
				cameraTypeIsValid = !String.IsNullOrWhiteSpace(CustomCameraType);
			}

			customCameraTypeTextBox.ValidationState = cameraTypeIsValid ? UIValidationState.Valid : UIValidationState.Invalid;
			customCameraTypeTextBox.ValidationText = "Provide a custom camera type";

			bool highFrameRateInfoIsValid = !MaterialIncludesHighFrameRateFootage || !String.IsNullOrWhiteSpace(AdditionalInfoAboutHighFrameRateFootage);

			additionalInfoAboutHighFrameRateTextBox.ValidationState = highFrameRateInfoIsValid ? UIValidationState.Valid : UIValidationState.Invalid;
			additionalInfoAboutHighFrameRateTextBox.ValidationText = "Provide additional info";

			bool isOtherCardsToBeReturnedValid = ingestMainSection.IngestDestination != InterplayPamElements.Helsinki || (bool)CardCanBeReused || !String.IsNullOrEmpty(CardsToBeReturned); 
			cardsToBeReturnedTextBox.ValidationState = isOtherCardsToBeReturnedValid ? UIValidationState.Valid : UIValidationState.Invalid;
			cardsToBeReturnedTextBox.ValidationText = "Provide cards to be returned";

			return cameraTypeIsValid && highFrameRateInfoIsValid && isOtherCardsToBeReturnedValid;
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(cameraOrAudioTypeLabel, ++row, 0);
			AddWidget(cameraOrAudioTypeDropDown, row, 1, 1, 2);

			AddWidget(customCameraTypeLabel, ++row, 0);
			AddWidget(customCameraTypeTextBox, row, 1, 1, 2);

			AddWidget(materialIncludesHighFrameRateLabel, ++row, 0);
			AddWidget(materialIncludesHighFrameRateCheckBox, row, 1, 1, 2);

			AddWidget(additionalInfoAboutHighFrameRateLabel, ++row, 0);
			AddWidget(additionalInfoAboutHighFrameRateTextBox, row, 1, 1, 2);

			AddWidget(numberOfSimilarCardsLabel, ++row, 0);
			AddWidget(numberOfSimilarCardsNumeric, row, 1, 1, 2);

			AddWidget(cardCanBeReusedLabel, ++row, 0);
			AddWidget(cardCanBeReusedCheckBox, row, 1, 1, 2);

			AddWidget(cardsToBeReturnedLabel, ++row, 0);
			AddWidget(cardsToBeReturnedTextBox, row, 1, 1, 2);

            AddWidget(locationOfCardsToBeReturnedLabel, ++row, 0);
			AddWidget(locationOfCardsToBeReturnedDropdown, row, 1, 1, 2);

            AddWidget(nameOfRecipientLabel, ++row, 0);
			AddWidget(nameOfTheRecipientTextbox, row, 1, 1, 2);

            AddWidget(plNumberForCardsreturnLabel, ++row, 0);
			AddWidget(plNumberForCardsreturnTextbox, row, 1, 1, 2);

            AddWidget(additionalInformationLabel, ++row, 0);
			AddWidget(additionalInformationTextBox, row, 1, 1, 2);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			customCameraTypeLabel.IsVisible = CameraOrAudioType == CameraOrAudioTypes.OTHER;
			customCameraTypeTextBox.IsVisible = customCameraTypeLabel.IsVisible;

			materialIncludesHighFrameRateLabel.IsVisible = CameraOrAudioType != CameraOrAudioTypes.AudioCard;
			materialIncludesHighFrameRateCheckBox.IsVisible = materialIncludesHighFrameRateLabel.IsVisible;

			additionalInfoAboutHighFrameRateLabel.IsVisible = MaterialIncludesHighFrameRateFootage;
			additionalInfoAboutHighFrameRateTextBox.IsVisible = additionalInfoAboutHighFrameRateLabel.IsVisible;

			cardCanBeReusedLabel.IsVisible = ingestMainSection.IngestDestination == InterplayPamElements.Helsinki;
			cardCanBeReusedCheckBox.IsVisible = cardCanBeReusedLabel.IsVisible;

			cardsToBeReturnedLabel.IsVisible = ingestMainSection.IngestDestination == InterplayPamElements.Helsinki && !cardCanBeReusedCheckBox.IsChecked;
			cardsToBeReturnedTextBox.IsVisible = cardsToBeReturnedLabel.IsVisible;

            locationOfCardsToBeReturnedLabel.IsVisible = ingestMainSection.IngestDestination == InterplayPamElements.Helsinki && !cardCanBeReusedCheckBox.IsChecked;
            locationOfCardsToBeReturnedDropdown.IsVisible = locationOfCardsToBeReturnedLabel.IsVisible;

            nameOfRecipientLabel.IsVisible = locationOfCardsToBeReturnedDropdown.IsVisible && locationOfCardsToBeReturnedDropdown.Selected == EnumExtensions.GetDescriptionFromEnumValue(LocationOfCardsToBeReturned.InternalMail);
            nameOfTheRecipientTextbox.IsVisible = nameOfRecipientLabel.IsVisible;

            plNumberForCardsreturnLabel.IsVisible = nameOfTheRecipientTextbox.IsVisible;
            plNumberForCardsreturnTextbox.IsVisible = plNumberForCardsreturnLabel.IsVisible;

            ToolTipHandler.SetTooltipVisibility(this);
        }

		public override void UpdateIngest(Ingest ingest)
		{
			ingest.CardImportDetails = ingest.CardImportDetails ?? new List<CardIngestDetails>();

            ingest.CardImportDetails.Add(new CardIngestDetails
            {
                CameraOrAudioType = EnumExtensions.GetDescriptionFromEnumValue(this.CameraOrAudioType),
                CustomCameraType = CustomCameraType,
                MaterialIncludesHighFrameRateFootage = this.MaterialIncludesHighFrameRateFootage,
                AdditionalInfoAboutHighFrameRateFootage = AdditionalInfoAboutHighFrameRateFootage,
                NumberOfSimilarCards = this.NumberOfSimilarCards,
                CardCanBeReused = CardCanBeReused,
                CardsToBeReturned = CardsToBeReturned,
                CardsToBeReturnedLocation = locationOfCardsToBeReturnedDropdown.Selected,
                CardsToBeReturnedNameOfTheRecipient = nameOfTheRecipientTextbox.Text,
                CardsToBeReturnedPlNumber = plNumberForCardsreturnTextbox.Text,
				AdditionalInformation = additionalInformationTextBox.Text
			});
		}

        internal void InitializeCameraOrAudioTypeDropdown()
        {
            cameraOrAudioTypeDropDown.Options = ingestMainSection.IngestDestination != InterplayPamElements.UA ? 
				EnumExtensions.GetEnumDescriptions<CameraOrAudioTypes>().OrderBy(x => x) : 
				EnumExtensions.GetEnumDescriptions<CameraOrAudioTypes>().Where(x => x != EnumExtensions.GetDescriptionFromEnumValue(CameraOrAudioTypes.SonyA7)).OrderBy(x => x);

            if (ingestMainSection.IngestDestination == InterplayPamElements.UA)
            {
				CameraOrAudioType = CameraOrAudioTypes.PanasonicP2;
            }
            else
            {
				CameraOrAudioType = CameraOrAudioTypes.SonyFS;
            }

            if (CardIngestDetail != null && cameraOrAudioTypeDropDown.Options.Any(o => o == CardIngestDetail.CameraOrAudioType))
            {
				CameraOrAudioType = EnumExtensions.GetEnumValueFromDescription<CameraOrAudioTypes>(CardIngestDetail.CameraOrAudioType);
            }
        }

        private void InitializeCardIngestDetails()
        {
            locationOfCardsToBeReturnedDropdown.Options = EnumExtensions.GetEnumDescriptions<LocationOfCardsToBeReturned>();
            locationOfCardsToBeReturnedDropdown.Selected = EnumExtensions.GetDescriptionFromEnumValue(LocationOfCardsToBeReturned.ValmiitKaapi);

            if (CardIngestDetail != null)
            {
                CameraOrAudioType = EnumExtensions.GetEnumValueFromDescription<CameraOrAudioTypes>(CardIngestDetail.CameraOrAudioType);
                CustomCameraType = String.IsNullOrWhiteSpace(CardIngestDetail.CustomCameraType) ? String.Empty : CardIngestDetail.CustomCameraType;
                MaterialIncludesHighFrameRateFootage = CardIngestDetail.MaterialIncludesHighFrameRateFootage;
                AdditionalInfoAboutHighFrameRateFootage = String.IsNullOrWhiteSpace(CardIngestDetail.AdditionalInfoAboutHighFrameRateFootage) ? String.Empty : CardIngestDetail.AdditionalInfoAboutHighFrameRateFootage;
                NumberOfSimilarCards = CardIngestDetail.NumberOfSimilarCards;
                CardCanBeReused = CardIngestDetail.CardCanBeReused;
                CardsToBeReturned = String.IsNullOrWhiteSpace(CardIngestDetail.CardsToBeReturned) ? String.Empty : CardIngestDetail.CardsToBeReturned;
                locationOfCardsToBeReturnedDropdown.Selected = CardIngestDetail.CardsToBeReturnedLocation;
                nameOfTheRecipientTextbox.Text = string.IsNullOrWhiteSpace(CardIngestDetail.CardsToBeReturnedNameOfTheRecipient) ? string.Empty : CardIngestDetail.CardsToBeReturnedNameOfTheRecipient;
                plNumberForCardsreturnTextbox.Text = string.IsNullOrWhiteSpace(CardIngestDetail.CardsToBeReturnedPlNumber) ? string.Empty : CardIngestDetail.CardsToBeReturnedPlNumber;
				additionalInformationTextBox.Text = String.IsNullOrWhiteSpace(CardIngestDetail.AdditionalInformation) ? String.Empty : CardIngestDetail.AdditionalInformation;
            }

            InitializeCameraOrAudioTypeDropdown();
        }

		private void CameraOrAudioTypeDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				CameraOrAudioType = EnumExtensions.GetEnumValueFromDescription<CameraOrAudioTypes>(e.Selected);
				customCameraTypeTextBox.Text = String.Empty;

				if (CameraOrAudioType == CameraOrAudioTypes.AudioCard) MaterialIncludesHighFrameRateFootage = false;

				HandleVisibilityAndEnabledUpdate();
				IsValid();
			}
		}

		private void MaterialIncludesHighFrameRateCheckBox_Changed(object sender, CheckBox.CheckBoxChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				MaterialIncludesHighFrameRateFootage = e.IsChecked;

				HandleVisibilityAndEnabledUpdate();
				IsValid();
			}
		}

		private void CardCanBeReusedCheckBox_Changed(object sender, CheckBox.CheckBoxChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				CardCanBeReused = e.IsChecked;

				if (e.IsChecked)
				{
					CardsToBeReturned = null;
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

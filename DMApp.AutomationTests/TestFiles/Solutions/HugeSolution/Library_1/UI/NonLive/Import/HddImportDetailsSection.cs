namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.Import
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class HddImportDetailsSection : ImportSubSection
	{
		private readonly Label sourceFolderLabel = new Label("Source folder(s)");
		private readonly YleTextBox sourceFolderTextBox = new YleTextBox { IsMultiline = true, PlaceHolder = "Please type folder name(s) here.", Height = 200 };

		private readonly Label materialIncludesHighFrameRateLabel = new Label("Material includes high frame rate");
		private readonly CheckBox materialIncludesHighFrameRateCheckBox = new CheckBox { IsChecked = false };

		private readonly Label additionalInfoAboutHighFrameRateLabel = new Label("Additional info about high frame rate footage");
		private readonly YleTextBox additionalInfoAboutHighFrameRateTextBox = new YleTextBox { IsMultiline = true, Height = 200, PlaceHolder = "Please give the names of the clips that have special frame rate and the purpose for it (are supposed to be in slow motion or fast motion after the import)." };

		private readonly Label materialIsAvidProjectLabel = new Label("Material is Avid Project");
		private readonly CheckBox materialIsAvidProjectCheckBox = new CheckBox { IsChecked = false };

		private readonly Label avidProjectNameLabel = new Label("Avid project name");
		private readonly YleTextBox avidProjectNameTextBox = new YleTextBox();

		private readonly Label hddIsEvsDiskLabel = new Label("HDD is EVS-disk");
		private readonly CheckBox hddIsEvsDiskCheckBox = new CheckBox { IsChecked = false };

		private readonly Label avidProjectAdditionalInformationLabel = new Label("Additional information");
		private readonly YleTextBox avidProjectAdditionalInformationTextBox = new YleTextBox { IsMultiline = true, Height = 200, PlaceHolder = "Please give the name of the sequences or bins that you would like to be imported." };

		private readonly Label avidProjectContactEditorInformationLabel = new Label("Contact information of editor");
		private readonly YleTextBox avidProjectContactEditorInformationTextBox = new YleTextBox { IsMultiline = true, Height = 50 };

		private readonly Label additionalInformationLabel = new Label("Additional Information");
		private readonly YleTextBox additionalInformationTextBox = new YleTextBox { IsMultiline = true, Height = 100, PlaceHolder = "Provide some additional information about the request." };

		private readonly Label returnSourceMediaToKalustovarastoLabel = new Label("Source media can be returned to Kalustovarasto");
		private readonly CheckBox returnSourceMediaToKalustovarastoCheckBox = new CheckBox { IsChecked = true };

		private readonly Label kalustovarastoWhereShouldCardsBeReturnedLabel = new Label("Where should the cards be returned to");
		private readonly DropDown kalustovarastoWhereShouldCardsBeReturnedDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<LocationOfCardsToBeReturned>(), LocationOfCardsToBeReturned.ValmiitKaapi.GetDescription());

		private readonly Label kalustovarastoNameOfTheRecipientLabel = new Label("Name of the recipient");
		private readonly YleTextBox kalustovarastoNameOFTheRecipientTextBox = new YleTextBox();

		private readonly Label kalustovarastoPlNumberForReturnOfCardsLabel = new Label("Enter the PL for the return of the cards");
		private readonly YleTextBox kalustovarastoPlNumberForReturnOfCardsTextBox = new YleTextBox();

		public HddImportDetailsSection(Helpers helpers, ISectionConfiguration configuration, ImportMainSection section, HddIngestDetails hddIngestDetails = null) : base(helpers, configuration, section, null)
		{
			materialIncludesHighFrameRateCheckBox.Changed += MaterialIncludesHighFrameRateCheckBox_Changed;
			materialIsAvidProjectCheckBox.Changed += MaterialIsAvidProjectCheckBox_Changed;
			returnSourceMediaToKalustovarastoCheckBox.Changed += (o, e) => HandleVisibilityAndEnabledUpdate();
			kalustovarastoWhereShouldCardsBeReturnedDropDown.Changed += (o, e) => HandleVisibilityAndEnabledUpdate();

			InitializeHddImportDetails(hddIngestDetails);

			GenerateUi(out int row);
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

		public bool IsAvidProject
		{
			get
			{
				return materialIsAvidProjectCheckBox.IsChecked;
			}
			private set
			{
				materialIsAvidProjectCheckBox.IsChecked = value;
			}
		}

		public string AvidProjectName
		{
			get
			{
				return avidProjectNameTextBox.Text;
			}
			private set
			{
				avidProjectNameTextBox.Text = value;
			}
		}

		public string AvidProjectAdditionalInformation
		{
			get
			{
				return avidProjectAdditionalInformationTextBox.Text;
			}
			private set
			{
				avidProjectAdditionalInformationTextBox.Text = value;
			}
		}

		public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections
		{
			get
			{
				return new NonLiveManagerTreeViewSection[0];
			}
		}

		public override bool IsValid()
		{
			bool highFrameRateInfoIsValid = (MaterialIncludesHighFrameRateFootage && !String.IsNullOrWhiteSpace(AdditionalInfoAboutHighFrameRateFootage)) || (!MaterialIncludesHighFrameRateFootage);

			additionalInfoAboutHighFrameRateTextBox.ValidationState = highFrameRateInfoIsValid ? UIValidationState.Valid : UIValidationState.Invalid;
			additionalInfoAboutHighFrameRateTextBox.ValidationText = "Provide additional info";

			bool isAvidProjectNameValid = !IsAvidProject || !String.IsNullOrEmpty(AvidProjectName);
			avidProjectNameTextBox.ValidationState = isAvidProjectNameValid ? UIValidationState.Valid : UIValidationState.Invalid;
			avidProjectNameTextBox.ValidationText = "Provide an avid project name";

			bool isAvidEditorInfoValid = !IsAvidProject || !String.IsNullOrEmpty(avidProjectContactEditorInformationTextBox.Text);
			avidProjectContactEditorInformationTextBox.ValidationState = isAvidEditorInfoValid ? UIValidationState.Valid : UIValidationState.Invalid;
			avidProjectContactEditorInformationTextBox.ValidationText = "Provide editor contact information";

			return highFrameRateInfoIsValid && isAvidProjectNameValid && isAvidEditorInfoValid;
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(sourceFolderLabel, ++row, 0, HorizontalAlignment.Left, VerticalAlignment.Top);
			AddWidget(sourceFolderTextBox, row, 1, 1, 2);

			AddWidget(materialIncludesHighFrameRateLabel, ++row, 0);
			AddWidget(materialIncludesHighFrameRateCheckBox, row, 1, 1, 2);

			AddWidget(additionalInfoAboutHighFrameRateLabel, ++row, 0);
			AddWidget(additionalInfoAboutHighFrameRateTextBox, row, 1, 1, 2);

			AddWidget(materialIsAvidProjectLabel, ++row, 0);
			AddWidget(materialIsAvidProjectCheckBox, row, 1, 1, 2);

			AddWidget(avidProjectNameLabel, ++row, 0);
			AddWidget(avidProjectNameTextBox, row, 1, 1, 2);

			AddWidget(avidProjectAdditionalInformationLabel, ++row, 0);
			AddWidget(avidProjectAdditionalInformationTextBox, row, 1, 1, 2);

			AddWidget(avidProjectContactEditorInformationLabel, ++row, 0);
			AddWidget(avidProjectContactEditorInformationTextBox, row, 1, 1, 2);

			AddWidget(hddIsEvsDiskLabel, ++row, 0);
			AddWidget(hddIsEvsDiskCheckBox, row, 1, 1, 2);

			AddWidget(additionalInformationLabel, ++row, 0);
			AddWidget(additionalInformationTextBox, row, 1, 1, 2);

			AddWidget(returnSourceMediaToKalustovarastoLabel, ++row, 0);
			AddWidget(returnSourceMediaToKalustovarastoCheckBox, row, 1, 1, 2);

			AddWidget(kalustovarastoWhereShouldCardsBeReturnedLabel, ++row, 0);
			AddWidget(kalustovarastoWhereShouldCardsBeReturnedDropDown, row, 1, 1, 2);

			AddWidget(kalustovarastoNameOfTheRecipientLabel, ++row, 0);
			AddWidget(kalustovarastoNameOFTheRecipientTextBox, row, 1, 1, 2);

			AddWidget(kalustovarastoPlNumberForReturnOfCardsLabel, ++row, 0);
			AddWidget(kalustovarastoPlNumberForReturnOfCardsTextBox, row, 1, 1, 2);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			sourceFolderLabel.IsVisible = IsVisible;
			sourceFolderTextBox.IsVisible = IsVisible;
			sourceFolderTextBox.IsEnabled = IsEnabled;

			materialIncludesHighFrameRateLabel.IsVisible = IsVisible;
			materialIncludesHighFrameRateCheckBox.IsVisible = IsVisible;
			materialIncludesHighFrameRateCheckBox.IsEnabled = IsEnabled;

			additionalInfoAboutHighFrameRateLabel.IsVisible = IsVisible && MaterialIncludesHighFrameRateFootage;
			additionalInfoAboutHighFrameRateTextBox.IsVisible = additionalInfoAboutHighFrameRateLabel.IsVisible;
			additionalInfoAboutHighFrameRateTextBox.IsEnabled = IsEnabled;

			materialIsAvidProjectLabel.IsVisible = IsVisible;
			materialIsAvidProjectCheckBox.IsVisible	= IsVisible;
			materialIsAvidProjectCheckBox.IsEnabled = IsEnabled;

			avidProjectNameLabel.IsVisible = IsVisible && IsAvidProject;
			avidProjectNameTextBox.IsVisible = avidProjectNameLabel.IsVisible;
			avidProjectNameTextBox.IsEnabled = IsEnabled;

			avidProjectAdditionalInformationLabel.IsVisible = IsVisible && IsAvidProject;
			avidProjectAdditionalInformationTextBox.IsVisible = avidProjectAdditionalInformationLabel.IsVisible;
			avidProjectContactEditorInformationTextBox.IsEnabled = IsEnabled;

			avidProjectContactEditorInformationLabel.IsVisible = IsVisible && IsAvidProject;
			avidProjectContactEditorInformationTextBox.IsVisible = avidProjectContactEditorInformationLabel.IsVisible;
			avidProjectContactEditorInformationTextBox.IsEnabled = IsEnabled;

			hddIsEvsDiskLabel.IsVisible = IsVisible;
			hddIsEvsDiskCheckBox.IsVisible = IsVisible;
			hddIsEvsDiskCheckBox.IsEnabled = IsEnabled;

			additionalInformationLabel.IsVisible = IsVisible;
			additionalInformationTextBox.IsVisible = IsVisible;
			additionalInformationTextBox.IsEnabled = IsEnabled;

			returnSourceMediaToKalustovarastoLabel.IsVisible = IsVisible && ingestMainSection.IngestDestination == AvidInterplayPAM.InterplayPamElements.Helsinki;
			returnSourceMediaToKalustovarastoCheckBox.IsVisible = returnSourceMediaToKalustovarastoLabel.IsVisible;
			returnSourceMediaToKalustovarastoCheckBox.IsEnabled = IsEnabled;

			kalustovarastoWhereShouldCardsBeReturnedLabel.IsVisible = IsVisible && ingestMainSection.IngestDestination == AvidInterplayPAM.InterplayPamElements.Helsinki && !returnSourceMediaToKalustovarastoCheckBox.IsChecked;
			kalustovarastoWhereShouldCardsBeReturnedDropDown.IsVisible = kalustovarastoWhereShouldCardsBeReturnedLabel.IsVisible;
			kalustovarastoWhereShouldCardsBeReturnedDropDown.IsEnabled = IsEnabled;

			kalustovarastoNameOfTheRecipientLabel.IsVisible = IsVisible && ingestMainSection.IngestDestination == AvidInterplayPAM.InterplayPamElements.Helsinki && kalustovarastoWhereShouldCardsBeReturnedDropDown.Selected == LocationOfCardsToBeReturned.InternalMail.GetDescription() && !returnSourceMediaToKalustovarastoCheckBox.IsChecked;
			kalustovarastoNameOFTheRecipientTextBox.IsVisible = kalustovarastoNameOfTheRecipientLabel.IsVisible;
			kalustovarastoNameOFTheRecipientTextBox.IsEnabled = IsEnabled;

			kalustovarastoPlNumberForReturnOfCardsLabel.IsVisible = IsVisible && ingestMainSection.IngestDestination == AvidInterplayPAM.InterplayPamElements.Helsinki && kalustovarastoWhereShouldCardsBeReturnedDropDown.Selected == LocationOfCardsToBeReturned.InternalMail.GetDescription() && !returnSourceMediaToKalustovarastoCheckBox.IsChecked;
			kalustovarastoPlNumberForReturnOfCardsTextBox.IsVisible = kalustovarastoPlNumberForReturnOfCardsLabel.IsVisible;
			kalustovarastoPlNumberForReturnOfCardsTextBox.IsEnabled = IsEnabled;

			ToolTipHandler.SetTooltipVisibility(this);		
        }

		public override void UpdateIngest(Ingest ingest)
		{
			ingest.HddImportDetails = ingest.HddImportDetails ?? new List<HddIngestDetails>();

			ingest.HddImportDetails.Add(new HddIngestDetails
			{
				SourceFolder = sourceFolderTextBox.Text,
				MaterialIncludesHighFrameRateFootage = this.MaterialIncludesHighFrameRateFootage,
				AdditionalInfoAboutHighFrameRateFootage = MaterialIncludesHighFrameRateFootage ? this.AdditionalInfoAboutHighFrameRateFootage : null,
				IsAvidProject = materialIsAvidProjectCheckBox.IsChecked,
				AvidProjectName = this.IsAvidProject ? this.AvidProjectName : null,
				AvidProjectAdditionalInformation = this.IsAvidProject ? this.AvidProjectAdditionalInformation : null,
				EditorContactInformation = this.IsAvidProject ? avidProjectContactEditorInformationTextBox.Text : null,
				IsHddEvsDisk = hddIsEvsDiskCheckBox.IsChecked,
				AdditionalInformation = additionalInformationTextBox.Text,
				CanSourceMediaBeReturnedToKalustovarasto = returnSourceMediaToKalustovarastoCheckBox.IsChecked,
				KalustovarastoWhereShouldCardsBeReturnedTo = kalustovarastoWhereShouldCardsBeReturnedDropDown.Selected,
				KalustovarastoNameOfTheRecipient = kalustovarastoNameOFTheRecipientTextBox.Text,
				KalustovarastoPlNumberForReturnOfTheCards = kalustovarastoPlNumberForReturnOfCardsTextBox.Text
			});
		}

		private void InitializeHddImportDetails(HddIngestDetails hddIngestDetails)
		{
			if (hddIngestDetails != null)
			{
				sourceFolderTextBox.Text = hddIngestDetails.SourceFolder;
				MaterialIncludesHighFrameRateFootage = hddIngestDetails.MaterialIncludesHighFrameRateFootage;
				AdditionalInfoAboutHighFrameRateFootage = hddIngestDetails.AdditionalInfoAboutHighFrameRateFootage != null ? hddIngestDetails.AdditionalInfoAboutHighFrameRateFootage : this.AdditionalInfoAboutHighFrameRateFootage;
				IsAvidProject = hddIngestDetails.IsAvidProject;
				AvidProjectName = hddIngestDetails.AvidProjectName;
				AvidProjectAdditionalInformation = hddIngestDetails.AvidProjectAdditionalInformation;
				avidProjectContactEditorInformationTextBox.Text = hddIngestDetails.EditorContactInformation;
				hddIsEvsDiskCheckBox.IsChecked = hddIngestDetails.IsHddEvsDisk;
				additionalInformationTextBox.Text = String.IsNullOrWhiteSpace(hddIngestDetails.AdditionalInformation) ? String.Empty : hddIngestDetails.AdditionalInformation;
				returnSourceMediaToKalustovarastoCheckBox.IsChecked = hddIngestDetails.CanSourceMediaBeReturnedToKalustovarasto;
				kalustovarastoWhereShouldCardsBeReturnedDropDown.Selected = hddIngestDetails.KalustovarastoWhereShouldCardsBeReturnedTo;
				kalustovarastoNameOFTheRecipientTextBox.Text = hddIngestDetails.KalustovarastoNameOfTheRecipient;
				kalustovarastoPlNumberForReturnOfCardsTextBox.Text = hddIngestDetails.KalustovarastoPlNumberForReturnOfTheCards;
			}
		}

		private void MaterialIncludesHighFrameRateCheckBox_Changed(object sender, CheckBox.CheckBoxChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				HandleVisibilityAndEnabledUpdate();
				IsValid();
			}
		}

		private void MaterialIsAvidProjectCheckBox_Changed(object sender, CheckBox.CheckBoxChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				if (!e.IsChecked)
				{
					AvidProjectName = null;
					AvidProjectAdditionalInformation = null;
					avidProjectContactEditorInformationTextBox.Text = null;
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

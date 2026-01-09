namespace LiveOrderForm_6.AudioConfiguration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Sections.LiveOrderFormSections;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.InteractiveAutomationToolkit;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;

	public class AudioChannelPair
	{
		private const string Other = "Other";
		private readonly IEngine engine;
		private readonly AudioChannelConfiguration configuration;

		private readonly int channel;
		private readonly List<string> allOptions;
		private readonly List<string> allOptionsWithoutDecodedDolby;

		private readonly Label descriptionLabel = new Label { IsVisible = false };
		private readonly CheckBox stereoCheckBox = new CheckBox("Stereo") { IsChecked = true, IsVisible = false };
		private readonly Label descriptionFirstChannelLabel = new Label { IsVisible = false };
		private readonly Label descriptionSecondChannelLabel = new Label { IsVisible = false };
		private readonly DropDown firstChannelDropDown = new DropDown(new List<string>(), "None") { IsVisible = false };
		private readonly TextBox firstChannelOtherTextBox = new TextBox { IsVisible = false };
		private readonly DropDown secondChannelDropDown = new DropDown(new List<string>(), "None") { IsVisible = false };
		private readonly TextBox secondChannelOtherTextBox = new TextBox { IsVisible = false };
		private readonly CheckBox dolbyDecodingCheckBox = new CheckBox("Decode Dolby-E") { IsVisible = false, IsChecked = false };

		private bool isVisible;
		private bool isReadOnly;

		/// <summary>
		/// Constructor for an audio channel pair.
		/// </summary>
		/// <param name="engine">The engine object.</param>
		/// <param name="configuration">The audio channel configuration where this pair is part of.</param>
		/// <param name="firstChannel">The profile parameter for the first channel.</param>
		/// <param name="firstChannelDescription">The profile parameter for the description of the first channel.</param>
		/// <param name="secondChannel">The profile parameter for the second channel.</param>
		/// <param name="secondChannelDescription">The profile parameter for the description of the second channel.</param>
		public AudioChannelPair(IEngine engine, AudioChannelConfiguration configuration, ProfileParameter firstChannel, ProfileParameter firstChannelDescription, ProfileParameter secondChannel, ProfileParameter secondChannelDescription, ProfileParameter dolbyDecoding)
		{
			this.engine = engine;
			this.configuration = configuration;

			channel = Convert.ToInt32(firstChannel.Name.Split(' ').Last());

			FirstChannel = firstChannel;
			FirstChannelDescription = firstChannelDescription;
			SecondChannel = secondChannel;
			SecondChannelDescription = secondChannelDescription;
			DolbyDecodingProfileParameter = dolbyDecoding;

			stereoCheckBox.Changed += (sender, args) => HandleStereoUpdate();
			firstChannelDropDown.Changed += (sender, args) => HandleChannelUpdate(args.Selected);
			secondChannelDropDown.Changed += (sender, args) => HandleChannelUpdate(args.Selected);
			dolbyDecodingCheckBox.Changed += (sender, args) => HandleDolbyDecodingUpdate();

			descriptionLabel.Text = Description;
			descriptionFirstChannelLabel.Text = $"Audio Channel {channel}";
			descriptionSecondChannelLabel.Text = $"Audio Channel {channel + 1}";

			allOptions = FirstChannel.Discreets.Select(d => d.DisplayValue).ToList();
			allOptionsWithoutDecodedDolby = allOptions.Where(o => !(o.StartsWith("Dolby") && o.Contains("&"))).ToList();

			// update the options for the channels
			// only the source should initially have any options
			// don't include the decoded dolby channels in the source
			var options = new List<string> { "None" };
			if (configuration.IsSource) options.AddRange(allOptionsWithoutDecodedDolby.OrderBy(o => o));

			firstChannelDropDown.Options = options;
			secondChannelDropDown.Options = options;
		}

		/// <summary>
		/// The profile parameter for the first channel.
		/// </summary>
		public ProfileParameter FirstChannel { get; set; }

		/// <summary>
		/// The profile parameter for the description of the first channel.
		/// </summary>
		public ProfileParameter FirstChannelDescription { get; set; }

		/// <summary>
		/// The profile parameter for the second channel.
		/// </summary>
		public ProfileParameter SecondChannel { get; set; }

		/// <summary>
		/// The profile parameter for the description of the second channel.
		/// </summary>
		public ProfileParameter SecondChannelDescription { get; set; }

		/// <summary>
		/// The dolby decoding profile parameter.
		/// </summary>
		public ProfileParameter DolbyDecodingProfileParameter { get; set; }

		/// <summary>
		/// The channel of this audio channel pair.
		/// </summary>
		public int Channel => channel;

		/// <summary>
		/// The description of this audio channel pair.
		/// </summary>
		public string Description => $"Audio Channel {Channel}&{Channel + 1}";

		/// <summary>
		/// This indicates if this specific audio channel pair should be visible.
		/// Note that it will only be visible in the UI if the configuration is visible.
		/// </summary>
		public bool IsVisible
		{
			get => isVisible;
			set
			{
				isVisible = value;

				HandleVisibilityUpdate();
			}
		}

		/// <summary>
		/// Indicates if this audio channel pair can be changed.
		/// </summary>
		public bool IsReadOnly
		{
			get => isReadOnly;
			set
			{
				isReadOnly = value;
				HandleVisibilityUpdate();
			}
		}

		/// <summary>
		/// Indicates if this audio channel pair is stereo or mono.
		/// </summary>
		public bool IsStereo
		{
			get => stereoCheckBox.IsChecked;
			set => stereoCheckBox.IsChecked = value;
		}

		/// <summary>
		/// Indicates if dolby decoding is required.
		/// </summary>
		public bool DolbyDecoding
		{
			get => dolbyDecodingCheckBox.IsChecked && IsVisible /*&& configuration.IsVisible*/ && configuration.IsSource && FirstChannelValue.StartsWith("Dolby");
			set => dolbyDecodingCheckBox.IsChecked = value;
		}

		/// <summary>
		/// The value of the first channel.
		/// </summary>
		public string FirstChannelValue
		{
			get
			{
				string actualValue = FirstChannelSelectedOption.Split(':').Last().Trim();
				if (FirstChannelSelectedSourceOption.Contains(Other)) actualValue = actualValue.Split('-').First().Trim();

				return actualValue;
			}
		}

		/// <summary>
		/// The selected option of the first channel.
		/// </summary>
		public string FirstChannelSelectedOption
		{
			get
			{
				var selectedValue = firstChannelDropDown.Selected;
				if (String.IsNullOrEmpty(selectedValue))
				{
					selectedValue = "None";
				}

				return selectedValue;
			}
			set => firstChannelDropDown.Selected = value;
		}

		/// <summary>
		/// The selected option of the first channel containing details about the source.
		/// </summary>
		public string FirstChannelSelectedSourceOption
		{
			get
			{
				var option = FirstChannelSelectedOption;

				if (option == "None") return option;
				
				string lastPart = option == Other ? $" - {FirstChannelOtherText}" : string.Empty;

				return IsStereo ? $"Source {Description}: {option}{lastPart}"
					: $"Source Audio Channel {Channel}: {option}{lastPart}";
			}
		}

		/// <summary>
		/// The extra details for this first channel in case "Other" is selected.
		/// </summary>
		public string FirstChannelOtherText
		{
			get => firstChannelOtherTextBox.Text;
			set => firstChannelOtherTextBox.Text = value;
		}

		/// <summary>
		/// The value of the second channel.
		/// </summary>
		public string SecondChannelValue
		{
			get
			{
				string actualValue = SecondChannelSelectedOption.Split(':').Last().Trim();
				if (SecondChannelSelectedSourceOption.Contains(Other)) actualValue = actualValue.Split('-').First().Trim();

				return actualValue;
			}
		}

		/// <summary>
		/// The selected option of the second channel.
		/// </summary>
		public string SecondChannelSelectedOption
		{
			get
			{
				var selectedValue = secondChannelDropDown.Selected;
				if (String.IsNullOrEmpty(selectedValue))
				{
					selectedValue = "None";
				}

				return selectedValue;
			}

			set => secondChannelDropDown.Selected = value;
		}

		/// <summary>
		/// The selected option of the second channel containing details about the source.
		/// </summary>
		public string SecondChannelSelectedSourceOption
		{
			get
			{
				var option = SecondChannelSelectedOption;

				if (option == "None") return option;

				string lastPart = option == Other ? $" - {SecondChannelOtherText}" : string.Empty;

				return IsStereo ? $"Source {Description}: {option}{lastPart}"
					: $"Source Audio Channel {Channel + 1}: {option}{lastPart}";
			}
		}

		/// <summary>
		/// The extra details for this second channel in case "Other" is selected.
		/// </summary>
		public string SecondChannelOtherText
		{
			get => secondChannelOtherTextBox.Text;
			set => secondChannelOtherTextBox.Text = value;
		}

		/// <summary>
		/// Add the widgets for this audio channel pair to the parent section.
		/// </summary>
		/// <param name="section">The section to add the widgets to.</param>
		/// <param name="isSource">Indicates if the section is a source section.</param>
		/// <param name="row">The row to start adding the widgets.</param>
		public void AddWidgetsToSection(ServiceSection section, bool isSource, ref int row, int labelColumn, int labelColumnSpan, int inputWidgetColumn, int inputWidgetColumnSpan)
		{
			section.AddWidget(descriptionLabel, new WidgetLayout(++row, labelColumn, 1, labelColumnSpan, HorizontalAlignment.Left, VerticalAlignment.Center));
			section.AddWidget(stereoCheckBox, new WidgetLayout(row, inputWidgetColumn, 1, inputWidgetColumnSpan, HorizontalAlignment.Left, VerticalAlignment.Center));

			section.AddWidget(descriptionFirstChannelLabel, new WidgetLayout(++row, labelColumn, 1, labelColumnSpan));
			section.AddWidget(firstChannelDropDown, new WidgetLayout(row, inputWidgetColumn, 1, inputWidgetColumnSpan));
			section.AddWidget(firstChannelOtherTextBox, new WidgetLayout(row, inputWidgetColumn + inputWidgetColumnSpan));

			section.AddWidget(descriptionSecondChannelLabel, new WidgetLayout(++row, labelColumn, 1, labelColumnSpan));
			section.AddWidget(secondChannelDropDown, new WidgetLayout(row, inputWidgetColumn, 1, inputWidgetColumnSpan));
			section.AddWidget(secondChannelOtherTextBox, new WidgetLayout(row, inputWidgetColumn + inputWidgetColumnSpan));

			if (isSource)
			{
				section.AddWidget(dolbyDecodingCheckBox, new WidgetLayout(++row, inputWidgetColumn, 1, inputWidgetColumnSpan));
			}
		}

		/// <summary>
		/// Update the profile parameters with the selected values.
		/// </summary>
		public void UpdateProfileParameterValues()
		{
			FirstChannel.Value = FirstChannelValue;
			FirstChannelDescription.Value = FirstChannelOtherText;

			if (IsStereo)
			{
				SecondChannel.Value = FirstChannelValue;
				SecondChannelDescription.Value = FirstChannelOtherText;
			}
			else
			{
				SecondChannel.Value = SecondChannelValue;
				SecondChannelDescription.Value = SecondChannelOtherText;
			}
		}

		/// <summary>
		/// Load the values from the profile parameters.
		/// </summary>
		/// <param name="audioProfileParameters">The audio profile parameters.</param>
		/// <param name="dolbyDecodingProfileParameter">The dolby decoding profile parameter.</param>
		public void LoadProfileParameterValues(List<ProfileParameter> audioProfileParameters, ProfileParameter dolbyDecodingProfileParameter = null)
		{
			var dolbyChannelSelected = false;

			var firstChannel = audioProfileParameters.FirstOrDefault(a => a.Name.Split(' ').Last() == Convert.ToString(Channel));
			if (firstChannel != null)
			{
				var firstChannelOption = Convert.ToString(firstChannel.Value);
				if (!firstChannelDropDown.Options.Contains(firstChannelOption)) firstChannelDropDown.AddOption(firstChannelOption);

				FirstChannelSelectedOption = firstChannelOption;
				if (firstChannelOption.StartsWith("Dolby"))
				{
					dolbyChannelSelected = true;

					if (dolbyDecodingProfileParameter != null)
					{
						DolbyDecoding = Convert.ToString(dolbyDecodingProfileParameter.Value) == "Yes";
					}
				}
			}

			var firstChannelOtherDescription = audioProfileParameters.FirstOrDefault(p => p.Id == FirstChannelDescription.Id);
			if (firstChannelOtherDescription != null) FirstChannelOtherText = Convert.ToString(firstChannelOtherDescription.Value);

			var secondChannel = audioProfileParameters.FirstOrDefault(p => p.Id == SecondChannel.Id);
			if (secondChannel != null)
			{
				var secondChannelOption = Convert.ToString(secondChannel.Value);
				if (!secondChannelDropDown.Options.Contains(secondChannelOption)) secondChannelDropDown.AddOption(secondChannelOption);

				SecondChannelSelectedOption = Convert.ToString(secondChannel.Value);
			}

			var secondChannelOtherDescription = audioProfileParameters.FirstOrDefault(p => p.Id == SecondChannelDescription.Id);
			if (secondChannelOtherDescription != null) SecondChannelOtherText = Convert.ToString(secondChannelOtherDescription.Value);

			if (FirstChannelValue == "Other" && SecondChannelValue == "Other")
			{
				IsStereo = FirstChannelOtherText == SecondChannelOtherText;
			}
			else
			{
				IsStereo = FirstChannelValue == SecondChannelValue || dolbyChannelSelected;
			}

			IsVisible = FirstChannelSelectedOption != "None" || SecondChannelSelectedOption != "None";

			HandleVisibilityUpdate();
		}

		/// <summary>
		/// Update the available audio channel options based on the passed options.
		/// </summary>
		/// <param name="options">The options to base the selection on.</param>
		public void UpdateAvailableAudioChannelOptions(List<string> options)
		{
			var audioChannelPairChanged = false;

			// first check if no update is required to the selected option in case a destination was loaded from the SRM object
			// could be that the destination has "Internation" selected and now that it's linked to a source it should be "Source XXX: International"
			if (!options.Contains(FirstChannelSelectedOption))
			{
				var newFirstChannelSelectedOption = options.FirstOrDefault(o => o.Contains(FirstChannelSelectedOption));

				if (newFirstChannelSelectedOption != null)
				{
					// options could get further trimmed when removing the second channel selected option but needs to be set here to be able to update the current selected option
					firstChannelDropDown.Options = options;
					FirstChannelSelectedOption = newFirstChannelSelectedOption;

					audioChannelPairChanged = true;
				}
			}

			if (!options.Contains(SecondChannelSelectedOption))
			{
				var newSecondChannelSelectedOption = options.FirstOrDefault(o => o.Contains(SecondChannelSelectedOption));

				if (newSecondChannelSelectedOption != null)
				{
					// options could get further trimmed when removing the first channel selected option but needs to be set here to be able to update the current selected option
					secondChannelDropDown.Options = options;
					SecondChannelSelectedOption = newSecondChannelSelectedOption;

					audioChannelPairChanged = true;
				}
			}

			var firstChannelOptions = new List<string>(options);
			var secondChannelOptions = new List<string>(options);
			if (!IsStereo)
			{
				// make sure the first channel options doesn't included the selected second channel option and vice versa
				if (SecondChannelSelectedOption != "None" && firstChannelOptions.Contains(SecondChannelSelectedOption)) firstChannelOptions.Remove(SecondChannelSelectedOption);
				if (FirstChannelSelectedOption != "None" && secondChannelOptions.Contains(FirstChannelSelectedOption)) secondChannelOptions.Remove(FirstChannelSelectedOption);
			}

			// update the options
			if (!firstChannelOptions.Contains(FirstChannelSelectedOption)) audioChannelPairChanged = true;
			firstChannelDropDown.Options = firstChannelOptions;
		
			if (!secondChannelOptions.Contains(SecondChannelSelectedOption)) audioChannelPairChanged = true;
			secondChannelDropDown.Options = secondChannelOptions;

			// trigger reevaluation of the audio channel pair
			if (audioChannelPairChanged) OnAudioChannelPairChanged(new AudioChannelPairChangedEventArgs { AudioChannelPair = this });
		}

		/// <summary>
		/// When other option is selected in one of the source audio channels, the other defined text need to correspond with the selected audio pair channel option within the child service.
		/// </summary>
		/// <param name="sourceAudioChannelConfiguration">Audio channel configuration of the source service.</param>
		public void UpdateChildAudioOtherOptionChannels(AudioChannelConfiguration sourceAudioChannelConfiguration)
		{
			FilterOutChannelNumbersFromSelectedOption(out var firstChannelNumberToSearchFor, out var secondChannelNumberToSearchFor);
			
			var matchingAudioChannelPairs = sourceAudioChannelConfiguration.AudioChannelPairs.Where(a => firstChannelNumberToSearchFor == a.Channel.ToString() || secondChannelNumberToSearchFor == a.Channel.ToString() || firstChannelNumberToSearchFor == (a.Channel + 1).ToString() || secondChannelNumberToSearchFor == (a.Channel + 1).ToString()).ToList();

			MapAudioOtherTextBoxValuesFromSourceToChild(matchingAudioChannelPairs, firstChannelNumberToSearchFor, secondChannelNumberToSearchFor);
		}

		/// <summary>
		/// Indicates if this audio channel pair contains the provided profile parameter.
		/// </summary>
		/// <param name="parameter">The profile parameter.</param>
		/// <returns>True in case the profile parameter is part of this audio channel configuration.</returns>
		public bool Contains(ProfileParameter parameter)
		{
			return FirstChannel.Equals(parameter) || FirstChannelDescription.Equals(parameter) || SecondChannel.Equals(parameter) || SecondChannelDescription.Equals(parameter);
		}

		/// <summary>
		/// Trigger in case of changes to this audio channel pair.
		/// </summary>
		/// <param name="e">The arguments containing the changed audio channel pair.</param>
		public virtual void OnAudioChannelPairChanged(AudioChannelPairChangedEventArgs e)
		{
			EventHandler<AudioChannelPairChangedEventArgs> handler = AudioChannelPairChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		/// <summary>
		/// The event handler for audio channel pair changes.
		/// </summary>
		public event EventHandler<AudioChannelPairChangedEventArgs> AudioChannelPairChanged;

		/// <summary>
		/// Copy the audio channel pair configuration from the previous source.
		/// </summary>
		/// <param name="previousSourceAudioChannelPair">The previous source audio channel pair.</param>
		public void CopyFromPreviousSource(AudioChannelPair previousSourceAudioChannelPair, bool isThereAnyPreviousExistingDolby)
		{
			IsReadOnly = false;

			IsStereo = previousSourceAudioChannelPair.IsStereo;

			var previousFirstchannelOptions = previousSourceAudioChannelPair.FirstChannel.Discreets.Select(d => d.DisplayValue);

			firstChannelDropDown.Options = previousFirstchannelOptions.Where(o => !o.Contains("&")).OrderBy(o => o);
			FirstChannelSelectedOption = previousSourceAudioChannelPair.FirstChannelSelectedOption;
			FirstChannelOtherText = previousSourceAudioChannelPair.FirstChannelOtherText;

			secondChannelDropDown.Options = firstChannelDropDown.Options;
			SecondChannelSelectedOption = previousSourceAudioChannelPair.SecondChannelSelectedOption;
			SecondChannelOtherText = previousSourceAudioChannelPair.SecondChannelOtherText;

			IsVisible = FirstChannelValue != "None" || SecondChannelValue != "None";

			if (isThereAnyPreviousExistingDolby)
			{
				RemoveDolbyOption();
			}
		}

		/// <summary>
		/// Copy the audio channel pair configuration from the source.
		/// </summary>
		/// <param name="sourceAudioChannelPair">The source audio channel pair.</param>
		public void CopyFromSource(AudioChannelPair sourceAudioChannelPair)
		{
			IsReadOnly = false;

			IsStereo = sourceAudioChannelPair.IsStereo;

			firstChannelDropDown.Options = new HashSet<string> { "None", sourceAudioChannelPair.FirstChannelSelectedSourceOption };
			FirstChannelSelectedOption = sourceAudioChannelPair.FirstChannelSelectedSourceOption;
			FirstChannelOtherText = sourceAudioChannelPair.FirstChannelOtherText;

			secondChannelDropDown.Options = new List<string> { "None", sourceAudioChannelPair.SecondChannelSelectedSourceOption };
			SecondChannelSelectedOption = sourceAudioChannelPair.SecondChannelSelectedSourceOption;
			SecondChannelOtherText = sourceAudioChannelPair.SecondChannelOtherText;

			IsVisible = FirstChannelValue != "None" || SecondChannelValue != "None";
		}

		/// <summary>
		/// Copy the audio channel pair configuration from the source decoded dolby audio channel pair.
		/// </summary>
		/// <param name="sourceAudioChannelPair">The source audio channel pair.</param>
		/// <param name="decodedDolbyChannel">Indicates which decoded dolby channel this is.</param>
		public void CopyDecodedDolbyFromSource(AudioChannelPair sourceAudioChannelPair, int decodedDolbyChannel)
		{
			IsReadOnly = true;

			IsStereo = true;

			var decodedDolbyChannelOption = allOptions.FirstOrDefault(o => o.StartsWith("Dolby") && o.Contains($"{decodedDolbyChannel}&"));
			firstChannelDropDown.Options = new HashSet<string> { decodedDolbyChannelOption };
			FirstChannelSelectedOption = decodedDolbyChannelOption;

			IsVisible = true;
		}

		/// <summary>
		/// Copy the previous source audio channel pair into the current matching source audio channel pair.
		/// </summary>
		/// <param name="previousDecodeDolbyAudioChannelPair">The source audio channel pair.</param>
		public void CopyDolbyFromPreviousSource(AudioChannelPair previousDecodeDolbyAudioChannelPair)
		{
			IsReadOnly = false;

			IsStereo = previousDecodeDolbyAudioChannelPair.IsStereo;

			DolbyDecoding = previousDecodeDolbyAudioChannelPair.DolbyDecoding;

			firstChannelDropDown.Options = previousDecodeDolbyAudioChannelPair.FirstChannel.Discreets.Select(d => d.DisplayValue).Where(d => !d.Contains("&")).OrderBy(o => o);
			FirstChannelSelectedOption = previousDecodeDolbyAudioChannelPair.FirstChannelSelectedOption;

			IsVisible = FirstChannelValue != "None" || SecondChannelValue != "None";
		}

		/// <summary>
		/// Remove the Dolby-E option from all (other) source channels as soon as it's selected for one of the source channels.
		/// </summary>
		public void RemoveDolbyOption()
		{
			var optionsWithoutDolby = new List<string> { "None" };
			if (configuration.IsSource) optionsWithoutDolby.AddRange(allOptionsWithoutDecodedDolby.Where(o => !o.Contains("Dolby")).OrderBy(o => o));

			firstChannelDropDown.Options = optionsWithoutDolby;
			secondChannelDropDown.Options = optionsWithoutDolby;
		}

		/// <summary>
		/// Reset the options in case Dolby-E is no longer selected in any source channel.
		/// </summary>
		public void ResetOptions()
		{
			IsReadOnly = false;

			var optionsWithDolby = new List<string> { "None" };
			if (configuration.IsSource) optionsWithDolby.AddRange(allOptionsWithoutDecodedDolby.OrderBy(o => o));

			firstChannelDropDown.Options = optionsWithDolby;
			secondChannelDropDown.Options = optionsWithDolby;
		}

		/// <summary>
		/// Handle updates to this audio channel pair.
		/// </summary>
		public void HandleUpdate()
		{
			HandleVisibilityUpdate();

			OnAudioChannelPairChanged(new AudioChannelPairChangedEventArgs { AudioChannelPair = this });
		}

		/// <summary>
		/// Handle updates to the visibility of this audio channel pair.
		/// </summary>
		public void HandleVisibilityUpdate()
		{
			// show the correct widgets from the audio channel pair
			// the widgets are not shown when the audio channel pair or configuration are not visible
			descriptionLabel.IsVisible = IsVisible && configuration.IsVisible && !configuration.CopyFromSource;

			stereoCheckBox.IsVisible = IsVisible && configuration.IsVisible && !configuration.CopyFromSource;
			stereoCheckBox.IsEnabled = !IsReadOnly;

			descriptionFirstChannelLabel.IsVisible = IsVisible && configuration.IsVisible && !IsStereo && !configuration.CopyFromSource;

			firstChannelDropDown.IsVisible = IsVisible && configuration.IsVisible && !configuration.CopyFromSource;
			firstChannelDropDown.IsEnabled = !IsReadOnly;

			firstChannelOtherTextBox.IsVisible = firstChannelDropDown.IsVisible && FirstChannelValue == "Other" && !configuration.CopyFromSource;
			firstChannelOtherTextBox.IsEnabled = !IsReadOnly;

			descriptionSecondChannelLabel.IsVisible = IsVisible && configuration.IsVisible && !IsStereo && !configuration.CopyFromSource;

			secondChannelDropDown.IsVisible = IsVisible && configuration.IsVisible && !IsStereo && !configuration.CopyFromSource;
			secondChannelDropDown.IsEnabled = !IsReadOnly;

			secondChannelOtherTextBox.IsVisible = secondChannelDropDown.IsVisible && SecondChannelValue == "Other" && !configuration.CopyFromSource;
			secondChannelOtherTextBox.IsEnabled = !IsReadOnly;

			dolbyDecodingCheckBox.IsVisible = IsVisible && configuration.IsVisible && configuration.IsSource && FirstChannelValue.StartsWith("Dolby");
			dolbyDecodingCheckBox.IsEnabled = configuration.IsEbu || !IsReadOnly;
		}

		/// <summary>
		/// Based on the found channel numbers, the correct source audio pair channel need to be filtered out. This source audio pair contains the actual other text value.
		/// </summary>
		/// <param name="linkedSourceAudioChannelPairs">Matching source audio channel pairs.</param>
		/// <param name="firstChannelNumberToSearchFor">First channel number which is linked to the source first channel of the same pair.</param>
		/// <param name="secondChannelNumberToSearchFor">Second channel number which is linked to the source second channel of the same pair.</param>
		private void MapAudioOtherTextBoxValuesFromSourceToChild(List<AudioChannelPair> linkedSourceAudioChannelPairs, string firstChannelNumberToSearchFor, string secondChannelNumberToSearchFor)
		{
			if (linkedSourceAudioChannelPairs != null && linkedSourceAudioChannelPairs.Any())
			{
				if (IsStereo)
				{
					var matchingAudioChannelPair = linkedSourceAudioChannelPairs.First(x => x != null);
					FirstChannelOtherText = matchingAudioChannelPair.FirstChannelOtherText;
					return;
				}

				var matchingAudioChannelPairForFirstChannel = linkedSourceAudioChannelPairs.FirstOrDefault(x => firstChannelNumberToSearchFor == x.Channel.ToString() || firstChannelNumberToSearchFor == (x.Channel + 1).ToString());

				var matchingAudioChannelPairForSecondChannel = linkedSourceAudioChannelPairs.FirstOrDefault(x => secondChannelNumberToSearchFor == x.Channel.ToString() || secondChannelNumberToSearchFor == (x.Channel + 1).ToString());

				FillInOtherTextBoxesBasedOnSourceAudioSelection(matchingAudioChannelPairForFirstChannel, matchingAudioChannelPairForSecondChannel, firstChannelNumberToSearchFor, secondChannelNumberToSearchFor);
			}
		}

		/// <summary>
		/// When copy from source is not applied, user can choose any audio pair combination.
		/// Inside every drop down selection the channel number is defined which is matching with a source audio channel pair.
		/// </summary>
		/// <param name="firstChannelToSearchFor">First channel number which is linked to the source first channel of the same pair.</param>
		/// <param name="secondChannelToSearchFor">Second channel number which is linked to the source second channel of the same pair.</param>
		private void FilterOutChannelNumbersFromSelectedOption(out string firstChannelToSearchFor, out string secondChannelToSearchFor)
		{
			var allChannelNumbers = Regex.Split(FirstChannelSelectedOption, @"\D+").Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

			firstChannelToSearchFor = allChannelNumbers.Any() ? allChannelNumbers.First() : string.Empty;
			secondChannelToSearchFor = string.Empty;

			if (!IsStereo)
			{
				var secondChannelSelectedChannels = Regex.Split(SecondChannelSelectedOption, @"\D+").Where(x => !string.IsNullOrWhiteSpace(x));
				if (secondChannelSelectedChannels.Any())
				{
					allChannelNumbers.AddRange(secondChannelSelectedChannels);
					secondChannelToSearchFor = secondChannelSelectedChannels.First();
				}
			}
		}

		/// <summary>
		/// Filling in the actual other text box value for each pair channel based on the matching source audio channel pair.
		/// </summary>
		/// <param name="matchingSourceAudioChannelPairForFirstChannel">Source audio channel pair which contains the other value for the first channel.</param>
		/// <param name="matchingSourceAudioChannelPairForSecondChannel">Source audio channel pair which contains the other value for the second channel.</param>
		/// <param name="firstChannelNumberToSearchFor">First channel number which is linked to the source first channel of the same pair.</param>
		/// <param name="secondChannelNumberToSearchFor">Second channel number which is linked to the source second channel of the same pair.</param>
		private void FillInOtherTextBoxesBasedOnSourceAudioSelection(AudioChannelPair matchingSourceAudioChannelPairForFirstChannel, AudioChannelPair matchingSourceAudioChannelPairForSecondChannel, string firstChannelNumberToSearchFor, string secondChannelNumberToSearchFor)
		{			
			if (matchingSourceAudioChannelPairForFirstChannel != null)
			{
				FirstChannelOtherText = firstChannelNumberToSearchFor == matchingSourceAudioChannelPairForFirstChannel.Channel.ToString() ? matchingSourceAudioChannelPairForFirstChannel.FirstChannelOtherText : matchingSourceAudioChannelPairForFirstChannel.SecondChannelOtherText;
			}
			
			if (matchingSourceAudioChannelPairForSecondChannel != null)
			{
				SecondChannelOtherText = secondChannelNumberToSearchFor == matchingSourceAudioChannelPairForSecondChannel.Channel.ToString() ? matchingSourceAudioChannelPairForSecondChannel.FirstChannelOtherText : matchingSourceAudioChannelPairForSecondChannel.SecondChannelOtherText;
			}
		}

		/// <summary>
		/// Handle updates to the stereo check box.
		/// </summary>
		private void HandleStereoUpdate()
		{
			if (IsReadOnly)
			{
				// when readonly the value cannot be changed
				IsStereo = true;

				return;
			}

			// check that the selected audio channel(s) match either stereo or mono
			// if not set them back to "None"
			if (IsStereo)
			{
				// this means a mono channel was selected and should be reset
				// this does not only apply to the source
				if (!FirstChannelSelectedOption.Contains("&") && !configuration.IsSource) FirstChannelSelectedOption = "None";

				SecondChannelSelectedOption = "None";
			}
			else
			{
				// this means a stereo channel was selected and should be reset
				// when the selection option contains '&' it means it uses the following format "Source ChX&Y"
				// when the selection option contains 'Dolby' it means that "Dolby-E XXX" was selected and this also requires Stereo (this only applies to the source)
				if (FirstChannelSelectedOption.Contains("&") || FirstChannelValue.Contains("Dolby")) FirstChannelSelectedOption = "None";
			}

			HandleUpdate();
		}

		/// <summary>
		/// Handle updates to one of the channels.
		/// </summary>
		/// <param name="selectedValue">The selected value for the updated channel.</param>
		private void HandleChannelUpdate(string selectedValue)
		{
			if (selectedValue != "None")
			{
				// check that the selected value is indeed a stereo channel
				// if not then disable the stereo check
				// this does not only apply to the source
				if (IsStereo && !selectedValue.Contains("&") && !configuration.IsSource)
				{
					IsStereo = false;
				}
				else if (selectedValue.Contains("&") || selectedValue.Contains("Dolby"))
				{
					// this means a stereo channel was selected while only a mono channel is valid
					// in this case we need to set the stereo check
					IsStereo = true;

					// check which channel was updated and update the selection to make it valid for stereo 
					if (FirstChannelSelectedOption != selectedValue) FirstChannelSelectedOption = SecondChannelSelectedOption;

					SecondChannelSelectedOption = "None";
				}
			}

			HandleUpdate();
		}

		/// <summary>
		/// Handle updates to the dolby decoding checkbox.
		/// </summary>
		private void HandleDolbyDecodingUpdate()
		{
			HandleUpdate();
		}
	}
}
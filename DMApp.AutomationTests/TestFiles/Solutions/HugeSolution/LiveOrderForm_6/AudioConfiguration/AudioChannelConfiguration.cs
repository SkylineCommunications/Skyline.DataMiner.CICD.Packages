namespace LiveOrderForm_6.AudioConfiguration
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using Sections.LiveOrderFormSections;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.InteractiveAutomationToolkit;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;

	/// <summary>
	/// Contains the configuration of the Audio Channel profile parameters
	/// </summary>
	public class AudioChannelConfiguration
	{
		private readonly IEngine engine;
		private readonly ServiceSection section;

		private AudioChannelConfiguration sourceAudioChannelConfiguration;
		private readonly List<AudioChannelConfiguration> childAudioChannelConfigurations;
		private readonly ReadOnlyCollection<AudioChannelConfiguration> readonlyChildAudioChannelConfigurations;

		private readonly Label audioEmbeddingRequiredLabel = new Label("Audio Embedding");
		private readonly DropDown audioEmbeddingRequiredDropdown = new DropDown();

		private readonly Label audioDeembeddingRequiredLabel = new Label("Audio Deembedding");
		private readonly DropDown audioDeembeddingRequiredDropdown = new DropDown();

		private readonly Label audioShufflingRequiredLabel = new Label("Audio Shuffling");
		private readonly DropDown audioShufflingRequiredDropdown = new DropDown();

		private readonly Label audioChannelConfigurationLabel = new Label("Audio Channel Configuration");
		private readonly CheckBox copyAudioChannelConfigurationFromSourceCheckBox = new CheckBox("Copy from Source") { IsChecked = true };

		private readonly Button addAudioPairButton = new Button("Add Audio Pair") { Width = 200 };
		private readonly Button deleteAudioPairButton = new Button("Delete Audio Pair") { Width = 200 };

		private bool isVisible = true;
		private bool isReadOnly = false;
		private bool canCopyFromSource = true;

		/// <summary>
		/// Initialize the audio channel configuration.
		/// </summary>
		/// <param name="audioProfileParameters">The audio profile parameters for a service.</param>
		public AudioChannelConfiguration(IEngine engine, List<ProfileParameter> audioProfileParameters, ServiceSection section)
		{
			this.engine = engine;
			this.section = section;

			AudioChannelPairs = new List<AudioChannelPair>();

			childAudioChannelConfigurations = new List<AudioChannelConfiguration>();
			readonlyChildAudioChannelConfigurations = new ReadOnlyCollection<AudioChannelConfiguration>(childAudioChannelConfigurations);

			copyAudioChannelConfigurationFromSourceCheckBox.Changed += (s, e) => { CopyFromSource = e.IsChecked; };

			addAudioPairButton.Pressed += AddAudioPair;
			deleteAudioPairButton.Pressed += DeleteAudioPair;

			Initialize(audioProfileParameters);
		}

		/// <summary>
		/// Indicates if this is the source audio configuration.
		/// </summary>
		public bool IsSource => section.IsSource;

		public bool IsEbu => section.IsEbu;

		/// <summary>
		/// This indicates if the configuration is visible.
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
		/// Indicates if the audio configuration needs to be copied from the source.
		/// Does not apply for the source itself.
		/// </summary>
		public bool CopyFromSource
		{
			get => !IsSource && copyAudioChannelConfigurationFromSourceCheckBox.IsChecked;
			set
			{
				if (!CanCopyFromSource) return;

				copyAudioChannelConfigurationFromSourceCheckBox.IsChecked = value;

				// when the "Copy from Source" checkbox changes we can always copy the configuration from the source
				HandleSourceAudioConfigurationUpdate();
				HandleVisibilityUpdate();
			}
		}

		public bool CanCopyFromSource
		{
			get => !IsSource && canCopyFromSource;
			set
			{
				canCopyFromSource = value;
				if (!canCopyFromSource) CopyFromSource = false;
				HandleVisibilityUpdate();
			}
		}

		/// <summary>
		/// Indicates if an audio channel pair can be added.
		/// </summary>
		public bool CanAddAudioChannelPair
		{
			get
			{
				return IsVisible && !CopyFromSource && AudioChannelPairs.Count(p => p.IsVisible) < AudioChannelPairs.Count;
			}
		}

		/// <summary>
		/// Indicates if an audio channel pair can be deleted.
		/// </summary>
		public bool CanDeleteAudioChannelPair
		{
			get
			{
				return IsVisible && !CopyFromSource && AudioChannelPairs.Any(p => p.IsVisible && !p.IsReadOnly);
			}
		}

		/// <summary>
		/// The parameter indicating if dolby-e decoding is required.
		/// Only applicable in case dolby-e is selected in the source.
		/// </summary>
		public ProfileParameter AudioDolbyDecodingRequiredProfileParameter { get; set; }

		/// <summary>
		/// The parameter indicating if embedding is required.
		/// Not applicable in the source.
		/// </summary>
		public ProfileParameter AudioEmbeddingRequiredProfileParameter { get; set; }

		/// <summary>
		/// The parameter indicating if deembedding is required.
		/// Not applicable in the source.
		/// </summary>
		public ProfileParameter AudioDeembeddingRequiredProfileParameter { get; set; }

		/// <summary>
		/// The parameter indicating if shuffling is required.
		/// Not applicable in the source.
		/// </summary>
		public ProfileParameter AudioShufflingRequiredProfileParameter { get; set; }

		/// <summary>
		/// The list of audio channel pairs.
		/// </summary>
		public List<AudioChannelPair> AudioChannelPairs { get; private set; }

		/// <summary>
		/// This contains the audio channel configuration of the source.
		/// This is not set when this is the audio channel configuration of the source itself.
		/// </summary>
		public AudioChannelConfiguration SourceAudioChannelConfiguration
		{
			get => sourceAudioChannelConfiguration;
			private set => sourceAudioChannelConfiguration = value;
		}

		/// <summary>
		/// This contains the audio channel configurations of the children (destination, recording, ...).
		/// This is only applicable in case this is the source audio channel configuration.
		/// </summary>
		public IEnumerable<AudioChannelConfiguration> ChildAudioChannelConfigurations => readonlyChildAudioChannelConfigurations;

		/// <summary>
		/// Add the widgets for the Audio Channel configuration to the section.
		/// </summary>
		/// <param name="isSource">Indicates if this is for the source.</param>
		/// <param name="linkedWidgetsToCollapseButton">The collapse button where some widgets need to be linked to.</param>
		/// <param name="row">the row to add the widgets on.</param>
		public void AddWidgetsToSection(List<Widget> linkedWidgetsToCollapseButton, ref int row, int labelColumn, int labelColumnSpan, int inputWidgetColumn, int inputWidgetColumnSpan)
		{
			if (section.UserInfo.IsMcrUser)
			{
				if (AudioDeembeddingRequiredProfileParameter != null)
				{
					section.AddWidget(audioDeembeddingRequiredLabel, new WidgetLayout(++row, labelColumn, 1, labelColumnSpan));
					if (linkedWidgetsToCollapseButton != null) linkedWidgetsToCollapseButton.Add(audioDeembeddingRequiredLabel);

					section.AddWidget(audioDeembeddingRequiredDropdown, new WidgetLayout(row, inputWidgetColumn, 1, inputWidgetColumnSpan));
					if (linkedWidgetsToCollapseButton != null) linkedWidgetsToCollapseButton.Add(audioDeembeddingRequiredDropdown);
				}

				if (AudioShufflingRequiredProfileParameter != null)
				{
					section.AddWidget(audioShufflingRequiredLabel, new WidgetLayout(++row, labelColumn, 1, labelColumnSpan));
					if (linkedWidgetsToCollapseButton != null) linkedWidgetsToCollapseButton.Add(audioShufflingRequiredLabel);

					section.AddWidget(audioShufflingRequiredDropdown, new WidgetLayout(row, inputWidgetColumn, 1, inputWidgetColumnSpan));
					if (linkedWidgetsToCollapseButton != null) linkedWidgetsToCollapseButton.Add(audioShufflingRequiredDropdown);
				}

				if (AudioEmbeddingRequiredProfileParameter != null)
				{
					section.AddWidget(audioEmbeddingRequiredLabel, new WidgetLayout(++row, labelColumn, 1, labelColumnSpan));
					if (linkedWidgetsToCollapseButton != null) linkedWidgetsToCollapseButton.Add(audioEmbeddingRequiredLabel);

					section.AddWidget(audioEmbeddingRequiredDropdown, new WidgetLayout(row, inputWidgetColumn, 1, inputWidgetColumnSpan));
					if (linkedWidgetsToCollapseButton != null) linkedWidgetsToCollapseButton.Add(audioEmbeddingRequiredDropdown);
				}
			}

			section.AddWidget(audioChannelConfigurationLabel, new WidgetLayout(++row, labelColumn, 1, labelColumnSpan, HorizontalAlignment.Left, VerticalAlignment.Center));
			if (linkedWidgetsToCollapseButton != null) linkedWidgetsToCollapseButton.Add(audioChannelConfigurationLabel);

			if (!IsSource)
			{
				section.AddWidget(copyAudioChannelConfigurationFromSourceCheckBox, new WidgetLayout(row, inputWidgetColumn, 1, inputWidgetColumnSpan));
				if (linkedWidgetsToCollapseButton != null) linkedWidgetsToCollapseButton.Add(copyAudioChannelConfigurationFromSourceCheckBox);
			}

			foreach (var audioChannelPair in AudioChannelPairs)
			{
				audioChannelPair.AddWidgetsToSection(section, IsSource, ref row, labelColumn, labelColumnSpan, inputWidgetColumn, inputWidgetColumnSpan);
			}

			section.AddWidget(deleteAudioPairButton, new WidgetLayout(++row, inputWidgetColumn, 1, inputWidgetColumnSpan));
			section.AddWidget(addAudioPairButton, new WidgetLayout(++row, inputWidgetColumn, 1, inputWidgetColumnSpan));
		}

		/// <summary>
		/// Set the source audio channel configuration.
		/// </summary>
		/// <param name="configuration">The source audio channel configuration to set.</param>
		public void SetSourceAudioChannelConfiguration(AudioChannelConfiguration configuration, bool setOnServiceInitialization = false)
		{
			SourceAudioChannelConfiguration = configuration;

			if (setOnServiceInitialization)
			{
				VerifyIsCopyFromSource();
			}
			else if (!IsSource && SourceAudioChannelConfiguration != null)
			{
				// trigger the update from the source
				HandleSourceAudioConfigurationUpdate();
				HandleVisibilityUpdate();
			}
			else
			{
				// Do nothing
			}
		}

		/// <summary>
		/// Add the child audio channel configuration.
		/// </summary>
		/// <param name="configuration">The child audio channel configuration to add.</param>
		public void AddChildAudioChannelConfiguration(AudioChannelConfiguration configuration)
		{
			childAudioChannelConfigurations.Add(configuration);
		}

		/// <summary>
		/// Remove the child audio channel configuration.
		/// </summary>
		/// <param name="configuration">The child audio channel configuration to remove.</param>
		public void RemoveChildAudioChannelConfiguration(AudioChannelConfiguration configuration)
		{
			childAudioChannelConfigurations.Remove(configuration);
		}

		/// <summary>
		/// Handle the changed source audio configuration.
		/// </summary>
		public void HandleSourceAudioConfigurationUpdate()
		{
			if (SourceAudioChannelConfiguration == null) return;

			if (CopyFromSource)
			{
				CopySourceAudioConfiguration();
			}
			else
			{
				UpdateBasedOnSourceAudioChannelConfiguration();
			}
		}

		/// <summary>
		/// Copies the AudioConfiguration from the previous Source Section into the current Source Section
		/// </summary>
		public void HandleCopyAudioConfigurationFromPreviousSource(AudioChannelConfiguration previousSourceAudioChannelConfiguration)
		{
			if (previousSourceAudioChannelConfiguration == null) return;
			
			CopyPreviousAudioConfiguration(previousSourceAudioChannelConfiguration.AudioChannelPairs);
			HandleVisibilityUpdate();
		}

		/// <summary>
		/// Load the initial data from the audio channel configuration as configured in the source service.
		/// </summary>
		/// <param name="audioProfileParameterValues">The audio profile parameters from the source service.</param>
		/// <param name="audioDolbyDecodingProfileParameterValue">The dolby decoding profile parameter from the source service.</param>
		public void LoadSourceAudioChannelConfiguration(List<ProfileParameter> audioProfileParameterValues, ProfileParameter audioDolbyDecodingProfileParameterValue)
		{
			foreach (var audioChannelPair in AudioChannelPairs)
			{
				audioChannelPair.LoadProfileParameterValues(audioProfileParameterValues, audioDolbyDecodingProfileParameterValue);
			}
		}

		/// <summary>
		/// Load the initial data from the audio channel configuration as configured in the child service. 
		/// </summary>
		/// <param name="audioProfileParameterValues">The audio profile parameters from the child service.</param>
		public void LoadChildAudioChannelConfiguration(List<ProfileParameter> audioProfileParameterValues)
		{
			if (AudioEmbeddingRequiredProfileParameter != null)
			{
				var audioEmbeddingRequiredProfileParameter = audioProfileParameterValues.FirstOrDefault(a => a.Id == AudioEmbeddingRequiredProfileParameter.Id);
				if (audioEmbeddingRequiredProfileParameter != null)
				{
					AudioEmbeddingRequiredProfileParameter.Value = audioEmbeddingRequiredProfileParameter.Value;
					audioEmbeddingRequiredDropdown.Selected = Convert.ToString(AudioEmbeddingRequiredProfileParameter.Value);
				}
			}

			if (AudioDeembeddingRequiredProfileParameter != null)
			{
				var audioDeembeddingRequiredProfileParameter = audioProfileParameterValues.FirstOrDefault(a => a.Id == AudioDeembeddingRequiredProfileParameter.Id);
				if (audioDeembeddingRequiredProfileParameter != null)
				{
					AudioDeembeddingRequiredProfileParameter.Value = audioDeembeddingRequiredProfileParameter.Value;
					audioDeembeddingRequiredDropdown.Selected = Convert.ToString(AudioDeembeddingRequiredProfileParameter.Value);
				}
			}

			if (AudioShufflingRequiredProfileParameter != null)
			{
				var audioShufflingRequiredProfileParameter = audioProfileParameterValues.FirstOrDefault(a => a.Id == AudioShufflingRequiredProfileParameter.Id);
				if (audioShufflingRequiredProfileParameter != null)
				{
					AudioShufflingRequiredProfileParameter.Value = audioShufflingRequiredProfileParameter.Value;
					audioShufflingRequiredDropdown.Selected = Convert.ToString(AudioShufflingRequiredProfileParameter.Value);
				}
			}

			foreach (var audioChannelPair in AudioChannelPairs)
			{
				audioChannelPair.LoadProfileParameterValues(audioProfileParameterValues, null);
			}

			VerifyIsCopyFromSource(updateIsCopyFromSourceCheckBoxOnly: true);
		}

		/// <summary>
		/// Initialize the properties linked to audio profile parameters.
		/// </summary>
		/// <param name="audioProfileParameters">The list of audio profile parameters.</param>
		private void Initialize(List<ProfileParameter> audioProfileParameters)
		{
			AudioDolbyDecodingRequiredProfileParameter = audioProfileParameters.FirstOrDefault(a => a.Name.StartsWith("Audio Dolby Decoding"));

			AudioEmbeddingRequiredProfileParameter = audioProfileParameters.FirstOrDefault(a => a.Name.StartsWith("Audio Embedding"));
			if (AudioEmbeddingRequiredProfileParameter != null)
			{
				audioEmbeddingRequiredDropdown.Options = AudioEmbeddingRequiredProfileParameter.Discreets.Select(d => d.DisplayValue);

				var defaultValue = AudioEmbeddingRequiredProfileParameter.DefaultValue?.StringValue;
				audioEmbeddingRequiredDropdown.Selected = defaultValue;
				AudioEmbeddingRequiredProfileParameter.Value = defaultValue;

				audioEmbeddingRequiredDropdown.Changed += (s, e) => { AudioEmbeddingRequiredProfileParameter.Value = e.Selected; };
			}

			AudioDeembeddingRequiredProfileParameter = audioProfileParameters.FirstOrDefault(a => a.Name.StartsWith("Audio Deembedding"));
			if (AudioDeembeddingRequiredProfileParameter != null)
			{
				audioDeembeddingRequiredDropdown.Options = AudioDeembeddingRequiredProfileParameter.Discreets.Select(d => d.DisplayValue);

				var defaultValue = AudioDeembeddingRequiredProfileParameter.DefaultValue?.StringValue;
				audioDeembeddingRequiredDropdown.Selected = defaultValue;
				AudioDeembeddingRequiredProfileParameter.Value = defaultValue;

				audioDeembeddingRequiredDropdown.Changed += (s, e) => { AudioDeembeddingRequiredProfileParameter.Value = e.Selected; };
			}

			AudioShufflingRequiredProfileParameter = audioProfileParameters.FirstOrDefault(a => a.Name.StartsWith("Audio Shuffling"));
			if (AudioShufflingRequiredProfileParameter != null)
			{
				audioShufflingRequiredDropdown.Options = AudioShufflingRequiredProfileParameter.Discreets.Select(d => d.DisplayValue);

				var defaultValue = AudioShufflingRequiredProfileParameter.DefaultValue?.StringValue;
				audioShufflingRequiredDropdown.Selected = defaultValue;
				AudioShufflingRequiredProfileParameter.Value = defaultValue;

				audioShufflingRequiredDropdown.Changed += (s, e) => { AudioShufflingRequiredProfileParameter.Value = e.Selected; };
			}

			foreach (var audioProfileParameter in audioProfileParameters)
			{
				if (AudioDolbyDecodingRequiredProfileParameter != null && AudioDolbyDecodingRequiredProfileParameter.Id == audioProfileParameter.Id) continue;
				if (AudioEmbeddingRequiredProfileParameter != null && AudioEmbeddingRequiredProfileParameter.Id == audioProfileParameter.Id) continue;
				if (AudioDeembeddingRequiredProfileParameter != null && AudioDeembeddingRequiredProfileParameter.Id == audioProfileParameter.Id) continue;
				if (AudioShufflingRequiredProfileParameter != null && AudioShufflingRequiredProfileParameter.Id == audioProfileParameter.Id) continue;

				// check that this audio channel profile parameter is not yet processed
				// every Audio Channel Pair uses a number of profile parameters 
				if (AudioChannelPairs.Any(p => p.Contains(audioProfileParameter))) continue;

				int channel;
				if (!Int32.TryParse(audioProfileParameter.Name.Split(' ').Last(), out channel) || channel % 2 == 0)
				{
					// an Audio Channel Pair contains the configuration of 2 audio channel pairs
					// we can skip this in case the channel is not uneven as it will be automatically added to the correct audio channel pair already
					continue;
				}

				var firstChannel = audioProfileParameter;
				var firstChannelDescription = audioProfileParameters.FirstOrDefault(p => p.Name == String.Format("{0} Description", firstChannel.Name));
				var secondChannel = audioProfileParameters.FirstOrDefault(p => p.Name == firstChannel.Name.Replace(channel.ToString(), (channel + 1).ToString()));
				var secondChannelDescription = audioProfileParameters.FirstOrDefault(p => p.Name == String.Format("{0} Description", secondChannel.Name));

				var audioChannel = new AudioChannelPair(engine, this, firstChannel, firstChannelDescription, secondChannel, secondChannelDescription, AudioDolbyDecodingRequiredProfileParameter);

				// subscribe on any changed in the audio channel pair
				audioChannel.AudioChannelPairChanged += HandleAudioChannelPairChanges;

				AudioChannelPairs.Add(audioChannel);
			}
		}

		/// <summary>
		/// Verify if this child audio channel configuration is a copy from the source audio channel configuration.
		/// </summary>
		/// <param name="updateIsCopyFromSourceCheckBoxOnly">A boolean to avoid the execution of logic in the setter of the <see cref="CopyFromSource"/> property.</param>
		private void VerifyIsCopyFromSource(bool updateIsCopyFromSourceCheckBoxOnly = false)
		{
			if (SourceAudioChannelConfiguration == null)
			{
				if (updateIsCopyFromSourceCheckBoxOnly) copyAudioChannelConfigurationFromSourceCheckBox.IsChecked = false;
				else CopyFromSource = false;
				return;
			}

			var channelOffset = 0;
			foreach (var sourceAudioChannelPair in SourceAudioChannelConfiguration.AudioChannelPairs)
			{
				if (sourceAudioChannelPair.DolbyDecoding)
				{
					if (!TryVerifyDolbyAudioChannelPairValues(sourceAudioChannelPair, updateIsCopyFromSourceCheckBoxOnly)) return;

					channelOffset += 6;
				}
				else if(!TryVerifyStandardAudioChannelPairValues(sourceAudioChannelPair, channelOffset, updateIsCopyFromSourceCheckBoxOnly))
				{
					return;
				}
				else
				{
					// Do nothing
				}
			}

			if (updateIsCopyFromSourceCheckBoxOnly) copyAudioChannelConfigurationFromSourceCheckBox.IsChecked = true;
			else CopyFromSource = true;
		}

		private bool TryVerifyDolbyAudioChannelPairValues(AudioChannelPair sourceAudioChannelPair, bool updateIsCopyFromSourceCheckBoxOnly = false)
		{
			var firstAudioChannelPair = AudioChannelPairs.FirstOrDefault(a => a.Channel == sourceAudioChannelPair.Channel);
			var secondAudioChannelPair = AudioChannelPairs.FirstOrDefault(a => a.Channel == sourceAudioChannelPair.Channel + 2);
			var thirdAudioChannelPair = AudioChannelPairs.FirstOrDefault(a => a.Channel == sourceAudioChannelPair.Channel + 4);
			var fourthAudioChannelPair = AudioChannelPairs.FirstOrDefault(a => a.Channel == sourceAudioChannelPair.Channel + 6);
			if (firstAudioChannelPair == null || secondAudioChannelPair == null || thirdAudioChannelPair == null || fourthAudioChannelPair == null)
			{
				if (updateIsCopyFromSourceCheckBoxOnly) copyAudioChannelConfigurationFromSourceCheckBox.IsChecked = false;
				else CopyFromSource = false;
				return false;
			}

			bool isFirstAudioPairFirstChannelInValid = !firstAudioChannelPair.FirstChannelValue.StartsWith("Dolby") || !firstAudioChannelPair.FirstChannelValue.Contains(String.Format("{0}&", 1));
			bool isSecondAudioPairFirstChannelInValid = !secondAudioChannelPair.FirstChannelValue.StartsWith("Dolby") || !secondAudioChannelPair.FirstChannelValue.Contains(String.Format("{0}&", 3));

			bool isThirdAudioPairFirstChannelInValid = !thirdAudioChannelPair.FirstChannelValue.StartsWith("Dolby") || !thirdAudioChannelPair.FirstChannelValue.Contains(String.Format("{0}&", 5));

			bool isFourthAudioPairFirstChanneInValid = !fourthAudioChannelPair.FirstChannelValue.StartsWith("Dolby") || !fourthAudioChannelPair.FirstChannelValue.Contains(String.Format("{0}&", 7));

			if ( isFirstAudioPairFirstChannelInValid || isSecondAudioPairFirstChannelInValid || isThirdAudioPairFirstChannelInValid || isFourthAudioPairFirstChanneInValid)
			{
				if (updateIsCopyFromSourceCheckBoxOnly) copyAudioChannelConfigurationFromSourceCheckBox.IsChecked = false;
				else CopyFromSource = false;
				return false;
			}

			return true;
		}

		private bool TryVerifyStandardAudioChannelPairValues(AudioChannelPair sourceAudioChannelPair, int channelOffset, bool updateIsCopyFromSourceCheckBoxOnly = false)
		{
			var audioChannel = sourceAudioChannelPair.Channel + channelOffset;
			if (audioChannel > 16)
			{
				// if there is a dolby decoded audio channel then we can stop the check here
				if (updateIsCopyFromSourceCheckBoxOnly) copyAudioChannelConfigurationFromSourceCheckBox.IsChecked = true;
				else CopyFromSource = true;
				return false;
			}

			var audioChannelPair = AudioChannelPairs.FirstOrDefault(a => a.Channel == audioChannel);
			if (audioChannelPair == null)
			{
				if (updateIsCopyFromSourceCheckBoxOnly) copyAudioChannelConfigurationFromSourceCheckBox.IsChecked = false;
				else CopyFromSource = false;
				return false;
			}

			bool stereoIsDifferentFromSource = audioChannelPair.IsStereo != sourceAudioChannelPair.IsStereo;
			bool firstChannelIsDifferentFromSource = audioChannelPair.FirstChannelValue != sourceAudioChannelPair.FirstChannelValue;
			bool firstChannelOtherTextIsDifferentFromSource = audioChannelPair.FirstChannelOtherText != sourceAudioChannelPair.FirstChannelOtherText;
			bool secondChannelIsDifferentFromSource = audioChannelPair.SecondChannelValue != sourceAudioChannelPair.SecondChannelValue;
			bool secondChannelOtherTextIsDifferentFromSource = audioChannelPair.SecondChannelOtherText != sourceAudioChannelPair.SecondChannelOtherText;

			bool firsChannelIsDifferent = firstChannelIsDifferentFromSource || firstChannelOtherTextIsDifferentFromSource;
			bool secondChannelIsDifferent = secondChannelIsDifferentFromSource || secondChannelOtherTextIsDifferentFromSource;
			if (stereoIsDifferentFromSource || firsChannelIsDifferent || secondChannelIsDifferent)
			{
				if (updateIsCopyFromSourceCheckBoxOnly) copyAudioChannelConfigurationFromSourceCheckBox.IsChecked = false;
				else CopyFromSource = false;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Add an audio pair.
		/// </summary>
		/// <param name="sender">The trigger to add the pair.</param>
		/// <param name="e">The arguments.</param>
		private void AddAudioPair(object sender, EventArgs e)
		{
			foreach (var audioChannelPair in AudioChannelPairs.OrderBy(p => p.Channel))
			{
				if (audioChannelPair.IsVisible) continue;

				audioChannelPair.IsVisible = true;
				break;
			}

			addAudioPairButton.IsVisible = CanAddAudioChannelPair;
			deleteAudioPairButton.IsVisible = CanDeleteAudioChannelPair;
		}

		/// <summary>
		/// Delete an audio pair.
		/// </summary>
		/// <param name="sender">The trigger to delete the pair.</param>
		/// <param name="e">The arguments.</param>
		private void DeleteAudioPair(object sender, EventArgs e)
		{
			foreach (var audioChannelPair in AudioChannelPairs.OrderByDescending(p => p.Channel))
			{
				if (!audioChannelPair.IsVisible || audioChannelPair.IsReadOnly) continue;

				audioChannelPair.FirstChannelSelectedOption = "None";
				audioChannelPair.SecondChannelSelectedOption = "None";

				audioChannelPair.IsVisible = false;

				HandleAudioChannelPairChanges(this, new AudioChannelPairChangedEventArgs { AudioChannelPair = audioChannelPair });

				break;
			}

			addAudioPairButton.IsVisible = CanAddAudioChannelPair;
			deleteAudioPairButton.IsVisible = CanDeleteAudioChannelPair;
		}

		/// <summary>
		/// Update the visibility of the widgets.
		/// </summary>
		private void HandleVisibilityUpdate()
		{
			audioEmbeddingRequiredLabel.IsVisible = !IsSource && IsVisible;
			audioEmbeddingRequiredDropdown.IsVisible = !IsSource && IsVisible;
			audioEmbeddingRequiredDropdown.IsEnabled = !IsReadOnly;

			audioDeembeddingRequiredLabel.IsVisible = !IsSource && IsVisible;
			audioDeembeddingRequiredDropdown.IsVisible = !IsSource && IsVisible;
			audioDeembeddingRequiredDropdown.IsEnabled = !IsReadOnly;

			audioShufflingRequiredLabel.IsVisible = !IsSource && IsVisible;
			audioShufflingRequiredDropdown.IsVisible = !IsSource && IsVisible;
			audioShufflingRequiredDropdown.IsEnabled = !IsReadOnly;

			audioChannelConfigurationLabel.IsVisible = !IsSource && IsVisible;
			copyAudioChannelConfigurationFromSourceCheckBox.IsVisible = !IsSource && CanCopyFromSource && IsVisible;
			copyAudioChannelConfigurationFromSourceCheckBox.IsEnabled = !IsReadOnly;

			foreach (var audioChannelPair in AudioChannelPairs)
			{
				audioChannelPair.IsReadOnly = IsReadOnly;
				audioChannelPair.HandleVisibilityUpdate();
			}

			addAudioPairButton.IsVisible = CanAddAudioChannelPair;
			addAudioPairButton.IsEnabled = !IsReadOnly;

			deleteAudioPairButton.IsVisible = CanDeleteAudioChannelPair;
			deleteAudioPairButton.IsEnabled = !IsReadOnly;
		}

		/// <summary>
		/// Handle any change done to the audio channel pair.
		/// </summary>
		/// <param name="sender">The trigger.</param>
		/// <param name="e">The arguments containing the audio channel pair that was changed.</param>
		private void HandleAudioChannelPairChanges(object sender, AudioChannelPairChangedEventArgs e)
		{
			if (IsSource)
			{
				HandleSourceAudioChannelPairUpdate();
			}
			else
			{
				// refresh the dropdowns in the child based on the source 
				HandleSourceAudioConfigurationUpdate();
			}
		}

		/// <summary>
		/// Handle the audio channel pair updated in the source.
		/// </summary>
		private void HandleSourceAudioChannelPairUpdate()
		{
			// update the options in the source audio channel pairs itself
			var dolbyAudioChannelPair = AudioChannelPairs.FirstOrDefault(a => a.FirstChannelValue.StartsWith("Dolby"));
			if (dolbyAudioChannelPair != null)
			{
				// remove the option from all other source options as Dolby-E can only be selected once
				foreach (var audioChannelPair in AudioChannelPairs)
				{
					if (audioChannelPair.Channel == dolbyAudioChannelPair.Channel) continue;

					audioChannelPair.RemoveDolbyOption();
				}
			}
			else
			{
				// reset the options to make sure all options are available
				// this is actually only needed in case Dolby-E was unselected
				foreach (var audioChannelPair in AudioChannelPairs) audioChannelPair.ResetOptions();
			}

			// update the options in the child audio channel pairs
			if (ChildAudioChannelConfigurations == null) return;

			foreach (var childAudioChannelConfiguration in ChildAudioChannelConfigurations)
			{
				childAudioChannelConfiguration.HandleSourceAudioConfigurationUpdate();
			}
		}

		/// <summary>
		/// Copy the source audio configuration in the child.
		/// </summary>
		private void CopySourceAudioConfiguration()
		{
			var childChannelOffset = 0;
			foreach (var sourceAudioChannelPair in SourceAudioChannelConfiguration.AudioChannelPairs)
			{
				if (sourceAudioChannelPair.DolbyDecoding)
				{
					CopySourceDolbyDecodedAudioChannelPair(sourceAudioChannelPair);

					childChannelOffset += 6;
				}
				else
				{
					var audioChannelPair = AudioChannelPairs.FirstOrDefault(a => a.Channel == sourceAudioChannelPair.Channel + childChannelOffset);
					if (audioChannelPair == null) continue;

					audioChannelPair.CopyFromSource(sourceAudioChannelPair);
				}
			}
		}

		/// <summary>
		/// Copy the previous Source audio configuration into the current Source Audio Channel Configuration.
		/// </summary>
		private void CopyPreviousAudioConfiguration(List<AudioChannelPair> previousSourceAudioChannelPairs)
		{
			bool isThereAnyPreviousExistingDolby = previousSourceAudioChannelPairs.Any(a => a != null && a.FirstChannelValue.StartsWith("Dolby"));

			foreach (var previousAudioChannelPair in previousSourceAudioChannelPairs)
			{
				if (previousAudioChannelPair == null) continue;

				var currentAudioChannelPair = AudioChannelPairs.FirstOrDefault(a => a.Channel == previousAudioChannelPair.Channel);
				if (currentAudioChannelPair == null) continue;

				if (previousAudioChannelPair.FirstChannelValue.StartsWith("Dolby"))
				{
					currentAudioChannelPair.CopyDolbyFromPreviousSource(previousAudioChannelPair);
				}
				else
				{
					currentAudioChannelPair.CopyFromPreviousSource(previousAudioChannelPair, isThereAnyPreviousExistingDolby);
				}
			}
		}

		/// <summary>
		/// Update the audio channel configuration based on the source audio channel configuration.
		/// </summary>
		private void UpdateBasedOnSourceAudioChannelConfiguration()
		{
			// loop through the source audio channel pairs and copy the source dolby decoded channels immediately
			// the rest of the destination channel pairs need to be updated with the remaining options
			var childChannelsToSkip = new List<int>();
			var sourceAudioChannelOptions = new HashSet<string> { "None" };

			foreach (var sourceAudioChannelPair in SourceAudioChannelConfiguration.AudioChannelPairs)
			{
				// skip audio channel pairs that are not used
				if (!sourceAudioChannelPair.IsVisible) continue;

				if (sourceAudioChannelPair.DolbyDecoding)
				{
					CopySourceDolbyDecodedAudioChannelPair(sourceAudioChannelPair);

					// add the child audio channels to the skipped list to not update them
					childChannelsToSkip.AddRange(new[] { sourceAudioChannelPair.Channel, sourceAudioChannelPair.Channel + 2, sourceAudioChannelPair.Channel + 4, sourceAudioChannelPair.Channel + 6 });

					continue;
				}

				sourceAudioChannelOptions.Add(sourceAudioChannelPair.FirstChannelSelectedSourceOption);
				if (!sourceAudioChannelPair.IsStereo) sourceAudioChannelOptions.Add(sourceAudioChannelPair.SecondChannelSelectedSourceOption);
			}

			foreach (var audioChannelPair in AudioChannelPairs)
			{
				if (childChannelsToSkip.Contains(audioChannelPair.Channel)) continue;

				var audioChannelPairOptions = new List<string>(sourceAudioChannelOptions);

				// loop through the other audio channel pairs and remove any option that is already selected
				foreach (var otherAudioChannelPair in AudioChannelPairs)
				{
					if (childChannelsToSkip.Contains(otherAudioChannelPair.Channel)) continue;

					// skip this specific audio channel pair
					// skip audio channel pairs that are not configured
					if (otherAudioChannelPair.Channel == audioChannelPair.Channel || !otherAudioChannelPair.IsVisible) continue;

					if (otherAudioChannelPair.FirstChannelSelectedOption != "None") audioChannelPairOptions.Remove(otherAudioChannelPair.FirstChannelSelectedOption);
					if (otherAudioChannelPair.SecondChannelSelectedOption != "None") audioChannelPairOptions.Remove(otherAudioChannelPair.SecondChannelSelectedOption);
				}

				audioChannelPair.UpdateAvailableAudioChannelOptions(audioChannelPairOptions);

				if (!CopyFromSource) audioChannelPair.UpdateChildAudioOtherOptionChannels(SourceAudioChannelConfiguration);
			}
		}

		/// <summary>
		/// Update the audio channel pairs that contain the Dolby decoded audio.
		/// </summary>
		/// <param name="sourceAudioChannelPair">The dolby decoded source audio channel pair.</param>
		private void CopySourceDolbyDecodedAudioChannelPair(AudioChannelPair sourceAudioChannelPair)
		{
			var firstAudioChannelPair = AudioChannelPairs.FirstOrDefault(a => a.Channel == sourceAudioChannelPair.Channel);
			if (firstAudioChannelPair != null) firstAudioChannelPair.CopyDecodedDolbyFromSource(sourceAudioChannelPair, 1);

			var secondAudioChannelPair = AudioChannelPairs.FirstOrDefault(a => a.Channel == sourceAudioChannelPair.Channel + 2);
			if (secondAudioChannelPair != null) secondAudioChannelPair.CopyDecodedDolbyFromSource(sourceAudioChannelPair, 3);

			var thirdAudioChannelPair = AudioChannelPairs.FirstOrDefault(a => a.Channel == sourceAudioChannelPair.Channel + 4);
			if (thirdAudioChannelPair != null) thirdAudioChannelPair.CopyDecodedDolbyFromSource(sourceAudioChannelPair, 5);

			var fourthAudioChannelPair = AudioChannelPairs.FirstOrDefault(a => a.Channel == sourceAudioChannelPair.Channel + 6);
			if (fourthAudioChannelPair != null) fourthAudioChannelPair.CopyDecodedDolbyFromSource(sourceAudioChannelPair, 7);
		}
	}
}
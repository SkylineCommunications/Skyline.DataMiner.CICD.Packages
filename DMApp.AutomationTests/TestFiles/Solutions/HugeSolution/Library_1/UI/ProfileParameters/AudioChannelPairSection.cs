namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ProfileParameters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.AudioChannelPair;

    /// <summary>
    /// An audio channel pair based on the profile parameters for a specific audio channel pair.
    /// </summary>
    public class AudioChannelPairSection : Section
	{
		private readonly AudioChannelPairSectionConfiguration configuration;
		private readonly Helpers helpers;

		private readonly Label descriptionLabel = new Label();

		private readonly Label descriptionFirstChannelLabel = new Label();

		private readonly Label descriptionSecondChannelLabel = new Label();

		/// <summary>
		/// Initializes a new instance of the <see cref="AudioChannelPairSection" /> class
		/// </summary>
		/// <param name="audioChannelPair">AudioChannelPair that is being displayed by this Section.</param>
		/// <param name="configuration"></param>
		/// <param name="helpers"></param>
		public AudioChannelPairSection(AudioChannelPair audioChannelPair, AudioChannelPairSectionConfiguration configuration = null, Helpers helpers = null)
        {
            this.configuration = configuration ?? new AudioChannelPairSectionConfiguration();
            this.helpers = helpers;
            AudioChannelPair = audioChannelPair ?? throw new ArgumentNullException(nameof(audioChannelPair));

            Initialize();
            GenerateUI();
            HandleVisibilityUpdate();
        }

        /// <summary>
        /// Gets the AudioChannelPair that is displayed by this section.
        /// </summary>
        public AudioChannelPair AudioChannelPair { get; private set; }

		/// <summary>
		/// This indicates if this specific audio channel pair should be visible.
		/// Note that it will only be visible in the UI if the configuration is visible.
		/// </summary>
		public new bool IsVisible
		{
			get => base.IsVisible;

			set
			{
				base.IsVisible = value;
				HandleVisibilityUpdate();
			}
		}

		/// <summary>
		/// Gets the checkBox that displays the IsStereo configuration parameter.
		/// </summary>
		public YleCheckBox StereoCheckBox { get; private set; }

		/// <summary>
		/// Gets the dropDown that displays the First Audio Channel ProfileParameter.
		/// </summary>
		public YleDropDown FirstChannelDropDown { get; private set; }

		/// <summary>
		/// Gets the textBox that displays the First Audio Channel Description ProfileParameter.
		/// </summary>
		public YleTextBox FirstChannelOtherTextBox { get; private set; }

		/// <summary>
		/// Gets the dropDown that displays the Second Audio Channel ProfileParameter.
		/// </summary>
		public YleDropDown SecondChannelDropDown { get; private set; }

		/// <summary>
		/// Gets the textBox that displays the Second Audio Channel Description ProfileParameter.
		/// </summary>
		public YleTextBox SecondChannelOtherTextBox { get; private set; }

		/// <summary>
		/// Gets the checkBox that displays the Dolby Decoding configuration parameter.
		/// </summary>
		public YleCheckBox DolbyDecodingCheckBox { get; private set; }

		public void RegenerateUI()
		{
			Clear();
			GenerateUI();
		}

		/// <summary>
		/// Adds the widgets to this section.
		/// </summary>
		private void GenerateUI()
		{
			Clear();

			int row = -1;
			int otherTextBoxColumn = configuration.InputWidgetColumn + configuration.InputWidgetSpan;

			AddWidget(descriptionLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(StereoCheckBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(descriptionFirstChannelLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(FirstChannelDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
			AddWidget(FirstChannelOtherTextBox, row, otherTextBoxColumn);

			AddWidget(descriptionSecondChannelLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(SecondChannelDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
			AddWidget(SecondChannelOtherTextBox, row, otherTextBoxColumn);

			AddWidget(DolbyDecodingCheckBox, ++row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
		}

		/// <summary>
		/// Initializes the widgets within this section and the linking with the underlying model objects.
		/// </summary>
		private void Initialize()
		{
			InitializeWidgets();
			SubscribeToModel();
		}

		private void InitializeWidgets()
		{
			descriptionLabel.Text = AudioChannelPair.Description;
			descriptionFirstChannelLabel.Text = $"Audio Channel {AudioChannelPair.Channel}";
			descriptionSecondChannelLabel.Text = $"Audio Channel {AudioChannelPair.Channel + 1}";

			// update the options for the channels
			// only the source should initially have any options
			// don't include the decoded Dolby channels in the source
			StereoCheckBox = new YleCheckBox("Stereo") { Id = AudioChannelPair.FirstChannelProfileParameter.Id, IsChecked = AudioChannelPair.IsStereo};

			FirstChannelDropDown = new YleDropDown(AudioChannelPair.FirstChannelOptions.Select(x => x.DisplayValue), AudioChannelPair.FirstChannel.DisplayValue) { Id = AudioChannelPair.FirstChannelProfileParameter.Id, Name = "First Channel Dropdown" };
			FirstChannelOtherTextBox = new YleTextBox(AudioChannelPair.FirstChannel.OtherDescription) { Id = AudioChannelPair.FirstChannelDescriptionProfileParameter.Id };

			SecondChannelDropDown = new YleDropDown(AudioChannelPair.SecondChannelOptions.Select(x => x.DisplayValue), AudioChannelPair.SecondChannel.DisplayValue) { Id = AudioChannelPair.SecondChannelProfileParameter.Id, Name = "Second Channel Dropdown" };
			SecondChannelOtherTextBox = new YleTextBox(AudioChannelPair.SecondChannel.OtherDescription) { Id = AudioChannelPair.SecondChannelDescriptionProfileParameter.Id };

			DolbyDecodingCheckBox = new YleCheckBox("Decode Dolby-E") { Id = AudioChannelPair.FirstChannelProfileParameter.Id, IsChecked = AudioChannelPair.DolbyDecodingProfileParameter != null && AudioChannelPair.DolbyDecodingProfileParameter.StringValue == "Yes", IsVisible = configuration.DecodeDolbyECheckBoxIsVisible};
		}

		private void SubscribeToModel()
		{
			AudioChannelPair.FirstChannelOptionsChanged += AudioChannelPair_FirstAudioChannelOptionsChanged;
			AudioChannelPair.SecondChannelOptionsChanged += AudioChannelPair_SecondAudioChannelOptionsChanged;
            AudioChannelPair.FirstChannelChanged += FirstChannel_Changed;
			AudioChannelPair.FirstChannelDescriptionProfileParameter.ValueChanged += FirstChannelDescriptionProfileParameter_ValueChanged;
			AudioChannelPair.SecondChannelChanged += SecondChannel_Changed;
			AudioChannelPair.SecondChannelDescriptionProfileParameter.ValueChanged += SecondChannelDescriptionProfileParameter_ValueChanged;
			AudioChannelPair.IsStereoChanged += AudioChannelPair_IsStereoChanged;

			if (AudioChannelPair.DolbyDecodingProfileParameter != null)
			{
				AudioChannelPair.DolbyDecodingProfileParameter.ValueChanged += AudioChannelPair_DolbyDecodingChanged;
			}
		}

		private void AudioChannelPair_FirstAudioChannelOptionsChanged(object sender, IReadOnlyList<AudioChannelOption> options)
		{
			FirstChannelDropDown.Options = options.Select(x => x.DisplayValue);
		}

		private void AudioChannelPair_SecondAudioChannelOptionsChanged(object sender, IReadOnlyList<AudioChannelOption> options)
		{
			SecondChannelDropDown.Options = options.Select(x => x.DisplayValue);
		}

		/// <summary>
		/// Sets the visibility of the widgets and sections contained in this section.
		/// </summary>
		private void HandleVisibilityUpdate()
		{
			// Show the correct widgets from the audio channel pair
			// The widgets are not shown when the audio channel pair or configuration are not visible
			descriptionLabel.IsVisible = IsVisible;
			StereoCheckBox.IsVisible = IsVisible;
            
			descriptionFirstChannelLabel.IsVisible = IsVisible && !AudioChannelPair.IsStereo;
			FirstChannelDropDown.IsVisible = IsVisible;
			FirstChannelOtherTextBox.IsVisible = FirstChannelDropDown.IsVisible && Convert.ToString(AudioChannelPair.FirstChannelProfileParameter.Value).Equals("Other");

			descriptionSecondChannelLabel.IsVisible = IsVisible && !AudioChannelPair.IsStereo;
			SecondChannelDropDown.IsVisible = IsVisible && !AudioChannelPair.IsStereo;
			SecondChannelOtherTextBox.IsVisible = SecondChannelDropDown.IsVisible && Convert.ToString(AudioChannelPair.SecondChannelProfileParameter.Value).Equals("Other");

			DolbyDecodingCheckBox.IsVisible = IsVisible && configuration.DecodeDolbyECheckBoxIsVisible && Convert.ToString(AudioChannelPair.FirstChannelProfileParameter.Value).StartsWith("Dolby") && AudioChannelPair.IsStereo;
		}

		/// <summary>
		/// Executed when the value of the First Audio Channel ProfileParameter is updated.
		/// </summary>
		/// <param name="sender">ProfileParameter of which the value was changed.</param>
		/// <param name="option">Updated value of the ProfileParameter.</param>
		private void FirstChannel_Changed(object sender, AudioChannelOptionChangedEventArgs option)
		{
			FirstChannelDropDown.Selected = option.NewValue.DisplayValue;
			HandleVisibilityUpdate();
		}

		/// <summary>
		/// Executed when the value of the Second Audio Channel ProfileParameter is updated.
		/// </summary>
		/// <param name="sender">ProfileParameter of which the value was changed.</param>
		/// <param name="option">Updated value of the ProfileParameter.</param>
		private void SecondChannel_Changed(object sender, AudioChannelOptionChangedEventArgs option)
		{
			SecondChannelDropDown.Selected = option.NewValue.DisplayValue;
			HandleVisibilityUpdate();
		}

		/// <summary>
		/// Executed when the value of the First Audio Channel Description ProfileParameter is updated.
		/// </summary>
		/// <param name="sender">ProfileParameter of which the value was changed.</param>
		/// <param name="e">Updated value of the ProfileParameter.</param>
		private void FirstChannelDescriptionProfileParameter_ValueChanged(object sender, object e)
		{
			FirstChannelOtherTextBox.Text = Convert.ToString(e);
		}

		/// <summary>
		/// Executed when the value of the Second Audio Channel Description ProfileParameter is updated.
		/// </summary>
		/// <param name="sender">ProfileParameter of which the value was changed.</param>
		/// <param name="e">Updated value of the ProfileParameter.</param>
		private void SecondChannelDescriptionProfileParameter_ValueChanged(object sender, object e)
		{
			SecondChannelOtherTextBox.Text = Convert.ToString(e);
		}

		/// <summary>
		/// Executed when the Dolby Decoding configuration parameter is updated.
		/// </summary>
		/// <param name="sender">ProfileParameter of which the value was changed.</param>
		/// <param name="e">Updated value of the ProfileParameter ("Yes" or "No").</param>
		private void AudioChannelPair_DolbyDecodingChanged(object sender, object e)
		{
			DolbyDecodingCheckBox.IsChecked = Convert.ToString(e) == "Yes";
			HandleVisibilityUpdate();
		}

		/// <summary>
		/// Executed when the IsStereo configuration parameter is updated.
		/// </summary>
		/// <param name="sender">ProfileParameter of which the value was changed.</param>
		/// <param name="e">Updated value of the ProfileParameter.</param>
		private void AudioChannelPair_IsStereoChanged(object sender, bool e)
		{
			StereoCheckBox.IsChecked = e;
			HandleVisibilityUpdate();
		}
	}
}
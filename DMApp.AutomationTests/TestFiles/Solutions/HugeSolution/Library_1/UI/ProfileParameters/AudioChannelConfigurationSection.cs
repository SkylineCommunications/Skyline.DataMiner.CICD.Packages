namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ProfileParameters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    /// <summary>
    /// This section is used to display an audio channel configuration.
    /// </summary>
    public class AudioChannelConfigurationSection : Section
    {
        private readonly AudioChannelConfigurationSectionConfiguration configuration;
        private readonly Helpers helpers;

        private readonly List<AudioChannelPairSection> audioChannelPairSections = new List<AudioChannelPairSection>();

        private readonly Label headerLabel = new Label("Audio Details") { Style = TextStyle.Heading };

        private readonly Label audioEmbeddingRequiredLabel = new Label("Audio Embedding");

        private readonly Label audioDeembeddingRequiredLabel = new Label("Audio Deembedding");

        private readonly Label audioShufflingRequiredLabel = new Label("Audio Shuffling");

        /// <summary>
        /// Initialize the audio channel configuration.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the provided AudioChannelConfiguration is null.</exception>
        public AudioChannelConfigurationSection(AudioChannelConfiguration audioChannelConfiguration, AudioChannelConfigurationSectionConfiguration configuration, Helpers helpers = null)
        {
            AudioChannelConfiguration = audioChannelConfiguration ?? throw new ArgumentNullException(nameof(audioChannelConfiguration));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.helpers = helpers;

            Initialize();
            if (audioChannelConfiguration.AudioChannelPairs.Any()) GenerateUI();
            HandleVisibilityAndEnabledUpdate();
        }

        public YleDropDown AudioEmbeddingRequiredDropDown { get; private set; }

        public YleDropDown AudioDeembeddingRequiredDropDown { get; private set; }

        public YleDropDown AudioShufflingRequiredDropDown { get; private set; }

        public CheckBox CopyFromSourceCheckBox { get; private set; }

        /// <summary>
        /// Gets the button that can be used to add an Audio Channel Pair to the UI.
        /// </summary>
        public Button AddAudioPairButton { get; private set; }

        /// <summary>
        /// Gets the button that can be used to remove an Audio Channel Pair from the UI.
        /// </summary>
        public Button DeleteAudioPairButton { get; private set; }

        /// <summary>
        /// Gets a list of the underlying audio channel pair sections.
        /// </summary>
        public IReadOnlyCollection<AudioChannelPairSection> AudioChannelPairSections => audioChannelPairSections;

        /// <summary>
        /// Gets or sets a value indicating if the section is visible or not.
        /// </summary>
        public new bool IsVisible
        {
            get => base.IsVisible;
            set
            {
                base.IsVisible = value;
                HandleVisibilityAndEnabledUpdate();
            }
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

        /// <summary>
        /// The list of audio channel pairs.
        /// </summary>
        public AudioChannelConfiguration AudioChannelConfiguration { get; private set; }

        public void RegenerateUI()
        {
            Clear();
            foreach (AudioChannelPairSection pairSection in AudioChannelPairSections) pairSection.RegenerateUI();
            GenerateUI();
        }

        /// <summary>
        /// Initializes the widgets within this section and the linking with the underlying model objects.
        /// </summary>
        private void Initialize()
        {
            InitializeWidgets();
            SubscribeToWidgets();
            SubscribeToAudioConfiguration();
        }

        private void InitializeWidgets()
        {
			if (configuration.SectionDividerCharacter != default)
			{
				headerLabel.Text = $"{new string(configuration.SectionDividerCharacter, 85)}\nAudio Details";
			}

            if (AudioChannelConfiguration.AudioDeembeddingRequiredProfileParameter != null)
            {
                var options = AudioChannelConfiguration.AudioDeembeddingRequiredProfileParameter.Discreets.Select(d => d.DisplayValue);
                AudioDeembeddingRequiredDropDown = new YleDropDown(options, !string.IsNullOrWhiteSpace(AudioChannelConfiguration.AudioDeembeddingRequiredProfileParameter.StringValue) ? AudioChannelConfiguration.AudioDeembeddingRequiredProfileParameter.StringValue : AudioChannelConfiguration.AudioDeembeddingRequiredProfileParameter.DefaultValue.StringValue) { Id = AudioChannelConfiguration.AudioDeembeddingRequiredProfileParameter.Id, Name = nameof(AudioDeembeddingRequiredDropDown) };
            }

            if (AudioChannelConfiguration.AudioEmbeddingRequiredProfileParameter != null)
            {
                var options = AudioChannelConfiguration.AudioEmbeddingRequiredProfileParameter.Discreets.Select(d => d.DisplayValue);
                AudioEmbeddingRequiredDropDown = new YleDropDown(options, !string.IsNullOrWhiteSpace(AudioChannelConfiguration.AudioEmbeddingRequiredProfileParameter.StringValue) ? AudioChannelConfiguration.AudioEmbeddingRequiredProfileParameter.StringValue : AudioChannelConfiguration.AudioEmbeddingRequiredProfileParameter.DefaultValue.StringValue) { Id = AudioChannelConfiguration.AudioEmbeddingRequiredProfileParameter.Id, Name = nameof(AudioEmbeddingRequiredDropDown) };
            }

            if (AudioChannelConfiguration.AudioShufflingRequiredProfileParameter != null)
            {
                var options = AudioChannelConfiguration.AudioShufflingRequiredProfileParameter.Discreets.Select(d => d.DisplayValue);
                AudioShufflingRequiredDropDown = new YleDropDown(options, !string.IsNullOrWhiteSpace(AudioChannelConfiguration.AudioShufflingRequiredProfileParameter.StringValue) ? AudioChannelConfiguration.AudioShufflingRequiredProfileParameter.StringValue : AudioChannelConfiguration.AudioShufflingRequiredProfileParameter.DefaultValue.StringValue) { Id = AudioChannelConfiguration.AudioShufflingRequiredProfileParameter.Id, Name = nameof(AudioShufflingRequiredDropDown) };
            }

            CopyFromSourceCheckBox = new YleCheckBox("Copy from source") { IsChecked = AudioChannelConfiguration.IsCopyFromSource, Name = nameof(CopyFromSourceCheckBox) };

            foreach (var audioChannelPair in AudioChannelConfiguration.AudioChannelPairs)
            {
                audioChannelPairSections.Add(new AudioChannelPairSection(audioChannelPair, configuration.AudioChannelPairSectionConfiguration));
            }

            AddAudioPairButton = new YleButton("Add Audio Pair") { IsVisible = false, Width = 200 };
            DeleteAudioPairButton = new YleButton("Delete Audio Pair") { IsVisible = false, Width = 200 };

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
        }

        private void SubscribeToWidgets()
        {
            CopyFromSourceCheckBox.Changed += (o, e) => HandleVisibilityAndEnabledUpdate();
        }

        private void SubscribeToAudioConfiguration()
        {
            AudioChannelConfiguration.AudioChannelPairAdded += (o, e) => HandleVisibilityAndEnabledUpdate();
            AudioChannelConfiguration.AudioChannelPairRemoved += (o, e) =>
            {
                HandleVisibilityAndEnabledUpdate();
                ClearHiddenAudioChannelPairs();
            };

            AudioChannelConfiguration.IsCopyFromSourceChanged += (s, e) =>
            {
                CopyFromSourceCheckBox.IsChecked = e;
                HandleVisibilityAndEnabledUpdate();
            };

            if (AudioChannelConfiguration.AudioShufflingRequiredProfileParameter != null)
            {
                AudioChannelConfiguration.AudioShufflingRequiredProfileParameter.ValueChanged += (s, e) =>
                {
                    AudioShufflingRequiredDropDown.Selected = AudioChannelConfiguration.AudioShufflingRequiredProfileParameter.StringValue;
                };
            }
        }

        private void ClearHiddenAudioChannelPairs()
        {
            foreach (var pair in AudioChannelPairSections.Select(s => s.AudioChannelPair).Where(p => p.Channel > AudioChannelConfiguration.LastDisplayedAudioPairchannel))
            {
                pair.Clear();
            }
        }

        /// <summary>
        /// Adds the widgets to this section.
        /// </summary>
        private void GenerateUI()
        {
            int row = -1;

            AddWidget(headerLabel, new WidgetLayout(++row, 0, 1, 20, horizontalAlignment: HorizontalAlignment.Left));

            if (AudioChannelConfiguration.AudioDeembeddingRequiredProfileParameter != null)
            {
                AddWidget(audioDeembeddingRequiredLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan));
                AddWidget(AudioDeembeddingRequiredDropDown, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));
            }

            if (AudioChannelConfiguration.AudioShufflingRequiredProfileParameter != null)
            {
                AddWidget(audioShufflingRequiredLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan));
                AddWidget(AudioShufflingRequiredDropDown, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));
            }

            if (AudioChannelConfiguration.AudioEmbeddingRequiredProfileParameter != null)
            {
                AddWidget(audioEmbeddingRequiredLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan));
                AddWidget(AudioEmbeddingRequiredDropDown, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));
            }

            AddWidget(CopyFromSourceCheckBox, new WidgetLayout(++row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));

            foreach (AudioChannelPairSection audioChannelPairSection in audioChannelPairSections)
            {
                AddSection(audioChannelPairSection, new SectionLayout(++row, 0));
                row += audioChannelPairSection.RowCount;
            }

            AddWidget(DeleteAudioPairButton, new WidgetLayout(++row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));
            AddWidget(AddAudioPairButton, new WidgetLayout(++row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
        }

		/// <summary>
		/// Update the visibility of the widgets and underlying sections.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "<Pending>")]
		private void HandleVisibilityAndEnabledUpdate()
        {
            headerLabel.IsVisible = IsVisible;

            if (AudioDeembeddingRequiredDropDown != null)
            {
                audioDeembeddingRequiredLabel.IsVisible = IsVisible && configuration.AudioDeembeddingRequiredDropDownIsVisible;
                AudioDeembeddingRequiredDropDown.IsVisible = IsVisible && configuration.AudioDeembeddingRequiredDropDownIsVisible;
            }

            if (AudioShufflingRequiredDropDown != null)
            {
                audioShufflingRequiredLabel.IsVisible = IsVisible && configuration.AudioShufflingRequiredDropDownIsVisible;
                AudioShufflingRequiredDropDown.IsVisible = IsVisible && configuration.AudioShufflingRequiredDropDownIsVisible;

                AudioShufflingRequiredDropDown.IsEnabled = IsEnabled && CopyFromSourceCheckBox.IsChecked;
            }

            if (AudioEmbeddingRequiredDropDown != null)
            {
                audioEmbeddingRequiredLabel.IsVisible = IsVisible && configuration.AudioEmbeddingRequiredDropDownIsVisible;
                AudioEmbeddingRequiredDropDown.IsVisible = IsVisible && configuration.AudioEmbeddingRequiredDropDownIsVisible;
            }

            CopyFromSourceCheckBox.IsVisible = IsVisible && configuration.CopyFromSourceCheckBoxIsVisible;
            CopyFromSourceCheckBox.IsEnabled = IsEnabled && configuration.CopyFromSourceCheckBoxIsEnabled;

            foreach (AudioChannelPairSection audioChannelPairSection in audioChannelPairSections)
            {
                audioChannelPairSection.IsVisible = IsVisible && (audioChannelPairSection.AudioChannelPair.Channel <= AudioChannelConfiguration.LastDisplayedAudioPairchannel);
                audioChannelPairSection.IsEnabled = IsEnabled && !CopyFromSourceCheckBox.IsChecked;
            }

            AddAudioPairButton.IsVisible = IsVisible && !CopyFromSourceCheckBox.IsChecked && AudioChannelConfiguration.CanAddAudioPair;
            DeleteAudioPairButton.IsVisible = IsVisible && !CopyFromSourceCheckBox.IsChecked && AudioChannelConfiguration.CanRemoveAudioPair;

            ToolTipHandler.SetTooltipVisibility(this);
        }
    }
}
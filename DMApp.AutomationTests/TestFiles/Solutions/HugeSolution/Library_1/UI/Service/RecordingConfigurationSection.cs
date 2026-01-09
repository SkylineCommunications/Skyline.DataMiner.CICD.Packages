namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Utilities;

	public class RecordingConfigurationSection : Section
	{
		private readonly RecordingConfiguration recordingConfiguration;
		private readonly RecordingConfigurationSectionConfiguration configuration;
		private readonly Helpers helpers;

		private readonly Label additionalSubRecordingDetailsTitle = new Label("Additional Sub-Recording Details") { Style = TextStyle.Heading, IsVisible = true };
		private readonly Label subRecordingsNeededLabel = new Label("Sub-Recordings Needed");

		private readonly Label recordingFileDetailsTitle = new Label("Recording File Details") { Style = TextStyle.Heading, IsVisible = true };
		private readonly Label recordingNameLabel = new Label("Recording Name");
		private readonly Label plasmaIdForArchiveLabel = new Label("PLASMA ID for Archive");
		private readonly Label recordingFileDestinationLabel = new Label("Recording File Destination");
		private readonly Label dlForArchivingLabel = new Label("Deadline for Archiving");
		private readonly Label recordingFileVideoResolutionLabel = new Label("Recording File Video Resolution");
		private readonly Label recordingFileVideoCodecLabel = new Label("Recording File Video Codec");
		private readonly Label recordingFileTimeCodeLabel = new Label("Recording File Time Code");
		private readonly Label subtitleProxyLabel = new Label("Subtitle Proxy");
		private readonly Label proxyFormatLabel = new Label("Proxy Format");
		private readonly Label fastRerunCopyLabel = new Label("Fast Rerun Copy");
		private readonly Label fastAreenaCopyLabel = new Label("Fast Areena Copy");
		private readonly Label broadcastReadyLabel = new Label("Broadcast Ready (Lähetysvalmis)");
		private readonly Label recordingFileDestinationPathLabel = new Label("Recording File Destination Path");

		private bool adjustServiceDetails = false;
		private string serviceDefinitionDescription = String.Empty;

		[DisplaysProperty(nameof(RecordingConfiguration.RecordingName))]
		private readonly YleTextBox recordingNameTextBox = new YleTextBox();

		[DisplaysProperty(nameof(RecordingConfiguration.PlasmaIdForArchive))]
		private readonly YleTextBox plasmaIdForArchiveTextBox = new YleTextBox() { ValidationPredicate = text => !string.IsNullOrWhiteSpace(text) };

		[DisplaysProperty(nameof(RecordingConfiguration.CopyPlasmaIdFromOrder))]
		private readonly YleCheckBox copyPlasmaIdFromOrderCheckBox = new YleCheckBox("Copy from Order");

		[DisplaysProperty(nameof(RecordingConfiguration.RecordingFileDestination))]
		private readonly DropDown recordingFileDestinationDropDown = new DropDown();

		[DisplaysProperty(nameof(RecordingConfiguration.DeadLineForArchiving))]
		private readonly DateTimePicker dlForArchivingDateTimePicker = new DateTimePicker();

		[DisplaysProperty(nameof(RecordingConfiguration.RecordingFileVideoResolution))]
		private readonly DropDown recordingFileVideoResolutionDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<VideoResolution>());

		[DisplaysProperty(nameof(RecordingConfiguration.RecordingFileVideoCodec))]
		private readonly DropDown recordingFileVideoCodecDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<VideoCodec>());

		[DisplaysProperty(nameof(RecordingConfiguration.RecordingFileTimeCodec))]
		private readonly DropDown recordingFileTimeCodeDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<TimeCodec>());

		[DisplaysProperty(nameof(RecordingConfiguration.SubtitleProxy))]
		private readonly CheckBox subtitleProxyCheckBox = new CheckBox("Required");

		[DisplaysProperty(nameof(RecordingConfiguration.ProxyFormat))]
		private readonly DropDown proxyFormatDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<ProxyFormat>());

		[DisplaysProperty(nameof(RecordingConfiguration.FastRerunCopy))]
		private readonly CheckBox fastRerunCopyCheckBox = new CheckBox("Required");

		[DisplaysProperty(nameof(RecordingConfiguration.FastAreenaCopy))]
		private readonly CheckBox fastAreenaCopyCheckBox = new CheckBox("Required");

		[DisplaysProperty(nameof(RecordingConfiguration.BroadcastReady))]
		private readonly CheckBox broadcastReadyCheckBox = new CheckBox("Required");

		[DisplaysProperty(nameof(RecordingConfiguration.EvsMessiNewsTarget))]
		private readonly YleDropDown evsMessiNewsDestinationPathDropDown = new YleDropDown { IsSorted = true };

		private Button addNewSubRecordingButton;
		private CheckBox subRecordingsNeededCheckBox;

		public RecordingConfigurationSection(RecordingConfiguration recordingConfiguration, RecordingConfigurationSectionConfiguration configuration, Helpers helpers = null)
		{
			this.recordingConfiguration = recordingConfiguration ?? throw new ArgumentNullException(nameof(recordingConfiguration));
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.helpers = helpers;

			Initialize();
			GenerateUI();
			HandleVisibilityAndEnabledUpdate();
		}

		public List<SubRecordingSection> SubRecordingSections { get; private set; } = new List<SubRecordingSection>();

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

		public bool AdjustServiceDetails
		{
			get => adjustServiceDetails;
			set
			{
				adjustServiceDetails = value;
				HandleVisibilityAndEnabledUpdate();
			}
		}

		public string ServiceDefinitionDescription
		{
			get => serviceDefinitionDescription;
			set
			{
				serviceDefinitionDescription = value;
				HandleVisibilityAndEnabledUpdate();
			}
		}

		public event EventHandler<DisplayedPropertyEventArgs> DisplayedPropertyChanged;

		public event EventHandler<bool> SubRecordingsNeededChanged;

		public event EventHandler<bool> CopyPlasmaIdFromOrder;

		public event EventHandler AddNewSubRecordingButtonPressed;

		public event EventHandler RegenerateDialog;

		public event EventHandler<SubRecordingSection> SubRecordingSectionsAdded;

		public void RegenerateUI()
		{
			Clear();
			foreach (var subRecordingSection in SubRecordingSections) subRecordingSection.RegenerateUI();
			GenerateUI();
		}

		public bool UpdateRecordingConfiguration()
		{
			bool changes = false;
			if (recordingConfiguration == null) return changes;

			string cleanedRecordingName = recordingNameTextBox.Text.Clean();
			string cleanedPlasmaIdForArchive = plasmaIdForArchiveTextBox.Text;
			string recordingFileDestinationPath = recordingConfiguration.SelectableEvsMessiNewsTargets.FirstOrDefault(x => x.DestinationPath == evsMessiNewsDestinationPathDropDown.Selected)?.Target;

			changes |= !String.Equals(recordingConfiguration.RecordingName, cleanedRecordingName);
			changes |= !String.Equals(recordingConfiguration.PlasmaIdForArchive, cleanedPlasmaIdForArchive);
			changes |= !String.Equals(recordingConfiguration.EvsMessiNewsTarget, recordingFileDestinationPath);

			recordingConfiguration.RecordingName = cleanedRecordingName;
			recordingConfiguration.PlasmaIdForArchive = cleanedPlasmaIdForArchive;
			recordingConfiguration.EvsMessiNewsTarget = recordingFileDestinationPath;

			return changes;
		}

		private void SubscribeToWidgets()
		{
			recordingNameTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(recordingNameTextBox)), Convert.ToString(e.Value).Clean()));

			plasmaIdForArchiveTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(plasmaIdForArchiveTextBox)), e.Value));

			copyPlasmaIdFromOrderCheckBox.Changed += (s, e) =>
			{
				CopyPlasmaIdFromOrder?.Invoke(this, copyPlasmaIdFromOrderCheckBox.IsChecked);
				HandleVisibilityAndEnabledUpdate();
			};

			recordingFileDestinationDropDown.Changed += (s, e) =>
			{
				DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(recordingFileDestinationDropDown)), e.Selected.GetEnumValue<FileDestination>()));
				HandleVisibilityAndEnabledUpdate();
			};

			recordingFileVideoResolutionDropDown.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(recordingFileVideoResolutionDropDown)), e.Selected.GetEnumValue<VideoResolution>()));

			recordingFileVideoCodecDropDown.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(recordingFileVideoCodecDropDown)), e.Selected.GetEnumValue<VideoCodec>()));

			recordingFileTimeCodeDropDown.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(recordingFileTimeCodeDropDown)), e.Selected.GetEnumValue<TimeCodec>()));

			subtitleProxyCheckBox.Changed += (s, e) =>
			{
				DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(subtitleProxyCheckBox)), e.IsChecked));
				HandleVisibilityAndEnabledUpdate();
			};

			proxyFormatDropDown.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(proxyFormatDropDown)), e.Selected.GetEnumValue<ProxyFormat>()));

			fastRerunCopyCheckBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(fastRerunCopyCheckBox)), e.IsChecked));

			fastAreenaCopyCheckBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(fastAreenaCopyCheckBox)), e.IsChecked));

			broadcastReadyCheckBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(broadcastReadyCheckBox)), e.IsChecked));

			evsMessiNewsDestinationPathDropDown.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(evsMessiNewsDestinationPathDropDown)), recordingConfiguration.SelectableEvsMessiNewsTargets.First(x => x.DestinationPath.Equals(e.Value)).Target));

			dlForArchivingDateTimePicker.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(dlForArchivingDateTimePicker)), e.DateTime));

			subRecordingsNeededCheckBox.Changed += (o, e) =>
			{
				SubRecordingsNeededChanged?.Invoke(this, e.IsChecked);
				HandleVisibilityAndEnabledUpdate();
			};

			addNewSubRecordingButton.Pressed += (o, e) => AddNewSubRecordingButtonPressed?.Invoke(this, EventArgs.Empty);
		}

		private void Initialize()
		{
			InitializeWidgets();
			SubscribeToWidgets();
			SubscribeToRecordingConfiguration();
		}

		private void SubscribeToRecordingConfiguration()
		{
			recordingConfiguration.RecordingNameChanged += (s, e) => recordingNameTextBox.Text = e;

			recordingConfiguration.PlasmaIdForArchiveChanged += (s, e) => plasmaIdForArchiveTextBox.Text = e;

			recordingConfiguration.RecordingFileDestinationChanged += (s, e) => recordingFileDestinationDropDown.Selected = e.GetDescription();
			recordingConfiguration.SelectableRecordingFileDestinationsChanged += (s, e) => recordingFileDestinationDropDown.Options = recordingConfiguration.SelectableRecordingFileDestinations;

			recordingConfiguration.EvsMessiNewsTargetChanged += (s, e) => evsMessiNewsDestinationPathDropDown.Selected = recordingConfiguration.SelectedEvsMessiNewsTarget?.DestinationPath;
			recordingConfiguration.SelectableEvsMessiNewsTargetsChanged += (s, e) => evsMessiNewsDestinationPathDropDown.Options = recordingConfiguration.SelectableEvsMessiNewsTargets.Select(x => x.DestinationPath).ToList();

			recordingConfiguration.SubRecordingsAdded += RecordingConfiguration_SubRecordingsAdded;
			recordingConfiguration.SubRecordingsDeleted += RecordingConfiguration_SubRecordingsDeleted;

			this.SubscribeToDisplayedObjectValidation(recordingConfiguration);
		}

		private void InitializeWidgets()
		{
			recordingNameTextBox.Text = recordingConfiguration.RecordingName;

			plasmaIdForArchiveTextBox.Text = recordingConfiguration.PlasmaIdForArchive;
			copyPlasmaIdFromOrderCheckBox.IsChecked = recordingConfiguration.CopyPlasmaIdFromOrder;

			recordingFileDestinationDropDown.Options = recordingConfiguration.SelectableRecordingFileDestinations ?? new List<string>();
			recordingFileDestinationDropDown.Selected = recordingFileDestinationDropDown.Options.FirstOrDefault();

			recordingFileVideoResolutionDropDown.Selected = recordingConfiguration.RecordingFileVideoResolution.GetDescription();

			recordingFileVideoCodecDropDown.Selected = recordingConfiguration.RecordingFileVideoCodec.GetDescription();

			recordingFileTimeCodeDropDown.Selected = recordingConfiguration.RecordingFileTimeCodec.GetDescription();

			subtitleProxyCheckBox.IsChecked = recordingConfiguration.SubtitleProxy;

			proxyFormatDropDown.Selected = recordingConfiguration.ProxyFormat.GetDescription();

			fastRerunCopyCheckBox.IsChecked = recordingConfiguration.FastRerunCopy;

			fastAreenaCopyCheckBox.IsChecked = recordingConfiguration.FastAreenaCopy;

			broadcastReadyCheckBox.IsChecked = recordingConfiguration.BroadcastReady;

			evsMessiNewsDestinationPathDropDown.Options = recordingConfiguration.SelectableEvsMessiNewsTargets.Select(x => x.DestinationPath);
			evsMessiNewsDestinationPathDropDown.Selected = recordingConfiguration.SelectedEvsMessiNewsTarget?.DestinationPath;

			dlForArchivingDateTimePicker.DateTime = recordingConfiguration.DeadLineForArchiving;

			subRecordingsNeededCheckBox = new CheckBox { IsChecked = recordingConfiguration.SubRecordingsNeeded };

			addNewSubRecordingButton = new Button("Add Sub-Recording") { Width = 150, IsVisible = recordingConfiguration.SubRecordingsNeeded };

			SubRecordingSections = recordingConfiguration.SubRecordings.Select(s => new SubRecordingSection(s)).ToList();
		}

		private void RecordingConfiguration_SubRecordingsAdded(object sender, SubRecording subRecording)
		{
			SubRecordingSection subRecordingSection = new SubRecordingSection(subRecording);
			SubRecordingSections.Add(subRecordingSection);

			SubRecordingSectionsAdded?.Invoke(this, subRecordingSection);
			RegenerateDialog?.Invoke(this, new EventArgs());
		}

		private void RecordingConfiguration_SubRecordingsDeleted(object sender, IEnumerable<Guid> removedSubRecordingIds)
		{
			SubRecordingSections.RemoveAll(section => removedSubRecordingIds.Contains(section.SubRecordingId));
			RegenerateDialog?.Invoke(this, new EventArgs());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "<Pending>")]
		private void HandleVisibilityAndEnabledUpdate()
		{
			bool isMessiNewsRecording = ServiceDefinitionDescription?.Contains("News") ?? false;

			recordingNameLabel.IsVisible = IsVisible && configuration.RecordingNameIsVisible;
			recordingNameTextBox.IsVisible = IsVisible && configuration.RecordingNameIsVisible;
			recordingNameTextBox.IsEnabled = IsEnabled && configuration.IsEnabled;

			plasmaIdForArchiveLabel.IsVisible = IsVisible && configuration.PlasmaIdForArchiveIsVisible;
			plasmaIdForArchiveTextBox.IsVisible = IsVisible && configuration.PlasmaIdForArchiveIsVisible;
			plasmaIdForArchiveTextBox.IsEnabled = IsEnabled && configuration.IsEnabled && !copyPlasmaIdFromOrderCheckBox.IsChecked;
			copyPlasmaIdFromOrderCheckBox.IsVisible = IsVisible && configuration.PlasmaIdForArchiveIsVisible;
			copyPlasmaIdFromOrderCheckBox.IsEnabled = IsEnabled && configuration.IsEnabled;

			recordingFileDestinationLabel.IsVisible = IsVisible && configuration.RecordingFileDestinationIsVisible;
			recordingFileDestinationDropDown.IsVisible = IsVisible && configuration.RecordingFileDestinationIsVisible;
			recordingFileDestinationDropDown.IsEnabled = IsEnabled && configuration.IsEnabled;

			recordingFileDestinationPathLabel.IsVisible = IsVisible && configuration.RecordingFileDestinationPathIsVisible && isMessiNewsRecording;
			evsMessiNewsDestinationPathDropDown.IsVisible = IsVisible && configuration.RecordingFileDestinationPathIsVisible && isMessiNewsRecording;
			evsMessiNewsDestinationPathDropDown.IsEnabled = IsEnabled && configuration.IsEnabled;

			dlForArchivingLabel.IsVisible = IsVisible && configuration.DeadlineForArchivingIsVisible && recordingConfiguration.RecordingFileDestination == FileDestination.ArchiveMetro && !isMessiNewsRecording && AdjustServiceDetails;
			dlForArchivingDateTimePicker.IsVisible = IsVisible && configuration.DeadlineForArchivingIsVisible && recordingConfiguration.RecordingFileDestination == FileDestination.ArchiveMetro && !isMessiNewsRecording && AdjustServiceDetails;
			dlForArchivingDateTimePicker.IsEnabled = IsEnabled && configuration.IsEnabled;

			recordingFileVideoResolutionLabel.IsVisible = IsVisible && configuration.RecordingFileVideoResolutionIsVisible && !isMessiNewsRecording && AdjustServiceDetails;
			recordingFileVideoResolutionDropDown.IsVisible = IsVisible && configuration.RecordingFileVideoResolutionIsVisible && !isMessiNewsRecording && AdjustServiceDetails;
			recordingFileVideoResolutionDropDown.IsEnabled = IsEnabled && configuration.IsEnabled;

			recordingFileVideoCodecLabel.IsVisible = IsVisible && configuration.RecordingFileVideoCodecIsVisible && !isMessiNewsRecording && AdjustServiceDetails;
			recordingFileVideoCodecDropDown.IsVisible = IsVisible && configuration.RecordingFileVideoCodecIsVisible && !isMessiNewsRecording && AdjustServiceDetails;
			recordingFileVideoCodecDropDown.IsEnabled = IsEnabled && configuration.IsEnabled;

			recordingFileTimeCodeLabel.IsVisible = IsVisible && configuration.RecordingFileTimeCodeIsVisible && !isMessiNewsRecording && AdjustServiceDetails;
			recordingFileTimeCodeDropDown.IsVisible = IsVisible && configuration.RecordingFileTimeCodeIsVisible && !isMessiNewsRecording && AdjustServiceDetails;
			recordingFileTimeCodeDropDown.IsEnabled = IsEnabled && configuration.IsEnabled;

			subtitleProxyLabel.IsVisible = IsVisible && configuration.SubtitleProxyIsVisible;
			subtitleProxyCheckBox.IsVisible = IsVisible && configuration.SubtitleProxyIsVisible;
			subtitleProxyCheckBox.IsEnabled = IsEnabled && configuration.IsEnabled;

			proxyFormatLabel.IsVisible = IsVisible && configuration.ProxyFormatIsVisible && recordingConfiguration.SubtitleProxy;
			proxyFormatDropDown.IsVisible = IsVisible && configuration.ProxyFormatIsVisible && recordingConfiguration.SubtitleProxy;
			proxyFormatDropDown.IsEnabled = IsEnabled && configuration.IsEnabled;

			fastRerunCopyLabel.IsVisible = IsVisible && configuration.FastRerunCopyIsVisible;
			fastRerunCopyCheckBox.IsVisible = IsVisible && configuration.FastRerunCopyIsVisible;
			fastRerunCopyCheckBox.IsEnabled = IsEnabled && configuration.IsEnabled;

			fastAreenaCopyLabel.IsVisible = IsVisible && configuration.FastAreenaCopyIsVisible;
			fastAreenaCopyCheckBox.IsVisible = IsVisible && configuration.FastAreenaCopyIsVisible;
			fastAreenaCopyCheckBox.IsEnabled = IsEnabled && configuration.IsEnabled;

			broadcastReadyLabel.IsVisible = IsVisible && configuration.BroadcastReadyIsVisible;
			broadcastReadyCheckBox.IsVisible = IsVisible && configuration.BroadcastReadyIsVisible;
			broadcastReadyCheckBox.IsEnabled = IsEnabled && configuration.IsEnabled;

			addNewSubRecordingButton.IsVisible = IsVisible && subRecordingsNeededCheckBox.IsVisible && subRecordingsNeededCheckBox.IsChecked;
			addNewSubRecordingButton.IsEnabled = IsEnabled && configuration.IsEnabled;

			foreach (var subRecordingSection in SubRecordingSections)
			{
				subRecordingSection.IsVisible = IsVisible;
				subRecordingSection.IsEnabled = IsEnabled && configuration.IsEnabled;
			}

			ToolTipHandler.SetTooltipVisibility(this);
		}

		private void GenerateUI()
		{
			int row = -1;

			AddWidget(recordingFileDetailsTitle, new WidgetLayout(++row, 0, 1, configuration.LabelSpan));

			AddWidget(recordingNameLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan));
			AddWidget(recordingNameTextBox, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));

			AddWidget(plasmaIdForArchiveLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan));
			AddWidget(plasmaIdForArchiveTextBox, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));
			AddWidget(copyPlasmaIdFromOrderCheckBox, new WidgetLayout(row, configuration.InputWidgetColumn + configuration.InputWidgetSpan));

			AddWidget(recordingFileDestinationLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan));
			AddWidget(recordingFileDestinationDropDown, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));

			AddWidget(recordingFileDestinationPathLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan));
			AddWidget(evsMessiNewsDestinationPathDropDown, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));

			AddWidget(dlForArchivingLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan));
			AddWidget(dlForArchivingDateTimePicker, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));

			AddWidget(recordingFileVideoResolutionLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan));
			AddWidget(recordingFileVideoResolutionDropDown, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));

			AddWidget(recordingFileVideoCodecLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan));
			AddWidget(recordingFileVideoCodecDropDown, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));

			AddWidget(recordingFileTimeCodeLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan));
			AddWidget(recordingFileTimeCodeDropDown, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));

			AddWidget(subtitleProxyLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan, verticalAlignment: VerticalAlignment.Center));
			AddWidget(subtitleProxyCheckBox, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan, verticalAlignment: VerticalAlignment.Center));

			AddWidget(proxyFormatLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan));
			AddWidget(proxyFormatDropDown, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));

			AddWidget(fastRerunCopyLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan, verticalAlignment: VerticalAlignment.Center));
			AddWidget(fastRerunCopyCheckBox, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan, verticalAlignment: VerticalAlignment.Center));

			AddWidget(fastAreenaCopyLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan, verticalAlignment: VerticalAlignment.Center));
			AddWidget(fastAreenaCopyCheckBox, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan, verticalAlignment: VerticalAlignment.Center));

			AddWidget(broadcastReadyLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan, verticalAlignment: VerticalAlignment.Center));
			AddWidget(broadcastReadyCheckBox, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan, verticalAlignment: VerticalAlignment.Center));

			AddWidget(additionalSubRecordingDetailsTitle, new WidgetLayout(++row, 0, 1, configuration.LabelSpan));

			AddWidget(subRecordingsNeededLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan, verticalAlignment: VerticalAlignment.Center));
			AddWidget(subRecordingsNeededCheckBox, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan, verticalAlignment: VerticalAlignment.Center));

			foreach (var subRecordingSection in SubRecordingSections)
			{
				AddSection(subRecordingSection, new SectionLayout(++row, 0));
				row += subRecordingSection.RowCount;

				AddWidget(new WhiteSpace(), new WidgetLayout(++row, 1, 1, 2));
			}

			AddWidget(addNewSubRecordingButton, new WidgetLayout(++row, 1, 1, 2));

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
		}
	}
}
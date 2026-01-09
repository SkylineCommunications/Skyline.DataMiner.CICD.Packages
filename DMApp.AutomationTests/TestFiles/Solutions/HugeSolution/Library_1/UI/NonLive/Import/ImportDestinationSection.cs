namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.Import
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ImportDestinationSection : ImportSubSection
	{
		private readonly Label ingestDestinationTitleLabel = new Label("Import destination details") { Style = TextStyle.Bold };

		private readonly Dictionary<InterplayPamElements, NonLiveManagerTreeViewSection> treeViewSections = new Dictionary<InterplayPamElements, NonLiveManagerTreeViewSection>();

		public ImportDestinationSection(Helpers helpers, ISectionConfiguration configuration, ImportMainSection section, Ingest ingest = null) : base(helpers, configuration, section, ingest)
		{
			var helsinkiTreeView = new NonLiveManagerTreeViewSection(helpers, configuration, new AvidInterplayPamManager(helpers, InterplayPamElements.Helsinki), TreeViewType.FolderSelector);
			helsinkiTreeView.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			helsinkiTreeView.RegenerateUiRequired += HandleRegenerateUiRequired;

			var vaasaTreeView = new NonLiveManagerTreeViewSection(helpers, configuration, new AvidInterplayPamManager(helpers, InterplayPamElements.Vaasa), TreeViewType.FolderSelector);
			vaasaTreeView.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			vaasaTreeView.RegenerateUiRequired += HandleRegenerateUiRequired;

			var tampereTreeView = new NonLiveManagerTreeViewSection(helpers, configuration, new AvidInterplayPamManager(helpers, InterplayPamElements.Tampere), TreeViewType.FolderSelector);
			tampereTreeView.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			tampereTreeView.RegenerateUiRequired += HandleRegenerateUiRequired;

			helsinkiTreeView.SelectedItemChanged += (sender, args) => IsValid();
			vaasaTreeView.SelectedItemChanged += (sender, args) => IsValid();
			tampereTreeView.SelectedItemChanged += (sender, args) => IsValid();

			helsinkiTreeView.SourceLabel.Text = "Interplay destination folder";
			vaasaTreeView.SourceLabel.Text = "Interplay destination folder";
			tampereTreeView.SourceLabel.Text = "Interplay destination folder";

			helsinkiTreeView.InitRoot();
			vaasaTreeView.InitRoot();
			tampereTreeView.InitRoot();

			treeViewSections.Add(InterplayPamElements.Helsinki, helsinkiTreeView);
			treeViewSections.Add(InterplayPamElements.Vaasa, vaasaTreeView);
			treeViewSections.Add(InterplayPamElements.Tampere, tampereTreeView);

			InitializeIngest();

			GenerateUi(out int row);
            IsValid();
		}

		private Folder DestinationFolder => ingestMainSection.IngestDestination == InterplayPamElements.UA ? null : treeViewSections[ingestMainSection.IngestDestination].SelectedFolder;

		public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections
		{
			get
			{
				return treeViewSections.Select(x => x.Value);
			}
		}

		public override bool IsValid()
		{
			if (ingestMainSection.IngestDestination == InterplayPamElements.UA) return true;		
			
			bool isValidFolder = DestinationFolder != null;

			treeViewSections[ingestMainSection.IngestDestination].ValidationState = isValidFolder ? UIValidationState.Valid : UIValidationState.Invalid;
			treeViewSections[ingestMainSection.IngestDestination].ValidationText = "Select a destination folder";

			return isValidFolder;
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(ingestDestinationTitleLabel, new WidgetLayout(++row, 0));

			if (ingestMainSection.IngestDestination != InterplayPamElements.UA)
			{
				AddSection(treeViewSections[ingestMainSection.IngestDestination], new SectionLayout(++row, 0));
			}

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			ingestDestinationTitleLabel.IsVisible = IsVisible && ingestMainSection.IngestDestination != InterplayPamElements.UA;
			
			foreach (var treeViewSection in treeViewSections.Values)
			{
				treeViewSection.IsVisible = ingestDestinationTitleLabel.IsVisible;
				treeViewSection.IsEnabled = IsEnabled;
			}

            ToolTipHandler.SetTooltipVisibility(this);
        }

		public override void UpdateIngest(Ingest ingest)
		{
			ingest.IngestDestination = new IngestDestination
			{
				Destination = EnumExtensions.GetDescriptionFromEnumValue(ingestMainSection.IngestDestination),
				InterplayDestinationFolder = DestinationFolder?.URL
			};

			// Update non - live teams filter
			ingest.TeamMgmt = false;
			ingest.TeamNews = ingestMainSection.IngestDestination == InterplayPamElements.UA;
			ingest.TeamHki = ingestMainSection.IngestDestination == InterplayPamElements.Helsinki;
			ingest.TeamTre = ingestMainSection.IngestDestination == InterplayPamElements.Tampere;
			ingest.TeamVsa = ingestMainSection.IngestDestination == InterplayPamElements.Vaasa;
		}

		public void IngestDestinationDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				InvokeRegenerateUi();
				IsValid();
			}
        }

		private void InitializeIngest()
		{
			if (ingest == null) return;

			ingestMainSection.IngestDestinationSelection = ingest.IngestDestination.Destination;
			if (ingestMainSection.IngestDestination != InterplayPamElements.UA && !String.IsNullOrWhiteSpace(ingest.IngestDestination.InterplayDestinationFolder))
			{
				treeViewSections[ingestMainSection.IngestDestination].Initialize(new string[] { ingest.IngestDestination.InterplayDestinationFolder });
			}
		}

		public override void RegenerateUi()
		{
			GenerateUi(out int row);
		}
	}
}

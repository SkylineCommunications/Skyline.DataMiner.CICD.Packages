namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class UploadSynopsisSection : Section
	{
		private readonly Label title = new Label("Linked Synopsis") { Style = TextStyle.Heading };
		private readonly YleButton uploadSynopsisButton = new YleButton("Upload Synopsis");
		private readonly Label uploadedSynopsisLabel = new Label("Uploaded Synopsis");
		private readonly List<Tuple<Label, Button>> synopsisLabels = new List<Tuple<Label, Button>>();

		private readonly Service service;
		private readonly UploadSynopsisSectionConfiguration configuration;

		public UploadSynopsisSection(Service service, UploadSynopsisSectionConfiguration configuration)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

			Initialize();
			GenerateUI();
		}

		public event EventHandler<Service> UploadSynopsisButtonPressed;

		public event EventHandler<string> DeleteSynopsisButtonPressed;

		public event EventHandler RegenerateDialog;

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

		public void RegenerateUI()
		{
			Clear();
			GenerateUI();
		}

		/// <summary>
		/// Initializes the widgets within this section and the linking with the underlying model objects.
		/// </summary>
		private void Initialize()
		{
			InitializeWidgets();
			SubscribeToWidgets();
			SubscribeToService();
		}

		private void SubscribeToService()
		{
			service.SynopsisFiles.CollectionChanged += (s, e) =>
			{
				InitializeWidgets();
				RegenerateDialog?.Invoke(this, EventArgs.Empty);
			};
		}

		private void InitializeWidgets()
		{
			synopsisLabels.Clear();

			foreach (var synopsis in service.SynopsisFiles)
			{
				var deleteButton = new Button("Delete");
				deleteButton.Pressed += (s, e) => DeleteSynopsisButtonPressed?.Invoke(this, synopsis);
				string fileName = Path.GetFileName(synopsis);
				synopsisLabels.Add(new Tuple<Label, Button>(new Label(fileName), deleteButton));
			}
		}

		private void SubscribeToWidgets()
		{
			uploadSynopsisButton.Pressed += (s, e) => UploadSynopsisButtonPressed?.Invoke(this, service);
		}

		/// <summary>
		/// Adds the widgets to this section.
		/// </summary>
		private void GenerateUI()
		{
			int row = -1;

			AddWidget(title, ++row, 0, 1, 5);

			AddWidget(uploadedSynopsisLabel, ++row, 0, 1, configuration.LabelSpan, verticalAlignment: VerticalAlignment.Top);
			foreach (var tuple in synopsisLabels)
			{
				AddWidget(tuple.Item1, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan - 1);
				AddWidget(tuple.Item2, row, configuration.InputWidgetColumn + configuration.InputWidgetSpan - 1);
				++row;
			}

			AddWidget(uploadSynopsisButton, ++row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
		}

		private void HandleVisibilityAndEnabledUpdate()
		{
			uploadedSynopsisLabel.IsVisible = IsVisible && synopsisLabels.Any();
		}
	}

	public class DeleteSynopsisEventArgs : EventArgs
	{
		public DeleteSynopsisEventArgs(Service service, string synopsis)
		{
			Service = service ?? throw new ArgumentNullException(nameof(service));
			Synopsis = synopsis ?? throw new ArgumentNullException(nameof(synopsis));
		}

		public Service Service { get; private set; }

		public string Synopsis { get; private set; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Skyline.DataMiner.ConnectorAPI.EVS.IPD_VIA.Model;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.EVS;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using Label = Skyline.DataMiner.Utils.InteractiveAutomationScript.Label;

namespace Debug_2.Debug.Integrations
{
	public class SendEvsNotificationDialog : Dialog
	{
		private readonly EvsManager evsManager;

		private readonly Label evsLabel = new Label("EVS") { Style = TextStyle.Heading };

		private readonly Label idLabel = new Label("ID");
		private readonly YleTextBox idTextBox = new YleTextBox { ValidationText = "Required field", ValidationPredicate = text => !String.IsNullOrWhiteSpace(text), Text = Guid.NewGuid().ToString() };
		private readonly Button randomIdButton = new Button("Enter Random Guid") { Width = 150 };

		private readonly Label nameLabel = new Label("Name");
		private readonly TextBox nameTextBox = new TextBox();

		private readonly Label startLabel = new Label("Start");
		private readonly DateTimePicker startDateTimePicker = new DateTimePicker(DateTime.Now.AddDays(1));

		private readonly Label endLabel = new Label("End");
		private readonly DateTimePicker endDateTimePicker = new DateTimePicker(DateTime.Now.AddDays(1).AddHours(1));

		private readonly Label recorderRefLabel = new Label("Recorder Reference");
		private readonly TextBox recorderRefTextBox = new TextBox { Text = "XS-TEST IN-1" };

		private readonly Label targetRefLabel = new Label("Target Reference");
		private readonly TextBox targetRefTextbox = new TextBox { Text = "Ingest to Nearline" };

		private readonly Button metadataTemplateButton = new Button("Use Metadata Template");

		private readonly ListSection<string> metaDataProfileFqnSection = new ListSection<string>("Metadata Profile Fqns");
		private readonly DictionarySection<string, string> valuesSection = new DictionarySection<string, string>("Values") { Value = new Dictionary<string, string>
		{
			{ "yle-id", "987654" },
			{ "subtitle-proxy", "False" },
			{ "plasma-id", "132456" },
			{ "order-name", "Order Name" },
			{ "fast-rerun-copy", "False" },
			{ "areena-copy", "True" },
			{ "additional-information", "Some random info" },
		}};

		private readonly ListSection<string> contextSection = new ListSection<string>("Contexts");

		private readonly Button sendNotificationButton = new Button("Send Notification") { Width = 200 };

		private readonly List<RequestResultSection> responseSections = new List<RequestResultSection>();

		public SendEvsNotificationDialog(Helpers helpers) : base(helpers.Engine)
		{
			this.evsManager = new EvsManager(helpers);

			Title = "EVS";

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		private void Initialize()
		{
			metaDataProfileFqnSection.SetValue(new List<string> { "yle-nameandid" });

			randomIdButton.Pressed += (s, e) => idTextBox.Text = Guid.NewGuid().ToString();

			metadataTemplateButton.Pressed += MetadataTemplateButton_Pressed;
			metaDataProfileFqnSection.RegenerateUi += (s, e) => GenerateUi();
			valuesSection.RegenerateUi += (s, e) => GenerateUi();
			contextSection.RegenerateUi += (s, e) => GenerateUi();

			sendNotificationButton.Pressed += SendNotificationButton_Pressed;
		}

		private void MetadataTemplateButton_Pressed(object sender, EventArgs e)
		{
			string defaultMetaDataProfileFqn = "ylebasicprogrammemetadata";

			var defaultMetaDataProfileFieldNames = new List<string>
			{
				"finnish-main-title",
				"plasma-id",
				"yle-id",
				"audio-tracks-1-2",
				"content-1-2",
				"original-language",
				"qc-needed",
				"additional-information",
				"status",
			};

			var dict = new Dictionary<string, string>
			{
				{ "_type", "MetadataProfileField" }
			};

			foreach (var metadataProfileFieldName in defaultMetaDataProfileFieldNames)
			{
				dict.Add($"metadata[{defaultMetaDataProfileFqn}].{metadataProfileFieldName}", string.Empty);
			}

			metaDataProfileFqnSection.SetValue(new List<string> { defaultMetaDataProfileFqn });
			valuesSection.Value = dict;
		}

		private void SendNotificationButton_Pressed(object sender, EventArgs e)
		{
			var recordingSession = new RecordingSession
			{
				Id = idTextBox.Text,
				Name = nameTextBox.Text,
				Start = startDateTimePicker.DateTime.Truncate(TimeSpan.FromMinutes(1)),
				End = endDateTimePicker.DateTime.Truncate(TimeSpan.FromMinutes(1)),
				Recorder = recorderRefTextBox.Text,
				Targets = new[] { targetRefTextbox.Text },
				Metadata = metaDataProfileFqnSection.GetValue().Select(x => new Metadata { Profile = x, Values = valuesSection.Value }).ToArray()
			};

			try
			{
				var debugRecordingService = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.DisplayedService { Name = "Test Recording Service" };

				evsManager.AddOrUpdateRecordingSession(debugRecordingService, recordingSession);

				ShowRequestResult($"Sent notification", JsonConvert.SerializeObject(recordingSession) + "\n" + "Received ID: " + debugRecordingService.EvsId);
				idTextBox.Text = debugRecordingService.EvsId;
			}
			catch (Exception ex)
			{
				ShowRequestResult($"Sent notification and got exception", JsonConvert.SerializeObject(recordingSession) + "\n" + "Exception: " + ex);
			}
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0);

			AddWidget(evsLabel, ++row, 0, 1, 2);

			AddWidget(idLabel, ++row, 0);
			AddWidget(idTextBox, row, 1);
			AddWidget(randomIdButton, row, 2);

			AddWidget(nameLabel, ++row, 0);
			AddWidget(nameTextBox, row, 1);

			AddWidget(startLabel, ++row, 0);
			AddWidget(startDateTimePicker, row, 1);

			AddWidget(endLabel, ++row, 0);
			AddWidget(endDateTimePicker, row, 1);

			AddWidget(recorderRefLabel, ++row, 0);
			AddWidget(recorderRefTextBox, row, 1);

			AddWidget(targetRefLabel, ++row, 0);
			AddWidget(targetRefTextbox, row, 1);

			//AddWidget(new WhiteSpace(), ++row, 0); 
			//AddWidget(metadataTemplateButton, ++row, 0); 

			AddSection(metaDataProfileFqnSection, new SectionLayout(++row, 0));
			row += metaDataProfileFqnSection.RowCount;

			AddSection(valuesSection, new SectionLayout(++row, 0));
			row += valuesSection.RowCount;

			//AddSection(contextSection, new SectionLayout(++row, 0));
			//row += contextSection.RowCount;

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(sendNotificationButton, ++row, 0, 1, 2);

			row++;
			foreach (var responseSection in responseSections)
			{
				responseSection.Collapse();
				AddSection(responseSection, row, 0);
				row += responseSection.RowCount;
			}
		}

		private void ShowRequestResult(string header, params string[] results)
		{
			responseSections.Add(new RequestResultSection(header, results));

			GenerateUi();
		}
	}
}

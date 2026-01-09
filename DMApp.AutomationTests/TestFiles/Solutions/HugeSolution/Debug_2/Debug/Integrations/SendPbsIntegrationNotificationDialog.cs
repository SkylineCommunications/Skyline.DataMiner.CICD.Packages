namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;

	public class SendPbsIntegrationNotificationDialog : Dialog
	{
		private readonly Utilities.Helpers helpers;
		private readonly OrderManagerElement orderManagerElement;
		private readonly IDmsElement pbsElement;

		private readonly Label pebbleBeachLabel = new Label("Pebble Beach") { Style = TextStyle.Heading };

		private readonly Label pbsTitleLabel = new Label("Title");
		private readonly TextBox pbsTitleTextBox = new TextBox();

		private readonly Label pbsUidLabel = new Label("Pebble Beach UID");
		private readonly TextBox pbsUidTextBox = new TextBox();
		private readonly Button randomUidButton = new Button("Enter Random UID") { Width = 150 };

		private readonly Label pbsPlasmaIdLabel = new Label("Plasma ID");
		private readonly TextBox pbsPlasmaIdTextBox = new TextBox();
		private readonly CheckBox pbsPlasmaIdNaCheckBox = new CheckBox("N/A") { IsChecked = false };

		private readonly Label pbsPlaylistIdLabel = new Label("Playlist ID");
		private readonly TextBox pbsPlaylistIdTextBox = new TextBox("5000");

		private readonly Label pbsTypeLabel = new Label("Type");
		private readonly DropDown pbsTypeDropDown = new DropDown { Options = new string[] { "Live", "PrimaryVideo" }, Selected = "Live" };

		private readonly Label pbsEstimatedStartTimeLabel = new Label("Estimated Start Time");
		private readonly DateTimePicker pbsEstimatedStartTimeDateTimePicker = new DateTimePicker { DateTime = DateTime.Now };

		private readonly Label pbsActualStartTimeLabel = new Label("Actual Start Time");
		private readonly DateTimePicker pbsActualStartTimeDateTimePicker = new DateTimePicker { DateTime = DateTime.Now, IsEnabled = false };
		private readonly CheckBox pbsActualStartTimeNaCheckBox = new CheckBox ("N/A") { IsChecked = true };

		private readonly Label pbsEstimatedEndTimeLabel = new Label("Estimated End Time");
		private readonly DateTimePicker pbsEstimatedEndTimeDateTimePicker = new DateTimePicker { DateTime = DateTime.Now.AddHours(1) };

		private readonly Label pbsActualEndTimeLabel = new Label("Actual End Time");
		private readonly DateTimePicker pbsActualEndTimeDateTimePicker = new DateTimePicker { DateTime = DateTime.Now.AddHours(1), IsEnabled = false };
		private readonly CheckBox pbsActualEndTimeNaCheckBox = new CheckBox("N/A") { IsChecked = true };

		private readonly Label pbsSourceLabel = new Label("Source");
		private readonly TextBox pbsSourceTextBox = new TextBox();
		private readonly CheckBox pbsSourceNaCheckBox = new CheckBox("N/A") { IsChecked = false };

		private readonly Label pbsBackupSourceLabel = new Label("Backup Source");
		private readonly TextBox pbsBackupSourceTextBox = new TextBox();
		private readonly CheckBox pbsBackupSourceNaCheckBox = new CheckBox("N/A") { IsChecked = false };

		private readonly Button sendNotificationButton = new Button("Send Notification") { Width = 200 };

		private readonly List<PbsNotificationSection> notificationSections = new List<PbsNotificationSection>();

		public SendPbsIntegrationNotificationDialog(Utilities.Helpers helpers) : base(helpers.Engine)
		{
			this.helpers = helpers;
			orderManagerElement = new OrderManagerElement(helpers);

			try
			{
				pbsElement = orderManagerElement.PebbleBeachElement;

				Title = "Send Pebble Beach Integration Notification";

				Initialize();
				GenerateUi();
			}
			catch (Exception e)
			{
				helpers.Log(nameof(SendPbsIntegrationNotificationDialog), nameof(SendPbsIntegrationNotificationDialog), $"Unable to construct dialog: {e}");

				Title = "Something went wrong :'(";
			}
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		private void Initialize()
		{
			randomUidButton.Pressed += (s, e) => pbsUidTextBox.Text = Guid.NewGuid().ToString();
			pbsPlasmaIdNaCheckBox.Changed += (s, e) => pbsPlasmaIdTextBox.IsEnabled = !pbsPlasmaIdNaCheckBox.IsChecked;
			pbsActualStartTimeNaCheckBox.Changed += (s, e) => pbsActualStartTimeDateTimePicker.IsEnabled = !pbsActualStartTimeNaCheckBox.IsChecked;
			pbsActualEndTimeNaCheckBox.Changed += (s, e) => pbsActualEndTimeDateTimePicker.IsEnabled = !pbsActualEndTimeNaCheckBox.IsChecked;
			pbsSourceNaCheckBox.Changed += (s, e) => pbsSourceTextBox.IsEnabled = !pbsSourceNaCheckBox.IsChecked;
			pbsBackupSourceNaCheckBox.Changed += (s, e) => pbsBackupSourceTextBox.IsEnabled = !pbsBackupSourceNaCheckBox.IsChecked;

			sendNotificationButton.Pressed += SendNotificationButton_Pressed;
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0);

			AddWidget(pebbleBeachLabel, ++row, 0, 1, 2);

			AddWidget(pbsTitleLabel, ++row, 0);
			AddWidget(pbsTitleTextBox, row, 1);

			AddWidget(pbsUidLabel, ++row, 0);
			AddWidget(pbsUidTextBox, row, 1);
			AddWidget(randomUidButton, row, 2);

			AddWidget(pbsPlasmaIdLabel, ++row, 0);
			AddWidget(pbsPlasmaIdTextBox, row, 1);
			AddWidget(pbsPlasmaIdNaCheckBox, row, 2);

			AddWidget(pbsPlaylistIdLabel, ++row, 0);
			AddWidget(pbsPlaylistIdTextBox, row, 1);

			AddWidget(pbsTypeLabel, ++row, 0);
			AddWidget(pbsTypeDropDown, row, 1);

			AddWidget(pbsEstimatedStartTimeLabel, ++row, 0);
			AddWidget(pbsEstimatedStartTimeDateTimePicker, row, 1);

			AddWidget(pbsActualStartTimeLabel, ++row, 0);
			AddWidget(pbsActualStartTimeDateTimePicker, row, 1);
			AddWidget(pbsActualStartTimeNaCheckBox, row, 2);

			AddWidget(pbsEstimatedEndTimeLabel, ++row, 0);
			AddWidget(pbsEstimatedEndTimeDateTimePicker, row, 1);

			AddWidget(pbsActualEndTimeLabel, ++row, 0);
			AddWidget(pbsActualEndTimeDateTimePicker, row, 1);
			AddWidget(pbsActualEndTimeNaCheckBox, row, 2);

			AddWidget(pbsSourceLabel, ++row, 0);
			AddWidget(pbsSourceTextBox, row, 1);
			AddWidget(pbsSourceNaCheckBox, row, 2);

			AddWidget(pbsBackupSourceLabel, ++row, 0);
			AddWidget(pbsBackupSourceTextBox, row, 1);
			AddWidget(pbsBackupSourceNaCheckBox, row, 2);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(sendNotificationButton, ++row, 0, 1, 3);

			foreach (var notificationSection in notificationSections)
			{
				AddSection(notificationSection, ++row, 0);
				row += notificationSection.RowCount;
			}
		}

		private void SendNotificationButton_Pressed(object sender, EventArgs e)
		{
			var notification = new PbsNotification
			{
				Title = pbsTitleTextBox.Text,
				Uid = pbsUidTextBox.Text,
				PlaylistId = pbsPlaylistIdTextBox.Text,
				PlasmaId = pbsPlasmaIdNaCheckBox.IsChecked ? null : pbsPlasmaIdTextBox.Text,
				Type = pbsTypeDropDown.Selected,
				EstimatedStartTime = pbsEstimatedStartTimeDateTimePicker.DateTime,
				ActualStartTime = pbsActualStartTimeNaCheckBox.IsChecked ? null : (DateTime?)pbsActualStartTimeDateTimePicker.DateTime,
				EstimatedEndTime = pbsEstimatedEndTimeDateTimePicker.DateTime,
				ActualEndTime = pbsActualEndTimeNaCheckBox.IsChecked ? null : (DateTime?)pbsActualEndTimeDateTimePicker.DateTime,
				Source = pbsSourceNaCheckBox.IsChecked ? null : pbsSourceTextBox.Text,
				BackupSource = pbsBackupSourceNaCheckBox.IsChecked ? null : pbsBackupSourceTextBox.Text
			};

			// check if playlist exists (optionally add playlist)
			if (!DataMinerInterface.DataMinerInterface.IDmsElement.TableContainsPrimaryKey(helpers, pbsElement, Utils.YLE.Integrations.PebbleBeach.PebbleBeach.PlaylistsTablePid, notification.PlaylistId))
			{
				AddPlaylist(notification);
			}

			// add or update event entry in events table
			string key = AddOrUpdateEvent(notification);

			// update subscription parameter with id of new or updated event
			DataMinerInterface.DataMinerInterface.IDmsElement.SetParameter(helpers, pbsElement, Utils.YLE.Integrations.PebbleBeach.PebbleBeach.UpdatedEventIdsParameterPid, key);

			// Generate section
			var pbsNotificationSection = new PbsNotificationSection(notification);
			pbsNotificationSection.LoadButton.Pressed += NotificationSection_LoadButton_Pressed;
			notificationSections.Add(pbsNotificationSection);
			GenerateUi();
		}

		private void NotificationSection_LoadButton_Pressed(object sender, EventArgs e)
		{
			var section = notificationSections.FirstOrDefault(x => x.LoadButton.Equals(sender));
			if (section == null) return;

			// Load values in form
			pbsTitleTextBox.Text = section.Notification.Title;
			pbsUidTextBox.Text = section.Notification.Uid;
			pbsPlaylistIdTextBox.Text = section.Notification.PlaylistId;
			pbsPlasmaIdTextBox.Text = section.Notification.PlasmaId;
			pbsPlasmaIdNaCheckBox.IsChecked = String.IsNullOrEmpty(section.Notification.PlasmaId);
			pbsTypeDropDown.Selected = section.Notification.Type;
			pbsEstimatedStartTimeDateTimePicker.DateTime = section.Notification.EstimatedStartTime;
			pbsEstimatedEndTimeDateTimePicker.DateTime = section.Notification.EstimatedEndTime;
			pbsActualStartTimeDateTimePicker.DateTime = section.Notification.ActualStartTime.HasValue ? section.Notification.ActualStartTime.Value : section.Notification.EstimatedStartTime;
			pbsActualStartTimeNaCheckBox.IsChecked = !section.Notification.ActualStartTime.HasValue;
			pbsActualEndTimeDateTimePicker.DateTime = section.Notification.ActualEndTime.HasValue ? section.Notification.ActualEndTime.Value : section.Notification.EstimatedEndTime;
			pbsActualEndTimeNaCheckBox.IsChecked = !section.Notification.ActualEndTime.HasValue;
			pbsSourceTextBox.Text = section.Notification.Source;
			pbsSourceNaCheckBox.IsChecked = String.IsNullOrEmpty(section.Notification.Source);
			pbsBackupSourceTextBox.Text = section.Notification.BackupSource;
			pbsBackupSourceNaCheckBox.IsChecked = String.IsNullOrEmpty(section.Notification.BackupSource);
		}

		private void AddPlaylist(PbsNotification notification)
		{
			object[] playlistRow = notification.GeneratePbsPlaylistRow();
			IDmsTable playlistTable = pbsElement.GetTable(Utils.YLE.Integrations.PebbleBeach.PebbleBeach.PlaylistsTablePid);
			playlistTable.AddRow(playlistRow);
		}

		private string AddOrUpdateEvent(PbsNotification notification)
		{
			object[] eventRow = notification.GeneratePbsEventRow();
			IDmsTable eventsTable = pbsElement.GetTable(Utils.YLE.Integrations.PebbleBeach.PebbleBeach.EventsTablePid);
			if (eventsTable.GetPrimaryKeys().Contains(notification.Key))
			{
				eventsTable.SetRow(notification.Key, eventRow);
			}
			else
			{
				eventsTable.AddRow(eventRow);
			}

			return notification.Key;
		}

		private sealed class PbsNotification
		{
			public string Key
			{
				get
				{
					return $"{PlaylistId}.{Uid}";
				}
			}

			public string Title { get; set; }

			public string Uid { get; set; }

			public string PlasmaId { get; set; }

			public string PlaylistId { get; set; }

			public string Type { get; set; }

			public DateTime EstimatedStartTime { get; set; }

			public DateTime? ActualStartTime { get; set; }

			public DateTime EstimatedEndTime { get; set; }

			public DateTime? ActualEndTime { get; set; }

			public string Source { get; set; }

			public string BackupSource { get; set; }

			public object[] GeneratePbsPlaylistRow()
			{
				object[] playlistRow = new object[]
				{
					PlaylistId,					// ID
					$"Playlist {PlaylistId}",	// Name [IDX]
					1,											// Redundant Object
					"-1",										// Redundant Component ID
					"-1",										// Redundant Component Name
					"-1"                                        // Health Flag
				};

				return playlistRow;
			}

			public object[] GeneratePbsEventRow()
			{
				double duration = EstimatedEndTime.Subtract(EstimatedStartTime).TotalSeconds;
				double estimatedStartTime = EstimatedStartTime.ToOADate();
				double actualStartTime = ActualStartTime.HasValue ? ((DateTime)ActualStartTime).ToOADate() : -1;
				double actualEndTime = ActualEndTime.HasValue ? ((DateTime)ActualEndTime).ToOADate() : -1;
				string plasmaId = String.IsNullOrEmpty(PlasmaId) ? "-1" : PlasmaId;
				string source = String.IsNullOrEmpty(Source) ? "-1" : Source;
				string backupSource = String.IsNullOrEmpty(BackupSource) ? "-1" : BackupSource;

				object[] eventRow = new object[]
				{
					Key,							// ID
					Uid,				// UID
					PlaylistId,		// Playlist ID
					Title,					// Title
					plasmaId,		// House ID
					1,								// Status
					Type,		// Type
					estimatedStartTime,				// Scheduled start time
					duration,						// Duration
					String.Empty,					// Reconcile Keys
					source,			// Source
					backupSource,	// Backup Source
					"-1",							// Destination Auto Type
					"-1",							// Backup Destination Auto Type
					"-1",							// Block ID
					"-1",							// Running State
					String.Empty,					// Response
					String.Empty,					// Possible Sources
					actualStartTime,				// Actual start time
					estimatedStartTime,				// Estimated start time
					0,								// End Type
					2,								// Start Type
					actualEndTime,					// Actual end time
					0,								// Actual start time frame
					0                               // Actual end time frame
				};

				return eventRow;
			}
		}

		private sealed class PbsNotificationSection : Section
		{
			private readonly PbsNotification notification;

			private readonly CollapseButton collapseButton = new CollapseButton { CollapseText = "-", ExpandText = "+", Width = 44 };
			private readonly Label notificationTitleLabel = new Label { Style = TextStyle.Heading };

			private readonly Label pbsTitleLabel = new Label("Title");
			private readonly Label pbsTitleValueLabel = new Label();

			private readonly Label pbsKeyLabel = new Label("Event Table Key");
			private readonly Label pbsKeyValueLabel = new Label();

			private readonly Label pbsUidLabel = new Label("Pebble Beach UID");
			private readonly Label pbsUidValueLabel = new Label();

			private readonly Label pbsPlasmaIdLabel = new Label("Plasma ID");
			private readonly Label pbsPlasmaIdValueLabel = new Label();

			private readonly Label pbsPlaylistIdLabel = new Label("Playlist ID");
			private readonly Label pbsPlaylistIdValueLabel = new Label();

			private readonly Label pbsTypeLabel = new Label("Type");
			private readonly Label pbsTypeValueLabel = new Label();

			private readonly Label pbsEstimatedStartTimeLabel = new Label("Estimated Start Time");
			private readonly Label pbsEstimatedStartTimeValueLabel = new Label();

			private readonly Label pbsActualStartTimeLabel = new Label("Actual Start Time");
			private readonly Label pbsActualStartTimeValueLabel = new Label();

			private readonly Label pbsEstimatedEndTimeLabel = new Label("Estimated End Time");
			private readonly Label pbsEstimatedEndTimeValueLabel = new Label();

			private readonly Label pbsActualEndTimeLabel = new Label("Actual End Time");
			private readonly Label pbsActualEndTimeValueLabel = new Label();

			private readonly Label pbsSourceLabel = new Label("Source");
			private readonly Label pbsSourceValueLabel = new Label();

			private readonly Label pbsBackupSourceLabel = new Label("Backup Source");
			private readonly Label pbsBackupSourceValueLabel = new Label();

			public PbsNotificationSection(PbsNotification notification)
			{
				this.notification = notification;

				notificationTitleLabel.Text = $"Sent notification {notification.Title} - {notification.Key}";

				pbsTitleValueLabel.Text = notification.Title;
				pbsKeyValueLabel.Text = notification.Key;
				pbsUidValueLabel.Text = notification.Uid;
				pbsTypeValueLabel.Text = notification.Type;
				pbsPlasmaIdValueLabel.Text = notification.PlasmaId;
				pbsPlasmaIdValueLabel.Text = notification.PlaylistId;
				pbsEstimatedStartTimeValueLabel.Text = Convert.ToString(notification.EstimatedStartTime);
				pbsEstimatedEndTimeValueLabel.Text = Convert.ToString(notification.EstimatedEndTime);
				pbsActualStartTimeValueLabel.Text = Convert.ToString(notification.ActualStartTime);
				pbsActualEndTimeValueLabel.Text = Convert.ToString(notification.ActualEndTime);
				pbsSourceValueLabel.Text = notification.Source;
				pbsBackupSourceValueLabel.Text = notification.BackupSource;

				collapseButton.LinkedWidgets.Add(pbsTitleLabel);
				collapseButton.LinkedWidgets.Add(pbsTitleValueLabel);
				collapseButton.LinkedWidgets.Add(pbsKeyLabel);
				collapseButton.LinkedWidgets.Add(pbsKeyValueLabel);
				collapseButton.LinkedWidgets.Add(pbsUidLabel);
				collapseButton.LinkedWidgets.Add(pbsUidValueLabel);
				collapseButton.LinkedWidgets.Add(pbsPlasmaIdLabel);
				collapseButton.LinkedWidgets.Add(pbsPlasmaIdValueLabel);
				collapseButton.LinkedWidgets.Add(pbsPlaylistIdLabel);
				collapseButton.LinkedWidgets.Add(pbsPlaylistIdValueLabel);
				collapseButton.LinkedWidgets.Add(pbsTypeLabel);
				collapseButton.LinkedWidgets.Add(pbsTypeValueLabel);
				collapseButton.LinkedWidgets.Add(pbsEstimatedStartTimeLabel);
				collapseButton.LinkedWidgets.Add(pbsEstimatedStartTimeValueLabel);
				collapseButton.LinkedWidgets.Add(pbsActualStartTimeLabel);
				collapseButton.LinkedWidgets.Add(pbsActualStartTimeValueLabel);
				collapseButton.LinkedWidgets.Add(pbsEstimatedEndTimeLabel);
				collapseButton.LinkedWidgets.Add(pbsEstimatedEndTimeValueLabel);
				collapseButton.LinkedWidgets.Add(pbsActualEndTimeLabel);
				collapseButton.LinkedWidgets.Add(pbsActualEndTimeValueLabel);
				collapseButton.LinkedWidgets.Add(pbsSourceLabel);
				collapseButton.LinkedWidgets.Add(pbsSourceValueLabel);
				collapseButton.LinkedWidgets.Add(pbsBackupSourceLabel);
				collapseButton.LinkedWidgets.Add(pbsBackupSourceValueLabel);
				collapseButton.Collapse();

				GenerateUi();
			}

			public Button LoadButton { get; private set; } = new Button("Load");

			public PbsNotification Notification
			{
				get
				{
					return notification;
				}
			}

			private void GenerateUi()
			{
				Clear();

				int row = -1;

				AddWidget(collapseButton, ++row, 0);
				AddWidget(notificationTitleLabel, row, 1, 1, 2);
				AddWidget(LoadButton, row, 3);

				AddWidget(pbsTitleLabel, ++row, 1);
				AddWidget(pbsTitleValueLabel, row, 2);

				AddWidget(pbsKeyLabel, ++row, 1);
				AddWidget(pbsKeyValueLabel, row, 2);

				AddWidget(pbsUidLabel, ++row, 1);
				AddWidget(pbsUidValueLabel, row, 2);

				AddWidget(pbsPlasmaIdLabel, ++row, 1);
				AddWidget(pbsPlasmaIdValueLabel, row, 2);

				AddWidget(pbsPlaylistIdLabel, ++row, 1);
				AddWidget(pbsPlaylistIdValueLabel, row, 2);

				AddWidget(pbsTypeLabel, ++row, 1);
				AddWidget(pbsTypeValueLabel, row, 2);

				AddWidget(pbsEstimatedStartTimeLabel, ++row, 1);
				AddWidget(pbsEstimatedStartTimeValueLabel, row, 2);

				AddWidget(pbsActualStartTimeLabel, ++row, 1);
				AddWidget(pbsActualStartTimeValueLabel, row, 2);

				AddWidget(pbsEstimatedEndTimeLabel, ++row, 1);
				AddWidget(pbsEstimatedEndTimeValueLabel, row, 2);

				AddWidget(pbsActualEndTimeLabel, ++row, 1);
				AddWidget(pbsActualEndTimeValueLabel, row, 2);

				AddWidget(pbsSourceLabel, ++row, 1);
				AddWidget(pbsSourceValueLabel, row, 2);

				AddWidget(pbsBackupSourceLabel, ++row, 1);
				AddWidget(pbsBackupSourceValueLabel, row, 2);
			}
		}
	}
}

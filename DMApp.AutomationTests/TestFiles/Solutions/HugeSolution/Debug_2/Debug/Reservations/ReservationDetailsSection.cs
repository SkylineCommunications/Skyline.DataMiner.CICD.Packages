namespace Debug_2.Debug.Reservations
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.History;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.ToolsSpace.Collections;
	using SLDataGateway.API.Querying;

	public class ReservationDetailsSection : ResponseSection
	{
		private readonly CollapseButton collapseButton;
		private readonly Label header;
		private readonly List<PropertySection> propertySections = new List<PropertySection>();
		private readonly Label noReservationHistoryAvailableLabel = new Label("No Reservation Instance History Available");
		private readonly List<HistorySection> reservationHistorySections = new List<HistorySection>();
		private readonly bool isOrder;
		private readonly Helpers helpers;
		private readonly Button logButton = new Button("Log Reservation JSON");
		private readonly CollapseButton reservationHistoryCollapseButton = new CollapseButton { IsCollapsed = true, CollapseText = "Hide Reservation History", ExpandText = "Show Reservation History" };

		public ReservationDetailsSection(Helpers helpers, ReservationInstance reservationInstance, string serviceConfiguration)
		{
			if (reservationInstance == null)
			{
				header = new Label("Error: no reservation found");
				collapseButton = new CollapseButton(true) { CollapseText = "-", ExpandText = "+", Width = 44 };
				return;
			}

			this.helpers = helpers;
			Reservation = reservationInstance;

			isOrder = reservationInstance.Properties.Any(x => x.Key.Equals("Booking Manager") && x.Value != null && x.Value.Equals("Order Booking Manager"));

			this.header = new Label($"Reservation {reservationInstance.Name} ({reservationInstance.ID})") { Style = TextStyle.Heading };

			foreach (var property in reservationInstance.GetType().GetProperties())
			{
				if (!property.CanRead) continue;

				object value = property.GetValue(reservationInstance);
				if (value == null) continue;

				if (property.Name.Equals("Properties") && value is JSONSerializableDictionary)
				{
					foreach (KeyValuePair<string, object> kvp in (JSONSerializableDictionary)value)
					{
						propertySections.Add(new PropertySection($"Properties[{kvp.Key}]", kvp.Value));
					}
				}
				else
				{
					propertySections.Add(new PropertySection(property.Name, value));
				}
			}

			propertySections.Add(new PropertySection("Service Configuration", serviceConfiguration));

			this.collapseButton = new CollapseButton(propertySections.SelectMany(x => x.Widgets), true) { CollapseText = "-", ExpandText = "+", Width = 44 };

			logButton.Pressed += (sender, e) => helpers.Log(nameof(ReservationDetailsSection), "LogButton Pressed", Reservation.ToJson());
			reservationHistoryCollapseButton.Pressed += ReservationHistoryButton_Pressed;

			GenerateUi();
		}

		public ReservationInstance Reservation { get; }

		public event EventHandler RegenerateUi;

		public override void Collapse()
		{
			collapseButton.IsCollapsed = true;
		}

		private void ReservationHistoryButton_Pressed(object sender, EventArgs e)
		{
			if (Reservation == null || reservationHistoryCollapseButton.IsCollapsed) return;

			try
			{
				string subjectId = $"ReservationInstanceID_{Reservation.ID}";
				var filter = new ANDFilterElement<HistoryChange>(HistoryChangeExposers.SubjectID.Equal(subjectId, StringComparison.OrdinalIgnoreCase));
				var response = (ManagerStorePagingResponse<HistoryChange>)helpers.Engine.SendSLNetSingleResponseMessage(new ManagerStoreStartPagingRequest<HistoryChange>(filter.ToQuery(), 100) { ExtraTypeIdentifier = "ReservationInstance" });

				ClearReservationHistorySections();
				foreach (var historyChange in response.Objects)
				{
					foreach (var change in historyChange.Changes)
					{
						var historySection = new HistorySection(change, historyChange.Time, historyChange.FullUsername, historyChange.DmaId, isOrder);
						reservationHistoryCollapseButton.LinkedWidgets.AddRange(historySection.Widgets);
						reservationHistorySections.Add(historySection);
					}
				}

				reservationHistoryCollapseButton.LinkedWidgets.Add(noReservationHistoryAvailableLabel);
			}
			catch (Exception exception)
			{
				helpers.Log(nameof(ReservationDetailsSection), nameof(ReservationHistoryButton_Pressed), $"Exception occurred: {exception}");
			}

			GenerateUi();
			RegenerateUi(this, new EventArgs());
		}

		private void ClearReservationHistorySections()
		{
			reservationHistoryCollapseButton.LinkedWidgets.Clear();
			reservationHistorySections.Clear();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(collapseButton, ++row, 0);
			AddWidget(header, row, 1, 1, 2);

			AddWidget(logButton, ++row, 1);

			foreach (var propertySection in propertySections.OrderBy(x => x.NameLabel.Text))
			{
				AddSection(propertySection, new SectionLayout(++row, 1));
			}

			AddWidget(reservationHistoryCollapseButton, ++row, 1);
			if (!reservationHistoryCollapseButton.IsCollapsed && !reservationHistorySections.Any())
			{
				AddWidget(noReservationHistoryAvailableLabel, ++row, 1);
			}
			else
			{
				foreach (var historySection in reservationHistorySections.OrderBy(x => x.Time))
				{
					AddSection(historySection, new SectionLayout(++row, 1));
					row += historySection.RowCount;
				}
			}
		}

		private class PropertySection : Section
		{
			public PropertySection(string name, object value)
			{
				NameLabel = new Label(name);

				if (value == null)
				{
					ValueLabel = new Label(String.Empty);
				}
				else if (value is DateTime)
				{
					ValueLabel = new Label(((DateTime)value).ToString("G", CultureInfo.InvariantCulture));
				}
				else if (value is string)
				{
					ValueLabel = new Label(value.ToString());
				}
				else if(value is System.Collections.IEnumerable enumerable)
				{
					var sb = new StringBuilder();

					foreach (var obj in enumerable)
					{
						if (obj is Skyline.DataMiner.Net.ResourceManager.Objects.ServiceResourceUsageDefinition resourceUsage)
						{
							sb.Append($"Node [{resourceUsage.ServiceDefinitionNodeID}] has resource [{resourceUsage.NodeConfiguration.ResourceID}]");
						}
						else
						{
							sb.Append(obj.ToString());
						}

						sb.Append(",");
					}

					ValueLabel = new Label(sb.ToString().Trim(','));
				}
				else
				{
					ValueLabel = new Label(value.ToString());
				}

				AddWidget(NameLabel, 0, 0);
				AddWidget(ValueLabel, 0, 1, 1, 2);
			}

			public Label NameLabel { get; private set; }

			public Label ValueLabel { get; private set; }
		}

	}
}

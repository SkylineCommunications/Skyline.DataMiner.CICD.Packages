namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Integrations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Plasma;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.Enums;
	using Skyline.DataMiner.Utils.YLE.Integrations.Plasma;

	public class AnalyzePlasmaRecordingsDialog : DebugDialog
	{
		private readonly Label analyzePlasmaRecordingsLabel = new Label("Analyze Plasma Recordings") { Style = TextStyle.Heading };

		private GetOrderReservationsSection getOrderReservationsSection;

		private readonly Button analyzePlasmaRecordingsButton = new Button("Analyze Plasma Recordings") { Width = 200 };

		private readonly Label descriptionLabel = new Label("This feature analyses the Plasma orders that are part of the filter. It checks if the Plasma orders have an additional PGM recording and removes it if not applicable.");

		public AnalyzePlasmaRecordingsDialog(Helpers helpers) : base(helpers)
		{
			Title = "Analyze Plasma Recordings";

			Initialize();
			GenerateUi();
		}

		public event EventHandler<ConfirmationDialog> ShowConfirmationDialog;

		public event EventHandler<ProgressDialog> ShowProgressDialog;

		public event EventHandler OrdersUpdated;

		private void Initialize()
		{
			getOrderReservationsSection = new GetOrderReservationsSection(helpers);
			getOrderReservationsSection.AdditionalFilters.Add(ReservationInstanceExposers.Properties.DictStringField("Integration").Equal("Plasma"));

			analyzePlasmaRecordingsButton.Pressed += AnalyzePlasmaOrdersButton_Pressed;
		}

		private void AnalyzePlasmaOrdersButton_Pressed(object sender, EventArgs e)
		{
			if (!getOrderReservationsSection.IsValid)
			{
				ShowRequestResult("Invalid order IDs", "Invalid Order IDs");
			}

			var plasmaReservations = getOrderReservationsSection.GetOrderReservations().OrderBy(x => x.Start).ToList();
			if (!plasmaReservations.Any()) return;

			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"Are you sure you want to analyze the following {plasmaReservations.Count} Plasma order(s):");
			foreach (var plasmaReservation in plasmaReservations) sb.AppendLine($"\t-{plasmaReservation.Name} ({plasmaReservation.Start.ToLocalTime()} - {plasmaReservation.End.ToLocalTime()})");

			ConfirmationDialog confirmationDialog = new ConfirmationDialog(helpers.Engine, sb.ToString());

			confirmationDialog.YesButton.Pressed += (s, args) => AnalyzePlasmaOrders(plasmaReservations);

			ShowConfirmationDialog?.Invoke(this, confirmationDialog);
		}

		private void AnalyzePlasmaOrders(IEnumerable<ServiceReservationInstance> plasmaReservations)
		{
			ProgressDialog progressDialog = new ProgressDialog(helpers.Engine) { Title = "Analyzing Plasma Recordings" };
			progressDialog.OkButton.Pressed += (sender, args) => OrdersUpdated?.Invoke(this, EventArgs.Empty);
			progressDialog.Show(false);

			var succeededGuids = new List<Guid>();
			var failedGuids = new List<Guid>();

			OrderManagerElement orderManagerElement = new OrderManagerElement(helpers);
			PlasmaIntegration integration = new PlasmaIntegration(helpers, orderManagerElement);
			foreach (var plasmaReservation in plasmaReservations)
			{
				try
				{
					progressDialog.AddProgressLine($"Retrieving order {plasmaReservation.Name}...");
					var order = helpers.OrderManager.GetOrder(plasmaReservation);
					progressDialog.AddProgressLine($"Order Retrieved");

					if (order.ShouldBeRunning)
					{
						progressDialog.AddProgressLine($"Plasma order is running -> skip");
						succeededGuids.Add(plasmaReservation.ID);
						continue;
					}

					if (String.IsNullOrEmpty(order.PlasmaId))
					{
						progressDialog.AddProgressLine($"Plasma order doesn't have a Plasma ID -> unable to retrieve Plasma information");
						failedGuids.Add(plasmaReservation.ID);
						continue;
					}

					progressDialog.AddProgressLine($"Getting Plasma Information for {order.PlasmaId}...");
					var program = RetrieveProgram(orderManagerElement.PlasmaElement, order.PlasmaId);
					progressDialog.AddProgressLine($"Plasma Information Retrieved");

					if (program.ShouldAddPgmNewsRecording(helpers))
					{
						progressDialog.AddProgressLine($"Plasma order requires a PGM recording -> nothing to do");
						succeededGuids.Add(plasmaReservation.ID);
						continue;
					}

					var pgmRecordingService = GetPgmRecording(order);
					if (pgmRecordingService == null)
					{
						progressDialog.AddProgressLine($"Plasma order doesn't have a PGM recording -> nothing to do");
						succeededGuids.Add(plasmaReservation.ID);
						continue;
					}

					progressDialog.AddProgressLine($"Removing PGM Recording {pgmRecordingService.Name}...");
					order.RemoveChildService(pgmRecordingService.Id, true);
					var result = order.AddOrUpdate(helpers, true, YLE.Order.OrderUpdates.OrderUpdateHandler.OptionFlags.None);

					if (result.UpdateWasSuccessful)
					{
						progressDialog.AddProgressLine($"PGM Recording removed");
						succeededGuids.Add(order.Id);
					}
					else
					{
						progressDialog.AddProgressLine($"Failed to remove PGM Recording");
						failedGuids.Add(order.Id);
					}
				}
				catch (Exception e)
				{
					progressDialog.AddProgressLine($"Something went wrong when analyzing Plasma order {plasmaReservation.Name}[{plasmaReservation.ID}]: {e}");
					failedGuids.Add(plasmaReservation.ID);
				}
			}

			ShowRequestResult($"Updated Plasma Orders {DateTime.Now.ToShortTimeString()}", $"Succeeded Guids:\n{string.Join("\n", succeededGuids)}\n\nFailed Guids:\n{string.Join("\n", failedGuids)}");

			progressDialog.Finish();
			ShowProgressDialog?.Invoke(this, progressDialog);
		}

		/// <summary>
		/// Initializes a Program object for a given Plasma ID by retrieving the data from the Plasma integration element.
		/// </summary>
		/// <param name="plasmaElement">Plasma element that holds the required entry.</param>
		/// <param name="plasmaId">The Plasma ID for this program.</param>
		/// <returns></returns>
		public Program RetrieveProgram(IDmsElement plasmaElement, string plasmaId)
		{
			throw new NotSupportedException();
			/*
			IDmsTable programTable = plasmaElement.GetTable(MediagenixWhatsOnProtocol.ProgramsTable.TablePid) ?? throw new Exception("Programs table could not be retrieved");
			IDmsTable transmissionTable = plasmaElement.GetTable(MediagenixWhatsOnProtocol.PublicationEventsTable.TablePid) ?? throw new Exception("Transmission table could not be retrieved");
			IDmsTable videoAssetInformationTable = plasmaElement.GetTable(MediagenixWhatsOnProtocol.VideoResourcesTable.TablePid) ?? throw new Exception("Video Assets table could not be retrieved");
			IDmsTable audioAssetInformationTable = plasmaElement.GetTable(MediagenixWhatsOnProtocol.AudioResourcesTable.TablePid) ?? throw new Exception("Audio Assets table could not be retrieved");
			IDmsTable subtitleAssetInformationTable = plasmaElement.GetTable(MediagenixWhatsOnProtocol.SubtitleResourcesTable.TablePid) ?? throw new Exception("Subtitle Assets table could not be retrieved");
			IDmsTable productionJobTable = plasmaElement.GetTable(MediagenixWhatsOnProtocol.ProductionJobsTable.TablePid) ?? throw new Exception("Production Jobs table could not be retrieved");

			var programRow = programTable.QueryData(new[]
				{
					new ColumnFilter { Pid = MediagenixWhatsOnProtocol.ProgramsTable.Pid.PlasmaId, Value = plasmaId, ComparisonOperator = ComparisonOperator.Equal }
				}).SingleOrDefault();

			if (programRow is null)
			{
				helpers.Log(nameof(AnalyzePlasmaRecordingsDialog), nameof(RetrieveProgram), "No entry found in the Programs table for: " + plasmaId);

				// return null in case this row no longer exists
				// will be treated as a delete in this case
				return null;
			}

			string programId = Convert.ToString(programRow[MediagenixWhatsOnProtocol.ProgramsTable.Idx.Id]);

			var publicationEventRows = transmissionTable.QueryData(new[]
				{
					new ColumnFilter { Pid = MediagenixWhatsOnProtocol.PublicationEventsTable.Pid.ProgramId, Value = programId, ComparisonOperator = ComparisonOperator.Equal }
				}).ToList();

			List<object[]> videoAssetInformationRows = videoAssetInformationTable.QueryData(new[]
				{
					new ColumnFilter { Pid = MediagenixWhatsOnProtocol.VideoResourcesTable.Pid.ProgramId, Value = programId, ComparisonOperator = ComparisonOperator.Equal }
				}).ToList();

			List<object[]> audioAssetInformationRows = audioAssetInformationTable.QueryData(new[]
				{
					new ColumnFilter { Pid = MediagenixWhatsOnProtocol.AudioResourcesTable.Pid.ProgramId, Value = programId, ComparisonOperator = ComparisonOperator.Equal }
				}).ToList();

			List<object[]> subtitleAssetInformationRows = subtitleAssetInformationTable.QueryData(new[]
				{
					new ColumnFilter { Pid = MediagenixWhatsOnProtocol.SubtitleResourcesTable.Pid.ProgramId, Value = programId, ComparisonOperator = ComparisonOperator.Equal }
				}).ToList();

			string productionJobId = Convert.ToString(programRow[MediagenixWhatsOnProtocol.ProgramsTable.Idx.ProductionJobId]);

			var productionJobRows = productionJobTable.QueryData(new[]
			{
				new ColumnFilter { Pid = MediagenixWhatsOnProtocol.ProductionJobsTable.Pid.Id, Value = productionJobId, ComparisonOperator = ComparisonOperator.Equal }
			}).SingleOrDefault();

			var program = new Program(programRow, videoAssetInformationRows, audioAssetInformationRows, subtitleAssetInformationRows, publicationEventRows, productionJobRows);

			helpers.Log(nameof(AnalyzePlasmaRecordingsDialog), nameof(RetrieveProgram), $"Retrieved program from Plasma element: {program.ToString()}");

			return program;
			*/
		}

		private YLE.Service.Service GetPgmRecording(YLE.Order.Order order)
		{
			foreach (var plasmaRecordingService in order.AllServices.Where(x => x.Definition.VirtualPlatform == ServiceDefinition.VirtualPlatform.Recording && x.IntegrationType == IntegrationType.Plasma))
			{
				var recordingFunction = plasmaRecordingService.Functions.FirstOrDefault();
				var feedTypeProfileParameter = recordingFunction?.Parameters?.FirstOrDefault(p => p.Name == SrmConfiguration.FeedTypeProfileParameterName);

				if (feedTypeProfileParameter?.Value?.ToString() == NewsRecordingType.PGM.ToString()) return plasmaRecordingService;
			}

			return null;
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddWidget(analyzePlasmaRecordingsLabel, ++row, 0, 1, 5);

			AddWidget(descriptionLabel, ++row, 0, 1, 5);

			AddSection(getOrderReservationsSection, ++row, 0);
			row += getOrderReservationsSection.RowCount;

			AddWidget(analyzePlasmaRecordingsButton, ++row, 0, 1, 2);

			AddWidget(new WhiteSpace(), ++row, 1);

			AddResponseSections(row);
		}
	}
}

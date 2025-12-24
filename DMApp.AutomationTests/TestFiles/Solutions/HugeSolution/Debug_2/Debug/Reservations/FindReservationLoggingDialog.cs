namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Reservations
{
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Skyline.DataMiner.Net.ResourceManager.Objects;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class FindReservationLoggingDialog : Dialog
    {
        private readonly string orderLoggingPath = @"C:\Skyline_Data\OrderLogging";

        private readonly Helpers helpers;

        private readonly Label findReservationsLoggingLabel = new Label("Find Reservation Order Logging") { Style = TextStyle.Heading };
        private readonly Label reservationIdLabel = new Label("GUID");
        private readonly Button enterCurrentReservationIdButton = new Button("Enter Current ID") { Width = 200 };

        private readonly List<RequestResultSection> responseSections = new List<RequestResultSection>();

        public FindReservationLoggingDialog(Helpers helpers) : base(helpers.Engine)
        {
            Title = "Find Reservation Order Logging";

            this.helpers = helpers;

            Initialize();
            GenerateUi();
        }

        public Button BackButton { get; } = new Button("Back...") { Width = 150 };

        private TextBox ReservationIdTextBox { get; set; }

        private Button FindOrderLoggingByReservationIdButton { get; set; }

        private void Initialize()
        {
            ReservationIdTextBox = new TextBox { PlaceHolder = "GUID", ValidationText = "Invalid GUID", Width = 400 };

            FindOrderLoggingByReservationIdButton = new Button("Find By GUID") { Width = 150 };
            FindOrderLoggingByReservationIdButton.Pressed += FindOrderLoggingByReservationIdButton_Pressed;

            enterCurrentReservationIdButton.Pressed += (sender, args) => ReservationIdTextBox.Text = helpers.Engine.GetScriptParam(1)?.Value;
        }

        private void ShowRequestResult(string header, params string[] results)
        {
            var requestResultSection = new RequestResultSection(header, results);
            requestResultSection.SetContentTextBoxWidth(1400);
            requestResultSection.SetContentTextBoxHeight(800);

            responseSections.Add(requestResultSection);

            GenerateUi();
        }

        private void GenerateUi()
        {
            Clear();

            int row = -1;

            AddWidget(BackButton, ++row, 0, 1, 3);

            AddWidget(findReservationsLoggingLabel, ++row, 0, 1, 5);

            AddWidget(reservationIdLabel, ++row, 0, 1, 2);
            AddWidget(ReservationIdTextBox, row, 2);
            AddWidget(FindOrderLoggingByReservationIdButton, row, 3);
            AddWidget(enterCurrentReservationIdButton, ++row, 2);

            AddWidget(new WhiteSpace(), ++row, 1);

            row++;
            foreach (var responseSection in responseSections)
            {
                responseSection.Collapse();
                AddSection(responseSection, row, 0);
                row += responseSection.RowCount;
            }
        }

        private void ShowOrderDetails()
        {
            GenerateUi();
        }

        private void UpdateReservationIdTextBoxValidation(UIValidationState validationState, string validationText)
        {
            ReservationIdTextBox.ValidationState = validationState;
            ReservationIdTextBox.ValidationText = validationText;
        }

        private void FindOrderLoggingByReservationIdButton_Pressed(object sender, EventArgs e)
        {
            if (!Guid.TryParse(ReservationIdTextBox.Text, out var guid))
            {
                ShowOrderDetails();
                UpdateReservationIdTextBoxValidation(UIValidationState.Invalid, "Invalid GUID");
                return;
            }

            try
            {
                var combinedPath = Path.Combine(orderLoggingPath, guid.ToString() + ".txt");
                if (File.Exists(combinedPath))
                {
                    var orderLoggingContent = File.ReadAllText(combinedPath);
                    ShowRequestResult($"Order Logging {guid}", orderLoggingContent);
                    UpdateReservationIdTextBoxValidation(UIValidationState.Valid, string.Empty);
                }
                else
                {
                    UpdateReservationIdTextBoxValidation(UIValidationState.Invalid, "Order logging file path couldn't be found");                  
                }
            }
            catch (Exception ex)
            {
                helpers.Log(nameof(FindReservationLoggingDialog), nameof(FindOrderLoggingByReservationIdButton_Pressed), $"Something went wrong while opening of order logging file {ex}");
            }
        }
    }
}

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Utilities;

	public class InvoiceReportDialog : Dialog
    {
        private readonly Label companyLabel = new Label("Company");
        private readonly DropDown companyDropDown = new DropDown();

        private readonly Label startTimeLabel = new Label("Start Time");
        private readonly DateTimePicker startTimeDateTimePicker = new DateTimePicker(DateTime.Now);

        private readonly Label endTimeLabel = new Label("End Time");
        private readonly DateTimePicker endTimeDateTimePicker = new DateTimePicker(DateTime.Now.AddDays(30));

        private readonly Label validationLabel = new Label(string.Empty);

        public InvoiceReportDialog(Helpers helpers) : base(helpers.Engine)
        {
            Title = "Create Invoice Report";

            AllExistingCompanies = helpers.ContractManager.GetAllCompanies().ToList();
            companyDropDown.Options = AllExistingCompanies;
            companyDropDown.Selected = AllExistingCompanies.FirstOrDefault();

            GenerateUi();
        }

        public Button CreateInvoiceReportButton { get; private set; } = new Button("Create Invoice Report");

        public List<string> AllExistingCompanies { get; set; }

        public string SelectedCompany => companyDropDown.Selected;

        public DateTime SelectedStartTime => startTimeDateTimePicker.DateTime;

        public DateTime SelectedEndTime => endTimeDateTimePicker.DateTime;

        private void GenerateUi()
        {
            int row = -1;
            AddWidget(companyLabel, ++row, 0);
            AddWidget(companyDropDown, row, 1);

            AddWidget(new WhiteSpace(), ++row, 0);

            AddWidget(startTimeLabel, ++row, 0);
            AddWidget(startTimeDateTimePicker, row, 1);
            AddWidget(endTimeLabel, ++row, 0);
            AddWidget(endTimeDateTimePicker, row, 1);

            AddWidget(new WhiteSpace(), ++row, 0);

            AddWidget(CreateInvoiceReportButton, ++row, 0);
            AddWidget(validationLabel, ++row, 0);
        }
    }
}

using Skyline.DataMiner.Automation;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ReportTasks
{
    using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

    public class GenerateInvoiceReportExcelTask : Task
    {
        private readonly string requestedCompany;
        private readonly DateTime requestedStartTime;
        private readonly DateTime requestedEndTime;

        public GenerateInvoiceReportExcelTask(Helpers helpers, DateTime requestedStartTime, DateTime requestedEndTime, string requestedCompany) : base(helpers)
        {
            this.requestedCompany = requestedCompany;
            this.requestedStartTime = requestedStartTime;
            this.requestedEndTime = requestedEndTime;
            
            IsBlocking = true;
        }

        public override string Description => $"Generating {requestedCompany} Invoice Report";

        public override Task CreateRollbackTask()
        {
            return null;
        }

        protected override void InternalExecute()
        {
            var invoiceReportHandler = new InvoiceReportHandler(helpers, requestedCompany, requestedStartTime, requestedEndTime);
            invoiceReportHandler.GenerateExcelFile();
        }
    }
}

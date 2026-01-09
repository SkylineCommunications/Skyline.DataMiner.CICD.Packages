using Skyline.DataMiner.Library;
using System.Collections.Generic;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration
{
    public static class ContractManagerProtocol
    {
        public static readonly string ProtocolName = "Finnish Broadcasting Company Contract Manager";

        public static readonly int UsersGroupTableID = 3000;
        public static readonly int GroupId = 0;
        public static readonly int GroupNameIdx = 1;
        public static readonly int CompanyIdx = 11;
        public static readonly int SportsIdx = 12;
        public static readonly int McrIdx = 13;
        public static readonly int GroupCustomNameIdx = 14;
        public static readonly int FilterInternalItemsIdx = 24;
        public static readonly int ConfigureTemplatesIdx = 25;
        public static readonly int NewsIdx = 32;
        public static readonly int SwapResourcesIdx = 33;

        public static readonly IReadOnlyDictionary<int, string> UserGroupParamsIDX = new Dictionary<int, string> {
            { CompanyIdx, "Company" },
            { SportsIdx, "Sports" },
            { McrIdx, "MCR" },
            { GroupCustomNameIdx, "Custom Name" },
            { FilterInternalItemsIdx, "Filter Internal Items" },
            { ConfigureTemplatesIdx, "Configure Templates" },
            { NewsIdx, "News" },
            { SwapResourcesIdx, "Swap Resources" }
        };

        public static readonly int UsersTableID = 14000;
        public static readonly int UsersID = 0;
        public static readonly int UsersNameIDX = 1;
        public static readonly int UsersEmailIDX = 3;
        public static readonly int ConfirmationByOperatorIDX = 4;
        public static readonly int RejectionByOperatorIDX = 5;
        public static readonly int CompletionByDataminerIDX = 6;
        public static readonly int CompletionWithErrorByDataminerIDX = 7;
        public static readonly int CancellationByCustomerIDX = 8;
        public static readonly int CancellationByOperatorIDX = 9;
        public static readonly int CancellationByIntegrationIDX = 10;
        public static readonly int NloReassignedByOperatorIDX = 11;
        public static readonly int NloRejectionByOperatorIDX = 12;
        public static readonly int NloCompletionByDataminerIDX = 13;
        public static readonly int NloCancellationByOperatorIDX = 14;
        public static readonly int NloWorkInProgressByOperatorIDX = 15;
        public static readonly int ServicesBookedIDX = 17;
        public static readonly int ServicesBookedByIntegrationsIDX = 18;
        public static readonly int NloEditedByOperatorIDX = 19;
        public static readonly int NloCreationByOperatorIDX = 20;
        public static readonly int NloIplayFolderCreationByOperatorIDX = 21;
        public static readonly int UsersPhoneIDX = 22;
        public static readonly int UnableToBookRecurringVizrem = 23;

        public static readonly IReadOnlyDictionary<int, string> NotificationParamsIDX = new Dictionary<int, string> {
            { UsersEmailIDX, "Email" },
            { UsersPhoneIDX, "Phone" },
            { ConfirmationByOperatorIDX, "Live Order Confirmation by Operator Notification" },
            { RejectionByOperatorIDX, "Live Order Rejection by Operator Notification" },
            { CompletionByDataminerIDX, "Live Order Completion by DataMiner Notification" },
            { CompletionWithErrorByDataminerIDX, "Live Order Completion with Errors by DataMiner Notification" },
            { CancellationByCustomerIDX, "Live Order Cancellation by Customer Notification" },
            { CancellationByOperatorIDX, "Live Order Cancellation by Operator Notification" },
            { CancellationByIntegrationIDX, "Live Order Cancellation by Integration Notification" },
            { NloReassignedByOperatorIDX, "Non Live Order Reassigned by Operator" },
            { NloRejectionByOperatorIDX, "Non Live Order Rejection by Operator" },
            { NloCompletionByDataminerIDX, "Non Live Order Completion by DataMiner" },
            { NloCancellationByOperatorIDX, "Non Live Order Cancellation by Operator" },
            { NloWorkInProgressByOperatorIDX, "Non Live Order Work in Progress by Operator" },
            { ServicesBookedIDX, "Live Order Services Booked Notification" },
            { ServicesBookedByIntegrationsIDX, "Live Order Services Booked by Integrations Notification" },
            { NloEditedByOperatorIDX, "Non Live Order Edited by Operator" },
            { NloCreationByOperatorIDX, "Non Live Order Creation by Operator" },
            { NloIplayFolderCreationByOperatorIDX, "Non Live Order Iplay Folder Creation by Operator" },
            { UnableToBookRecurringVizrem, "Unable to Book Recurring Vizrem Order" }
        };
    }
}
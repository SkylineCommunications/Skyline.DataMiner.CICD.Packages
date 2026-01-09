namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Aspera
{
    using Newtonsoft.Json;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    public class Aspera : NonLiveOrder
    {
        [JsonProperty]
        public override IngestExport.Type OrderType => Type.AsperaOrder;

        [JsonIgnore]
        public override string ShortDescription => $"{OrderDescription} - {Type.AsperaOrder.GetDescription()}";

        [JsonProperty]
        public string AsperaType { get; set; }

        [JsonProperty]
        public string Workgroup { get; set; }

        [JsonProperty]
        public string[] SendersEmails { get; set; } = new string[0];

		[JsonProperty]
        public double ValidDays { get; set; }

        [JsonProperty]
        public string ImportDepartment { get; set; }

        [JsonProperty]
        public string NameofTheShare { get; set; }

        [JsonProperty]
        public string PurposeAndUsage { get; set; }

        [JsonProperty]
        public string[] ParticipantsEmailAddress { get; set; } = new string[0];

        [JsonProperty]
        public string AdditionalInfo { get; set; }
    }
}
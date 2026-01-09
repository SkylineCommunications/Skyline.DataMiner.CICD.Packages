using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement
{
	public enum BookServicesStatus
    {
        NotApplicable = -1,
        ToBeScheduled = 0,
        Scheduled = 1,
        Ongoing = 2,
        Ok = 3,
        Fail = 4,
        Timeout = 5,
        ReScheduledAfterFailure = 6,
    }

    public class OrderInfo
    {
		public string ReservationId { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Name { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
		public DateTime? StartTime { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
		public DateTime? EndTime { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<string> ServicesToRemoveReservationIds { get; set; }

        public BookServicesStatus? BookServicesStatus { get; set; }

        public string BookServicesFailureReason { get; set; }

        public BookServicesStatus? BookEventLevelReceptionServicesStatus { get; set; }

        public string BookEventLevelReceptionServicesFailureReason { get; set; }

        public bool IsHighPriority { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? AgentId { get; set; }

        [JsonIgnore]
        public DateTime? LastBookServiceStatusChange { get; set; }

        [JsonIgnore]
        public TimeSpan? LastBookServicesDuration { get; set; }

        [JsonIgnore]
        public DateTime? LastBookEventLevelReceptionServiceStatusChange { get; set; }

        [JsonIgnore]
        public TimeSpan? LastBookEventLevelReceptionServicesDuration { get; set; }

        public bool ProcessChronologically{ get; set; }

        public static OrderInfo Deserialize(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<OrderInfo>(json);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string Serialize()
        {
            try
            {
                return JsonConvert.SerializeObject(this, Formatting.None);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

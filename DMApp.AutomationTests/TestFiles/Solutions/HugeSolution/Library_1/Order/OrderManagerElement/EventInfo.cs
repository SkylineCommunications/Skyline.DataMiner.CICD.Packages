using System;
using Newtonsoft.Json;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement
{
	public class EventInfo
    {
        public EventInfo()
        {

        }

        public string JobId { get; set; }

        public string Name { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        [JsonIgnore]
        public bool StartEventHasTriggered { get; set; }

        [JsonIgnore]
        public bool EndEventHasTriggered { get; set; }

        public static EventInfo Deserialize(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<EventInfo>(json);
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

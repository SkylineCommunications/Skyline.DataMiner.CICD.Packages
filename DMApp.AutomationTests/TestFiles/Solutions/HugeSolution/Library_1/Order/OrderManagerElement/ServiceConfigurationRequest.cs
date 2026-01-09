namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement
{
    using System;
    using Newtonsoft.Json;

    public class ServiceConfigurationRequest
    {
        public string RequestId { get; set; }

        public string OrderId { get; set; }

        public DateTime OrderEnd { get; set; }

        public string ServiceConfiguration { get; set; }

        public ServiceConfigurationRequestType RequestType { get; set; }

        public static ServiceConfigurationRequest Deserialize(string request)
        {
            try
            {
                return JsonConvert.DeserializeObject<ServiceConfigurationRequest>(request);
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

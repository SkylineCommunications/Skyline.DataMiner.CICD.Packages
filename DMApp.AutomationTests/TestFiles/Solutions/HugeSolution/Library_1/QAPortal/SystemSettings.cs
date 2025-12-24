namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.QAPortal
{
    using Newtonsoft.Json;
    using System;
    using System.IO;

    public class SystemSettings
    {
        private const string SystemSettingsPath = @"C:\RTManager\Settings\SystemSettings.json";

        private SystemSettings()
        {

        }

        public string ClientId { get; set; }

        public string ApiKey { get; set; }

#pragma warning disable S3996 // URI properties should not be strings
        public string EndPointUrl { get; set; }

        [JsonIgnore]
        public string PerformanceStartTestUrl { get; set; }

        [JsonIgnore]
        public string PerformanceAddTestUrl { get; set; }
#pragma warning restore S3996 // URI properties should not be strings

        public static SystemSettings Load()
        {
            if (File.Exists(SystemSettingsPath))
            {
                try
                {
                    var json = File.ReadAllText(SystemSettingsPath);
                    var systemSettings = JsonConvert.DeserializeObject<SystemSettings>(json);
                    if (!String.IsNullOrEmpty(systemSettings.EndPointUrl))
                    {
                        systemSettings.PerformanceStartTestUrl = systemSettings.EndPointUrl;
                        systemSettings.PerformanceAddTestUrl = systemSettings.EndPointUrl;

                        return systemSettings;
                    }
                }
                catch (Exception)
                {
                    // serialization issues don't need to be logged
                }
            }

            return new SystemSettings
            {
                PerformanceStartTestUrl = @"https://qaportal.skyline.local/performance/starttest",
                PerformanceAddTestUrl = @"https://qaportal.skyline.local/performance/add"
            };
        }
    }
}
namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Net.Exceptions;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Messages.Advanced;

    public static class FileSyncExtension
    {
        public static void SendFileChangeNotification(this IEngine engine, NotifyType notifyType, string destinationPath)
        {
            SetDataMinerInfoMessage message = new SetDataMinerInfoMessage
            {
                What = (int)NotifyType.SendDmsFileChange,
                StrInfo1 = destinationPath,
                IInfo2 = (int)notifyType
            };
            var response = engine.SendSLNetSingleResponseMessage(message);
            if (response == null)
            {
                throw new DataMinerException($" Server response was null.");
            }
            if (response is Skyline.DataMiner.Net.Messages.CreateProtocolFileResponse rsp && rsp.ErrorCode != 0)
            {
                throw new DataMinerException($" The returned error code was {rsp.ErrorCode}");
            }
        }
    }
}

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport
{
    using Newtonsoft.Json;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Net.Exceptions;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Messages.Advanced;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    public class FileAttachmentInfo
    {
        [JsonProperty]
        public List<string> SubtitleAttachmentFileNames { get; private set; } = new List<string>();

        [JsonIgnore]
        public List<string> SubtitleAttachmentFilesToRemove { get; private set; } = new List<string>();

        [JsonIgnore]
        public FileSelector FileSelector { get; set; } = new FileSelector();

        public void UpdateFilesToDirectory(Helpers helpers, string folderPath)
        {
            AddUploadedFilesToDirectory(helpers, folderPath);

            RemoveUploadedFilesToDirectory(helpers, folderPath);
        }

        private void AddUploadedFilesToDirectory(Helpers helpers, string folderPath)
        {
            try
            {                
                if (FileSelector != null && FileSelector.UploadedFilePaths.Any())
                {
                    FileSelector.CopyUploadedFiles(folderPath);
                }

                foreach (var fileName in SubtitleAttachmentFileNames)
                {
                    string filePath = Path.Combine(folderPath, fileName);
                    if (File.Exists(filePath)) continue;

                    helpers.Engine.SendFileChangeNotification(NotifyType.FileAdd, filePath);
                }
            }
            catch (Exception e)
            {
                helpers.Log(nameof(FileAttachmentInfo), nameof(AddUploadedFilesToDirectory), "Something went wrong during file uploading: " + e.Message);
            }
        }

        private void RemoveUploadedFilesToDirectory(Helpers helpers, string folderPath)
        {
            try
            {
                foreach (var fileName in SubtitleAttachmentFilesToRemove)
                {
                    string filePath = Path.Combine(folderPath, fileName);
                    if (File.Exists(filePath)) File.Delete(filePath);

                    helpers.Engine.SendFileChangeNotification(NotifyType.FileRemoved, filePath);
                }
            }
            catch (Exception e)
            {
                helpers.Log(nameof(FileAttachmentInfo), nameof(RemoveUploadedFilesToDirectory), "Something went wrong during file removal: " + e.Message);
            }
        }

        private void SendFileChangeNotification(IEngine engine, NotifyType notifyType, string destinationPath)
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

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript.Components;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class DownloadDialog : Dialog
	{
		public DownloadButton DownloadButton { get; private set; } = new DownloadButton();

		public DownloadDialog(IEngine engine, string remoteFilePath) : base(engine)
		{
			DownloadButton.RemoteFilePath = remoteFilePath;
			DownloadButton.DownloadedFileName = Path.GetFileName(remoteFilePath);
			DownloadButton.StartDownloadImmediately = true;

			AddWidget(new Label("Download will start immediately"), 0, 0);
			AddWidget(DownloadButton, 1, 0);
		}
	}
}

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens
{
	using System;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ShowDialogEventArgs : EventArgs
	{
		public ShowDialogEventArgs(Dialog dialog)
		{
			Dialog = dialog;
		}

		public Dialog Dialog { get; private set; }
	}
}

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.UiDisabling
{
	using System;

	public class EnabledStateEventArgs : EventArgs
	{
		public EnabledStateEventArgs(EnabledState enabledState)
		{
			EnabledState = enabledState;
		}

		public EnabledState EnabledState { get; }
	}
}
namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI
{
	using System;

	public class UiDisabler : IDisposable
	{
		private bool disposedValue;
		private readonly IDisableableUi disableUI;

		private UiDisabler(IDisableableUi disableUI)
		{
			this.disableUI = disableUI;

			disableUI.DisableUi();
		}

		public static UiDisabler StartNew(IDisableableUi obj)
		{ 
			return new UiDisabler(obj);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue && disposing)
			{
				disableUI.EnableUi();
			}

			disposedValue = true;
		}

		~UiDisabler()
		{
			Dispose(false);
		}
	}
}
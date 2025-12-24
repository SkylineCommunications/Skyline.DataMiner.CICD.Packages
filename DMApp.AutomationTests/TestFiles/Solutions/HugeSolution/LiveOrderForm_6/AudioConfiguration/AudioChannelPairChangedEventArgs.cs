namespace LiveOrderForm_6.AudioConfiguration
{
	using System;

	public class AudioChannelPairChangedEventArgs : EventArgs
	{
		public AudioConfiguration.AudioChannelPair AudioChannelPair { get; set; }
	}
}
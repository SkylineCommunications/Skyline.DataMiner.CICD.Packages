namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class LockNotGrantedException : MediaServicesException
    {
		// this default message should not be changed as it's used by the Order Manager element to determine if order service booking should be rescheduled
		public static readonly string DefaultMessage = "Lock could not be retrieved";

		public LockNotGrantedException() : base(DefaultMessage)
		{
		}

		public LockNotGrantedException(Exception inner) : base(DefaultMessage, inner)
		{
		}
	}
}
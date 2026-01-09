using System;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking
{
	public class LockInfo
	{
		public LockInfo(bool isLockGranted, string lockUsername, string objectId, TimeSpan releaseLocksAfter)
		{
			IsLockGranted = isLockGranted;
			LockUsername = lockUsername;
			ObjectId = objectId;
            ReleaseLocksAfter = releaseLocksAfter;
		}

		/// <summary>
		/// Gets a value indicating whether the lock was granted or not.
		/// </summary>
		public bool IsLockGranted { get; private set; }

		/// <summary>
		/// Gets the name of the user was granted the lock to the requested object.
		/// </summary>
		public string LockUsername { get; private set; }

		/// <summary>
		/// Gets the Id of the requested object.
		/// </summary>
		public string ObjectId { get; private set; }

        /// <summary>
        /// Gets the time indication on when the requested lock will be released.
        /// </summary>
        public TimeSpan ReleaseLocksAfter { get; private set; }
    }
}
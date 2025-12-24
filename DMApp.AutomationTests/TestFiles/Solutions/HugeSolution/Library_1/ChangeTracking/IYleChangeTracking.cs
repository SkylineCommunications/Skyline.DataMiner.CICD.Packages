namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking
{
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public interface IYleChangeTracking
	{
		string UniqueIdentifier { get; }

		string DisplayName { get; }

		/// <summary>
		/// Gets a boolean indicating if Change Tracking is enabled.
		/// </summary>
		bool ChangeTrackingStarted { get; }

		Change Change { get; }

		void AcceptChanges(Helpers helpers = null);

		Change GetChangeComparedTo<T>(Helpers helpers, T oldObjectInstance);
	}

}
namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;

	public class FolderNotFoundException : MediaServicesException
	{
		public FolderNotFoundException()
		{
		}

		public FolderNotFoundException(string folderFriendlyName, IEnumerable<Folder> folders)
			: base($"Unable to find folder '{folderFriendlyName}' between {String.Join(";", folders.Select(x => x.FriendlyName))}")
		{
		}

		public FolderNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
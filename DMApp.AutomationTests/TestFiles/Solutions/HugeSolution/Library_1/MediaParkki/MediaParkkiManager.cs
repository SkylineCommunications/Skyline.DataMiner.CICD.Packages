namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.MediaParkki
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class MediaParkkiManager : INonLiveIntegrationManager
	{
		private readonly Skyline.DataMiner.Automation.Element element;
		private readonly Stopwatch stopwatch;

		private List<Folder> folders;
		private List<Folder> rootFolders;
		private List<File> files;

		public Helpers Helpers { get; private set; }

		public MediaParkkiManager(Helpers helpers)
		{
			Helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			this.stopwatch = new Stopwatch();

			element = helpers.Engine.FindElementsByProtocol(Configuration.MediaParkki.Name).SingleOrDefault(e => e.ElementName.Contains("Mediaparkki"));
			if(element == null) throw new ElementNotFoundException();
			if (!element.IsActive) throw new ElementNotActiveException();

			InitializeFoldersAndFiles();
		}

		public List<Folder> GetRootFolders()
		{
			return rootFolders;
		}

		public NonLiveManagerResponse GetChildren(string parentPath)
		{
			if(!element.IsActive) throw new ElementNotActiveException();

			Folder parentFolder;
			if (rootFolders.Any(f => f.URL == parentPath))
			{
				parentFolder = rootFolders.Single(f => f.URL == parentPath);
			}
			else
			{
				parentFolder = folders.Single(f => f.URL == parentPath);
			}

			var childFolders = new HashSet<Folder>(folders.Where(f => f.IsChildOf(parentFolder.URL)));
			parentFolder.Children = childFolders;

			var childFiles = files.Where(file => file.Parent == parentPath).ToArray();
			return new NonLiveManagerResponse { Files = childFiles, Folders = childFolders.ToArray() };
		}

		private void InitializeFoldersAndFiles()
		{
			if (!element.IsActive) throw new ElementNotActiveException();

			InitializeFiles();
			InitializeRegularFolders();
			InitializeRootFolders();
		}

		private void InitializeFiles()
		{
			stopwatch.Restart();
			Helpers.Log(nameof(MediaParkkiManager), nameof(InitializeFiles), "Getting all files");

			var filesTable = element.GetTable(Helpers.Engine, Configuration.MediaParkki.filesTablePid);

			Helpers.Log(nameof(MediaParkkiManager), nameof(InitializeFiles), "Files table has " + filesTable.Keys.Count + " rows");
			files = filesTable.Values.Select(fileRow => new File
			{
				URL = Convert.ToString(fileRow[0]),
				DisplayName = Convert.ToString(fileRow[2]),
				Parent = Convert.ToString(fileRow[1])
			}).ToList();

			stopwatch.Stop();
			Helpers.Log(nameof(MediaParkkiManager), nameof(InitializeFiles), "Done converting files table into file objects [" + stopwatch.Elapsed + "]");
		}

		private void InitializeRegularFolders()
		{
			stopwatch.Restart();
			Helpers.Log(nameof(MediaParkkiManager), nameof(InitializeRegularFolders), "Getting all folders.");

			var folderUrls = element.GetTablePrimaryKeys(Configuration.MediaParkki.foldersTablePid);

			Helpers.Log(nameof(MediaParkkiManager), nameof(InitializeRegularFolders), "Folders table has " + folderUrls.Length+" rows");

			folders = folderUrls.Select(folderUrl => new Folder
			{
				URL = folderUrl,
				FriendlyName = folderUrl.Split('\\').Last()
			}).ToList();

			stopwatch.Stop();
			Helpers.Log(nameof(MediaParkkiManager), nameof(InitializeRegularFolders), "Done converting all rows into folder objects [" + stopwatch.Elapsed+"]");
		}

		private void InitializeRootFolders()
		{
			stopwatch.Restart();
			Helpers.Log(nameof(MediaParkkiManager), nameof(InitializeRootFolders), "Getting all root folders");

			var rootFolderUrls = element.GetTablePrimaryKeys(Configuration.MediaParkki.rootFoldersTablePid);
			rootFolders = rootFolderUrls.Select(rootFolderUrl => new Folder
			{
				URL = rootFolderUrl,
				FriendlyName = rootFolderUrl.Split('\\').Last(),
				Files = new HashSet<File>()
			}).ToList();

			stopwatch.Stop();
			Helpers.Log(nameof(MediaParkkiManager), nameof(InitializeRootFolders), "Done converting root folders table into folder objects [" + stopwatch.Elapsed + "]");
		}
	}
}

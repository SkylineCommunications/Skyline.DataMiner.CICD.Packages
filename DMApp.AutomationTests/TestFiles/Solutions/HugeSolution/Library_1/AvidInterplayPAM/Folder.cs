namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class Folder
	{
		public string URL { get; set; }

		public string FriendlyName { get; set; }

		public HashSet<Folder> Children { get; set; } = new HashSet<Folder>();

		public HashSet<File> Files { get; set; } = new HashSet<File>();

		public void Update(string folderPath, IEnumerable<Folder> folders)
		{
			if (this.URL == folderPath)
			{
				foreach (Folder folder in folders) Children.Add(folder);
			}
			else
			{
				foreach (Folder child in Children) child.Update(folderPath, folders);
			}
		}

		public bool IsChildOf(string possibleParentFolderUrl)
		{
			var splitPossibleParentUrl = possibleParentFolderUrl.Split('\\');

			var splitUrl = URL.Split('\\');
			var splitUrlWithoutLastElement = splitUrl.Take(splitUrl.Length - 1);

			return splitUrlWithoutLastElement.SequenceEqual(splitPossibleParentUrl);
		}

		public override bool Equals(object obj)
		{
			Folder other = obj as Folder;
			if (other == null) return false;

			return URL.Equals(other.URL);
		}

		public override int GetHashCode()
		{
			return URL.GetHashCode();
		}

		public List<Folder> GetAllChildren()
		{
			List<Folder> allChildren = new List<Folder>();

			foreach (Folder folder in Children)
			{
				allChildren.Add(folder);
				allChildren.AddRange(folder.GetAllChildren());
			}

			return allChildren;
		}

		public List<File> GetAllFiles()
		{
			List<File> allFiles = new List<File>();
			allFiles.AddRange(Files);

			foreach (Folder folder in Children)
			{
				allFiles.AddRange(folder.GetAllFiles());
			}

			return allFiles;
		}
	}
}
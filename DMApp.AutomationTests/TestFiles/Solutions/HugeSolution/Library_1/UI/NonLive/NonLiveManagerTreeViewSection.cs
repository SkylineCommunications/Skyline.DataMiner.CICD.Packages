namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.AvidInterplayPAM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.AutomationUI.Objects;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class NonLiveManagerTreeViewSection : YleSection
	{
		private readonly ISectionConfiguration configuration;
		private readonly INonLiveIntegrationManager manager;
		private readonly Dictionary<string, Folder> cachedFolders = new Dictionary<string, Folder>();
		private readonly Dictionary<string, File> cachedFiles = new Dictionary<string, File>();
		private readonly HashSet<string> lazyLoadedItems = new HashSet<string>();

		private const string FileTextBoxPlaceHolder = "Select file(s)...";
		private const string FolderTextBoxPlaceHolder = "Select a folder...";

		public NonLiveManagerTreeViewSection(Helpers helpers, ISectionConfiguration configuration, INonLiveIntegrationManager manager, TreeViewType type) : base(helpers)
		{
			this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
			this.configuration = configuration;
			Type = type;

			SelectedItemTextBox.PlaceHolder = (Type == TreeViewType.FileSelector) ? FileTextBoxPlaceHolder : FolderTextBoxPlaceHolder;

			SelectedFolder = null;
			SelectedFiles = new HashSet<File>();

			TreeView = new TreeView(new TreeViewItem[0]);
			TreeView.Checked += TreeView_Checked;
			TreeView.Unchecked += TreeView_Unchecked;
			TreeView.Expanded += (s, a) => TreeViewItemExpanded(a);

			GenerateUi(out int row);
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(SourceLabel, ++row, 0, verticalAlignment: VerticalAlignment.Top);
			AddWidget(SelectedItemTextBox, row, 1, 1, 2);
			AddWidget(TreeView, ++row, 1, 1, 2);

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
		}

		public bool IsRequestSuccessful { get; private set; } = true;

		public event EventHandler<object> SelectedItemChanged;

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			SourceLabel.IsVisible = IsVisible;

			SelectedItemTextBox.IsVisible = IsVisible;
			SelectedItemTextBox.IsEnabled = false;

			TreeView.IsEnabled = IsEnabled && IsRequestSuccessful;
			TreeView.IsVisible = TreeView.IsEnabled && IsVisible; // temporary solution for not being able to disable tree view widgets (DCP 176214)

			ToolTipHandler.SetTooltipVisibility(this);
		}

		public TreeViewType Type { get; private set; }

		private TreeView TreeView { get; set; }

		public Label SourceLabel { get; private set; } = new Label("Interplay source folder");

		public YleTextBox SelectedItemTextBox { get; private set; } = new YleTextBox(String.Empty) { IsEnabled = false, IsMultiline = true, Height = 80 };

		public Folder SelectedFolder { get; private set; }

		public HashSet<File> SelectedFiles { get; private set; }

		public string[] GetSelectedFileUrls()
		{
			return SelectedItemTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Except(new string[] { FileTextBoxPlaceHolder, FolderTextBoxPlaceHolder }).ToArray();
		}

		public List<string> GetFolderUrls()
		{
			List<string> folderUrls = new List<string>();

			foreach (var selectedFileUrl in GetSelectedFileUrls())
			{

				int lastFoundIndex = selectedFileUrl.LastIndexOf('/');
				if (lastFoundIndex == -1) continue;

				folderUrls.Add(selectedFileUrl.Substring(0, lastFoundIndex));
			}

			return folderUrls.Distinct().ToList();
		}

		public UIValidationState ValidationState
		{
			get
			{
				return SelectedItemTextBox.ValidationState;
			}

			set
			{
				SelectedItemTextBox.ValidationState = value;
			}
		}

		public string ValidationText
		{
			get
			{
				return SelectedItemTextBox.ValidationText;
			}

			set
			{
				SelectedItemTextBox.ValidationText = value;
			}
		}

		/// <summary>
		/// Used to uncheck other items except for its parents in case they are also checked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TreeView_Checked(object sender, IEnumerable<TreeViewItem> e)
		{
			using (UiDisabler.StartNew(this))
			{
				bool changed = (Type == TreeViewType.FolderSelector) ? ItemCheckedInFolderTreeView(e) : ItemsCheckedInFileTreeView(e);

				if (changed && SelectedItemChanged != null) SelectedItemChanged(this, (object)SelectedFolder);
			}
		}

		private bool ItemCheckedInFolderTreeView(IEnumerable<TreeViewItem> e)
		{
			bool changed = false;
			foreach (var checkedItem in e)
			{
				if (!checkedItem.IsChecked || !cachedFolders.TryGetValue(checkedItem.KeyValue, out Folder checkedFolder)) continue;

				ClearTreeView();
				checkedItem.IsChecked = true;

				manager.Helpers.Log(nameof(NonLiveManagerTreeViewSection), nameof(ItemCheckedInFolderTreeView), $"Updating selected folder: {checkedFolder.FriendlyName}");

				SelectedFolder = checkedFolder;
				changed = true;
			}

			UpdateSelectedItemsTextBox();
			return changed;
		}

		private bool ItemsCheckedInFileTreeView(IEnumerable<TreeViewItem> e)
		{
			bool changed = false;
			foreach (var checkedItem in e.Where(x => x.IsChecked))
			{
				if (cachedFiles.ContainsKey(checkedItem.KeyValue))
				{
					// File got checked
					changed = true;

					// Check folder if all files in folder are checked
					TreeViewItem parentItem = TreeView.GetAllItems().FirstOrDefault(x => x.ChildItems.Exists(y => y.KeyValue.Equals(checkedItem.KeyValue))) ?? throw new NotFoundException("Unable to find parent item");
					if (parentItem.ChildItems.All(x => x.IsChecked)) parentItem.IsChecked = true;
				}
				else if (cachedFolders.ContainsKey(checkedItem.KeyValue))
				{
					// Folder got checked
					FolderCheckedInFileTreeView(checkedItem);
					changed = true;
				}
				else
				{
					return false;
				}
			}

			if (changed)
			{
				UpdateSelectedFiles();
				UpdateSelectedItemsTextBox();
			}

			return changed;
		}

		private void FolderCheckedInFileTreeView(TreeViewItem checkedItem)
		{
			// if a folder was checked and was not lazyloaded -> load child items + check files
			// Uncheck checkedItem if it contains folders (only check files)
			checkedItem.IsCollapsed = false;
			if (!lazyLoadedItems.Contains(checkedItem.KeyValue))
			{
				TreeViewItemExpanded(new[] { checkedItem });
			}

			// Check files in folder
			bool folderContainsFiles = false;
			bool folderContainsFolders = false;
			foreach (var childItem in checkedItem.ChildItems)
			{
				if (!cachedFiles.Any(file => file.Key == childItem.KeyValue))
				{
					folderContainsFolders = true;
					continue;
				}

				folderContainsFiles = true;
				childItem.IsChecked = true;
			}

			checkedItem.IsChecked = folderContainsFiles && !folderContainsFolders;
			checkedItem.ItemType = folderContainsFiles ? TreeViewItem.TreeViewItemType.CheckBox : TreeViewItem.TreeViewItemType.Empty;
		}

		private void UpdateSelectedFiles()
		{
			TreeView.UpdateItemCache();

			var checkedItems = TreeView.CheckedItems;
			SelectedFiles = new HashSet<File>(from kvp in cachedFiles where checkedItems.Any(x => x.KeyValue.Equals(kvp.Key)) select kvp.Value);
		}

		private void UpdateSelectedItemsTextBox()
		{
			if (Type == TreeViewType.FileSelector)
			{
				SelectedItemTextBox.Text = SelectedFiles.Any() ? String.Join(Environment.NewLine, SelectedFiles.Select(x => $"{x.Parent}/{x.DisplayName}").OrderBy(x => x)) : FileTextBoxPlaceHolder;
			}
			else
			{
				SelectedItemTextBox.Text = SelectedFolder != null ? SelectedFolder.URL : FolderTextBoxPlaceHolder;
			}
		}

		private void TreeView_Unchecked(object sender, IEnumerable<TreeViewItem> e)
		{
			using (UiDisabler.StartNew(this))
			{
				if (Type == TreeViewType.FileSelector)
				{
					foreach (var uncheckedItem in e)
					{
						if (cachedFolders.ContainsKey(uncheckedItem.KeyValue))
						{
							// Uncheck all files in folder
							foreach (var fileItem in uncheckedItem.ChildItems.Where(x => cachedFiles.ContainsKey(x.KeyValue))) fileItem.IsChecked = false;
						}

						// Uncheck all parent folders
						var treeviewItems = TreeView.GetAllItems();
						TreeViewItem parentItem = treeviewItems.FirstOrDefault(x => x.ChildItems.Any(y => y.KeyValue.Equals(uncheckedItem.KeyValue)));

						while (parentItem != null)
						{
							parentItem.IsChecked = false;
							parentItem = treeviewItems.FirstOrDefault(x => x.ChildItems.Any(y => y.KeyValue.Equals(parentItem.KeyValue)));
						}
					}

					TreeView.UpdateItemCache();
					UpdateSelectedFiles();
				}
				else
				{
					// Folder treeview only allows one item to be checked at a time -> uncheck all
					ClearTreeView();
					//if (!TreeView.CheckedItems.Any()) 
				}

				UpdateSelectedItemsTextBox();
			}
		}

		private void ClearTreeView()
		{
			foreach (var item in TreeView.CheckedItems) item.IsChecked = false;

			SelectedFolder = null;
			SelectedFiles = new HashSet<File>();
			SelectedItemChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Used for lazy loading files and folders.
		/// </summary>
		/// <param name="expandedFolders"></param>
		private void TreeViewItemExpanded(IEnumerable<TreeViewItem> expandedFolders)
		{
			using (UiDisabler.StartNew(this))
			{
				var nonLazyLoadedExpandedFolders = expandedFolders.Where(f => !lazyLoadedItems.Contains(f.KeyValue)).ToList();

				foreach (var folderItem in nonLazyLoadedExpandedFolders)
				{
					if (!TryGetChildren(folderItem.KeyValue, out NonLiveManagerResponse response)) return;

					AddChildrenToFolderAndCache(folderItem, response);

					if (Type == TreeViewType.FileSelector)
					{
						bool folderContainsFiles = false;
						foreach (File file in response.Files.Where(x => !String.IsNullOrWhiteSpace(x.DisplayName)).OrderBy(x => x.DisplayName).ToList())
						{
							folderContainsFiles = true;

							file.Parent = folderItem.KeyValue; // folder keyvalue = folder.URL

							string fileKeyValue = folderItem.KeyValue + file.URL;
							folderItem.ChildItems.Add(new TreeViewItem(file.DisplayName, fileKeyValue)
							{
								ItemType = TreeViewItem.TreeViewItemType.CheckBox,
								SupportsLazyLoading = false,
								IsCollapsed = true,
								IsChecked = false,
								CheckingBehavior = TreeViewItem.TreeViewItemCheckingBehavior.None
							});

							try
							{
								cachedFiles.Add(fileKeyValue, file);
							}
							catch (ArgumentException e)
							{
								manager.Helpers.Log(nameof(NonLiveManagerTreeViewSection), nameof(TreeViewItemExpanded), $"Exception occured: {e} | File keyValue: " + fileKeyValue);
							}
						}

						folderItem.ItemType = folderContainsFiles ? TreeViewItem.TreeViewItemType.CheckBox : TreeViewItem.TreeViewItemType.Empty;
					}

					lazyLoadedItems.Add(folderItem.KeyValue);
				}

				manager.Helpers.Log(nameof(NonLiveManagerTreeViewSection), nameof(TreeViewItemExpanded), $"Lazy loaded items: '{string.Join(";", lazyLoadedItems)}' after tree view item expantion is finished");
			}
		}

		private void AddChildrenToFolderAndCache(TreeViewItem folderItem, NonLiveManagerResponse response)
		{
			foreach (var folder in response.Folders.OrderBy(x => x.FriendlyName).ToList())
			{
				folderItem.ChildItems.Add(new TreeViewItem(folder.FriendlyName, folder.URL)
				{
					ItemType = TreeViewItem.TreeViewItemType.CheckBox,
					SupportsLazyLoading = true,
					IsCollapsed = true,
					IsChecked = false,
					CheckingBehavior = TreeViewItem.TreeViewItemCheckingBehavior.None
				});

				if (!cachedFolders.ContainsKey(folder.URL))
				{
					cachedFolders.Add(folder.URL, folder);
				}
			}
		}

		private List<TreeViewItem> GetAllChildItems(TreeViewItem treeViewItem)
		{
			List<TreeViewItem> items = new List<TreeViewItem>();
			foreach (var child in treeViewItem.ChildItems)
			{
				items.Add(child);
				items.AddRange(GetAllChildItems(child));
			}

			return items;
		}

		private bool TryGetRootFolders(out List<Folder> rootFolders)
		{
			rootFolders = new List<Folder>();
			try
			{
				rootFolders = manager.GetRootFolders();
				IsRequestSuccessful = true;
				ValidationState = UIValidationState.Valid;
				ValidationText = String.Empty;
				return true;
			}
			catch (ElementNotActiveException)
			{
				IsRequestSuccessful = false;
				ValidationState = UIValidationState.Invalid;
				ValidationText = "Element is inactive.";
				return false;
			}
			catch (ElementDidNotLogRequestException)
			{
				IsRequestSuccessful = false;
				ValidationState = UIValidationState.Invalid;
				ValidationText = "Element did not respond.";
				return false;
			}
			catch (IplayServiceDidNotRespondException e)
			{
				IsRequestSuccessful = false;
				ValidationState = UIValidationState.Invalid;
				ValidationText = e.Message;
				return false;
			}
		}

		private bool TryGetChildren(string parentPath, out NonLiveManagerResponse response)
		{
			response = null;
			try
			{
				response = manager.GetChildren(parentPath);
				IsRequestSuccessful = true;

				ValidationState = UIValidationState.Valid;
				ValidationText = String.Empty;
				return true;
			}
			catch (ElementNotActiveException)
			{
				IsRequestSuccessful = false;
				ValidationState = UIValidationState.Invalid;
				ValidationText = "Element is inactive.";
				return false;
			}
			catch (ElementDidNotLogRequestException)
			{
				IsRequestSuccessful = false;
				ValidationState = UIValidationState.Invalid;
				ValidationText = "Element did not respond.";
				return false;
			}
			catch (IplayServiceDidNotRespondException e)
			{
				IsRequestSuccessful = false;
				ValidationState = UIValidationState.Invalid;
				ValidationText = e.Message;
				return false;
			}
		}

		public void InitRoot()
		{
			cachedFolders.Clear();
			cachedFiles.Clear();

			if (!TryGetRootFolders(out var rootFolders)) return;

			List<TreeViewItem> treeViewItems = new List<TreeViewItem>();
			foreach (var rootFolder in rootFolders.OrderBy(x => x.FriendlyName).ToList())
			{
				treeViewItems.Add(new TreeViewItem(rootFolder.FriendlyName, rootFolder.URL)
				{
					ItemType = TreeViewItem.TreeViewItemType.CheckBox,
					SupportsLazyLoading = true,
					IsCollapsed = true,
					IsChecked = false,
					CheckingBehavior = TreeViewItem.TreeViewItemCheckingBehavior.None
				});

				cachedFolders.Add(rootFolder.URL, rootFolder);
			}

			TreeView.Items = treeViewItems;
		}

		/// <summary>
		/// Used to initialize the TreeView.
		/// </summary>
		/// <param name="folderUrls">Saved folder urls.</param>
		/// <param name="fileUrls">Saved optional URLs of the files to be selected in the selected folder.</param>
		public void Initialize(string[] folderUrls, string[] fileUrls = null)
		{
			if (!folderUrls.Any()) throw new ArgumentException(nameof(folderUrls));

			var initializeValidator = new InitializeValidator();

			cachedFolders.Clear();
			cachedFiles.Clear();

			if (!TryGetRootFolders(out var rootFolders))
			{
				manager.Helpers.Log(nameof(NonLiveManagerTreeViewSection), nameof(Initialize), "No root folders found, initialize will be skipped");
				return;
			}

			InitRootFoldersAndFiles(initializeValidator, folderUrls, rootFolders, fileUrls);

			manager.Helpers.Log(nameof(NonLiveManagerTreeViewSection), nameof(Initialize), $"Lazy load items after initialising root folders: '{string.Join(";", lazyLoadedItems)}'");

			if (initializeValidator.RootFolderPathMatching)
			{
				manager.Helpers.Log(nameof(NonLiveManagerTreeViewSection), nameof(Initialize), $"Initialize completed as one of the root folders need to be selected");
				return;
			}

			InitNonRootFoldersAndFiles(initializeValidator, folderUrls, fileUrls);
		}

		/// <summary>
		/// Initialisation of the root folders, if there are any files present among the matching root folder path then files will also be added.
		/// </summary>
		/// <param name="initializeValidator">Validator which holds initialisation data of root/non-root folders and files.</param>
		/// <param name="rootFolders">Root folders which are retrieved directly from the manager element.</param>
		/// <param name="folderUrls">Saved folder urls.</param>
		/// <param name="fileUrls">Saved file urls.</param>
		private void InitRootFoldersAndFiles(InitializeValidator initializeValidator, string[] folderUrls, List<Folder> rootFolders, string[] fileUrls = null)
		{
			var treeViewItems = new List<TreeViewItem>();
			foreach (var rootFolder in rootFolders.OrderBy(x => x.FriendlyName).ToList())
			{
				bool rootFolderPathMatchesArgument = folderUrls.Contains(rootFolder.URL);
				bool isPathInChildBranch = folderUrls.Any(folderUrl => folderUrl != null && folderUrl.StartsWith(rootFolder.URL));
				bool isCollapsed = !isPathInChildBranch || (rootFolderPathMatchesArgument && Type == TreeViewType.FolderSelector);

				var newRootFolderTreeViewItem = CreateNewRootFolderItemWhileInitializing(folderUrls, rootFolder);
				treeViewItems.Add(newRootFolderTreeViewItem);

				if (!isCollapsed) lazyLoadedItems.Add(rootFolder.URL);

				if (rootFolderPathMatchesArgument && Type == TreeViewType.FolderSelector)
				{
					SelectedFolder = rootFolder;
					SelectedItemTextBox.Text = rootFolder.URL;
					initializeValidator.RootFolderPathMatching = true;
				}
				else if (rootFolderPathMatchesArgument && Type == TreeViewType.FileSelector && fileUrls != null && TryGetChildren(rootFolder.URL, out var childFolderResponse))
				{
					/* When file path matches among root folder, child folders still need to be added among matching root folder.
					To make sure all childs are seen in the UI after initializing and to keep cache in sync. */

					initializeValidator.FilePathMatchesAmongRootFolders = InitFiles(fileUrls, newRootFolderTreeViewItem, folderUrls, childFolderResponse);
				}

				if (!cachedFolders.ContainsKey(rootFolder.URL)) cachedFolders.Add(rootFolder.URL, rootFolder);
			}

			TreeView.Items = treeViewItems; // From this point, tree view has a link between added items and its collections

			if (initializeValidator.FilePathMatchesAmongRootFolders)
			{
				UpdateSelectedFiles();
				UpdateSelectedItemsTextBox();
			}
		}

		/// <summary>
		/// Will add every non root folder and file to the treeview until the same layer of the saved urls are reached.
		/// </summary>
		/// <param name="initializeValidator">Validator which holds initialisation data of root/non-root folders and files.</param>
		/// <param name="folderUrls">Saved folder urls.</param>
		/// <param name="fileUrls">Saved file urls.</param>
		private void InitNonRootFoldersAndFiles(InitializeValidator initializeValidator, string[] folderUrls, string[] fileUrls)
		{
			var parentItems = new List<TreeViewItem>();

			var rootItems = TreeView.GetItems(0).Where(x => folderUrls.Any(folderUrl => x != null && folderUrl != null && folderUrl.StartsWith(x.KeyValue))).ToList();
			foreach (var rootItem in rootItems)
			{
				if (!TryGetChildren(rootItem.KeyValue, out var childrenResponse))
				{
					manager.Helpers.Log(nameof(NonLiveManagerTreeViewSection), nameof(InitNonRootFoldersAndFiles), $"Failed to retrieved children of root item with key: {rootItem.KeyValue}");
					continue;
				}

				parentItems.Add(rootItem);

				initializeValidator.InitializeNonRootFoldersCompleted = InitNonRootFolders(fileUrls, rootItem, folderUrls, childrenResponse);
			}

			manager.Helpers.Log(nameof(NonLiveManagerTreeViewSection), nameof(Initialize), $"Lazy load items after initialising non root folders and files: '{string.Join(";", lazyLoadedItems)}'");

			if (!initializeValidator.InitializeCompleted)
			{
				manager.Helpers.Log(nameof(NonLiveManagerTreeViewSection), nameof(Initialize), $"Initialisation failed, no folder or file could be found based on the saved urls of the order");

				// Unable to find folder or file.
				IsRequestSuccessful = folderUrls.All(url => url == null);
				ValidationState = UIValidationState.Invalid;
				ValidationText = $"Unable to retrieve {(Type == TreeViewType.FileSelector ? "file" : "folder")}: {string.Join(";", folderUrls)}";

				UpdateSelectedItemsTextBox();
			}

			foreach (var item in parentItems)
			{
				item.IsChecked = item.ItemType == TreeViewItem.TreeViewItemType.CheckBox && item.ChildItems.Count == 1;
			}
		}

		/// <summary>
		/// When the folder path is matching with the one which is saved among the current order then underlying files will be initialized.
		/// Files can be initialized among different parent folders. So this method can be called more than once.
		/// </summary>
		/// <param name="fileUrls">Saved file URLs.</param>
		/// <param name="parentItem">Current parent item.</param>
		/// <param name="folderUrls">Saved folder URLs.</param>
		/// <param name="childrenResponse">Current children response of parent folder item.</param>
		/// <returns>Returns true if the desired files could be selected based on the file urls which are saved inside the order.</returns>
		private bool InitFiles(string[] fileUrls, TreeViewItem parentItem, string[] folderUrls, NonLiveManagerResponse childrenResponse)
		{
			bool initializeCompleted = false;

			if (Type == TreeViewType.FileSelector && fileUrls.Any())
			{
				foreach (File file in childrenResponse.Files.Where(x => !String.IsNullOrWhiteSpace(x.DisplayName)).OrderBy(x => x.DisplayName).ToList())
				{
					file.Parent = parentItem.KeyValue; // parent item key value = folder.URL, setting parent is needed, parent can be null when retrieving the file directly from the element.

					bool pathMatches = folderUrls.Contains(parentItem.KeyValue);
					string fileKeyValue = parentItem.KeyValue + file.URL;
					string fullFilePath = $"{file.Parent}/{file.DisplayName}";

					parentItem.ChildItems.Add(new TreeViewItem(file.DisplayName, fileKeyValue)
					{
						ItemType = TreeViewItem.TreeViewItemType.CheckBox,
						SupportsLazyLoading = false,
						IsCollapsed = false,
						IsChecked = pathMatches && (fileUrls == null || fileUrls.Contains(fullFilePath)),
						CheckingBehavior = TreeViewItem.TreeViewItemCheckingBehavior.None
					});

					manager.Helpers.Log(nameof(NonLiveManagerTreeViewSection), nameof(InitFiles), $"file item added with name: {file.DisplayName} and path: {file.URL} among folder parent: Name={parentItem.DisplayValue}, path={parentItem.KeyValue}| {nameof(pathMatches)}={pathMatches} | {nameof(fullFilePath)}={fullFilePath}");

					if (!cachedFiles.ContainsKey(fileKeyValue)) cachedFiles.Add(fileKeyValue, file);

					if (pathMatches)
					{
						UpdateSelectedFiles();
						UpdateSelectedItemsTextBox();

						initializeCompleted = true;
					}
				}
			}

			return initializeCompleted;
		}

		/// <summary>
		/// Will initialize every child folder of the given parent item and will add it to the tree view component. Including his files when applicable.
		/// This method is using recursion to loop over every child until the path is matching, if no matching path was found tree view will be filled but will be invalid at the end.
		/// </summary>
		/// <param name="fileUrls">Saved file URLs.</param>
		/// <param name="parentItem">Current parent item.</param>
		/// <param name="folderUrls">Saved folder URLs.</param>
		/// <param name="childrenResponse">Current children response of parent folder item.</param>
		/// <returns>Returns true if the desired folders and files could be selected based on the folder and file urls which are saved inside the order.</returns>
		private bool InitNonRootFolders(string[] fileUrls, TreeViewItem parentItem, string[] folderUrls, NonLiveManagerResponse childrenResponse)
		{
			bool initializeCompleted = false;

			foreach (var folder in childrenResponse.Folders.OrderBy(x => x.FriendlyName).ToList())
			{
				bool pathMatches = folderUrls.Contains(folder.URL);
				bool isPathInChildBranch = folderUrls.Any(folderUrl => folderUrl != null && folderUrl.StartsWith(folder.URL));

				bool retrievingChildrenSucceeded = false;
				NonLiveManagerResponse childFolderResponse = null;
				if (pathMatches || isPathInChildBranch)
				{
					retrievingChildrenSucceeded = TryGetChildren(folder.URL, out childFolderResponse);
				}

				var newFolderParentTreeViewItem = CreateNewNonRootFolderItemWhileInitializing(fileUrls, folderUrls, folder, childFolderResponse);

				parentItem.ChildItems.Add(newFolderParentTreeViewItem);
				if (!newFolderParentTreeViewItem.IsCollapsed) lazyLoadedItems.Add(folder.URL);
				if (!cachedFolders.ContainsKey(folder.URL)) cachedFolders.Add(folder.URL, folder);

				if (pathMatches)
				{
					initializeCompleted = Type == TreeViewType.FolderSelector || fileUrls == null || !fileUrls.Any();
					SelectedFolder = folder;
					SelectedItemTextBox.Text = folder.URL;

					// When folder path is matching, initialize underlying folders and files if applicable.
					if (Type == TreeViewType.FileSelector)
					{
						initializeCompleted = InitNonRootFolders(fileUrls, newFolderParentTreeViewItem, folderUrls, childFolderResponse);
						initializeCompleted = InitFiles(fileUrls, newFolderParentTreeViewItem, folderUrls, childFolderResponse) || initializeCompleted;
					}
				}
				else if (isPathInChildBranch && retrievingChildrenSucceeded)
				{
					// Initialize children of current folder
					initializeCompleted = InitNonRootFolders(fileUrls, newFolderParentTreeViewItem, folderUrls, childFolderResponse) || initializeCompleted;
				}
			}

			return initializeCompleted;
		}

		private TreeViewItem CreateNewRootFolderItemWhileInitializing(string[] folderUrls, Folder rootFolder)
		{
			bool rootFolderPathMatchesArgument = folderUrls.Contains(rootFolder.URL);
			bool isPathInChildBranch = folderUrls.Any(folderUrl => folderUrl != null && folderUrl.StartsWith(rootFolder.URL));
			bool isCollapsed = !isPathInChildBranch || (rootFolderPathMatchesArgument && Type == TreeViewType.FolderSelector);

			var newRootFolderTreeViewItem = new TreeViewItem(rootFolder.FriendlyName, rootFolder.URL)
			{
				ItemType = TreeViewItem.TreeViewItemType.CheckBox,
				SupportsLazyLoading = true,
				IsCollapsed = isCollapsed,
				IsChecked = rootFolderPathMatchesArgument && Type == TreeViewType.FolderSelector,
				CheckingBehavior = TreeViewItem.TreeViewItemCheckingBehavior.None
			};

			manager.Helpers.Log(nameof(NonLiveManagerTreeViewSection), nameof(CreateNewRootFolderItemWhileInitializing), $"Root folder item added with name: {rootFolder.FriendlyName} and path: {rootFolder.URL} | {nameof(rootFolderPathMatchesArgument)}={rootFolderPathMatchesArgument}, {nameof(isPathInChildBranch)}={isPathInChildBranch}, {nameof(isCollapsed)}={isCollapsed}");

			return newRootFolderTreeViewItem;
		}

		private TreeViewItem CreateNewNonRootFolderItemWhileInitializing(string[] fileUrls, string[] folderUrls, Folder folder, NonLiveManagerResponse childFolderResponse)
		{
			bool pathMatches = folderUrls.Contains(folder.URL);
			bool isPathInChildBranch = folderUrls.Any(folderUrl => folderUrl != null && folderUrl.StartsWith(folder.URL));

			var matchingFileUrls = fileUrls?.Where(fileUrl => fileUrl.StartsWith(folder.URL)).ToArray();

			bool isCollapsed = (!isPathInChildBranch || pathMatches) && (fileUrls == null || !matchingFileUrls.Any());
			bool allFilesNeedToBeChecked = Type == TreeViewType.FileSelector && fileUrls != null && matchingFileUrls.Length == childFolderResponse?.Files.Length;
			bool canNewItemBeChecked = pathMatches && (Type == TreeViewType.FolderSelector || fileUrls == null || !fileUrls.Any() || allFilesNeedToBeChecked);

			var newFolderParentTreeViewItem = new TreeViewItem(folder.FriendlyName, folder.URL)
			{
				ItemType = TreeViewItem.TreeViewItemType.CheckBox,
				SupportsLazyLoading = true,
				IsCollapsed = isCollapsed,
				IsChecked = canNewItemBeChecked,
				CheckingBehavior = TreeViewItem.TreeViewItemCheckingBehavior.None
			};

			manager.Helpers.Log(nameof(NonLiveManagerTreeViewSection), nameof(InitNonRootFolders), $"folder item added with name: {folder.FriendlyName} and path: {folder.URL} | {nameof(pathMatches)}={pathMatches}, {nameof(isPathInChildBranch)}={isPathInChildBranch}, {nameof(isCollapsed)}={isCollapsed}");

			return newFolderParentTreeViewItem;
		}

		public override void RegenerateUi()
		{
			GenerateUi(out int row);
		}

		private sealed class InitializeValidator
		{
			public bool InitializeNonRootFoldersCompleted { get; set; }

			public bool FilePathMatchesAmongRootFolders { get; set; }

			public bool RootFolderPathMatching { get; set; }

			public bool InitializeCompleted => RootFolderPathMatching || FilePathMatchesAmongRootFolders || InitializeNonRootFoldersCompleted;
		}
	}

	public enum TreeViewType
	{
		FileSelector,
		FolderSelector
	}
}



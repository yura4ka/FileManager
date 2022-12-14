using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FileManager
{
	public partial class MainWindow : Window
	{
		private readonly FileSystem fs = new();
		private readonly TabController _leftTab;
		private readonly TabController _rigthTab;
		private Folder? _currentFolder = null;
		private Folder? CurrentFolder
		{
			get => _currentFolder;
			set
			{
				_currentFolder = value;
				CreateFileButton.IsEnabled = value != null;
				CreateFolderButton.IsEnabled = value != null;
			}
		}

		private bool HasSelected => _leftTab.HasSelected || _rigthTab.HasSelected;
		private bool HasBufferItems => fs.HasBufferItems;

		public MainWindow()
		{
			InitializeComponent();
			var root = fs.GetRoot();
			LeftTree.ItemsSource = root;
			RightTree.ItemsSource = root;

			_leftTab = new(LeftTree, LeftList, LeftStatus, LeftPath);
			_rigthTab = new(RightTree, RightList, RightStatus, RightPath);
		}

		private void SetToolButtons()
		{
			foreach (var b in ToolBar.Children.OfType<Button>())
				b.IsEnabled = HasSelected;
			PasteButton.IsEnabled = HasBufferItems;
			OpenInEditorButton.IsEnabled = GetCurrentSelection().OfType<File>().FirstOrDefault() != null;
			CreateFileButton.IsEnabled = CurrentFolder != null;
			CreateFolderButton.IsEnabled = CurrentFolder != null;
		}

		private void OnTreeItemClick(TabController tab)
		{
			Folder selected = (Folder)tab.Tree.SelectedItem;
			tab.SetListSourse(selected);
			tab.SetPath(selected);
			tab.History.Add(selected);
			CurrentFolder = selected;
		}

		private void LeftTreeItem_Click(object sender, MouseButtonEventArgs e)
		{
			OnTreeItemClick(_leftTab);
			e.Handled = true;
		}

		private void RightTreeItem_Click(object sender, MouseButtonEventArgs e)
		{
			OnTreeItemClick(_rigthTab);
			e.Handled = true;
		}

		private void Tree_Expanded(object sender, RoutedEventArgs e)
		{
			if (e.OriginalSource is not TreeViewItem item)
				return;
			((Folder)item.DataContext).TryInitializeChildren();
			foreach (FsItem i in item.Items)
			{
				if (i is Folder folder)
					folder.TryInitializeChildren();
			}
			item.Items.Refresh();
		}

		private void OnFolderDoubleClick(TabController tab, Folder folder)
		{
			tab.SetListSourse(folder);
			tab.History.Add(folder);
			tab.Path.Items.Add(folder);
			CurrentFolder = folder;
		}

		private void LeftItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (((DataGridRow)sender).Item is Folder folder)
				OnFolderDoubleClick(_leftTab, folder);
			else if (((DataGridRow)sender).Item is File file)
				Process.Start("explorer", "\"" + file.FullName + "\"");
		}

		private void RightItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (((DataGridRow)sender).Item is Folder folder)
				OnFolderDoubleClick(_rigthTab, folder);
			else if (((DataGridRow)sender).Item is File file)
				Process.Start("explorer", "\"" + file.FullName + "\"");
		}

		private void OnPathClick(TabController tab, Folder folder)
		{
			tab.SetListSourse(folder);
			tab.History.Add(folder);
			CurrentFolder = folder;
			var items = tab.Path.Items;
			int i = items.Count - 1;
			while (items[i] != folder)
				items.RemoveAt(i--);
		}

		private void LeftPath_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (((ListBoxItem)sender).Content is not Folder folder)
				return;

			OnPathClick(_leftTab, folder);
			((ListBoxItem)sender).IsSelected = false;
			e.Handled = true;
		}

		private void RightPath_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (((ListBoxItem)sender).Content is not Folder folder)
				return;

			OnPathClick(_leftTab, folder);
			((ListBoxItem)sender).IsSelected = false;
			e.Handled = true;
		}

		private void LeftList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_leftTab.List.SelectedItems.Count != 0)
			{
				fs.SelectedItem = (FsItem)_leftTab.List.SelectedItem;
				_rigthTab.List.UnselectAll();
			}
			SetToolButtons();
		}

		private void RightList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_rigthTab.List.SelectedItems.Count != 0)
			{
				fs.SelectedItem = (FsItem)_rigthTab.List.SelectedItem;
				_leftTab.List.UnselectAll();
			}

			SetToolButtons();
		}

		private IEnumerable<FsItem> GetCurrentSelection()
		{
			if (LeftList.SelectedItems.Count != 0)
				return LeftList.SelectedItems.Cast<FsItem>();
			else if (RightList.SelectedItems.Count != 0)
				return RightList.SelectedItems.Cast<FsItem>();
			return new List<FsItem>();
		}

		private void MoveButton_Click(object sender, RoutedEventArgs e)
		{
			fs.AddToBuffer(GetCurrentSelection());
			SetToolButtons();
		}

		private void CopyButton_Click(object sender, RoutedEventArgs e)
		{
			fs.AddToBuffer(GetCurrentSelection(), true);
			SetToolButtons();
		}

		private void RefreshLists()
		{
			_leftTab.RefreshList();
			_rigthTab.RefreshList();
		}

		private void RefreshTree(FsItem item, bool isParent = true)
		{
			_leftTab.RefreshTree(item, isParent);
			_rigthTab.RefreshTree(item, isParent);
		}

		private void RefreshAll(FsItem item)
		{
			_leftTab.RefreshAll(item);
			_rigthTab.RefreshAll(item);
		}

		private void PasteButton_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentFolder == null)
				return;
			if (!fs.MoveFromBuffer(CurrentFolder))
				return;
			RefreshLists();
			RefreshTree(CurrentFolder);
			SetToolButtons();
			if (fs.BufferSourseFile != null)
				RefreshTree(fs.BufferSourseFile);
		}

		private void RemoveButton_Click(object sender, RoutedEventArgs e)
		{
			List<FsItem> selection = GetCurrentSelection().ToList();

			if (selection.Count == 0)
				return;

			if (MessageBox.Show($"Ви впевнені, що хочете видалити назавжди {selection.Count} елементів",
				"File Manager",
				MessageBoxButton.OKCancel,
				MessageBoxImage.Exclamation)
			== MessageBoxResult.Cancel)
				return;
			FileSystem.RemoveItems(selection);
			if (CurrentFolder != null)
				RefreshAll(CurrentFolder);
		}

		private void RenameButton_Click(object sender, RoutedEventArgs e)
		{
			if (fs.SelectedItem != null && fs.RenameSelected())
				RefreshAll(fs.SelectedItem);
		}

		private void BackClick(TabController tab)
		{
			CurrentFolder = tab.OnBackClick();
		}

		private void ForwardClick(TabController tab)
		{
			CurrentFolder = tab.OnForwardClick();
		}

		private void LeftBack_Click(object sender, RoutedEventArgs e) => BackClick(_leftTab);
		private void LeftForward_Click(object sender, RoutedEventArgs e) => ForwardClick(_leftTab);
		private void RightBack_Click(object sender, RoutedEventArgs e) => BackClick(_rigthTab);
		private void RightForward_Click(object sender, RoutedEventArgs e) => ForwardClick(_rigthTab);

		private void OpenInEditorButton_Click(object sender, RoutedEventArgs e)
		{
			var selection = GetCurrentSelection().OfType<File>().ToList();
			if (selection.Count != 0)
				new TextEditor(selection[0].FullName).Show();
		}

		private void CreateFolderButton_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentFolder == null)
				return;
			var newFolder = FileSystem.CreateDirectory(CurrentFolder);
			if (newFolder != null)
				RefreshAll(newFolder);
		}

		private void CreateFileButton_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentFolder == null)
				return;
			var newFile = FileSystem.CreateFile(CurrentFolder);
			if (newFile != null)
				RefreshAll(newFile);
		}

		private void Refresh()
		{
			if (CurrentFolder == null)
				return;
			CurrentFolder.TryInitializeChildren();
			RefreshAll(CurrentFolder);
		}

		private void RefreshLeftButton_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentFolder == null)
				return;
			Refresh();
			_leftTab.SetListSourse(CurrentFolder);
		}

		private void RefreshRightButton_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentFolder == null)
				return;
			Refresh();
			_rigthTab.SetListSourse(CurrentFolder);
		}
	}
}

using System.Collections.Generic;
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

		public MainWindow()
		{
			InitializeComponent();
			var root = fs.GetRoot();
			LeftTree.ItemsSource = root;
			RightTree.ItemsSource = root;

			_leftTab = new(LeftTree, LeftList, LeftStatus, LeftPath);
			_rigthTab = new(RightTree, RightList, RightStatus, RightPath);
		}

		private static void OnTreeItemClick(TabController tab)
		{
			Folder selected = (Folder)tab.Tree.SelectedItem;
			tab.SetListSourse(selected);
			tab.SetPath(selected);
			tab.History.Add(selected);
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

			foreach (FsItem i in item.Items)
			{
				if (i is Folder folder)
					folder.TryInitializeChildren();
			}
		}

		private static void OnFolderDoubleClick(TabController tab, Folder folder)
		{
			tab.SetListSourse(folder);
			tab.History.Add(folder);
			tab.Path.Items.Add(folder);
		}

		private void LeftItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (((DataGridRow)sender).Item is Folder folder)
				OnFolderDoubleClick(_leftTab, folder);
		}

		private void RightItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (((DataGridRow)sender).Item is Folder folder)
				OnFolderDoubleClick(_rigthTab, folder);
		}

		private static void OnPathClick(TabController tab, Folder folder)
		{
			tab.SetListSourse(folder);
			tab.History.Add(folder);

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
				_rigthTab.List.UnselectAll();
		}

		private void RightList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_rigthTab.List.SelectedItems.Count != 0)
				_leftTab.List.UnselectAll();
		}
	}
}

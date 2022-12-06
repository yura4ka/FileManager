using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace FileManager
{
	class TabController
	{
		public FolderHistory History { get; private set; }
		public TreeView Tree { get; private set; }
		public DataGrid List { get; private set; }
		public TextBlock TotalItems { get; private set; }
		public TextBlock SelectedItems { get; private set; }
		public Button PathBack { get; private set; }
		public Button PathForward { get; private set; }
		public ListBox Path { get; private set; }

		public bool HasSelected => List.SelectedItems.Count > 0;

		public TabController(TreeView tree, DataGrid grid, StatusBar status, WrapPanel pathControll)
		{
			Tree = tree;
			List = grid;

			TotalItems = (TextBlock)status.Items.OfType<StatusBarItem>().First().Content;
			SelectedItems = (TextBlock)status.Items.OfType<StatusBarItem>().Last().Content;

			Path = pathControll.Children.OfType<ListBox>().First();
			var panel = pathControll.Children.OfType<StackPanel>().First();
			PathBack = panel.Children.OfType<Button>().First();
			PathForward = panel.Children.OfType<Button>().Last();

			History = new FolderHistory(PathBack, PathForward);

			grid.SelectionChanged += OnSelectionChange;
		}

		public void SetListSourse(Folder folder)
		{
			folder.TryInitializeChildren();
			List.ItemsSource = folder.Children;
			TotalItems.Text = $"{folder.Children.Count} елементів";
		}

		public void SetPath(Folder folder)
		{
			Path.Items.Clear();
			foreach (var i in folder.GetFullPathItems())
				Path.Items.Add(i);
		}

		public Folder OnBackClick()
		{
			var folder = History.Back();
			SetListSourse(folder);
			SetPath(folder);
			return folder;
		}

		public Folder OnForwardClick()
		{
			var folder = History.Forward();
			SetListSourse(folder);
			SetPath(folder);
			return folder;
		}

		private void OnSelectionChange(object sender, SelectionChangedEventArgs e)
		{
			int count = List.SelectedItems.Count;
			SelectedItems.Text = count == 0 ? "" : $"{count} вибрано";
		}

		public void RefreshAll(FsItem? item)
		{
			if (item == null)
				return;
			RefreshList();
			RefreshTree(item);
		}

		public void RefreshList()
		{
			List.Items.Refresh();
		}

		private TreeViewItem? GetTreeItem(FsItem item, bool isParent)
		{
			var pathToItem = item.GetFullPathItems();
			ItemsControl current = Tree;
			int counter = isParent ? 1 : 0;
			for (int i = 0; i < pathToItem.Count - counter; i++)
			{
				var p = pathToItem[i];
				TreeViewItem treeItem = (TreeViewItem)current
					.ItemContainerGenerator
					.ContainerFromItem(p);
				if (treeItem == null || !treeItem.IsExpanded)
					return null;
				current = treeItem;
			}
			if (current is TreeViewItem tvi)
				return tvi;
			return null;
		}

		private static void IterateTreeItem(TreeViewItem item, Dictionary<Folder, bool> dictionary)
		{
			bool setExpanded = dictionary.Count != 0;
			var stack = new Stack<TreeViewItem>();
			stack.Push(item);

			while (stack.Count != 0)
			{
				var currentItem = stack.Pop();
				foreach (var c in currentItem.Items)
				{
					TreeViewItem treeItem = (TreeViewItem)currentItem
					.ItemContainerGenerator
					.ContainerFromItem(c);
					if (treeItem == null || (!treeItem.IsExpanded && !setExpanded))
						continue;
					stack.Push(treeItem);
					if (setExpanded)
						treeItem.IsExpanded = dictionary.GetValueOrDefault((Folder)c);
					else
						dictionary.Add((Folder)c, treeItem.IsExpanded);
				}
			}
		}

		public void RefreshTree(FsItem item, bool refreshParent = true)
		{
			if (item is File)
				return;

			var toRefresh = GetTreeItem(item, refreshParent);
			if (toRefresh == null)
				return;

			var isExpanded = new Dictionary<Folder, bool>();
			IterateTreeItem(toRefresh, isExpanded);

			toRefresh.ItemsSource = ((Folder)(toRefresh.DataContext)).TreeView;
			toRefresh.IsExpanded = true;
			IterateTreeItem(toRefresh, isExpanded);
		}
	}
}

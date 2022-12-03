using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FileManager
{
	public partial class MainWindow : Window
	{
		private readonly FileSystem fs = new();
		private readonly FolderHistory _leftHistory;

		public MainWindow()
		{
			InitializeComponent();
			LeftTree.ItemsSource = fs.GetRoot();
			_leftHistory = new(LeftBack, LeftForward);
		}

		private static void SetPath(ListBox path, Folder folder)
		{
			path.Items.Clear();
			foreach (var i in folder.GetFullPathItems())
				path.Items.Add(i);
		}

		private static void SetListSourse(DataGrid list, TextBlock itemsCount, Folder folder)
		{
			folder.TryInitializeChildren();
			list.ItemsSource = folder.Children;
			itemsCount.Text = $"{folder.Children.Count} елементів";
		}

		private void TreeViewItem_MouseUp(object sender, MouseButtonEventArgs e)
		{
			Folder selected = (Folder)LeftTree.SelectedItem;
			SetListSourse(LeftList, LeftItems, selected);
			SetPath(LeftPath, selected);
			_leftHistory.Add(selected);
			e.Handled = true;
		}

		private void LeftTree_Expanded(object sender, RoutedEventArgs e)
		{
			if (e.OriginalSource is not TreeViewItem item)
				return;

			foreach (FsItem i in item.Items)
			{
				if (i is Folder folder)
					folder.TryInitializeChildren();
			}
		}

		private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (((DataGridRow)sender).Item is not Folder folder)
				return;

			SetListSourse(LeftList, LeftItems, folder);
			_leftHistory.Add(folder);
			LeftPath.Items.Add(folder);
		}

		private void LeftPath_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (((ListBoxItem)sender).Content is not Folder folder)
				return;

			SetListSourse(LeftList, LeftItems, folder);
			_leftHistory.Add(folder);

			var items = LeftPath.Items;
			int i = items.Count - 1;
			while (items[i] != folder)
				items.RemoveAt(i--);

			((ListBoxItem)sender).IsSelected = false;
			e.Handled = true;
		}

		private void LeftBack_Click(object sender, RoutedEventArgs e)
		{
			var folder = _leftHistory.Back();
			SetListSourse(LeftList, LeftItems, folder);
			SetPath(LeftPath, folder);
		}

		private void LeftForward_Click(object sender, RoutedEventArgs e)
		{
			var folder = _leftHistory.Forward();
			SetListSourse(LeftList, LeftItems, folder);
			SetPath(LeftPath, folder);
		}
	}
}

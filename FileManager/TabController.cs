using System.Linq;
using System.Windows;
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

			PathBack.Click += OnBackClick;
			PathForward.Click += OnForwardClick;
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

		private void OnBackClick(object sender, RoutedEventArgs e)
		{
			var folder = History.Back();
			SetListSourse(folder);
			SetPath(folder);
		}

		private void OnForwardClick(object sender, RoutedEventArgs e)
		{
			var folder = History.Forward();
			SetListSourse(folder);
			SetPath(folder);
		}

		private void OnSelectionChange(object sender, SelectionChangedEventArgs e)
		{
			int count = List.SelectedItems.Count;
			SelectedItems.Text = count == 0 ? "" : $"{count} вибрано";
		}
	}
}

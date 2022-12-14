using System.Windows;

namespace FileManager.Dialogs
{
	public partial class RenameFileDialog : Window
	{
		public string NewFileName { get; set; }
		public RenameFileDialog(string fileName, bool isNewFile = false)
		{
			InitializeComponent();
			DataContext = this;
			OldName.Text = fileName;
			NewFileName = fileName;
			if (isNewFile)
			{
				Title = "Create";
				Old.Visibility = Visibility.Collapsed;
				Confirm.Content = "Створити";
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			int indexOfDot = NewFileName.LastIndexOf('.');
			if (indexOfDot == -1)
				indexOfDot = NewFileName.Length;
			NewName.Focus();
			NewName.Select(0, indexOfDot);
		}

		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}
	}
}

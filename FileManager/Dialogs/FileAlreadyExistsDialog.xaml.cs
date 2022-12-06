using System.Windows;

namespace FileManager.Dialogs
{
	public partial class FileAlreadyExistsDialog : Window
	{
		public enum DialogResults
		{
			Rename,
			Skip,
			Replace,
		}

		public DialogResults Result { get; private set; } = DialogResults.Skip;

		public FileAlreadyExistsDialog(string? fileName = "")
		{
			InitializeComponent();
			FileName.Text = fileName;
		}

		private void Rename_Click(object sender, RoutedEventArgs e)
		{
			Result = DialogResults.Rename;
			DialogResult = true;
		}

		private void Skip_Click(object sender, RoutedEventArgs e)
		{
			Result = DialogResults.Skip;
			DialogResult = false;
		}

		private void Replace_Click(object sender, RoutedEventArgs e)
		{
			Result = DialogResults.Replace;
			DialogResult = true;
		}
	}
}

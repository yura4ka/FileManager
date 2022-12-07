using System.Linq;
using System.Windows;

namespace FileManager
{
	public partial class TextEditor : Window
	{
		public TextEditor(string path)
		{
			InitializeComponent();
			Editor.Text = System.IO.File.ReadAllLines(path).Aggregate((a, b) => $"{a}\n{b}");
			Editor.Focus();
			Editor.Select(0, 0);
		}

		private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
		{
			bool isSelectionEmpty = Editor.SelectedText.Length == 0;
			ChangeRegisterItem.IsEnabled = !isSelectionEmpty;
			if (isSelectionEmpty)
			{
				int row = Editor.GetLineIndexFromCharacterIndex(Editor.CaretIndex);
				int col = Editor.CaretIndex - Editor.GetCharacterIndexFromLineIndex(row);
				Position.Text = "Line " + (row + 1) + ", Column " + (col + 1);
				return;
			}
			Position.Text = Editor.SelectedText.Length.ToString() + " selected";
		}

		private void SentenseRegister_Click(object sender, RoutedEventArgs e)
		{

		}

		private void AllBig_Click(object sender, RoutedEventArgs e)
		{

		}

		private void AllSmall_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}

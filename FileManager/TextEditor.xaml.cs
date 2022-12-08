using FileManager.SaveController;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace FileManager
{
	public partial class TextEditor : Window
	{
		private bool _isSaved;
		private string _path;
		public TextEditor(string path)
		{
			InitializeComponent();
			_path = path;
			try
			{
				Editor.Text = System.IO.File.ReadAllText(path);
			}
			catch (Exception ex) 
			{ 
				FileSystem.ShowErrorMessage(ex.Message); 
				Close(); 
			}
			Title = Path.GetFileName(path) + " - Text Editor";
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Editor.Focus();
			Editor.Select(0, 0);
			_isSaved = true;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!ConfirmExit())
				e.Cancel = true;
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

		private void ChangeSelection(Func<string, string> func)
		{
			Editor.SelectedText = func(Editor.SelectedText);
		}

		private static int SkipToContent(string s, int startIndex = 0)
		{
			string spaces = " \n\r\t";
			while (startIndex < s.Length && spaces.Contains(s[startIndex]))
				startIndex++;
			return startIndex;
		}

		private void SentenseRegister_Click(object sender, RoutedEventArgs e)
		{
			ChangeSelection(s =>
			{
				var builder = new StringBuilder();
				char[] separators = { '.', '!', '?' };
				int start = 0;
				int index = s.IndexOfAny(separators, start);

				while (index != -1)
				{
					index = SkipToContent(s, index + 1);
					string sentence = start + 1 < index ? s[++start..index] : "";
					builder.Append(s[start - 1].ToString().ToUpper() + sentence.ToLower());
					start = index;
					index = s.IndexOfAny(separators, start);
				}

				if (start < s.Length)
				{
					string sentence = ++start < s.Length ? s[start..] : "";
					builder.Append(s[start - 1].ToString().ToUpper() + sentence.ToLower());
				}

				return builder.ToString();
			});
		}

		private void AllBig_Click(object sender, RoutedEventArgs e)
		{
			ChangeSelection(s => s.ToUpper());
		}

		private void AllSmall_Click(object sender, RoutedEventArgs e)
		{
			ChangeSelection(s => s.ToLower());
		}

		private void CamelCase_Click(object sender, RoutedEventArgs e)
		{
			ChangeSelection(s =>
			{
				var builer = new StringBuilder(s);
				char[] separators = { ' ', '\n', '\r', '\t' };
				int index = s.IndexOfAny(separators);
				while (++index != 0 && index < s.Length)
				{
					if (separators.Any(c => c == s[index]))
					{
						index = SkipToContent(s, index) - 1;
						continue;
					}
					builer[index] = builer[index].ToString().ToUpper()[0];
					index = s.IndexOfAny(separators, index + 1);
				}

				return builer.ToString();
			});
		}

		private void Editor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			_isSaved = false;
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			if (!Path.IsPathRooted(_path))
			{
				 SaveAs_Click(sender, e);
				 return;
			}
			try
			{
				System.IO.File.WriteAllText(_path, Editor.Text, Encoding.UTF8);
				_isSaved = true;
			}
			catch (Exception ex)
			{
				FileSystem.ShowErrorMessage(ex.Message);
			}
		}

		private void SetNewPath(bool? result, string newPath)
		{
			if (result == false)
				FileSystem.ShowErrorMessage("Не вдалося зберегти файл.");
			if (result == true)
			{
				_isSaved = true;
				_path = newPath;
				Title = Path.GetFileName(newPath) + " - Text Editor";
			}
		}

		private void SaveAs_Click(object sender, RoutedEventArgs e)
		{
			(var result, string newPath) = new FileSaverCreator()
				.CreateSaver(SaverCreator.SaverTypes.SIMPLE)
				.SaveFile(_path, Editor.Text);
			SetNewPath(result, newPath);
		}

		private void HtmlSave_Click(object sender, RoutedEventArgs e)
		{
			(var result, string newPath) = new FileSaverCreator()
				.CreateSaver(SaverCreator.SaverTypes.HTML)
				.SaveFile(_path, Editor.Text);
			SetNewPath(result, newPath);
		}

		private void BinSave_Click(object sender, RoutedEventArgs e)
		{
			(var result, string newPath) = new FileSaverCreator()
				.CreateSaver(SaverCreator.SaverTypes.BINARY)
				.SaveFile(_path, Editor.Text);
			SetNewPath(result, newPath);
			try { Editor.Text = System.IO.File.ReadAllText(newPath); }
			catch (Exception ex) { FileSystem.ShowErrorMessage(ex.Message); }
			_isSaved = true;
		}

		private bool ConfirmExit()
		{
			if (_isSaved)
				return true;
			var result = MessageBox.Show(
				"Ви впевнені, що хочете вийти. Незбережені дані будуть видалені!",
				"Text Editor",
				MessageBoxButton.OKCancel,
				MessageBoxImage.Exclamation);
			return result == MessageBoxResult.OK;
		}

		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void New_Click(object sender, RoutedEventArgs e)
		{
			if (!ConfirmExit())
				return;
			Editor.Text = "";
			SetNewPath(true, "New Document");
		}

		private void Open_Click(object sender, RoutedEventArgs e)
		{
			if (!ConfirmExit())
				return;
			try
			{
				var dialog = new OpenFileDialog
				{
					DefaultExt = "txt",
					Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
				};
				if (dialog.ShowDialog() != true) 
					return;
				string text = System.IO.File.ReadAllText(dialog.FileName);
				Editor.Text = text;
				SetNewPath(true, dialog.FileName);
			}
			catch(Exception ex)
			{
				FileSystem.ShowErrorMessage(ex.Message);
			}
		}
	}
}

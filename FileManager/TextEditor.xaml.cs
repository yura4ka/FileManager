using FileManager.Dialogs;
using FileManager.SaveController;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;
using WF = System.Windows.Forms;
using VB = Microsoft.VisualBasic.FileIO;

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
			string extension = Path.GetExtension(path);
			if (extension == ".html" || extension == ".htm")
				HtmlMenu.Visibility = Visibility.Visible;
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
			MoveHtml.IsEnabled = !isSelectionEmpty;
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

		private void SentenseRegister_Click(object sender, RoutedEventArgs e)
		{
			ChangeSelection(s => EditorUtils.ToSentenceRegister(s));
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
			ChangeSelection(s => EditorUtils.ToCamelCase(s));
		}

		private void Trim_Click(object sender, RoutedEventArgs e)
		{
			Editor.Text = EditorUtils.TrimText(Editor.Text);
		}

		private void Editor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			_isSaved = false;
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
				string extension = Path.GetExtension(newPath);
				if (extension == ".html" || extension == ".htm")
					HtmlMenu.Visibility = Visibility.Visible;
				else
					HtmlMenu.Visibility = Visibility.Collapsed;
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

		private void NewCommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			if (!ConfirmExit())
				return;
			Editor.Text = "";
			SetNewPath(true, "New Document");
		}

		private void OpenCommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
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
			catch (Exception ex)
			{
				FileSystem.ShowErrorMessage(ex.Message);
			}
		}

		private void SaveCommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
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

		private void GetImageList_Click(object sender, RoutedEventArgs e)
		{
			var images = EditorUtils.GetImagesFromHtml(Editor.Text);
			if (images.Count == 0)
				FileSystem.ShowErrorMessage("Не знайдено жодного посилання");
			else
				new ImagesFromHtml(images, Path.GetDirectoryName(_path) ?? "").Show();
		}

		private void MoveHtml_Click(object sender, RoutedEventArgs e)
		{
			(var result, _) = new FileSaverCreator()
				.CreateSaver(SaverCreator.SaverTypes.HTML)
				.SaveFile(_path, Editor.SelectedText);
			if (result != true)
				return;
			var images = EditorUtils.GetImagesFromHtml(Editor.Text);
			if (images.Count == 0)
				return;

			string parent = Path.GetDirectoryName(_path) ?? "";
			using var dialog = new WF.FolderBrowserDialog
			{
				Description = "Виберіть папку для графічних файлів",
				UseDescriptionForTitle = true,
				SelectedPath = parent,
			};

			if (dialog.ShowDialog() != WF.DialogResult.OK)
				return;
			foreach (var i in images)
			{
				(string path, bool isPath) = EditorUtils.MakePathAbsoulute(i, parent);
				if (!isPath || !System.IO.File.Exists(path))
					continue;
				VB.FileSystem.CopyFile(path,
					Path.Combine(dialog.SelectedPath, Path.GetFileName(path)),
					VB.UIOption.AllDialogs,
					VB.UICancelOption.DoNothing);
			}

		}
	}
}

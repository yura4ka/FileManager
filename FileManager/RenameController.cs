using FileManager.Dialogs;
using System;
using System.IO;
using FS = Microsoft.VisualBasic.FileIO.FileSystem;

namespace FileManager
{
	class RenameController
	{
		public static void ShowAlreadyExists() => FileSystem.ShowErrorMessage("Файл із таким ім'ям вже існує");
		public static void ShowNameWrongFormat() => FileSystem.ShowErrorMessage("Нове ім'я має невірний формат");

		public static string? AskNewName(string oldName, bool newFile = false)
		{
			var dialog = new RenameFileDialog(oldName, newFile);
			if (dialog.ShowDialog() != true)
				return null;
			return dialog.NewFileName.Trim();
		}

		public static string? AskNewName(FsItem item, bool newFile = false)
		{
			return AskNewName(item.Name, newFile);
		}

		private static bool? TryRenameFile(FsItem item, string newName)
		{
			try
			{
				if (item is File)
					FS.RenameFile(item.FullName, newName);
				else
					FS.RenameDirectory(item.FullName, newName);
				return true;
			}
			catch (ArgumentException)
			{
				ShowNameWrongFormat();
				return null;
			}
			catch (Exception ex)
			{
				FileSystem.ShowErrorMessage(ex.Message);
				return false;
			}
		}

		public static bool Rename(FsItem? item)
		{
			if (item == null)
				return false;

			while (true)
			{
				var newName = AskNewName(item);
				if (newName == null || newName.Length == 0)
					return false;

				if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
				{
					ShowNameWrongFormat();
					continue;
				}

				if (item.HasSiblingWithName(newName))
				{
					ShowAlreadyExists();
					continue;
				}
				if (newName == item.Name)
					return false;

				bool? result = TryRenameFile(item, newName);
				if (result == null)
					continue;
				item.Rename(newName);
				return result.Value;
			}
		}
	}
}

using FileManager.Dialogs;
using System;
using FS = Microsoft.VisualBasic.FileIO.FileSystem;

namespace FileManager
{
	class RenameController
	{
		public static void ShowAlreadyExists() => FileSystem.ShowErrorMessage("Файл із таким ім'ям вже існує");
		public static void ShowNameWrongFormat() => FileSystem.ShowErrorMessage("Нове ім'я має невірний формат");

		public static string? AskNewName(FsItem item)
		{
			var dialog = new RenameFileDialog(item.Name);
			if (dialog.ShowDialog() != true)
				return null;
			return dialog.NewFileName;
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
				if (newName == null)
					return false;

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

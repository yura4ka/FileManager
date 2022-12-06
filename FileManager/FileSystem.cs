using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Windows;
using FileManager.Dialogs;

using FS = Microsoft.VisualBasic.FileIO.FileSystem;

namespace FileManager
{
	class FileSystem
	{
		private readonly Folder[] _root;
		private readonly MoveController _mc;

		public ImmutableArray<Folder> GetRoot() => _root.ToImmutableArray();
		public FsItem? SelectedItem { get; set; }
		public FsItem? BufferSourseFile { get; private set; }
		public bool HasBufferItems => _mc.HasBufferItems;

		public FileSystem()
		{
			_mc = new();
			var _drives = DriveInfo.GetDrives();
			_root = new Folder[_drives.Length];

			for (int i = 0; i < _drives.Length; i++)
				_root[i] = new Folder(new FileData(_drives[i]), null, true);
		}

		public void AddToBuffer(IEnumerable<FsItem> items, bool isCopy = false)
		{
			_mc.AddToBuffer(items, isCopy);
			BufferSourseFile = items.FirstOrDefault()?.GetParent();
		}

		public bool RenameSelected() => Rename(SelectedItem);

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
				ShowErrorMessage(ex.Message);
				return false;
			}
		}

		private static bool Rename(FsItem? item)
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

		public static void ShowErrorMessage(string message)
		{
			MessageBox.Show(message, "File Manager", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		public static void ShowAlreadyExists() => ShowErrorMessage("Файл із таким ім'ям вже існує");

		public static void ShowNameWrongFormat() => ShowErrorMessage("Нове ім'я має невірний формат");

		public bool MoveFromBuffer(Folder destination)
		{
			var moveResult = _mc.TryMove(destination);

			while (moveResult.Result != MoveResult.Results.Success)
			{
				if (moveResult.Result == MoveResult.Results.AlreadyExists)
				{
					var dialog = new FileAlreadyExistsDialog(moveResult.Item?.Name);
					dialog.ShowDialog();
					switch (dialog.Result)
					{
						case FileAlreadyExistsDialog.DialogResults.Skip:
							moveResult = _mc.ContinueSkipOne();
							break;
						case FileAlreadyExistsDialog.DialogResults.Replace:
							moveResult = _mc.Continue(true);
							break;
						case FileAlreadyExistsDialog.DialogResults.Rename:
							moveResult = _mc.ContinueWithRename();
							break;
					}
				}
				else if (moveResult.Result == MoveResult.Results.SubFolderError
					|| moveResult.Result == MoveResult.Results.Error)
				{
					ShowErrorMessage($"Помилка під час копіювання {moveResult.Item?.Name}\n{moveResult.Message}");
					return false;
				}
			}

			return true;
		}

		public static void RemoveItems(IEnumerable<FsItem> items)
		{
			foreach (var item in items)
			{
				try
				{
					if (item is File)
						System.IO.File.Delete(item.FullName);
					else
						Directory.Delete(item.FullName, true);
					item.Remove();
				}
				catch (Exception ex)
				{
					ShowErrorMessage($"Помилка при видаленні ${item.Name}\n" + ex.Message);
				}
			}
		}
	}
}

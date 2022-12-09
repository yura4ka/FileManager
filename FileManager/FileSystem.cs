using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Windows;
using FileManager.Dialogs;

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

		public bool RenameSelected() => RenameController.Rename(SelectedItem);

		public static void ShowErrorMessage(string message)
		{
			MessageBox.Show(message, "File Manager", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		public bool MoveFromBuffer(Folder destination)
		{
			var moveResult = _mc.TryMove(destination);

			while (moveResult.Result != MoveResult.Results.Success)
			{
				if (moveResult.Result == MoveResult.Results.AlreadyExists)
				{
					var dialog = new FileAlreadyExistsDialog(moveResult.Item?.Name);
					if (dialog.ShowDialog() != true)
					{
						moveResult = _mc.Cancel();
						continue;
					}
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
				else if (moveResult.Result == MoveResult.Results.SubFolderError)
				{
					ShowErrorMessage($"Помилка! {moveResult.Item?.Name}\n{moveResult.Message}");
					moveResult = _mc.ContinueSkipOne();
				}
				else if (moveResult.Result == MoveResult.Results.Error)
				{
					ShowErrorMessage($"Помилка! {moveResult.Item?.Name}\n{moveResult.Message}");
					return false;
				}
				else if (moveResult.Result == MoveResult.Results.Cancel)
					return true;
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

		private static bool ValidateNewName(string? name, Folder parent)
		{
			if (name == null || name.Length == 0)
				return false;
			if (parent.HasChildWithName(name))
			{
				RenameController.ShowAlreadyExists();
				return false;
			}
			if (name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
			{
				RenameController.ShowNameWrongFormat();
				return false;
			}
			return true;
		}

		private static FsItem? CreateNewItem(Folder parent, bool isFolder)
		{
			var name = RenameController.AskNewName("", true);
			if (!ValidateNewName(name, parent))
				return null;
			try
			{
				string path = Path.GetFullPath($"{parent.FullName}/{name}");
				if (isFolder) Directory.CreateDirectory(path);
				else System.IO.File.Create(path);
				var info = new FileData(new FileInfo(path));
				FsItem item = isFolder ? new Folder(info, parent) : new File(info, parent);
				parent.Children.Add(item);
				return item;
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex.Message);
				return null;
			}
		}

		public static Folder? CreateDirectory(Folder parent)
		{
			return (Folder?)CreateNewItem(parent, true);
		}

		public static File? CreateFile(Folder parent)
		{
			return (File?)CreateNewItem(parent, false);
		}
	}
}

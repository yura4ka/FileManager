using System;
using System.Collections.Generic;
using System.IO;
using FS = Microsoft.VisualBasic.FileIO.FileSystem;

namespace FileManager
{
	class MoveController
	{
		private List<FsItem> _buffer = new();
		private int _bufferIndex = 0;
		private Folder? _destination = null;
		private bool? _isCopy;

		public bool HasBufferItems => _buffer.Count > 0;

		public void AddToBuffer(IEnumerable<FsItem> selection, bool isCopy = false)
		{
			_buffer = new List<FsItem>(selection);
			_bufferIndex = 0;
			_isCopy = isCopy;
		}

		private void DoAction(FsItem item, string path, bool replace)
		{
			if (_isCopy == null)
				throw new Exception();
			if (item is Folder folder)
			{
				if (_isCopy == true) FS.CopyDirectory(folder.FullName, path, replace);
				else FS.MoveDirectory(folder.FullName, path, replace);
				return;
			}

			if (_isCopy == true) FS.CopyFile(item.FullName, path, replace);
			else FS.MoveFile(item.FullName, path, replace);
		}

		private MoveResult StartAction(bool replace = false)
		{
			if (_destination == null)
				throw new Exception();

			try
			{
				for (; _bufferIndex < _buffer.Count; _bufferIndex++)
				{
					var item = _buffer[_bufferIndex];
					if (_isCopy == false && item.GetParent() == _destination)
						continue;
					if (!replace && _destination.HasChildWithName(item.Name))
						return new(MoveResult.Results.AlreadyExists, item);

					string newPath = Path.GetFullPath($"{_destination.FullName}/{item.Name}");
					if (item is Folder f
						&& (_destination == f || _destination.IsAncestorOf(f)))
					{
						return new(MoveResult.Results.SubFolderError, f);
					}

					DoAction(item, newPath, replace);
					var toReplace = _destination.GetChildrentByName(item.Name);
					if (replace && toReplace != null)
						toReplace.Remove();
					if (_isCopy == true) item.CloneToFolder(_destination);
					else item.MoveToFolder(_destination);
					replace = false;
				}
			}
			catch (Exception ex)
			{
				End();
				return new(MoveResult.Results.Error, _buffer[_bufferIndex], ex.Message);
			}

			End();
			return new(MoveResult.Results.Success);
		}

		public MoveResult TryMove(Folder destination)
		{
			if (_bufferIndex != 0)
				throw new Exception();
			_destination = destination;
			return StartAction();
		}

		public MoveResult Continue(bool replace = false)
		{
			return StartAction(replace);
		}

		public MoveResult ContinueSkipOne()
		{
			_bufferIndex++;
			return StartAction();
		}

		public MoveResult ContinueWithRename()
		{
			if (_destination == null)
				throw new Exception();

			var current = _buffer[_bufferIndex];
			var toRename = current.CloneToFolder(_destination);

			while (true)
			{
				var newName = RenameController.AskNewName(toRename);
				if (newName == null || newName.Length == 0)
				{
					toRename.Remove();
					return ContinueSkipOne();
				}

				if (newName == toRename.Name || toRename.HasSiblingWithName(newName))
				{
					RenameController.ShowAlreadyExists();
					continue;
				}

				if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
				{
					RenameController.ShowNameWrongFormat();
					continue;
				}

				toRename.Rename(newName);
				break;
			}

			try
			{
				string newPath = Path.GetFullPath($"{_destination.FullName}/{toRename.Name}");
				DoAction(current, newPath, false);
			}
			catch (Exception ex)
			{
				End();
				return new(MoveResult.Results.Error, current, ex.Message);
			}

			if (_isCopy == false)
				current.Remove();

			var result = ContinueSkipOne();
			return result;
		}

		public MoveResult Cancel()
		{
			End();
			return new MoveResult(MoveResult.Results.Cancel);
		}

		private void End()
		{
			_bufferIndex = 0;
			_destination = null;
			if (_isCopy == false)
				_buffer.Clear();
			_isCopy = null;
		}
	}

	class MoveResult
	{
		public enum Results
		{
			Success,
			AlreadyExists,
			SubFolderError,
			Error,
			Cancel,
		}

		public FsItem? Item { get; private set; }
		public Results Result { get; private set; }
		public string Message { get; private set; }

		public MoveResult(Results result, FsItem? item = null, string message = "")
		{
			Item = item;
			Result = result;
			if (message == "" && result == Results.SubFolderError)
				message = "Папка призначення є вкладеною папкою вихідної папки";
			Message = message;
		}
	}
}

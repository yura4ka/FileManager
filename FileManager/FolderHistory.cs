using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FileManager
{
	class FolderHistory
	{
		private const int MAX_COUNT = 10;
		private readonly LinkedList<Folder> _backFolders = new();
		private readonly LinkedList<Folder> _forwardFolders = new();
		private readonly Button _backButton;
		private readonly Button _forwardButton;

		public FolderHistory(Button backButton, Button forwardButton)
		{
			_backButton = backButton;
			_forwardButton = forwardButton;
		}

		private void SetButtons()
		{
			_backButton.IsEnabled = _backFolders.Count > 1;
			_forwardButton.IsEnabled = _forwardFolders.Count > 0;
		}

		private static void AddFolder(Folder folder, LinkedList<Folder> to)
		{
			to.AddLast(folder);
			if (to.Count > MAX_COUNT)
				to.RemoveFirst();
		}

		public void Add(Folder folder)
		{
			AddFolder(folder, _backFolders);
			_forwardFolders.Clear();
			SetButtons();
		}

		public Folder Back()
		{
			var toForward = _backFolders.Last();
			_backFolders.RemoveLast();
			AddFolder(toForward, _forwardFolders);
			SetButtons();
			return _backFolders.Last();
		}

		public Folder Forward()
		{
			var toBack = _forwardFolders.Last();
			_forwardFolders.RemoveLast();
			AddFolder(toBack, _backFolders);
			SetButtons();
			return toBack;
		}

		public void ClearForward()
		{
			_forwardFolders.Clear();
			SetButtons();
		}
	}
}

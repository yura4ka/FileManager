using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace FileManager
{
	class Folder : FsItem
	{
		private bool _isChildrenInitialized = false;

		public ObservableCollection<FsItem> Children { get; protected set; }
		public FsItem[] TreeView => Children.Where(i => i is Folder).ToArray();

		public Folder(FileData data, Folder? parent = null, bool isFillFolder = false) : base(data, parent) 
		{
			Children = new();

			if (isFillFolder)
				InitializeChildren();
		}

		private void InitializeChildren()
		{
			var items = Directory.GetFileSystemEntries(FullName);
			foreach (var item in items)
			{
				var info = new FileInfo(item);
				if (info.Attributes.HasFlag(FileAttributes.Hidden)) 
					continue;
				if (info.Attributes.HasFlag(FileAttributes.Directory))
					Children.Add(new Folder(new FileData(info), this));
				else
					Children.Add(new File(new FileData(info), this));

			}
			_isChildrenInitialized = true;
		}

		public bool TryInitializeChildren()
		{
			if (_isChildrenInitialized) 
				return false;
			InitializeChildren();
			return true;
		}

		public List<Folder> GetFullPathItems()
		{
			var result = new List<Folder>() { this };
			var currentparrent = _parent;

			while (currentparrent != null)
			{
				result.Add(currentparrent);
				currentparrent = currentparrent._parent;
			}

			result.Reverse();
			return result;
		}
	}
}

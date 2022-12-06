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
		public ObservableCollection<FsItem> TreeView => new(Children.Where(i => i is Folder).ToArray());

		public Folder(FileData data, Folder? parent = null, bool isFillFolder = false) : base(data, parent)
		{
			Children = new();

			if (isFillFolder)
				InitializeChildren();
		}

		private Folder(Folder folder) : base(folder)
		{
			Children = new();
			_isChildrenInitialized = folder._isChildrenInitialized;
			foreach (var c in folder.Children)
				Children.Add(c.Clone());
		}

		public override Folder Clone() => new(this);

		public override void Rename(string name)
		{
			base.Rename(name);
			foreach (var c in Children)
				c.UpdateFullName();
		}

		private void InitializeChildren()
		{
			var query =
				from e in Directory.GetFileSystemEntries(FullName)
				let i = new FileInfo(e)
				where !i.Attributes.HasFlag(FileAttributes.Hidden)
				select i;
			var infos = query.ToList();

			for (int i = 0; i < Children.Count; i++)
			{
				var c = Children[i];
				var info = infos.Where(info => c.FullName == info.FullName).FirstOrDefault();
				if (info == null)
					Children.RemoveAt(i);
				else
					c.UpdateInfo(info);
				infos.Remove(info);
			}

			foreach (var info in infos)
			{

				if (info.Attributes.HasFlag(FileAttributes.Directory))
					Children.Add(new Folder(new FileData(info), this));
				else
					Children.Add(new File(new FileData(info), this));

			}
			_isChildrenInitialized = true;
		}

		public bool TryInitializeChildren()
		{
			InitializeChildren();
			return true;
		}

		public override List<Folder> GetFullPathItems()
		{
			var result = base.GetFullPathItems();
			result.Add(this);
			return result;
		}

		public bool IsAncestorOf(Folder folder)
		{
			var currentParent = _parent;
			while (currentParent != null)
			{
				if (currentParent == folder)
					return true;
				currentParent = currentParent?._parent;
			}

			return false;
		}

		public bool HasChildWithName(string name) => Children.Any(c => c.Name == name);

		public FsItem? GetChildrentByName(string name) => Children.FirstOrDefault(c => c.Name == name);
	}
}

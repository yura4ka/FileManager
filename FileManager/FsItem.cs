using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileManager
{
	abstract class FsItem
	{
		protected Folder? _parent;
		public FileData Info { get; protected set; }
		public string FullName { get; protected set; }
		public string Name { get => Info.Name; protected set => Info.Name = value; }

		public FsItem(FileData data, Folder? parent = null)
		{
			Info = data;
			FullName = parent == null ? Name : Path.GetFullPath(parent?.FullName + "/" + Name);
			_parent = parent;
		}

		protected FsItem(FsItem item)
		{
			Info = item.Info.Clone();
			FullName = item.FullName;
			_parent = item._parent;
		}

		public abstract FsItem Clone();

		public virtual FsItem CloneToFolder(Folder folder)
		{
			var newFile = Clone();
			folder.Children.Add(newFile);
			newFile._parent = folder;
			newFile.UpdateFullName();
			return newFile;
		}

		public virtual void MoveToFolder(Folder folder)
		{
			_parent?.Children.Remove(this);
			_parent = folder;
			folder.Children.Add(this);
			UpdateFullName();
		}

		public bool HasSiblingWithName(string name)
		{
			return _parent?.Children.Any(i => i.Name == name) ?? false;
		}

		public virtual void Rename(string name)
		{
			Name = name;
			FullName = FullName.Remove(FullName.LastIndexOf('\\') + 1) + name;
		}

		public void UpdateFullName()
		{
			List<string> names = new() { Name };
			var currentParent = _parent;
			while (currentParent != null)
			{
				names.Add(currentParent.Name);
				currentParent = currentParent._parent;
			}

			names.Reverse();
			FullName = names.Aggregate((a, b) => Path.GetFullPath($"{a}/{b}"));
		}

		public virtual List<Folder> GetFullPathItems()
		{
			var result = new List<Folder>();
			var currentparrent = _parent;

			while (currentparrent != null)
			{
				result.Add(currentparrent);
				currentparrent = currentparrent._parent;
			}

			result.Reverse();
			return result;
		}

		public virtual void Remove()
		{
			_parent?.Children.Remove(this);
		}

		public Folder? GetParent() => _parent;

		public void UpdateInfo(FileInfo info)
		{
			Info = new FileData(info);
		}
	}
}

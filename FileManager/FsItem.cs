using System.Collections.Generic;
using System.IO;

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
	}
}

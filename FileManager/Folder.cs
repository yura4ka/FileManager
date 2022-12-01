using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
	public class Folder : FsItem
	{
		private bool _isChildrenInitialized = false;

		public List<FsItem> Children { get; protected set; }
		public FsItem[] TreeView => Children.Where(i => i is Folder).ToArray();

		public Folder(string name, Folder? parent = null, bool isFillFolder = false) : base(name, parent) 
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
					Children.Add(new Folder(info.Name, this));
				else
					Children.Add(new File(info.Name, this));

			}
			_isChildrenInitialized = true;
		}
	}
}

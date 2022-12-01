using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
	abstract public class FsItem
	{
		protected Folder? _parent = null;
		public string Name { get; protected set; }
		public string FullName { get; protected set; }

		public FsItem(string name, Folder? parent = null)
		{
			Name = name;
			FullName = parent?.FullName + name;
			_parent = parent;
		}
	}
}

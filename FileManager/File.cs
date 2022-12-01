using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
	class File : FsItem
	{
		public File(string name, Folder? parent = null) : base(name, parent) {}

	}
}

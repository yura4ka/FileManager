using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
	class FileSystem
	{
		private DriveInfo[] _drives = DriveInfo.GetDrives();
		private Folder[] _root;		
		public FileSystem()
		{
			_root = new Folder[_drives.Length];
			for (int i = 0; i < _drives.Length; i++)
				_root[i] = new Folder(_drives[i].Name, null, true);

		}

		public ImmutableArray<Folder> GetRoot() => _root.ToImmutableArray();
	}
}

using System.Collections.Immutable;
using System.IO;

namespace FileManager
{
	class FileSystem
	{
		private Folder[] _root;

		public FileSystem()
		{
			var _drives = DriveInfo.GetDrives();
			_root = new Folder[_drives.Length];

			for (int i = 0; i < _drives.Length; i++)
				_root[i] = new Folder(new FileData(_drives[i]), null, true);
		}

		public ImmutableArray<Folder> GetRoot() => _root.ToImmutableArray();
	}
}

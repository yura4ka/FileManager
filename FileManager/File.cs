namespace FileManager
{
	class File : FsItem
	{
		public File(FileData data, Folder? parent = null) : base(data, parent) { }

		private File(File file) : base(file) { }

		public override File Clone() => new(this);
	}
}

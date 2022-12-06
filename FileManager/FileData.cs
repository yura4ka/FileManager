using System.IO;
using System.Windows.Media.Imaging;

namespace FileManager
{
	class FileData
	{
		public string Name { get; set; }
		public string DateModified { get; set; }
		public string Type { get; set; }
		public string Size { get; set; }
		public BitmapSource Image { get; set; }

		public FileData(DriveInfo drive)
		{
			Name = drive.Name;
			DateModified = "-";
			Type = "Drive";
			Size = ConvertSize(drive.TotalSize);
			Image = Icons.DriveImage;
		}

		public FileData(FileInfo info)
		{
			bool isDirectory = Directory.Exists(info.FullName);

			Name = info.Name;
			DateModified = info.LastWriteTime.ToString();

			if (isDirectory)
			{
				Type = "Папка файлів";
				Size = "-";
				Image = Icons.FolderImage;
				return;
			}

			Type = info.Extension != "" ? info.Extension[1..].ToUpper() : "Файл";
			Size = ConvertSize(info.Length);
			Image = Icons.FromFile(info);
		}

		private static string ConvertSize(long size)
		{
			if (size > 1e9) return (size / 1e9).ToString("N2") + "GB";
			if (size > 1e6) return (size / 1e6).ToString("N2") + "MB";
			if (size > 1e3) return (size / 1e3).ToString("N2") + "KB";
			return size.ToString() + "B";
		}

		private FileData(FileData data)
		{
			Name = data.Name;
			DateModified = data.DateModified;
			Type = data.Type;
			Size = data.Size;
			Image = data.Image;
		}

		public FileData Clone() => new(this);
	}
}
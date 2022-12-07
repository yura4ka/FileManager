using Microsoft.Win32;
using System;
using System.IO;
using System.Text;

namespace FileManager.SaveController
{
	class BinaryFileSaver : FileSaver
	{
		public override (bool?, string) SaveFile(string path, string text)
		{
			try
			{
				var dialog = new SaveFileDialog
				{
					FileName = Path.GetFileNameWithoutExtension(path) + ".bin",
					DefaultExt = "bin",
					Filter = "Binary files (*.bin)|*.bin"
				};
				bool? result = dialog.ShowDialog();
				if (result != true)
					return (null, "");

				if (Path.GetExtension(dialog.FileName) != ".bin")
					dialog.FileName += ".bin";

				System.IO.File.WriteAllText(dialog.FileName, text.ToString());
				return (true, dialog.FileName);
			}
			catch
			{
				return (false, "");
			}
		}
	}
}

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
				var sb = new StringBuilder();
				foreach (char c in text.ToCharArray())
					sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));

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

				System.IO.File.WriteAllText(dialog.FileName, sb.ToString());
				return (true, dialog.FileName);
			}
			catch
			{
				return (false, "");
			}
		}
	}
}

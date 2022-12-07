using Microsoft.Win32;
using System;
using System.IO;
using System.Text;

namespace FileManager.SaveController
{
	class BinaryFileSaver : FileSaver
	{
		public override bool? SaveFile(string path, string text)
		{
			try
			{
				var dialog = new SaveFileDialog
				{
					FileName = Path.GetFileNameWithoutExtension(path),
					DefaultExt = "bin",
					Filter = "Binary files (*.bin)|*.bin"
				};
				bool? result = dialog.ShowDialog();
				if (result != true)
					return null;

				if (Path.GetExtension(dialog.FileName) != ".bin")
					dialog.FileName += ".bin";

				var sb = new StringBuilder();
				foreach (char c in text.ToCharArray())
					sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
				System.IO.File.WriteAllText(path, sb.ToString());

				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}

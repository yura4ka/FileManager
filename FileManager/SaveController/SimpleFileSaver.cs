using Microsoft.Win32;
using System.IO;
using System.Text;

namespace FileManager.SaveController
{
	class SimpleFileSaver : FileSaver
	{
		public override bool? SaveFile(string path, string text)
		{
			try
			{
				var dialog = new SaveFileDialog { FileName = Path.GetFileName(path), };
				bool? result = dialog.ShowDialog();
				if (result != true)
					return null;
				System.IO.File.WriteAllText(path, text, Encoding.UTF8);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}

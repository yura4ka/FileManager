using Microsoft.Win32;
using System.IO;
using System.Text;

namespace FileManager.SaveController
{
	class SimpleFileSaver : FileSaver
	{
		public override (bool?, string) SaveFile(string path, string text)
		{
			try
			{
				var dialog = new SaveFileDialog 
				{ 
					FileName = Path.GetFileName(path), 
					Filter = "All files (*.*)|*.*",
					DefaultExt = "txt",
				};
				bool? result = dialog.ShowDialog();
				if (result != true)
					return (null, "");
				System.IO.File.WriteAllText(dialog.FileName, text, Encoding.UTF8);
				return (true, dialog.FileName);
			}
			catch
			{
				return (false, "");
			}
		}
	}
}

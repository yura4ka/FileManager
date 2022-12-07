using Microsoft.Win32;
using System.IO;
using System.Text;

namespace FileManager.SaveController
{
	class HtmlFileSaver : FileSaver
	{
		public override (bool?, string) SaveFile(string path, string text)
		{
			try
			{
				var dialog = new SaveFileDialog
				{
					FileName = Path.GetFileNameWithoutExtension(path) + ".html",
					DefaultExt = "html",
					Filter = "HTML files (*.html)|*.html"
				};
				bool? result = dialog.ShowDialog();
				if (result != true)
					return (null, "");

				if (Path.GetExtension(dialog.FileName) != ".html")
					dialog.FileName += ".html";
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

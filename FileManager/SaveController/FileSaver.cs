namespace FileManager.SaveController
{
	abstract class FileSaver
	{
		public abstract (bool?, string) SaveFile(string path, string text);
	}
}

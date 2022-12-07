namespace FileManager.SaveController
{
	abstract class FileSaver
	{
		public abstract bool? SaveFile(string path, string text);
	}
}

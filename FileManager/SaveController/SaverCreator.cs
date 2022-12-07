namespace FileManager.SaveController
{
	abstract class SaverCreator
	{
		public enum SaverTypes
		{
			BINARY,
			HTML,
			SIMPLE,
		}
		public abstract FileSaver CreateSaver(SaverTypes type);
	}
}

namespace FileManager.SaveController
{
	class FileSaverCreator : SaverCreator
	{
		public override FileSaver CreateSaver(SaverTypes type)
		{
			return type switch
			{
				SaverTypes.BINARY => new BinaryFileSaver(),
				SaverTypes.HTML => new HtmlFileSaver(),
				_ => new SimpleFileSaver(),
			};
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace FileManager.Dialogs
{
	public partial class ImagesFromHtml : Window
	{
		private record ImageItem(string Path);
		public ImagesFromHtml(List<string> pathes, string parent)
		{
			InitializeComponent();
			var list = new List<ImageItem>();
			foreach (var path in pathes)
			{
				(string newPath, bool isPath) = EditorUtils.MakePathAbsoulute(path, parent);
				if (!isPath)
					list.Add(new ImageItem(newPath));
				else
					list.Add(new(newPath + (System.IO.File.Exists(newPath) ? "" : " (не знайдено!)")));
				
			}
			ListBox.ItemsSource = list;
		}
	}
}

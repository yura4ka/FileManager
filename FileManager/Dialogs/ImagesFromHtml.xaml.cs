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
				if (Uri.TryCreate(path, UriKind.Absolute, out _))
					list.Add(new ImageItem(path));
				else if (Path.IsPathFullyQualified(path) && path[0] != '/' && System.IO.File.Exists(path))
					list.Add(new(path));
				else
				{
					string newPath = Path.Join(parent, path).Replace('/', '\\');
					list.Add(new(newPath + (System.IO.File.Exists(newPath) ? "" : " (не знайдено!)")));
				}
			}
			ListBox.ItemsSource = list;
		}
	}
}

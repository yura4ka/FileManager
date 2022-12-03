using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FileManager
{
	static class Icons
	{
		public readonly static BitmapImage DriveImage = new(new Uri(@"pack://application:,,,/Images/drive.png"));
		public readonly static BitmapImage FolderImage = new(new Uri(@"pack://application:,,,/Images/folder.png"));
		public readonly static BitmapImage FileImage = new(new Uri(@"pack://application:,,,/Images/file.png"));

		private readonly static Dictionary<string, BitmapSource> ExtensionsIcons = new();

		public static BitmapSource FromFile(FileInfo info)
		{
			if (ExtensionsIcons.TryGetValue(info.Extension, out var image))
				return image;

			var icon = Icon.ExtractAssociatedIcon(info.FullName);
			var img = icon == null ? FileImage : ToBitmapImage(icon.ToBitmap());

			ExtensionsIcons.Add(info.Extension, img);
			return img;
		}

		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		private static extern bool DeleteObject(IntPtr hObject);

		private static BitmapSource ToBitmapImage(Bitmap bitmap)
		{
			IntPtr hBitmap = bitmap.GetHbitmap();
			BitmapSource result;
			try
			{
				result = Imaging.CreateBitmapSourceFromHBitmap(
							 hBitmap,
							 IntPtr.Zero,
							 Int32Rect.Empty,
							 BitmapSizeOptions.FromEmptyOptions());
			}
			finally
			{
				DeleteObject(hBitmap);
			}

			return result;
		}
	}
}

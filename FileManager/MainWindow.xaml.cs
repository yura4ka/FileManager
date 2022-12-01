using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileManager
{
	public partial class MainWindow : Window
	{
		private FileSystem fs = new();
		public MainWindow()
		{
			InitializeComponent();
			LeftTree.ItemsSource = fs.GetRoot();
		}

		private void LeftTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			var selected = LeftTree.SelectedItem;
		}

		private void LeftTree_Expanded(object sender, RoutedEventArgs e)
		{

		}
	}
}

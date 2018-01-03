using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CLan
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SelectBackground : Window
    {
        public SelectBackground()
        {
            InitializeComponent();
            DirectoryInfo folder = new DirectoryInfo(Directory.GetCurrentDirectory() + @"../../BackgroundImages");
            FileInfo[] images = folder.GetFiles("*.jpg");
            foreach (FileInfo img in images)
                Thumbnails.Items.Add(new BitmapImage(new Uri(img.FullName)));
        }

        private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Thumbnails.SelectedItems.Count > 0)
            {
                string imgname = new FileInfo(((sender as ListViewItem).Content as BitmapImage).UriSource.AbsolutePath).Name;
                Properties.Settings.Default.BackgroundPath = "/BackgroundImages/" + imgname;
                Properties.Settings.Default.Save();
            }
            this.Close();
        }
    }
}

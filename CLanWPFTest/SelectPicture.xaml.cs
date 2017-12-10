using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CLan
{
    /// <summary>
    /// Interaction logic for SelectPicture.xaml
    /// </summary>
    public partial class SelectPicture : Window
    {
        public SelectPicture()
        {
            InitializeComponent();
            DirectoryInfo folder = new DirectoryInfo(Directory.GetCurrentDirectory() + @"../../UserAvatars");
            FileInfo[] images = folder.GetFiles("*.png");
            foreach (FileInfo img in images)
                Thumbnails.Items.Add(new BitmapImage(new Uri(img.FullName)));               
        }

        private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Thumbnails.SelectedItems.Count > 0)
            {
                string imgpath = (sender as ListViewItem).Content.ToString();
                Properties.Settings.Default.PicturePath = imgpath;
            }
            this.Close();
        }
    }
}

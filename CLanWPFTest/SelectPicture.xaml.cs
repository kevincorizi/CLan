using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CLanWPFTest
{
    /// <summary>
    /// Interaction logic for SelectPicture.xaml
    /// </summary>
    public partial class SelectPicture : Window
    {
        public SelectPicture()
        {
            InitializeComponent();
            DirectoryInfo folder = new DirectoryInfo(Directory.GetCurrentDirectory() + @"../../UserAvatars/");
            FileInfo[] images = folder.GetFiles("*.png");
            foreach (FileInfo img in images)
                Thumbnails.Items.Add(new BitmapImage(new Uri(img.FullName)));          
        }

        private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           // Console.WriteLine("Double click!");
            //if (Thumbnails.SelectedItems.Count > 0)
            //{
            string imgpath = Thumbnails.SelectedItem.Path;
            Properties.Settings.Default.PicturePath = imgpath;
            //}
            this.Hide();
        }
    }
}

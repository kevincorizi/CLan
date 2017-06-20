using CLanWPFTest.Properties;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
            //DirectoryInfo folder = new DirectoryInfo(Directory.GetCurrentDirectory() + @"../../UserAvatars/");
            //FileInfo[] images = folder.GetFiles("*.png");
            //foreach (FileInfo img in Properties.Resources.)
            //Thumbnails.Items.Add(new BitmapImage(new Uri(img.FullName)));

            ListViewItem l1 = new ListViewItem();
            l1.Content = Properties.Resources.user01;
            Thumbnails.Items.Add(l1);
            Thumbnails.Items.Add(Properties.Resources.user02);
        }

        private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           Console.WriteLine("Double click!");
            if (Thumbnails.SelectedItems.Count > 0)
            {
                string imgpath = (Thumbnails.SelectedItem as ListViewItem).Content.ToString();
                //Properties.Settings.Default.PicturePath = imgpath;
                Trace.WriteLine(imgpath);
            }
            this.Hide();
        }
    }
}

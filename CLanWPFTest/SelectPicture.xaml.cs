using CLanWPFTest.Properties;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Resources;
using System.Collections;

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
            // Probably this is not the way you wanted to do :/
            DirectoryInfo folder = new DirectoryInfo(Directory.GetCurrentDirectory() + @"../../Resources");
            FileInfo[] images = folder.GetFiles("*.png");
            foreach (FileInfo img in images)
                Thumbnails.Items.Add(new BitmapImage(new Uri(img.FullName)));

            //ResXResourceSet resxSet = new ResXResourceSet(@"..\Properties\Resources.resx");
            
            /*
             foreach (DictionaryEntry entry in resxSet)
             {                            
                Thumbnails.Items.Add(new BitmapImage(new Uri("./Resources/user0.png")));
                // entry.Key is the name of the file
                // entry.Value is the actual object...add it to the list of images you were looking to keep track of
             }
             */

        }

        private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           Console.WriteLine("Double click!");
            if (Thumbnails.SelectedItems.Count > 0)
            {
                string imgpath = (Thumbnails.SelectedItem as ListViewItem).;
                //Properties.Settings.Default.PicturePath = imgpath;
                Trace.WriteLine(imgpath);
            }
            this.Hide();
        }
    }
}

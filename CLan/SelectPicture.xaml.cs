using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CLan
{
    /// <summary>
    /// Interaction logic for SelectPicture.xaml
    /// </summary>
    ///
    public static class RenewMethod
    {
        private static System.Action EmptyDelegate = delegate () { };

        public static void Renew(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }
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
                string imgname = new FileInfo(((sender as ListViewItem).Content as BitmapImage).UriSource.AbsolutePath).Name;               
                Properties.Settings.Default.PicturePath = "/UserAvatars/" + imgname;
                Properties.Settings.Default.Save();
            }
            this.Close();
        }
    }
}

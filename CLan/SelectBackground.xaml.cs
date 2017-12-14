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
            foreach (string s in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (s.StartsWith("CLan.UserAvatars."))
                {
                    Thumbnails.Items.Add(new BitmapImage(new Uri("pack://application:,,,/Images/BackgroundImages//" + s.Substring("CLan.UserAvatars.".Length))));
                }
            }
        }

        private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Thumbnails.SelectedItems.Count > 0)
            {
                string imgname = new FileInfo(((sender as ListViewItem).Content as BitmapImage).UriSource.AbsolutePath).Name;
                Properties.Settings.Default.PicturePath = "Images/BackgroundImages/" + imgname;
            }
            this.Close();
        }
    }
}

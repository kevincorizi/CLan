using CLan.Objects;
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
            String[] images = SettingsManager.GetResourcesUnder("BackgroundImages");
            foreach (String img in images)
                Thumbnails.Items.Add(new BitmapImage(new Uri("pack://application:,,,/BackgroundImages/" + img)));
        }

        private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Thumbnails.SelectedItems.Count > 0)
            {
                Uri imgname = ((sender as ListViewItem).Content as BitmapImage).UriSource;
                SettingsManager.BackgroundPicture = imgname;
            }
            this.Close();
        }
    }
}

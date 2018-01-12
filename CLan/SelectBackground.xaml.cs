using CLan.Objects;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CLan
{
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
            SettingsManager.BackgroundPicture = ((sender as ListViewItem).Content as BitmapImage).UriSource;
            this.Close();
        }
    }
}

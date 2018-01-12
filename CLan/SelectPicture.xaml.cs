using CLan.Objects;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CLan
{
    public partial class SelectPicture : Window
    {
        public SelectPicture()
        {    
            InitializeComponent();
            String[] images = SettingsManager.GetResourcesUnder("UserAvatars");
            foreach (String img in images)
                Thumbnails.Items.Add(new BitmapImage(new Uri("pack://application:,,,/UserAvatars/" + img)));
        }

        private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {             
            SettingsManager.UserPicture = ((sender as ListViewItem).Content as BitmapImage).UriSource;
            this.Close();
        }
    }
}

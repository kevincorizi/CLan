using CLan.Objects;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
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
            String[] images = SettingsManager.GetResourcesUnder("UserAvatars");
            foreach (String img in images)
                Thumbnails.Items.Add(new BitmapImage(new Uri("pack://application:,,,/UserAvatars/" + img)));
        }

        private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            if (Thumbnails.SelectedItems.Count > 0)
            {
                Uri imgname = ((sender as ListViewItem).Content as BitmapImage).UriSource;               
                SettingsManager.UserPicture = imgname;
            }
            this.Close();
        }
    }
}

using CLanWPFTest.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace CLanWPFTest
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    /// 
    public partial class UsersWindow : Page
    {
        public List<string> files;
        public List<User> destinations;

        public UsersWindow(List<string> lf)
        {
            InitializeComponent();

            destinations = new List<User>();
            files = new List<string>();

            this.DataContext = this;
            this.files = lf;
            this._continue.IsEnabled = false;    // Disable the "send" button until a user is selected.
        }
        private void PrivateMode_Checked(object sender, RoutedEventArgs e)
        {
            CLanUDPManager.GoOffline();
            Trace.WriteLine("private!");
        }

        private void PublicMode_Checked(object sender, RoutedEventArgs e)
        {
            CLanUDPManager.GoOnline();
            Trace.WriteLine("public!");
        }

        private void ContinueClick(object sender, RoutedEventArgs e)
        {
            List<User> users = new List<User>();
            users.Add(UserList.SelectedItem as User);

            Trace.WriteLine("UW.XAML.CS" + users.Count.ToString() + " USERS SELECTED");
            // Open the actual file transfer window, with progress and all
            FileTransferWindow ft = new FileTransferWindow(files, users);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingsPage());
        }

        private void UserList_Selected(object sender, RoutedEventArgs e)
        {
            _continue.IsEnabled = true;
            
            foreach(User item in ((ListView)UserList).SelectedItems)
            {
                destinations.Add(item);
            }
            
        }
    }
}
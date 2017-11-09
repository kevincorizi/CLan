using CLanWPFTest.Networking;
using CLanWPFTest.Objects;
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
        public UsersWindow()
        {
            InitializeComponent();

            this.DataContext = this;
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
            if (users != null)
            {
                this.NavigationService.Navigate(new FileSelection(users));
            }
            else
            {
                Trace.WriteLine("No users selected");
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingsPage());
        }

        private void UserList_Selected(object sender, RoutedEventArgs e)
        {
            _continue.IsEnabled = true;
        }
    }
}
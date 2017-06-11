using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CLanWPFTest
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    /// 
    public partial class UsersWindow : Page
    {
        public string toSend = null;
        
        public UsersWindow(string fileToSend = null) {
            InitializeComponent();
            this.DataContext = this;
            this.toSend = fileToSend;
            this._continue.IsEnabled = false;    // Disable the "send" button until a user is selected.
        }    
        private void PrivateMode_Checked(object sender, RoutedEventArgs e)
        {
            CLanUDPManager.GoOffline();
            Console.WriteLine("private!");
        }
        
        private void PublicMode_Checked(object sender, RoutedEventArgs e)
        {
            CLanUDPManager.GoOnline();
            Console.WriteLine("public!");
        }
        
        private void continueClick(object sender, RoutedEventArgs e)
        {
            List<User> users = new List<User>();
            users.Add(UserList.SelectedItem as User);
            if(users.Count > 0)
                this._continue.IsEnabled = true;    // Enable the "send" button if at least one user is selected.
            this.NavigationService.Navigate(new FileTransfer(toSend, users));
                  
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingsPage());
        }
    }
}

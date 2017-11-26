using CLanWPFTest.Networking;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            CLanUDPManager.Instance.GoOffline();
        }

        private void PrivateMode_Unchecked(object sender, RoutedEventArgs e)
        {
            CLanUDPManager.Instance.GoOnline();
        }

        private void ContinueClick(object sender, RoutedEventArgs e)
        {
            List<User> users = UserList.SelectedItems.OfType<User>().ToList();
            if(FileList.Items.Count > 0)
            {

                List<Objects.CLanFile> files = FileList.Items.OfType<Objects.CLanFile>().ToList();
                foreach (User u in users)
                {
                    Trace.WriteLine("UW.XAML.CS - ADDING FILE TRANSFER");
                    CLanFileTransfer cft = new CLanFileTransfer(u, files, CLanTransferType.SEND);
                    cft.Start();
                }
                App.SelectedFiles.Clear();
                Trace.WriteLine(App.SelectedFiles.Count);
            }
            else
            {
                NavigationService.Navigate(new FileSelection(users));
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            
            NavigationService.Navigate(new SettingsPage());
        }

        private void UserList_Selected(object sender, RoutedEventArgs e)
        {
            _continue.IsEnabled = true;
        }
    }
}
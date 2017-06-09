using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CLanWPFTest
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    /// 
    public partial class UsersWindow : Window
    {
        public string toSend = null;

        public UsersWindow(string fileToSend = null) {
            this.Closing += MainWindow_Closing;
            InitializeComponent();
            this.DataContext = this;
            this.toSend = fileToSend;
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
            // Following line is commented for the "namespace doesnt include" error. 
            // users.Add(UserList.SelectedItem as User);

             FileTransfer ft = new FileTransfer(toSend, users);
            System.Windows.Application.Current.Windows[0].Content = ft.Content;        
        }

        private void backClick(object sender, RoutedEventArgs e)
        {
            FileSelection sf = new FileSelection();
            System.Windows.Application.Current.Windows[0].Content = sf.Content;
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prevent window from closing
            e.Cancel = true;
            // Hide window
            this.Hide();
        }
    }
}

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
    public partial class MainWindow : Window
    {
        public string toSend = null;

        public MainWindow(string fileToSend = null) {
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
            users.Add(UserList.SelectedItem as User);
            FileTransfer ft = new FileTransfer(toSend, users);
            this.Content = ft.Content;                  // Update the same window with the transaction window 
            ft.Show();
        }      

        private void backClick(object sender, RoutedEventArgs e)
        {
           // selectFile sf = new selectFile();
           // this.Content = sf.Content;                  // Update the same window with the transaction window                      
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

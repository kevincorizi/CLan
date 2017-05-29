using System;
using System.Collections.ObjectModel;
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
        

        public MainWindow() {
            this.Closing += MainWindow_Closing;
            InitializeComponent();
            this.DataContext = this;

            
           
        }
        
        private void PrivateMode_Checked(object sender, RoutedEventArgs e)
        {
            CLanUDPManager.GoOffline();
            Console.WriteLine("private!");
        }
        
        private void PublicMode_Checked(object sender, RoutedEventArgs e)
        {

        }
        
        private void continueClick(object sender, RoutedEventArgs e)
        {
            FileTransfer ft = new FileTransfer();
            this.Content = ft.Content;                  // Update the same window with the transaction window 
            ft.Show();
        }      

        private void backClick(object sender, RoutedEventArgs e)
        {
           // selectFile sf = new selectFile();
           // this.Content = sf.Content;                  // Update the same window with the transaction window                      
        }

        private void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Controls.ListView list = e.Source as System.Windows.Controls.ListView;
            User dest = list.SelectedItem as User;
            CLanUDPManager.SendFileRequest(dest, "ciao.txt");
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

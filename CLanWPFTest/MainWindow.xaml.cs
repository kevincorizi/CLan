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
        public static User me;
        private readonly Dispatcher uiDispatcher;

        private static ObservableCollection<User> _onlineUsers = new ObservableCollection<User>();
        public ObservableCollection<User> OnlineUsers {
            get {
                return _onlineUsers;
            }
        }

        public static void AddUser(User u) {
            if (!_onlineUsers.Contains(u))
                _onlineUsers.Add(u);
        }

        public static void RemoveUser(User u) {
            if(_onlineUsers.Contains(u))
                _onlineUsers.Remove(u);
        }

        public MainWindow() {
            InitializeComponent();
            this.DataContext = this;

            uiDispatcher = Dispatcher.CurrentDispatcher;

            me = new User("Ezgi Akcora");           
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
    }
}

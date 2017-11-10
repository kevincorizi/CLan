using CLanWPFTest.Networking;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace CLanWPFTest
{
    /// <summary>
    /// Interaction logic for fileTransfer.xaml
    /// </summary>
    /// 
    public partial class FileTransferWindow : Window
    {
        private static FileTransferWindow ftw = null;

        private FileTransferWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public static void Open()
        {
            if (ftw == null)
            {
                var t = new Thread(() => {
                    ftw = new FileTransferWindow();
                    ftw.Show();
                    System.Windows.Threading.Dispatcher.Run();
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Do not close the window if there are ongoing file transfers,
            // Otherwise you lose the chance to check them and stop them
            if(App.IncomingTransfers.Count != 0 || App.OutgoingTransfers.Count != 0)
            {
                e.Cancel = true;
                this.WindowState = WindowState.Minimized;
            } else
            {
                ftw = null;
            }
        }

        void cancel_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            // Find the specific file transfer we want to stop
            CLanFileTransfer cft = b.DataContext as CLanFileTransfer;
            cft.Stop();
        }
    }
}

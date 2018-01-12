using CLan.Networking;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CLan
{
    public partial class FileTransferWindow : Window
    {
        public FileTransferWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        void cancel_Click(object sender, RoutedEventArgs e)
        {
            // Find the specific file transfer we want to stop, and stop it
            CLanFileTransfer cft = (sender as Button).DataContext as CLanFileTransfer;
            cft.Stop();
        }
    }
}

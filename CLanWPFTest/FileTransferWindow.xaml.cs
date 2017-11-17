using CLanWPFTest.Networking;
using System;
using System.ComponentModel;
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
        public FileTransferWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public void Open()
        {
            OnDisplay();
        }

        void cancel_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            // Find the specific file transfer we want to stop
            CLanFileTransfer cft = b.DataContext as CLanFileTransfer;
            cft.Stop();
        }

        public event EventHandler Display;
        public void OnDisplay()
        {
            Display?.Invoke(this, EventArgs.Empty);
        }
    }
}

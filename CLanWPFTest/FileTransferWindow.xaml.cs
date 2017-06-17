using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using CLanWPFTest.Networking;
using System.Diagnostics;

namespace CLanWPFTest

{
    /// <summary>
    /// Interaction logic for fileTransfer.xaml
    /// </summary>
    /// 

    public partial class FileTransferWindow : Window
    {
        // User will see one progress bar for each batch of files to the same destinations.
        // In this way we do not clutter the interface too much and we are still able to stay responsive and clear
        public ObservableCollection<CLanFileTransfer> FileTransfers { get; set; }

        public FileTransferWindow(List<string> files, List<User> dest)
        {
            InitializeComponent();

            this.DataContext = this;
            FileTransfers = new ObservableCollection<CLanFileTransfer>();

            foreach (User u in dest)
            {
                Trace.WriteLine("Adding file transfer");
                FileTransfers.Add(new CLanFileTransfer(u, files));
            }

            foreach (CLanFileTransfer cft in FileTransfers)
            {
                Trace.WriteLine("Starting file transfer");
                cft.Start();
            }

            this.Show();
        }
        void cancel_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}

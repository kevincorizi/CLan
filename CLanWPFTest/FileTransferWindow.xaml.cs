using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using CLanWPFTest.Networking;
using System.Diagnostics;
using System.Windows.Controls;
using CLanWPFTest.Objects;

namespace CLanWPFTest

{
    /// <summary>
    /// Interaction logic for fileTransfer.xaml
    /// </summary>
    /// 
    public partial class FileTransferWindow : Window
    {
        public FileTransferWindow(List<string> files, List<User> dest)
        {
            InitializeComponent();

            this.DataContext = this;

            List<CLanFile> cfiles = new List<CLanFile>();
            foreach (string s in files)
            {
                cfiles.Add(new CLanFile(s));
            }

            foreach (User u in dest)
            {
                Trace.WriteLine("FTW.XAML.CS - ADDING FILE TRANSFER");
                CLanFileTransfer cft = new CLanFileTransfer(u, cfiles, CLanTransferType.SEND);
                cft.Store();
            }

            foreach (CLanFileTransfer cft in App.FileTransfers)
            {
                cft.Ask();
            }

            this.Show();
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

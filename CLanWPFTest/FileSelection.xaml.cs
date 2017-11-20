using CLanWPFTest.Networking;
using CLanWPFTest.Objects;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace CLanWPFTest
{
    /// <summary>
    /// Interaction logic for selectFile.xaml
    /// </summary>

    public partial class FileSelection : Page
    {
        public List<CLanFile> files;
        public List<User> destinations;
        public FileSelection(List<User> users)
        {
            files = new List<CLanFile>();
            this.destinations = users;
            InitializeComponent();
        }

        private void selectFile_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog fd = new CommonOpenFileDialog();    // Opens a window to choose the file from the pc
            fd.Multiselect = true;
            fd.EnsureValidNames = true;
            fd.EnsureFileExists = true;
            fd.EnsurePathExists = true;
            CommonFileDialogResult result = fd.ShowDialog();
            if (result != CommonFileDialogResult.Ok)
                return;

            List<String> sFiles = new List<string>(fd.FileNames);   // The list of entries selected by the user

            files = CLanFile.GetFiles(sFiles);

            uploadButton.Visibility = Visibility.Hidden;
            continueButton.Visibility = Visibility.Visible;
            selectText.Visibility = Visibility.Hidden;
            continueText.Visibility = Visibility.Visible;
            continueButton.IsEnabled = true;
        }

        private void continueClick(object sender, RoutedEventArgs e)
        {
            foreach (User u in destinations)
            {
                Trace.WriteLine("FTW.XAML.CS - ADDING FILE TRANSFER");
                CLanFileTransfer cft = new CLanFileTransfer(u, files, CLanTransferType.SEND);
                cft.Start();
            }
        }
    }
}

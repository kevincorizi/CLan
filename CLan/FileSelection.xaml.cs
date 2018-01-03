using CLan.Networking;
using CLan.Objects;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace CLan
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

            selectFilesOrFolders(fd);
        }

        private void selectFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog fd = new CommonOpenFileDialog();
            fd.Multiselect = true;
            fd.IsFolderPicker = true;
            fd.AllowNonFileSystemItems = true;

            selectFilesOrFolders(fd);
        }

        private void selectFilesOrFolders(CommonOpenFileDialog cofd)
        {
            CommonFileDialogResult result = cofd.ShowDialog();
            if (result != CommonFileDialogResult.Ok)
                return;

            List<String> sFiles = new List<string>(cofd.FileNames);   // The list of entries selected by the user

            files = CLanFile.GetFiles(sFiles);

            selectionButtonBox.Visibility = Visibility.Hidden;
            continueButtonBox.Visibility = Visibility.Visible;
        }

        private void continueClick(object sender, RoutedEventArgs e)
        {
            foreach (User u in destinations)
            {
                Trace.WriteLine("FTW.XAML.CS - ADDING FILE TRANSFER");
                CLanFileTransfer cft = new CLanFileTransfer(u, files, CLanTransferType.SEND);
                cft.Start();
            }

            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void SelectBack_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(continueButtonBox.Visibility == Visibility.Visible)  // Files or folders already selected, flush and restore view
            {
                files.Clear();
                continueButtonBox.Visibility = Visibility.Hidden;
                selectionButtonBox.Visibility = Visibility.Visible;              
            }
            else
            {
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
        }
    }
}

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
    public partial class FileSelection : Page
    {
        // Files selected in the current session
        public List<CLanFile> files;
        // Users selected for the current session from the UsersWindow
        public List<User> destinations;
        public FileSelection(List<User> users)
        {
            files = new List<CLanFile>();
            destinations = users;
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

        // A common method that handles both selection of files and folders. The interaction
        // logic is the same in both cases, the only different thing is how the CommonOpenFileDialog
        // is instantiated.
        private void selectFilesOrFolders(CommonOpenFileDialog cofd)
        {
            CommonFileDialogResult result = cofd.ShowDialog();
            if (result != CommonFileDialogResult.Ok)
                return;

            // The list of entries selected by the user
            List<String> sFiles = new List<string>(cofd.FileNames);   
            // Converts the list of filenames to actual CLanFile objects, expanding the directory structure if needed
            files = CLanFile.GetFiles(sFiles);  

            selectionButtonBox.Visibility = Visibility.Hidden;
            continueButtonBox.Visibility = Visibility.Visible;
        }

        private void continueClick(object sender, RoutedEventArgs e)
        {
            // A separated session is created for the list of files for each user, in a different thread
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

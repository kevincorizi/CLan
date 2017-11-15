using CLanWPFTest.Networking;
using CLanWPFTest.Objects;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            //fd.DefaultExt = "*.*";
            fd.Multiselect = true;
            fd.EnsureValidNames = true;
            fd.EnsureFileExists = true;
            fd.EnsurePathExists = true;
            CommonFileDialogResult result = fd.ShowDialog();
            if (result != CommonFileDialogResult.Ok)
            {
                return;
            }

            List<String> sFiles = new List<string>(fd.FileNames);   // The list of entries selected by the user
            // For each of those entries I must check if they are files 
            // or if they are folders, and in case i have to enumerate all possible
            // subfolders and files

            // Convert filenames to CLanFiles here
            foreach (string p in sFiles)
            {
                FileAttributes attributes = File.GetAttributes(p);
                // now we will detect whether its a directory or file
                if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    // Directory
                    String folderName = new DirectoryInfo(Path.GetDirectoryName(p)).FullName + Path.DirectorySeparatorChar;
                    foreach (string f in Directory.EnumerateFiles(p, "*", SearchOption.AllDirectories))
                    {
                        // Now i have all the possible subdirectories of that folder and all files included
                        String relativeName = f.Replace(folderName, "");
                        files.Add(new CLanFile(f, relativeName));
                    }
                }
                else
                {
                    // File
                    files.Add(new CLanFile(p));
                }
            }

            foreach (CLanFile cf in files)
            {
                Trace.WriteLine(cf.Name);
            }

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

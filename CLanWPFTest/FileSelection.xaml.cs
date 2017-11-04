using System.Windows;
using Microsoft.Win32;
using System;
using System.Windows.Controls;
using System.Diagnostics;
using System.Collections.Generic;
using CLanWPFTest.Objects;
using System.IO;

namespace CLanWPFTest
{
    /// <summary>
    /// Interaction logic for selectFile.xaml
    /// </summary>

    public partial class FileSelection : Page
    {
        public List<CLanFile> files;
        public FileSelection()
        {
            //this.Closing += FileSelection_Closing; ---> TODO: This part gives me error after window to page change.
            InitializeComponent();
        }

        private void selectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();    // Opens a window to choose the file from the pc
            fd.DefaultExt = "*.*";
            fd.Multiselect = true;
            bool? result = fd.ShowDialog();
            if (result != true || fd.CheckFileExists != true)
            {
                return;
            }

            List<String> sFiles = new List<string>(fd.FileNames);   // The list of entries selected by the user
            // For each of those entries I must check if they are files 
            // or if they are folders, and in case i have to enumerate all possible
            // subfolders and files

            files = new List<CLanFile>();
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

            uploadButton.Visibility = Visibility.Hidden;
            continueButton.Visibility = Visibility.Visible;
            selectText.Visibility = Visibility.Hidden;
            continueText.Visibility = Visibility.Visible;
            continueButton.IsEnabled = true;
        }

        private void continueClick(object sender, RoutedEventArgs e)
        {
            if (files != null)
            {
                this.NavigationService.Navigate(new UsersWindow(files));
            }
            else
            {
                Trace.WriteLine("No file selected");
            }
        }
    }
}

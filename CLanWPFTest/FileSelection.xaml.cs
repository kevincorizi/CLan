using System.Windows;
using Microsoft.Win32;
using System.Drawing;
using System;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using CLanWPFTest.Objects;

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
            fd.ShowDialog();

            List<String> sFiles = new List<string>(fd.FileNames);               // The file path to be sent to the backend

            files = new List<CLanFile>();
            // Convert filenames to CLanFiles here
            foreach (string s in sFiles)
            {
                files.Add(new CLanFile(s));
            }

            if (fd.CheckFileExists == true)
            {
                uploadButton.Visibility = Visibility.Hidden;
                continueButton.Visibility = Visibility.Visible;
                selectText.Visibility = Visibility.Hidden;
                continueText.Visibility = Visibility.Visible;
                continueButton.IsEnabled = true;
            }
        }

        private void continueClick(object sender, RoutedEventArgs e)
        {
            if(files != null)
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

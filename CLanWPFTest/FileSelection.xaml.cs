using System.Windows;
using Microsoft.Win32;
using System.Drawing;
using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CLanWPFTest
{
    /// <summary>
    /// Interaction logic for selectFile.xaml
    /// </summary>

    public partial class FileSelection : Page
    {
        public string toSend = null;
        public FileSelection()
        {
            //this.Closing += FileSelection_Closing; ---> TODO: This part gives me error after window to page change.
            InitializeComponent();
        }

        private void selectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();    // Opens a window to choose the file from the pc
            fd.DefaultExt = "*.*";
            fd.ShowDialog();
            toSend = fd.FileName;               // The file path to be sent to the backend

            if (fd.CheckPathExists == true)
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
            if(toSend != null)
            {
                this.NavigationService.Navigate(new UsersWindow(toSend));      
            }
            else
            {
                Console.WriteLine("No file selected");
            }
        }

        void FileSelection_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prevent window from closing
            e.Cancel = true;

            // Hide window
            // this.Hide(); ---> TODO: This part gives me error after window to page change.
        }


    }
}

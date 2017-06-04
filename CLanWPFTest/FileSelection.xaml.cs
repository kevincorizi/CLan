using System.Windows;
using Microsoft.Win32;
using System.Drawing;
using System;

namespace CLanWPFTest
{
    /// <summary>
    /// Interaction logic for selectFile.xaml
    /// </summary>

    public partial class FileSelection : Window
    {
        public string toSend = null;
        public FileSelection()
        {
            this.Closing += FileSelection_Closing;
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
                UsersWindow mw = new UsersWindow(toSend);
                this.Content = mw.Content;                  // Update the same window with the user selection window  
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
            this.Hide();
        }

    }
}

using System.Windows;
using Microsoft.Win32;
using System.Drawing;

namespace CLanWPFTest
{
    /// <summary>
    /// Interaction logic for selectFile.xaml
    /// </summary>

    public partial class FileSelection : Window
    {
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
            string fullPath = fd.FileName;               // The file path to be sent to the backend

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
            MainWindow mw = new MainWindow();
            this.Content = mw.Content;                  // Update the same window with the user selection window     
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

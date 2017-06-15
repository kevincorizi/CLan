using System;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace CLanWPFTest
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        Nullable<bool> result;      // New image selection
        string filename;            // New image path
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void General_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new UsersWindow(null));
        }

        private void changePicture_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                filename = dlg.FileName;
                var brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(filename, UriKind.Relative));
                userImage.Background = brush;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (filename != null)   // If the image is changed
            {
                var brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(filename, UriKind.Relative));
                userImageThumb.Background = brush;
            }
        }

        private void DownloadPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            PathText.Text = dialog.SelectedPath;

        }
    }
}
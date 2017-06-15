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
using System.IO;

namespace CLanWPFTest
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        Nullable<bool> result;      // New image selection
        string filename;            // New image path
        Microsoft.Win32.OpenFileDialog dlg;
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
           dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG Files (*.png)|*.png";


            // Display OpenFileDialog by calling ShowDialog method 
            result = dlg.ShowDialog();

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

                string appPath = System.AppDomain.CurrentDomain.BaseDirectory + @"\images\"; // <---
                string filepath = dlg.FileName;    // <---
                File.Copy(filepath, appPath + "user.png", true); // <--- 
            }
        }

        private void DownloadPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            PathText.Text = dialog.SelectedPath;
        }

        private void EditName_Click(object sender, RoutedEventArgs e)
        {
            TransparencyLayer.Visibility = System.Windows.Visibility.Visible;
            NameBox.Visibility = System.Windows.Visibility.Visible;
            
        }

        private void SaveNameButton_Click(object sender, RoutedEventArgs e)
        {
            NameBox.Visibility = System.Windows.Visibility.Collapsed;
            TransparencyLayer.Visibility = System.Windows.Visibility.Collapsed;
            // Do something with the Input
            String input = InputTextBox.Text;

            // Clear InputBox.
            InputTextBox.Text = String.Empty;
            UserName.Text = input;
            UserNameThumb.Text = input;
        }

        private void CancelNameButton_Click(object sender, RoutedEventArgs e)
        {
            NameBox.Visibility = System.Windows.Visibility.Collapsed;
            TransparencyLayer.Visibility = System.Windows.Visibility.Collapsed;

            // Clear InputBox.
            InputTextBox.Text = String.Empty;
        }
    }
}
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.IO;

namespace CLanWPFTest
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void General_Click(object sender, RoutedEventArgs e)
        {

        }

        private void changePicture_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            var dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG Files (*.png)|*.png";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                var brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(filename, UriKind.Relative));
                userImage.Background = brush;

                // Update value in settings (this is not yet saved, it will be when the user presses the Save button)
                Properties.Settings.Default.PicturePath = filename;
            }
        } 

        private void DownloadPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            string filePath = dialog.SelectedPath;
            PathText.Text = filePath;

            // Update value in settings (this is not yet saved, it will be when the user presses the Save button)
            Properties.Settings.Default.DefaultSavePath = filePath;
        }

        private void EditName_Click(object sender, RoutedEventArgs e)
        {
            TransparencyLayer.Visibility = Visibility.Visible;
            NameBox.Visibility = Visibility.Visible;           
        }

        private void SaveNameButton_Click(object sender, RoutedEventArgs e)
        {
            NameBox.Visibility = Visibility.Collapsed;
            TransparencyLayer.Visibility = Visibility.Collapsed;
            // Do something with the Input
            string newName = InputTextBox.Text;

            // Clear InputBox.
            InputTextBox.Text = String.Empty;
            UserName.Text = newName;

            // Update value in settings (this is not yet saved, it will be when the user presses the Save button)
            Properties.Settings.Default.Name = newName;
        }

        private void CancelNameButton_Click(object sender, RoutedEventArgs e)
        {
            NameBox.Visibility = Visibility.Collapsed;
            TransparencyLayer.Visibility = Visibility.Collapsed;

            // Clear InputBox.
            InputTextBox.Text = String.Empty;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Now the modifications to settings become permanent
            Properties.Settings.Default.Save();
            DisplaySettings("Settings saved");
            if(NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            // Discard pending changes to settings
            Properties.Settings.Default.Reload();
            DisplaySettings("Settings discarded");

            // Go back to previous window
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        // DEBUG
        private void DisplaySettings(string pre)
        {
            Console.WriteLine(pre);
            foreach (System.Configuration.SettingsProperty currentProperty in Properties.Settings.Default.Properties)
            {
                Console.WriteLine(currentProperty.Name + ": " + currentProperty.DefaultValue);
            }
        }
    }
}
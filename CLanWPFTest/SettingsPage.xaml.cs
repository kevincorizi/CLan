using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

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
            SelectPicture sp = new SelectPicture();
            sp.Show();
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

            // Update data for current session
            App.me.Name = Properties.Settings.Default.Name;
            App.me.Picture = Properties.Settings.Default.PicturePath;
            Trace.WriteLine("Current session updated");

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
            Trace.WriteLine(pre);
            foreach (System.Configuration.SettingsProperty currentProperty in Properties.Settings.Default.Properties)
            {
                Trace.WriteLine(currentProperty.Name + ": " + currentProperty.DefaultValue);
            }
        }
    }
}
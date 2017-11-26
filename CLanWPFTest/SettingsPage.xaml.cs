using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

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
            Trace.WriteLine(Properties.Settings.Default.DefaultNetworkInterface);

            InterfacesList.SelectedItem = InterfacesList.Items.OfType<NetworkInterface>().
                FirstOrDefault(i => i.Id.CompareTo(Properties.Settings.Default.DefaultNetworkInterface) == 0);
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
        }

        #region NAME
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
        #endregion

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Now the modifications to settings become permanent

            // Check checkbox status
            Properties.Settings.Default.DefaultPrivate = (PrivateRadio.IsChecked == true);
            Properties.Settings.Default.DefaultAcceptTransfer = (AcceptAllTransfers.IsChecked == true);
            Properties.Settings.Default.DefaultAskSavePath = (UseDefaultPath.IsChecked != true);
            Properties.Settings.Default.DefaultSavePath = (PathText.Text);
            // Name is set individually
            // Picture is saved individually
            // TODO: backgroundpath
            // TODO: defaultrename
            Properties.Settings.Default.DefaultNetworkInterface = (InterfacesList.SelectedItem as NetworkInterface).Id;
            Properties.Settings.Default.Save();

            // Update data for current session
            App.me.Name = Properties.Settings.Default.Name;
            App.me.Picture = Properties.Settings.Default.PicturePath;

            if(NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            // Discard pending changes to settings
            Properties.Settings.Default.Reload();

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
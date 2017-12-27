using CLan.Networking;
using CLan.Objects;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
namespace CLan
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    /// 
    public static class ExtensionMethods
    {
        private static System.Action EmptyDelegate = delegate () { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }

    public partial class UsersWindow : Page
    {
        int counter = 0;
        bool isWindowOpen = false;
        public UsersWindow()
        {
            InitializeComponent();

            this.DataContext = this;
            this._continue.IsEnabled = false;    // Disable the "send" button until a user is selected.
            
        }

        // User list selection controller
        private void UserList_Selected(object sender, RoutedEventArgs e)
        {
            _continue.IsEnabled = true;
        }
        // Top toggle controller
        private void PrivateMode_Checked(object sender, RoutedEventArgs e)
        {
            CLanUDPManager.Instance.GoOffline();
        }
        private void PrivateMode_Unchecked(object sender, RoutedEventArgs e)
        {
            CLanUDPManager.Instance.GoOnline();
        }
        // Top flush controller
        private void FlushQueue_Click(object sender, RoutedEventArgs e)
        {
            App.SelectedFiles.Clear();
        }
        // Continue button controller
        private void ContinueClick(object sender, RoutedEventArgs e)
        {
            List<User> users = UserList.SelectedItems.OfType<User>().ToList();
            if(FileList.Items.Count > 0)
            {
                List<Objects.CLanFile> files = FileList.Items.OfType<Objects.CLanFile>().ToList();
                foreach (User u in users)
                {
                    Trace.WriteLine("UW.XAML.CS - ADDING FILE TRANSFER");
                    CLanFileTransfer cft = new CLanFileTransfer(u, files, CLanTransferType.SEND);
                    cft.Start();
                }
                App.SelectedFiles.Clear();
                Trace.WriteLine(App.SelectedFiles.Count);
            }
            else
            {
                NavigationService.Navigate(new FileSelection(users));
            }
        }
            
        // Settings menu icon toggle
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            counter++;
            if(counter % 2 == 1)  // open click
            {               
                while (this._SlidingMenu.Margin.Left != -10)
                {
                    _SlidingMenu.Margin = new Thickness(this._SlidingMenu.Margin.Left + 0.5, 0, 0, 0);
                    
                    this._SlidingMenu.Refresh();
                }
                while (this._SlidingMenu.Margin.Left != 0)
                {
                    _SlidingMenu.Margin = new Thickness(this._SlidingMenu.Margin.Left + 0.5, 0, 0, 0);
                    this._SlidingMenu.Refresh();
                    System.Threading.Thread.Sleep(1);                    
                }
            }
            else
            {                           
                while (this._SlidingMenu.Margin.Left != -310)  // close click
                {
                    this._SlidingMenu.Margin = new Thickness(this._SlidingMenu.Margin.Left - 0.5, 0, 0, 0);
                    this._SlidingMenu.Refresh();
                }
                while (this._SlidingMenu.Margin.Left != -320)
                {
                    _SlidingMenu.Margin = new Thickness(this._SlidingMenu.Margin.Left - 0.5, 0, 0, 0);
                    this._SlidingMenu.Refresh();
                    System.Threading.Thread.Sleep(1);
                }
            }
        }

        // Settings controllers
        private void changePicture_Click(object sender, RoutedEventArgs e)
        {
            SelectPicture newwindow = new SelectPicture();
            newwindow.ShowDialog(); // Open in modal mode: no other interaction is possible until the window is closed
        }
        private void DownloadPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = dialog.SelectedPath;
                PathText.Text = filePath;
            }
        }
        private void ChangeBGfile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
                       
            if (fd.ShowDialog() == DialogResult.OK)
            {
                string fileName = fd.FileName;
                SettingsManager.BackgroundPicture = fileName;
                background.Background = new ImageBrush(new BitmapImage(new System.Uri(fileName)));
            }
        }
        private void ChangeBGgallery_Click(object sender, RoutedEventArgs e)
        {
            SelectBackground newwindow = new SelectBackground();
            newwindow.ShowDialog();
        }
        private void _nightMode(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();
            if(Properties.Settings.Default.BackgroundPath == Properties.Settings.Default.SwapBackgroundPath)
                Properties.Settings.Default.BackgroundPath = Properties.Settings.Default.DarkBackgroundPath;
            _SlidingMenu.Background = (Brush)bc.ConvertFrom("#7e8489"); 
        }
        private void _notNightMode(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();
            if (Properties.Settings.Default.BackgroundPath == Properties.Settings.Default.DarkBackgroundPath)
                Properties.Settings.Default.BackgroundPath = Properties.Settings.Default.SwapBackgroundPath;
            _SlidingMenu.Background = (Brush)bc.ConvertFrom("#FFEFF4F9");
        }
        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            // Update data for current session
            App.me.Picture = SettingsManager.UserPicture;   // Set in separate window

            //Properties.Settings.Default.DefaultNetworkInterface = (InterfacesList.SelectedItem as System.Net.NetworkInformation.NetworkInterface).Id;

            SettingsManager.DefaultAcceptTransfer = (AcceptAllTransfers.IsChecked == true);
            SettingsManager.DefaultRenameOnDuplicate = (RenamingPolicy.IsChecked == true);
            // SettingsManager.DefaultHideNotifications = (HideAllNotifications.IsChecked == true);

            SettingsManager.SaveInDefaultPath = (UseDefaultPath.IsChecked != true);
            SettingsManager.DefaultSavePath = (PathText.Text);

            SettingsManager.DefaultPrivateMode = (PrivateRadio.IsChecked == true);
            SettingsManager.DefaultPublicMode = !SettingsManager.DefaultPrivateMode;

            App.Current.MainWindow.Background = new ImageBrush(new BitmapImage(new System.Uri(SettingsManager.BackgroundPicture)));

            // Now the modifications to settings become permanent
            SettingsManager.Save();
        }
        private void UndoSettings_Click(object sender, RoutedEventArgs e)
        {
            // Discard pending changes to settings
            SettingsManager.Undo();

            background.Background = new ImageBrush(new BitmapImage(new System.Uri(SettingsManager.BackgroundPicture)));
            App.Current.MainWindow.Background = new ImageBrush(new BitmapImage(new System.Uri(SettingsManager.BackgroundPicture)));
        }
    }
}
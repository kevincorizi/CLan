using CLan.Networking;
using CLan.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CLan
{
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
        public UsersWindow()
        {
            InitializeComponent();

            this.DataContext = this;           
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
            // If any file was selected via right click
            if(FileList.Items.Count > 0)
            {
                List<CLanFile> files = FileList.Items.OfType<CLanFile>().ToList();
                foreach (User u in users)
                {
                    Trace.WriteLine("UW.XAML.CS - ADDING FILE TRANSFER");
                    CLanFileTransfer cft = new CLanFileTransfer(u, files, CLanTransferType.SEND);
                    cft.Start();
                }
                App.SelectedFiles.Clear();
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
                while (this._SlidingMenu.Margin.Left != -320)  // close click
                {
                    this._SlidingMenu.Margin = new Thickness(this._SlidingMenu.Margin.Left - 0.5, 0, 0, 0);
                    this._SlidingMenu.Refresh();
                }
                while (this._SlidingMenu.Margin.Left != -330)
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

            userImage.Source = new BitmapImage(SettingsManager.UserPicture);
        }

        private void DownloadPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = dialog.SelectedPath;
                // The folder selector may return a value without the trailing folder separator
                // This would cause destination path errors
                if (!filePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    filePath += Path.DirectorySeparatorChar;
                PathText.Text = filePath;
            }
        }
        private void ChangeBGfile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
                       
            if (fd.ShowDialog() == DialogResult.OK)
            {
                SettingsManager.BackgroundPicture = new System.Uri(fd.FileName);
                background.Background = new ImageBrush(new BitmapImage(new System.Uri(fd.FileName)));               
            }
        }
        private void ChangeBGgallery_Click(object sender, RoutedEventArgs e)
        {
            SelectBackground newwindow = new SelectBackground();
            newwindow.ShowDialog();
            background.Background = new ImageBrush(new BitmapImage(SettingsManager.BackgroundPicture));
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
            App.Current.MainWindow.Background = new ImageBrush(new BitmapImage(SettingsManager.BackgroundPicture)); // Set in separate window

            SettingsManager.DefaultAcceptTransfer = (AcceptAllTransfers.IsChecked == true);
            SettingsManager.DefaultRenameOnDuplicate = (RenamingPolicy.IsChecked == true);

            SettingsManager.SaveInDefaultPath = (UseDefaultPath.IsChecked != true);
            SettingsManager.DefaultSavePath = PathText.Text;

            SettingsManager.DefaultPrivateMode = (PrivateRadio.IsChecked == true);
            SettingsManager.DefaultPublicMode = !SettingsManager.DefaultPrivateMode;

            // Now the modifications to settings become permanent
            SettingsManager.Save();
        }

        private void UndoSettings_Click(object sender, RoutedEventArgs e)
        {
            // Discard pending changes to settings
            SettingsManager.Undo();

            // Revert the manually set values
            background.Background = new ImageBrush(new BitmapImage(SettingsManager.BackgroundPicture));
            userImage.Source = new BitmapImage(SettingsManager.UserPicture);
        }
    }
}
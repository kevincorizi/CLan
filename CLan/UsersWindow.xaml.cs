using CLan.Networking;
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
        private void PrivateMode_Checked(object sender, RoutedEventArgs e)
        {
            CLanUDPManager.Instance.GoOffline();
        }

        private void PrivateMode_Unchecked(object sender, RoutedEventArgs e)
        {
            CLanUDPManager.Instance.GoOnline();
        }

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
                while (this._SlidingMenu.Margin.Left != -290)  // close click
                {
                    this._SlidingMenu.Margin = new Thickness(this._SlidingMenu.Margin.Left - 0.5, 0, 0, 0);
                    this._SlidingMenu.Refresh();
                }
                while (this._SlidingMenu.Margin.Left != -300)
                {
                    _SlidingMenu.Margin = new Thickness(this._SlidingMenu.Margin.Left - 0.5, 0, 0, 0);
                    this._SlidingMenu.Refresh();
                    System.Threading.Thread.Sleep(1);
                }
            }
        }

        private void changePicture_Click(object sender, RoutedEventArgs e)
        {

            // Prevent the user from opening the same window twice 
            
            foreach (Window w in App.Current.Windows)
            {
                if (w is SelectPicture)
                {
                    isWindowOpen = true;
                    w.Activate();
                }
            }

            if (!isWindowOpen)
            {
                SelectPicture newwindow = new SelectPicture();
                newwindow.Show();
            }

        }

        #region NAME
        private void EditName_Click(object sender, RoutedEventArgs e)
        {
            // TransparencyLayer.Visibility = Visibility.Visible;
            // NameBox.Visibility = Visibility.Visible;
        }

        #endregion

        private void DownloadPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            string filePath = dialog.SelectedPath;
            PathText.Text = filePath;
        }

        private void UserList_Selected(object sender, RoutedEventArgs e)
        {
            _continue.IsEnabled = true;
        }

        private void ChangeBGfile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
                       
            if (fd.ShowDialog() == DialogResult.OK)
            {
                string fileName = fd.FileName;
                Properties.Settings.Default.BackgroundPath = fileName;
                
            }
        }

        private void ChangeBGgallery_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window w in App.Current.Windows)
            {
                if (w is SelectBackground)
                {
                    isWindowOpen = true;
                    w.Activate();
                }
            }

            if (!isWindowOpen)
            {
                SelectBackground newwindow = new SelectBackground();
                newwindow.Show();
            }
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

        private void FlushQueue_Click(object sender, RoutedEventArgs e)
        {
            App.SelectedFiles.Clear();
        }
    }
}
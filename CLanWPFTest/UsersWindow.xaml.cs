using CLanWPFTest.Networking;
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
namespace CLanWPFTest
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
        private object _selectPicture;
        
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
               
                while (this._SlidingMenu.Margin.Left != -255)  // close click
                {
                    this._SlidingMenu.Margin = new Thickness(this._SlidingMenu.Margin.Left - 0.5, 0, 0, 0);
                    this._SlidingMenu.Refresh();
                }
                while (this._SlidingMenu.Margin.Left != -265)
                {
                    _SlidingMenu.Margin = new Thickness(this._SlidingMenu.Margin.Left - 0.5, 0, 0, 0);
                    this._SlidingMenu.Refresh();
                    System.Threading.Thread.Sleep(1);

                }
            }
        }

        private void changePicture_Click(object sender, RoutedEventArgs e)
        {
             SelectPicture sp = new SelectPicture();
             sp.Show();
            
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
    }
}
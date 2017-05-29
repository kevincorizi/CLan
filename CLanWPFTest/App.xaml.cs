using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CLanWPFTest
{
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    public partial class App : Application
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private bool _isExit;

        public static Task listener;
        public static Task advertiser;
        public static CancellationTokenSource ctsAd;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = new MainWindow();
            MainWindow.Closing += MainWindow_Closing;

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
            _notifyIcon.Icon = CLanWPFTest.Properties.Resources.TrayIcon;
            _notifyIcon.Visible = true;

            CreateContextMenu();

            ctsAd = new CancellationTokenSource();
            CancellationToken ctAd = ctsAd.Token;

            listener = Task.Run(CLanUDPManager.StartAdListening);
            advertiser = Task.Run(() => CLanUDPManager.StartBroadcastAdvertisement(ctAd), ctAd);
        }

        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip =
            new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Apri CLan").Click += (s, e) => ShowMainWindow();
            _notifyIcon.ContextMenuStrip.Items.Add("Esci").Click += (s, e) => ExitApplication();
        }

        private void ExitApplication()
        {
            CLanUDPManager.GoOffline();
            _isExit = true;
            MainWindow.Close();            
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        private void ShowMainWindow()
        {
            if (MainWindow.IsVisible)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    MainWindow.WindowState = WindowState.Normal;
                }
                MainWindow.Activate();
            }
            else
            {
                MainWindow.Show();
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
                MainWindow.Hide(); // A hidden window can be shown again, a closed one not
            }
        }
    }
}
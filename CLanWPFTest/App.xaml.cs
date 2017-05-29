using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace CLanWPFTest
{
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;

        public static Task listener;
        public static Task advertiser;
        private static CancellationTokenSource ctsAd;

        public static FileSelection fs = null;
        public static FileTransfer ft;

        public static User me;

        private readonly Dispatcher uiDispatcher = Dispatcher.CurrentDispatcher;
    
        private static ObservableCollection<User> _onlineUsers = new ObservableCollection<User>();
        public static ObservableCollection<User> OnlineUsers { get { return _onlineUsers; } }

        public static void AddUser(User u)
        {
            if (!_onlineUsers.Contains(u))
                _onlineUsers.Add(u);
        }

        public static void RemoveUser(User u)
        {
            if (_onlineUsers.Contains(u))
                _onlineUsers.Remove(u);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.DoubleClick += (s, args) => ShowFileSelection();
            _notifyIcon.Icon = CLanWPFTest.Properties.Resources.TrayIcon;
            _notifyIcon.Visible = true;

            CreateContextMenu();

            me = new User("Kevin Corizi");

            ActivateAdvertising();
        }

        public static void ActivateAdvertising()
        {
            ctsAd = new CancellationTokenSource();
            CancellationToken ctAd = ctsAd.Token;

            listener = Task.Run(() => CLanUDPManager.StartAdListening());
            advertiser = Task.Run(() => CLanUDPManager.StartBroadcastAdvertisement(ctAd), ctAd);
        }

        public static void DeactivateAdvertising()
        {
            ctsAd.Cancel();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (fs == null)
                fs = new FileSelection();
            ShowFileSelection();
        }

        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Apri CLan").Click += (s, e) => ShowFileSelection();
            _notifyIcon.ContextMenuStrip.Items.Add("Impostazioni").Click += (s, e) => ShowSettings();
            _notifyIcon.ContextMenuStrip.Items.Add("Attiva modalità privata").Click += (s, e) => TraySwitchToPrivate(s);
            _notifyIcon.ContextMenuStrip.Items.Add("Esci").Click += (s, e) => Current.Shutdown();
        }

        private void ShowFileSelection()
        {
            if (fs.IsVisible) { 
                if (fs.WindowState == WindowState.Minimized)
                {
                    fs.WindowState = WindowState.Normal;
                }
                fs.Activate();
            }
            else
            {
                fs.Show();
            }
        }

        private void ShowSettings()
        {
            // SHOW SETTINGS PAGE
        }

        private void TraySwitchToPrivate(object sender)
        {
            CLanUDPManager.GoOffline();
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                menuItem.Text = "Attiva modalità pubblica";
                menuItem.Click -= (s, e) => TraySwitchToPrivate(s);
                menuItem.Click += (s, e) => TraySwitchToPublic(s);
            }
        }

        private void TraySwitchToPublic(object sender)
        {
            CLanUDPManager.GoOnline();
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                menuItem.Text = "Attiva modalità privata";
                menuItem.Click -= (s, e) => TraySwitchToPublic(s);
                menuItem.Click += (s, e) => TraySwitchToPrivate(s);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Console.WriteLine("OnExit");
            CLanUDPManager.GoOffline();
            foreach (Window window in Current.Windows)
            {
                window.Close();
            }
            _notifyIcon.Dispose();
            _notifyIcon = null;
            base.OnExit(e);
        }
    }
}
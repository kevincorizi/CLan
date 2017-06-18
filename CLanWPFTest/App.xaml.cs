using CLanWPFTest.Networking;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace CLanWPFTest
{
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        /// <summary>
        /// List containing currently visible users on the network
        /// </summary>
        public static ObservableCollection<User> OnlineUsers { get; set; }

        // User will see one progress bar for each batch of files to the same destinations.
        // In this way we do not clutter the interface too much and we are still able to stay responsive and clear
        public static ObservableCollection<CLanFileTransfer> FileTransfers { get; set; }

        /// <summary>
        /// Current user
        /// </summary>
        public static User me { get; set; }

        private static int KEEP_ALIVE_TIMER_MILLIS = 10 * 1000;

        private NotifyIcon _notifyIcon;

        public static Task listener, advertiser, tcpListener, cleaner;
        private static CancellationTokenSource ctsAd;

        public static MainWindow mw = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.DoubleClick += (s, args) => ShowFileSelection();
            _notifyIcon.Icon = CLanWPFTest.Properties.Resources.TrayIcon;
            _notifyIcon.Visible = true;

            CreateContextMenu();

            // Initialize the list of users 
            OnlineUsers = new ObservableCollection<User>();

            FileTransfers = new ObservableCollection<CLanFileTransfer>();

            // Initialize current user with name from last saved settings
            me = new User(CLanWPFTest.Properties.Settings.Default.Name);

            ActivateAdvertising();
            ActivateUserCleaner();
            ActivateTCPListener();

            StartUpManager.AddApplicationToCurrentUserStartup();
        }

        public static void AddUser(User u)
        {
            if (!OnlineUsers.Contains(u)) {
                u.lastKeepAlive = DateTime.Now;
                OnlineUsers.Add(u);
            }
            else {
                User target = OnlineUsers.Single(user => user.Equals(u));

                // Refresh the timer for the user
                target.lastKeepAlive = DateTime.Now;

                // Update fields (in case the user updated name or picture)
                // These modifications will be visible because User implements INotifyPropertyChanged
                target.Name = u.Name;
                target.Picture = u.Picture;
            }
        }

        public static void RemoveUser(User u)
        {
            if (OnlineUsers.Contains(u))
                OnlineUsers.Remove(u);
        }

        public static void AddTransfer(CLanFileTransfer cft)
        {
            if(!FileTransfers.Contains(cft))
            {
                FileTransfers.Add(cft);
                Trace.WriteLine("APP.XAML.CS - TRANSFER ADDED");
            }
            else
            {
                Trace.WriteLine("Trying to insert a duplicate file transfer");
            }
        }

        public static void RemoveTransfer(CLanFileTransfer cft)
        {
            if(FileTransfers.Contains(cft))
            {
                cft.Stop();
                FileTransfers.Remove(cft);
            }
            else
            {
                Trace.WriteLine("Trying to remove a non-existent file transfer");
            }
        }
        public static void ActivateUserCleaner()
        {
            cleaner = Task.Run(() => {
                while (true) {
                    Trace.WriteLine("Cleaning...");
                    DateTime now = DateTime.Now;
                    foreach (User u in OnlineUsers) {
                        if((now.Subtract(u.lastKeepAlive)).Seconds > KEEP_ALIVE_TIMER_MILLIS / 1000) {
                            Trace.WriteLine("User is too old, removing");
                            Current.Dispatcher.BeginInvoke(new Action(() => RemoveUser(u)));
                        }
                    }
                    Thread.Sleep(KEEP_ALIVE_TIMER_MILLIS);
                }
            });
        }

        public static void ActivateAdvertising()
        {
            ctsAd = new CancellationTokenSource();
            CancellationToken ctAd = ctsAd.Token;

            listener = Task.Run(() => CLanUDPManager.StartAdListening());
            advertiser = Task.Run(() => CLanUDPManager.StartBroadcastAdvertisement(ctAd), ctAd);
        }

        public static void ActivateTCPListener()
        {
            tcpListener = Task.Run(() => CLanTCPManager.StartListening());
        }

        public static void DeactivateAdvertising()
        {
            ctsAd.Cancel();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (mw == null)
                mw = new MainWindow();
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
            if (mw.IsVisible) { 
                if (mw.WindowState == WindowState.Minimized)
                    mw.WindowState = WindowState.Normal;
                mw.Activate();
            }
            else
                mw.Show();
            mw._mainFrame.Navigate(new FileSelection());
        }

        private void ShowSettings()
        {
            if (mw.IsVisible)
            {
                if (mw.WindowState == WindowState.Minimized)
                    mw.WindowState = WindowState.Normal;
                mw.Activate();
            }
            else
                mw.Show();
            mw._mainFrame.Navigate(new SettingsPage());
        }

        private void TraySwitchToPrivate(object sender)
        {
            CLanUDPManager.GoOffline();
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null) {
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
            if (menuItem != null) {
                menuItem.Text = "Attiva modalità privata";
                menuItem.Click -= (s, e) => TraySwitchToPublic(s);
                menuItem.Click += (s, e) => TraySwitchToPrivate(s);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Trace.WriteLine("OnExit");
            CLanUDPManager.GoOffline();
            // Close all windows
            foreach (Window window in Current.Windows)
                window.Close();

            _notifyIcon.Dispose();
            _notifyIcon = null;
            base.OnExit(e);
        }        
    }
}
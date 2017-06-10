using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
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
    public partial class Application : System.Windows.Application
    {
        private static int KEEP_ALIVE_TIMER_MILLIS = 10 * 1000;

        private NotifyIcon _notifyIcon;

        public static Task listener, advertiser, tcpListener, cleaner;
        private static CancellationTokenSource ctsAd;

        public static MainWindow mw = null;
        public static FileTransfer ft;

        /// <summary>
        /// Current user
        /// </summary>
        public static User me;
    
        /// <summary>
        /// List containing currently visible users on the network
        /// </summary>
        public static ObservableCollection<User> OnlineUsers = new ObservableCollection<User>();

        /// <summary>
        /// Data context for the GUI list box
        /// </summary>
        public static ObservableCollection<CLanReceivedFile> ReceivedFiles = new ObservableCollection<CLanReceivedFile>();

        /// <summary>
        /// References to received files by remote ConnectionInfo
        /// </summary>
        public static Dictionary<ConnectionInfo, Dictionary<string, CLanReceivedFile>> ReceivedFilesDict = new Dictionary<ConnectionInfo, Dictionary<string, CLanReceivedFile>>();

        /// <summary>
        /// Incoming partial data cache. Keys are ConnectionInfo, PacketSequenceNumber. Value is partial packet data.
        /// </summary>
        public static Dictionary<ConnectionInfo, Dictionary<long, byte[]>> IncomingDataCache = new Dictionary<ConnectionInfo, Dictionary<long, byte[]>>();

        /// <summary>
        /// Incoming sendInfo cache. Keys are ConnectionInfo, PacketSequenceNumber. Value is sendInfo.
        /// </summary>
        public static Dictionary<ConnectionInfo, Dictionary<long, CLanFileInfo>> IncomingDataInfoCache = new Dictionary<ConnectionInfo, Dictionary<long, CLanFileInfo>>();

        /// <summary>
        /// Object used for ensuring thread safety.
        /// </summary>
        public static object syncRoot = new object();

        public static void AddUser(User u)
        {
            if (!OnlineUsers.Contains(u)) {
                u.lastKeepAlive = DateTime.Now;
                OnlineUsers.Add(u);
            }
            else {
                OnlineUsers.Single(user => user.Equals(u)).lastKeepAlive = DateTime.Now;
            }
        }

        public static void RemoveUser(User u)
        {
            if (OnlineUsers.Contains(u))
                OnlineUsers.Remove(u);
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
            ActivateUserCleaner();
            CLanTCPManager.StartListening();

            StartUpManager.AddApplicationToCurrentUserStartup();
        }

        public static void ActivateUserCleaner()
        {
            cleaner = Task.Run(() => {
                while (true) {
                    Console.WriteLine("Cleaning...");
                    DateTime now = DateTime.Now;
                    foreach (User u in OnlineUsers) {
                        if((now.Subtract(u.lastKeepAlive)).Seconds > KEEP_ALIVE_TIMER_MILLIS / 1000) {
                            Console.WriteLine("User is too old, removing");
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
                mw._mainFrame.Visibility = Visibility.Visible;
            }
            else
                mw.Show();  
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
            Console.WriteLine("OnExit");
            CLanUDPManager.GoOffline();
            // Close all windows
            foreach (Window window in Current.Windows)
                window.Close();

            //Close all files
            lock (syncRoot)
            {
                foreach (CLanReceivedFile file in ReceivedFiles)
                    file.Close();
            }

            NetworkComms.Shutdown();
            _notifyIcon.Dispose();
            _notifyIcon = null;
            base.OnExit(e);
        }

        
    }
}
using CLan.Networking;
using CLan.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace CLan
{
    public partial class App : System.Windows.Application
    {     
        #region Users
        // The current user
        public static User me { get; set; }
        // List containing currently visible users on the network.
        // It is only manipulated by the App dispatcher, so no need for it to be thread-safe
        public static ObservableCollection<User> OnlineUsers { get; set; }
        private void AddUser(object sender, User u)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (!OnlineUsers.Contains(u))
                {
                    u.lastKeepAlive = DateTime.Now;
                    OnlineUsers.Add(u);
                }
                else
                {
                    User target = OnlineUsers.Single(user => user.Equals(u));
                    // Refresh the timer for the user
                    target.lastKeepAlive = DateTime.Now;
                    // Update fields (in case the user updated name or picture)
                    // These modifications will be visible because User implements INotifyPropertyChanged
                    target.Name = u.Name;
                    target.Picture = u.Picture;
                }
            });
        }
        private void RemoveUser(object sender, User u)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (OnlineUsers.Contains(u))
                    OnlineUsers.Remove(u);
            });
        }
        #endregion

        #region Transfers
        // The user will see one progress bar for each batch of files to the same destination.
        // In this way we do not clutter the interface too much and we are still able to stay responsive and clear.
        // Even if this lists are conceptually manipulated by multiple threads, the actual manipulation is
        // delegated to the TransferWindow dispatcher, so it is not necessary for them to be thread-safe
        public static ObservableCollection<CLanFileTransfer> IncomingTransfers { get; set; }
        public static ObservableCollection<CLanFileTransfer> OutgoingTransfers { get; set; }

        // The user can select a certain set of files at each transfer, either via right click
        // or via graphical interface. Since there is a single point of access to this set, it 
        // is not necessary for it to be thread-safe
        public static ObservableCollection<CLanFile> SelectedFiles { get; set; }
        private void AddTransfer(object sender, CLanFileTransfer cft)
        {
            switch (cft.Type)
            {
                case CLanTransferType.RECEIVE:
                    TransferWindow.Dispatcher.Invoke(() => IncomingTransfers.Add(cft));
                    Trace.WriteLine("APP.XAML.CS - INCOMING TRANSFER ADDED");
                    break;
                case CLanTransferType.SEND:
                    TransferWindow.Dispatcher.Invoke(() => OutgoingTransfers.Add(cft));
                    Trace.WriteLine("APP.XAML.CS - OUTGOING TRANSFER ADDED");
                    break;
                default:
                    Trace.WriteLine("Invalid transfer-added type, please check your code");
                    break;
            }
            ShowTransferWindow();
        }
        private void RemoveTransfer(object sender, CLanFileTransfer cft)
        {
            switch (cft.Type)
            {
                case CLanTransferType.RECEIVE:
                    TransferWindow.Dispatcher.Invoke(() => IncomingTransfers.Remove(cft));
                    Trace.WriteLine("APP.XAML.CS - INCOMING TRANSFER ADDED");
                    break;
                case CLanTransferType.SEND:
                    TransferWindow.Dispatcher.Invoke(() => OutgoingTransfers.Remove(cft));
                    Trace.WriteLine("APP.XAML.CS - OUTGOING TRANSFER ADDED");
                    break;
                default:
                    Trace.WriteLine("Invalid transfer-removed type, please check your code");
                    break;
            }
        }
        #endregion

        #region Background Services
        private CLanTCPManager TCPManager;
        private CLanUDPManager UDPManager;

        private Task listener, advertiser, tcpListener, cleaner;
        private CancellationTokenSource ctsListener, ctsAdvertiser, ctsTcpListener, ctsCleaner;

        private void StartServices()
        {
            if (IsConnectionActive())
            {
                me = new User(SettingsManager.Username);

                if (SettingsManager.DefaultPublicMode)
                {
                    ActivateAdvertising();
                }
                ActivateUDPListener();
                ActivateUserCleaner();
                ActivateTCPListener();
            }
        }
        private void StopServices()
        {
            DeactivateAdvertising();
            DeactivateUDPListener();
            DeactivateTCPListener();
            DeactivateUserCleaner();
        }
        private void RestartServices()
        {
            StopServices();
            StartServices();
        }
        private void ActivateAdvertising(object sender = null, EventArgs args = null)
        {
            Trace.WriteLine("ActivateAdvertising");
            ctsAdvertiser = new CancellationTokenSource();
            CancellationToken ctAdvertiser = ctsAdvertiser.Token;
            advertiser = Task.Run(() => UDPManager.StartAdvertisement(ctAdvertiser), ctAdvertiser);
        }
        private void DeactivateAdvertising(object sender = null, EventArgs args = null)
        {
            Trace.WriteLine("DectivateAdvertising");
            ctsAdvertiser?.Cancel();        // May still be null if not started because in private mode
        }
        private void ActivateUDPListener()
        {
            Trace.WriteLine("ActivateUDPListener");
            ctsListener = new CancellationTokenSource();
            CancellationToken ctListener = ctsListener.Token;
            listener = Task.Run(() => UDPManager.StartListening(ctListener), ctListener);
        }
        private void DeactivateUDPListener()
        {
            Trace.WriteLine("DectivateUDPListener");
            ctsListener.Cancel();
        }

        private void ActivateUserCleaner()
        {
            Trace.WriteLine("ActivateUserCleaner");
            ctsCleaner = new CancellationTokenSource();
            CancellationToken ctCleaner = ctsCleaner.Token;
            cleaner = Task.Run(() => CleanUsers(ctCleaner), ctCleaner);
        }
        private void DeactivateUserCleaner()
        {
            Trace.WriteLine("DeactivateUserCleaner");
            ctsCleaner.Cancel();
        }
        private void CleanUsers(CancellationToken ct)
        {
            do
            {
                DateTime now = DateTime.Now;
                foreach (User u in OnlineUsers)
                {
                    if ((now.Subtract(u.lastKeepAlive)).Milliseconds > UDPManager.KEEP_ALIVE_TIMER_MILLIS)
                    {
                        Trace.WriteLine("User is too old, removing");
                        UDPManager.OnUserLeave(u);
                    }
                }
            } while (!ct.WaitHandle.WaitOne(UDPManager.KEEP_ALIVE_TIMER_MILLIS));

            if (ct.IsCancellationRequested)
            {
                Trace.WriteLine("Terminating cleaning");
            }
        }

        private void ActivateTCPListener()
        {
            ctsTcpListener = new CancellationTokenSource();
            CancellationToken ctTcpListener = ctsTcpListener.Token;
            tcpListener = Task.Run(() => TCPManager.StartListening(ctTcpListener), ctTcpListener);
        }
        private void DeactivateTCPListener()
        {
            Trace.WriteLine("DectivateTCPListening");
            ctsTcpListener.Cancel();
        }
        #endregion

        #region Connectivity Management
        // This method only notifies changes in the network availability while the app is running
        private void ChangeNetworkAvailability(object sender, NetworkAvailabilityEventArgs e)
        {
            Trace.WriteLine("Network availability changed");
            App.OnlineUsers?.Clear();
            if (e.IsAvailable)
            {
                StartServices();
            }
            else
            {
                StopServices();
            }
        }
        // This method checks for any active and operational interface to determine if the connection
        // is available at any moment, expecially at the application startup
        private bool IsConnectionActive()
        {
            if (NetworkInterface.GetAllNetworkInterfaces().Where(
                    nic => nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && nic.OperationalStatus == OperationalStatus.Up
                ).ToList().Count != 0)
                return true;
            return false;
        }
        #endregion

        #region Right Click
        // A (possibly) unique identifier for the instantiated mutex
        private readonly string AppID = "CLan_Akcora_Corizi_fvbnjefkod9c8ygdbnemkcixusygw";
        private bool ownsMutex;
        private Mutex instanceMutex;

        private void PassParameters(string[] args)
        {
            using (NamedPipeClientStream client = new NamedPipeClientStream(AppID))
            using (StreamWriter writer = new StreamWriter(client))
            {
                client.Connect(200);

                foreach (String argument in args)
                    writer.WriteLine(argument);
            }
        }
        private void StartReadParameters()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    using (NamedPipeServerStream server = new NamedPipeServerStream(AppID))
                    using (StreamReader reader = new StreamReader(server))
                    {
                        server.WaitForConnection();

                        List<String> arguments = new List<String>();
                        while (server.IsConnected && !reader.EndOfStream)
                        {
                            arguments.Add(reader.ReadLine());
                        }
                        // Here i have the list of files for the current right click
                        StoreParameters(arguments);
                    }
                }
            });
        }
        private void StoreParameters(List<string> parameters)
        {
            List<CLanFile> batchFiles = CLanFile.GetFiles(parameters);
            // The dispatcher is needed because this method is called from a separated thread
            Current.Dispatcher.Invoke(() =>
            {
                batchFiles.ForEach(SelectedFiles.Add);
            });
        }
        #endregion

        #region Windows
        public MainWindow mw = null;
        public FileTransferWindow TransferWindow = null;
        private void ShowUsersWindow()
        {
            if (mw.IsVisible)
            {
                if (mw.WindowState == WindowState.Minimized)
                    mw.WindowState = WindowState.Normal;
                mw.Activate();
            }
            else
                mw.Show();
            mw._mainFrame.Navigate(new UsersWindow());
        }

        private void ShowTransferWindow(object sender = null, EventArgs e = null)
        {
            App.Current.Dispatcher.Invoke(() => TransferWindow.Show());
        }
        private void CloseTransferWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Do not close the window if there are ongoing file transfers,
            // Otherwise you lose the chance to check them and stop them
            e.Cancel = true;
            if (IncomingTransfers.Count != 0 || OutgoingTransfers.Count != 0)
            {
                TransferWindow.WindowState = WindowState.Minimized;
            }
            else
            {
                TransferWindow.Visibility = Visibility.Hidden;
            }
        }
        #endregion

        #region TrayIcon
        private NotifyIcon NotifyIcon;
        private void CreateContextMenu()
        {
            NotifyIcon = new NotifyIcon();
            NotifyIcon.DoubleClick += (s, args) => ShowUsersWindow();
            NotifyIcon.Icon = CLan.Properties.Resources.TrayIcon;
            NotifyIcon.Visible = true;

            NotifyIcon.ContextMenuStrip = new ContextMenuStrip();
            NotifyIcon.ContextMenuStrip.Items.Add("Open CLan").Click += (s, e) => ShowUsersWindow();
            if(SettingsManager.DefaultPrivateMode)
                NotifyIcon.ContextMenuStrip.Items.Add("Public mode").Click += (s, e) => TraySwitchToPublic(s);
            else
                NotifyIcon.ContextMenuStrip.Items.Add("Private mode").Click += (s, e) => TraySwitchToPrivate(s);
            NotifyIcon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => Current.Shutdown();
        }
        private void TraySwitchToPrivate(object sender)
        {
            UDPManager.GoOffline();
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null) {
                menuItem.Text = "Public mode";
                menuItem.Click -= (s, e) => TraySwitchToPrivate(s);
                menuItem.Click += (s, e) => TraySwitchToPublic(s);
            }
        }
        private void TraySwitchToPublic(object sender)
        {
            UDPManager.GoOnline();
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null) {
                menuItem.Text = "Private mode";
                menuItem.Click -= (s, e) => TraySwitchToPublic(s);
                menuItem.Click += (s, e) => TraySwitchToPrivate(s);
            }
        }
        protected override void OnExit(ExitEventArgs e)
        {
            // All the shutdown operations are necessary only for the first instance of the app.
            // This is because any further instance will not start any of the app services,
            // it will only connect to the pipe, convey a list of files, and terminate 
            if (instanceMutex != null && ownsMutex)
            {
                Trace.WriteLine("OnExit");
                instanceMutex.ReleaseMutex();
                instanceMutex = null;
                Trace.WriteLine("Mutex released");

                StopServices();

                // Close all windows
                foreach (Window window in Current.Windows)
                    window.Close();

                NotifyIcon.Dispose();
                NotifyIcon = null;
                Trace.WriteLine("NotifyIcon disposed");
            }
            base.OnExit(e);
        }
        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            // This method checks if another instance of the program already exists
            // This means that the user right-clicked on some files, starting a new process.
            // In this case, the second instance must only pass the files and close immediately
            instanceMutex = new Mutex(true, AppID, out ownsMutex);
            if (ownsMutex)
            {
                // First instance, start server
                StartReadParameters();
            }
            else
            {
                // Second instance, start client
                PassParameters(e.Args);
                Current.Shutdown();
                Environment.Exit(0);
            }

            base.OnStartup(e);

            TCPManager = CLanTCPManager.Instance;

            UDPManager = CLanUDPManager.Instance;
            UDPManager.UserJoin += AddUser;
            UDPManager.UserLeave += RemoveUser;
            UDPManager.ToggleOnline += ActivateAdvertising;
            UDPManager.ToggleOffline += DeactivateAdvertising;

            CLanFileTransfer.TransferAdded += AddTransfer;
            CLanFileTransfer.TransferRemoved += RemoveTransfer;

            CreateContextMenu();

            OnlineUsers = new ObservableCollection<User>();
            IncomingTransfers = new ObservableCollection<CLanFileTransfer>();
            OutgoingTransfers = new ObservableCollection<CLanFileTransfer>();
            SelectedFiles = new ObservableCollection<CLanFile>();

            NetworkChange.NetworkAvailabilityChanged += ChangeNetworkAvailability;

            StartServices();

            if (mw == null)
                mw = new MainWindow();
            if (TransferWindow == null)
            {
                TransferWindow = new FileTransferWindow();
                TransferWindow.Display += ShowTransferWindow;
                TransferWindow.Closing += CloseTransferWindow;
            }
            ShowUsersWindow();

            if (e.Args.Length > 0)
            {
                StoreParameters(e.Args.ToList());
            }
        }
    }
}
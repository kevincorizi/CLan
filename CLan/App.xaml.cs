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
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        #region Collections
        /// List containing currently visible users on the network.
        /// It is only updated by the UDPManager, so no need for it to be thread-safe
        public static ObservableCollection<User> OnlineUsers { get; set; }
       
        // User will see one progress bar for each batch of files to the same destinations.
        // In this way we do not clutter the interface too much and we are still able to stay responsive and clear
        // This list will be modified by multiple threads (one per file transfer). We have to make sure that each thread
        // only accesses one element of the list, so it has to be thread-safe
        public static ObservableCollection<CLanFileTransfer> IncomingTransfers { get; set; }
        public static ObservableCollection<CLanFileTransfer> OutgoingTransfers { get; set; }
        public static ObservableCollection<CLanFile> SelectedFiles { get; set; }
        public static ObservableCollection<NetworkInterface> Interfaces { get; set; }
        #endregion
        // Current user
        public static User me { get; set; }

        private NotifyIcon NotifyIcon;

        private Task listener, advertiser, tcpListener, cleaner;
        private CancellationTokenSource ctsListener, ctsAdvertiser, ctsTcpListener, ctsCleaner;

        public MainWindow mw = null;
        public FileTransferWindow TransferWindow = null;
        private CLanTCPManager TCPManager;
        private CLanUDPManager UDPManager;

        private readonly string AppID = "CLan_Akcora_Corizi_fvbnjefkod9c8ygdbnemkcixusygw";
        private bool ownsMutex;
        private Mutex instanceMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            // This method checks if another instance of the program already exists
            // This means that the user right-clicked on some files, starting a new process.
            // In this case, the second instance must only pass the files and close immediately
            instanceMutex = new Mutex(true, AppID, out ownsMutex);
            if(ownsMutex)
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

            NotifyIcon = new NotifyIcon();
            NotifyIcon.DoubleClick += (s, args) => ShowUsersWindow();
            NotifyIcon.Icon = CLan.Properties.Resources.TrayIcon;
            NotifyIcon.Visible = true;

            CreateContextMenu();

            OnlineUsers = new ObservableCollection<User>();
            IncomingTransfers = new ObservableCollection<CLanFileTransfer>();
            OutgoingTransfers = new ObservableCollection<CLanFileTransfer>();
            SelectedFiles = new ObservableCollection<CLanFile>();
            Interfaces = new ObservableCollection<NetworkInterface>();
            SetupNetworkInterfaces();

            // Initialize current user with name from last saved settings
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

            if(e.Args.Length > 0)
            {
                StoreParameters(e.Args.ToList());
            }
        }

        #region Users
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
        private void AddTransfer(object sender, CLanFileTransfer cft)
        {
            switch(cft.Type)
            {
                case CLanTransferType.RECEIVE:
                    TransferWindow.Dispatcher.Invoke(()=> IncomingTransfers.Add(cft));
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
        private void StartServices()
        {
            if(IsConnectionActive())
            {
                me = new User(CLan.Properties.Settings.Default.Name);

                ActivateAdvertising();
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
            ctsAdvertiser.Cancel();
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
                Thread.Sleep(UDPManager.KEEP_ALIVE_TIMER_MILLIS);
            } while (!ct.WaitHandle.WaitOne(UDPManager.KEEP_ALIVE_TIMER_MILLIS));
                
            if(ct.IsCancellationRequested)
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

        #region NETWORK INTERFACES
        private void SetupNetworkInterfaces()
        {
            Interfaces = new ObservableCollection<NetworkInterface>(
                NetworkInterface.GetAllNetworkInterfaces().Where(
                    nic => nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && nic.OperationalStatus == OperationalStatus.Up
                )
            );
            NetworkChange.NetworkAvailabilityChanged += ChangeNetworkAvailability;
        }
        private void ChangeNetworkAvailability(object sender, NetworkAvailabilityEventArgs e)
        {
            SetupNetworkInterfaces();
            Trace.WriteLine("Network availability changed");
            if(e.IsAvailable)
            {
                StartServices();
            }
            else
            {
                StopServices();
            }
        }
        
        private bool IsConnectionActive()
        {
            foreach(NetworkInterface nic in Interfaces)
            {
          
                if(nic.OperationalStatus == OperationalStatus.Up)
                {
                    Trace.WriteLine(nic.Description);
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Right Click
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
                while(true)
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
            Current.Dispatcher.Invoke(() =>
            {
                batchFiles.ForEach(SelectedFiles.Add);
            });            
        }
        #endregion

        #region Windows
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
        private void CreateContextMenu()
        {
            NotifyIcon.ContextMenuStrip = new ContextMenuStrip();
            NotifyIcon.ContextMenuStrip.Items.Add("Apri CLan").Click += (s, e) => ShowUsersWindow();
            //NotifyIcon.ContextMenuStrip.Items.Add("Impostazioni").Click += (s, e) => ShowSettings();
            NotifyIcon.ContextMenuStrip.Items.Add("Attiva modalità privata").Click += (s, e) => TraySwitchToPrivate(s);
            NotifyIcon.ContextMenuStrip.Items.Add("Esci").Click += (s, e) => Current.Shutdown();
        }
        /*
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
        */
        private void TraySwitchToPrivate(object sender)
        {
            UDPManager.GoOffline();
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
            UDPManager.GoOnline();
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
            if(instanceMutex != null && ownsMutex)
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
    }
    #endregion
}
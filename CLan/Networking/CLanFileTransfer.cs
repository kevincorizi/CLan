using CLan.Objects;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace CLan.Networking
{
    public enum CLanTransferType { SEND, RECEIVE };
    public class CLanFileTransfer : INotifyPropertyChanged
    {
        public User Other { get; set; }
        public List<CLanFile> Files;
        public CLanTransferType Type;

        [JsonIgnore]
        private string currentFile;
        [JsonIgnore]
        public string CurrentFile {
            get
            {
                return currentFile;
            }
            set {
                if(value != currentFile)
                {
                    currentFile = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [JsonIgnore]
        public bool IsPending { get; set; }
        [JsonIgnore]
        private int progress;
        [JsonIgnore]
        public int Progress {
            get
            {
                return progress;
            }
            set
            {
                if(value != progress)
                {
                    progress = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [JsonIgnore]
        private TimeSpan secondsLeft;
        [JsonIgnore]
        public TimeSpan SecondsLeft
        {
            get
            {
                return secondsLeft;
            }
            set
            {
                if (value.CompareTo(secondsLeft) != 0)
                {
                    secondsLeft = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [JsonIgnore]
        private BackgroundWorker bw;
        [JsonIgnore]
        public BackgroundWorker BW
        {
            get { return bw; }
        }
        [JsonIgnore]
        CLanTCPManager TCPManager;
        [JsonIgnore]
        public Socket currentSocket;

        public CLanFileTransfer(User u, List<CLanFile> f, CLanTransferType t)
        {
            // Set class fields
            Other = u;
            Files = f;
            Type = t;
            IsPending = true;
            CurrentFile = "waiting for " + Other.Name + "...";

            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;

            if (t == CLanTransferType.SEND)
                bw.DoWork += WorkerStartSend;
            else
                bw.DoWork += WorkerStartReceive;

            bw.ProgressChanged += WorkerReportProgress;
            bw.RunWorkerCompleted += WorkerCompleted;

            TCPManager = CLanTCPManager.Instance;
        }

        public void Start()
        {
            Trace.WriteLine("CFT.CS - STARTING BW");
            bw.RunWorkerAsync();    // WorkerStartSend ot WorkerStartReceive, depending on the type of file transfer
        }
        public void Stop()
        {
            // This will have to notify the necessary network structures to terminate the communication gracefully
            bw.CancelAsync();
        }
        // Store the current file transfer in global transfer list
        private void Store()
        {
            OnTransferAdded(this);
        }
        private void Unstore()
        {
            OnTransferRemoved(this);
        }

        private void WorkerStartSend(object sender, DoWorkEventArgs e)
        {
            Trace.WriteLine("CTF.CS - WORKERSTARTSEND");
            // The sender will see the transfer window with a "waiting state" until the other answers
            Store();
            try
            {
                currentSocket = TCPManager.GetConnection(Other);
                CLanFileTransferRequest req = new CLanFileTransferRequest(App.me, Other, Files);
                byte[] requestData = new Message(App.me, MessageType.SEND, req).ToByteArray();
                TCPManager.Send(requestData, currentSocket);
            }
            catch (SocketException se)
            {
                Trace.WriteLine("Socket exception sending request");
                e.Cancel = true;
                return;
            }
            
            byte[] responseData = TCPManager.Receive(currentSocket);
            if (responseData != null)
            {
                Message responseMessage = Message.GetMessage(responseData);
                switch (responseMessage.messageType)
                {
                    case MessageType.ACK:
                        // Destination accepted the transfer
                        // Show the window with all file transfers                      
                        TCPManager.SendFiles(this);
                        break;
                    case MessageType.NACK:
                        // Destination refused the transfer
                        Trace.WriteLine("Destination refused the transfer");
                        e.Cancel = true;    // Suicide
                        break;
                    default:
                        Trace.WriteLine("This message should not be here, check your code");
                        break;
                }
            }
            else
            {
                Trace.WriteLine("An error occured receiving from Other");
                e.Cancel = true;    // Suicide
            }
            // CancellationPending is checked by the SendFiles, that terminates when it is set to True
            if (bw.CancellationPending)
                e.Cancel = true;
        }
        private void WorkerStartReceive(object sender, DoWorkEventArgs e)
        {
            // This method starts when we accept the file request (but before we inform the other), so we already have
            // all the information about it. We know the sender, how many files and
            // the size, name and path of each of them
            // Since we accepted the request, we may have to specify a save path, according to our settings
            // The client confirms the transfer only after the path has been specified
            // Please note that it is not possible to cancel the transfer during this phase: it will be again possible
            // during the transfer itself
            Trace.WriteLine("CTF.CS - WORKERSTARTRECEIVE");

            // Check if the user wants to use the default download folder or if it wants to change it
            string root = SettingsManager.DefaultSavePath;
            if (!SettingsManager.SaveInDefaultPath) // If ASK is default behaviour
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    CommonOpenFileDialog fbd = new CommonOpenFileDialog();
                    fbd.DefaultDirectory = System.Environment.SpecialFolder.MyComputer.ToString();
                    fbd.IsFolderPicker = true;
                    fbd.Multiselect = false;
                    if (fbd.ShowDialog() != CommonFileDialogResult.Ok)
                        return;

                    root = fbd.FileName + Path.DirectorySeparatorChar;
                });
            }
            currentSocket = TCPManager.GetConnection(Other);
            // Confirm the transfer
            byte[] toSend = new Message(App.me, MessageType.ACK, "My body is ready!").ToByteArray();
            TCPManager.Send(toSend, currentSocket);

            // Receive files
            Store();
            TCPManager.ReceiveFiles(this, root);
        }      

        private void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Trace.WriteLine("Operation was cancelled");
            }
            else
            {
                Trace.WriteLine("Operation completed: " + e.Result);
            }
            Unstore();
        }
        private void WorkerReportProgress(object sender, ProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
            IsPending = false;
        }

        public void UpdateTimeLeft(long seconds)
        {
            SecondsLeft = TimeSpan.FromMilliseconds(seconds * 1000);
        }

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        public static event EventHandler<CLanFileTransfer> TransferAdded;
        public static event EventHandler<CLanFileTransfer> TransferRemoved;
        
        private void NotifyPropertyChanged(String propertyName = "")
        {
            // This method is called by the Set accessor of each property.
            // The CallerMemberName attribute that is applied to the optional propertyName
            // parameter causes the property name of the caller to be substituted as an argument.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void OnTransferAdded(CLanFileTransfer ctf)
        {
            TransferAdded?.Invoke(this, ctf);
        }
        public void OnTransferRemoved(CLanFileTransfer ctf)
        {
            TransferRemoved?.Invoke(this, ctf);
        }
        #endregion
    }
}

using CLanWPFTest.Objects;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace CLanWPFTest.Networking
{
    // This class will be used on both sides of the transfer, either for sending or receiving
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
        private BackgroundWorker bw;
        public BackgroundWorker BW
        {
            get { return bw; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CLanFileTransfer(User u, List<CLanFile> f, CLanTransferType t)
        {
            // Set class fields
            Other = u;
            Files = f;
            Type = t;

            //SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(App.Current.Dispatcher));
            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;

            if (t == CLanTransferType.SEND)
            {
                bw.DoWork += WorkerStartSend;
            }
            else
            {
                bw.DoWork += WorkerStartReceive;
            }

            bw.ProgressChanged += WorkerReportProgress;
            bw.RunWorkerCompleted += WorkerCompleted;
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
            App.AddTransfer(this);
        }
        private void Unstore()
        {
            App.RemoveTransfer(this);
        }

        private void WorkerStartSend(object sender, DoWorkEventArgs e)
        {
            // STEPS:
            // 0) Build request message
            // 1) Connect to the destination
            // 2) Ask for file transfer
            // 3) Receive response
            // 4) Act accordingly
            Trace.WriteLine("CTF.CS - WORKERSTARTSEND");

            CLanFileTransferRequest req = new CLanFileTransferRequest(App.me, Other, Files);
            byte[] requestData = new Message(App.me, MessageType.SEND, req).ToByteArray();

            Socket otherSocket = CLanTCPManager.GetConnection(Other);
            CLanTCPManager.Send(requestData, Other);

            byte[] responseData = CLanTCPManager.Receive(otherSocket);
            if (responseData != null)
            {
                Message responseMessage = Message.GetMessage(responseData);
                switch (responseMessage.messageType)
                {
                    case MessageType.ACK:
                        // Destination accepted the transfer
                        // Show the window with all file transfers
                        Store();
                        FileTransferWindow.Open();
                        CLanTCPManager.SendFiles(this);
                        Unstore();
                        break;
                    case MessageType.NACK:
                        // Destination refused the transfer
                        Trace.WriteLine("Destination refused the transfer");
                        Stop();
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
            Trace.WriteLine("CTF.CS - WORKERSTARTRECEIVE");

            // Since we accepted the request, we may have to specify a save path, according to our settings
            // The client confirms the transfer only after the path has been specified
            // Please note that it is not possible to cancel the transfer during this phase: it will be again possible
            // during the transfer itself

            // Check if the user wants to use the default download folder or if it wants to change it
            string root = "";
            if (!Properties.Settings.Default.DefaultAskSavePath && Properties.Settings.Default.DefaultSavePath != "") // If ASK is not default behaviour
            {
                root = Properties.Settings.Default.DefaultSavePath; // Then start saving there
            }
            else
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

            // Confirm the transfer
            byte[] toSend = new Message(App.me, MessageType.ACK, "My body is ready!").ToByteArray();
            CLanTCPManager.Send(toSend, Other);

            // Receive files
            Store();
            FileTransferWindow.Open();
            CLanTCPManager.ReceiveFiles(this, root);
            Unstore();
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
        }

        private void WorkerReportProgress(object sender, ProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
        }

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

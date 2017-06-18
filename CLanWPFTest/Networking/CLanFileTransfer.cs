using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLanWPFTest.Objects;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Windows;

namespace CLanWPFTest.Networking
{
    // This class will be used on both sides of the transfer, either for sending or receiving
    public enum CLanTransferType { SEND, RECEIVE };
    public class CLanFileTransfer : INotifyPropertyChanged, IEquatable<CLanFileTransfer>
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

        public event PropertyChangedEventHandler PropertyChanged;

        public CLanFileTransfer(User u, List<CLanFile> f, CLanTransferType t)
        {
            // Set class fields
            Other = u;
            Files = f;
            Type = t;

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

        public void Ask()
        {
            // The application is built so that only the sender-to-be calls this method
            // It simply creates a new send request and sends it to the desired destination
            CLanFileTransferRequest req = new CLanFileTransferRequest(App.me, Other, Files);
            Message m = new Message(App.me, MessageType.SEND, req);
            CLanUDPManager.Send(Other, m);
        }

        public void Transfer()
        {
            // If and only if the destination replies with ACK, the UDPManager calls this method on
            // the filetransfer object dedicated to the particular destination

            // Start the worker for the specific user
            // The worker will send all the selected files to one user
            Trace.WriteLine("CFT.CS - STARTING BW");

            // This method is only called in senders, to the bw will execute WorkerStartSend
            bw.RunWorkerAsync();
        }

        private void WorkerStartSend(object sender, DoWorkEventArgs e)
        {
            // Here the TCP magic will happen
            // Hopefully in the near future
            Trace.WriteLine("CTF.CS - WORKERSTARTSEND");
            for (int i = 0; i < 100 && !bw.CancellationPending; i++)
            {
                CurrentFile = Files[i % Files.Count].Name;
                bw.ReportProgress(i);
                Thread.Sleep(1000);
            }
            if (bw.CancellationPending)
                e.Cancel = true;
        }

        public void Receive()
        {
            // This method is a twin of Transfer(), i added it as a matter of pure readability
            // This method is only called in the destination
            // If the destination replies with ACK, it then sets up a new FileTransfer object for the incoming file transfer
            // Before replying with ACK the destination has no info about the file transfer whatsoever

            Trace.WriteLine("CFT.CS - STARTING BW");

            // This method is only called in receivers, to the bw will execute WorkerStartReceive
            bw.RunWorkerAsync();
        }

        private void WorkerStartReceive(object sender, DoWorkEventArgs e)
        {
            // Some TCP magic will happen here as well
            Trace.WriteLine("CTF.CS - WORKERSTARTRECEIVE");
        }

        public void Stop()
        {
            // This will have to notify the necessary network structures to terminate the communication gracefully
            bw.CancelAsync();
            Application.Current.Dispatcher.Invoke(new Action(() => App.RemoveTransfer(this)));
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
            Progress = (int)e.ProgressPercentage;
        }

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool Equals(CLanFileTransfer other)
        {
            return Other.Equals(other.Other) && Files.Equals(other.Files) && Type.Equals(other.Type);
        }

        // Store the current file transfer in global transfer list
        public void Store()
        {
            Application.Current.Dispatcher.Invoke(new Action(() => App.AddTransfer(this)));
        }
    }
}

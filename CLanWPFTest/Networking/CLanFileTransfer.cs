using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLanWPFTest.Objects;
using System.Threading;
using System.Diagnostics;

namespace CLanWPFTest.Networking
{
    public class CLanFileTransfer : INotifyPropertyChanged
    {
        public User Destination { get; set; }
        public List<CLanFile> Files;

        private string currentFile;
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

        private int progress;
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

        private BackgroundWorker bw;

        public event PropertyChangedEventHandler PropertyChanged;

        public CLanFileTransfer(User u, List<string> f)
        {
            // Set class fields
            Destination = u;
            Files = new List<CLanFile>();
            foreach (string file in f)
            {
                Files.Add(new CLanFile(file));
            }

            // Setup actual transfer manager
            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.DoWork += WorkerStart;
            bw.ProgressChanged += WorkerReportProgress;
            bw.RunWorkerCompleted += WorkerCompleted;
        }

        public void Start()
        {
            // Start the worker for the specific user
            // The worker will send all the selected files to one user
            bw.RunWorkerAsync();
        }

        void WorkerStart(object sender, DoWorkEventArgs e)
        {
            for(int i = 0; i < 100; i++)
            {
                CurrentFile = Files[i % Files.Count].Name;
                bw.ReportProgress(i);
                Thread.Sleep(1000);
            }  
        }

        void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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

        void WorkerReportProgress(object sender, ProgressChangedEventArgs e)
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

    }
}

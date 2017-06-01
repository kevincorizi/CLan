using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CLanWPFTest

{
    /// <summary>
    /// Interaction logic for fileTransfer.xaml
    /// </summary>
    /// 

    public partial class FileTransfer : Window
    {
        bool isCancelled = false;
        public string fileName = null;
        public List<User> destinations = null;
        private List<BackgroundWorker> workers = null;

        public FileTransfer(string file, List<User> dest)
        {
            InitializeComponent();
            fileName = file;
            destinations = dest;

            // Workers is a list of BackGroundWorkers parallel to destination users
            workers = new List<BackgroundWorker>();

            for (int i = 0; i < destinations.Count; i++)
            {
                BackgroundWorker w = new BackgroundWorker();
                w.WorkerReportsProgress = true;
                w.WorkerSupportsCancellation = true;
                w.DoWork += worker_DoWork;
                w.ProgressChanged += worker_ProgressChanged;
                w.RunWorkerCompleted += worker_RunWorkerCompleted;
                workers.Add(w);
                Console.WriteLine("Added worker " + i);
            }
            for (int i = 0; i < workers.Count; i++)
            {
                Console.WriteLine("Starting worker...");
                workers[i].RunWorkerAsync(i);
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int index = (int)e.Argument;
            CLanTCPManager.SendFileRequest(fileName, destinations[index]);
            Console.WriteLine("Worker started: " + fileName + " to " + destinations[index].ip.ToString());
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
               
            }
            else
            {
               
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            // TODO: set a flag to cancel the transfer also if the window is closed with X button.
            foreach (BackgroundWorker w in workers)
            {
                w.CancelAsync();
            }
            isCancelled = true;
            this.Close();
        }
    }
}

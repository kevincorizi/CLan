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
            workers = new List<BackgroundWorker>(destinations.Count);

            for (int i = 0; i < workers.Count; i++)
            {
                workers[i] = new BackgroundWorker();
                workers[i].WorkerReportsProgress = true;
                workers[i].WorkerSupportsCancellation = true;
                workers[i].DoWork += worker_DoWork;
                workers[i].ProgressChanged += worker_ProgressChanged;
                workers[i].RunWorkerCompleted += worker_RunWorkerCompleted;
                workers[i].RunWorkerAsync(i);
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int index = (int)e.Argument;
            CLanTCPManager.SendFileRequest(fileName, destinations[index]);
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

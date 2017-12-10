using CLan.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows;

namespace CLan.Networking
{
    public class CLanFileTransferRequest
    {
        public User From { get; set; }
        public User To { get; set; }
        public List<CLanFile> Files { get; set; }

        public CLanFileTransferRequest(User from, User to, List<CLanFile> files)
        {
            From = from;
            To = to;
            Files = files;
        }

        public static CLanFileTransferRequest GetRequest(string data)
        {
            return JsonConvert.DeserializeObject<CLanFileTransferRequest>(data, CLanJSON.settings());
        }

        public void Prompt()
        {
            // Check if the user wants to accept all file transfers
            if (SettingsManager.DefaultAcceptTransfer)
            {
                OnTransferAccepted();
                return;
            }
            // Ask the user to accept or decline the file transfer
            MessageBoxResult result = MessageBox.Show(this.ToString(), "CLan Incoming Files", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                OnTransferAccepted();
                return;
            }
            OnTransferRefused();       
        }

        public override string ToString()
        {
            string s = From.Name + " wants to send the following files: \n";
            Files.ForEach((f) => s += f.Name + " " + "(" + f.Size + " bytes)\n");
            return s;
        }

        #region EVENTS
        public event EventHandler TransferAccepted;
        public event EventHandler TransferRefused;

        public void OnTransferAccepted()
        {
            TransferAccepted?.Invoke(this, EventArgs.Empty);
        }
        public void OnTransferRefused()
        {
            TransferRefused?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}

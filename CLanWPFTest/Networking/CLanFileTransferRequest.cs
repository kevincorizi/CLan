using CLanWPFTest.Objects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows;

namespace CLanWPFTest.Networking
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

        public bool Prompt()
        {
            // Check if the user wants to accept all file transfers
            if (Properties.Settings.Default.DefaultAcceptTransfer == true)
            {
                AcceptTransfer();
                return true;
            }

            // Ask the user to accept or decline the file transfer
            MessageBoxResult result = MessageBox.Show(this.ToString(), "CLan Incoming Files", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                AcceptTransfer();
                return true;
            }
            // Decline the request
            byte[] toSend = new Message(App.me, MessageType.NACK, "Maybe next time :/").ToByteArray();
            CLanTCPManager.Send(toSend, From);
            return false;
        }

        private void AcceptTransfer()
        {
            CLanFileTransfer cft = new CLanFileTransfer(From, Files, CLanTransferType.RECEIVE);
            cft.Start();    // Start the working thread, that will also ask for the save directory
        }

        public override string ToString()
        {
            string s = From.Name + " wants to send the following files: \n";
            foreach (CLanFile f in Files)
            {
                s += f.Name + " " + "(" + f.Size + " bytes)\n";
            }
            return s;
        }
    }
}

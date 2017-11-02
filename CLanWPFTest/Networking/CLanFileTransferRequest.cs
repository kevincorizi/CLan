using CLanWPFTest.Objects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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

        public bool Prompt()
        {
            // Ask the user to accept or decline the file transfer
            MessageBoxResult result = MessageBox.Show(this.ToString(), "CLan Incoming Files", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                CLanFileTransfer cft = new CLanFileTransfer(From, Files, CLanTransferType.RECEIVE);
                cft.Store();    // Store the transfer in the global list
                cft.Start();    // Start the working thread, that will also ask for the save directory
                return true;
            }
            // Decline the request
            Message response = new Message(App.me, MessageType.NACK, "Maybe next time :/");
            byte[] toSend = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(response, CLanJSON.settings()));
            CLanTCPManager.Send(toSend, From);
            return false;
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

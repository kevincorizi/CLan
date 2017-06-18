using CLanWPFTest.Objects;
using System.Collections.Generic;
using System.Diagnostics;
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

        public void Prompt()
        {
            Trace.WriteLine("CFTR.CS - PROMPTING");
            // Ask the user to accept or decline the file transfer
            MessageBoxResult result = MessageBox.Show(this.ToString(), "CLan Incoming Files", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    Accept();
                    break;
                case MessageBoxResult.No:
                    Decline();
                    break;
            }
        }

        public void Accept()
        {
            Message m = new Message(App.me, MessageType.ACK, "My body is ready");
            CLanUDPManager.Send(From, m);

            // Create a new FileTransfer instance as a receiver
            CLanFileTransfer cft = new CLanFileTransfer(From, Files, CLanTransferType.RECEIVE);
            cft.Store();
            cft.Receive();
        }

        public void Decline()
        {
            Message m = new Message(App.me, MessageType.NACK, "Maybe next time");
            CLanUDPManager.Send(From, m);
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

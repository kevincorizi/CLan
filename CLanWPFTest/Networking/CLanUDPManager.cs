using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using System.Windows;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;

namespace CLanWPFTest.Networking
{
    public class CLanUDPManager
    {
        private static int ADVERTISEMENT_INTERVAL = 5000;
        private static short udpPort = 20000;
        private static UdpClient inUDP = new UdpClient(udpPort);
        private static UdpClient outUDP = new UdpClient();

        public static async Task StartBroadcastAdvertisement(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, udpPort);
            do {
                try {
                    Message hello = new Message(App.me, MessageType.HELLO, "");
                    byte[] bytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(hello, CLanJSON.settings()));
                    await outUDP.SendAsync(bytes, bytes.Length, ip);
                }
                catch (OperationCanceledException oce) {
                    Trace.WriteLine("Terminating advertisement" + oce.Message);
                    return;
                }
                catch (SocketException se)
                {
                    Trace.WriteLine("Connection error: " + se.Message);
                }
            }
            while (!ct.WaitHandle.WaitOne(ADVERTISEMENT_INTERVAL));     // Sleeps for AD_IN seconds but wakes up if token is canceled
            Trace.WriteLine("Exiting advertisement");
        }

        public static async Task StartAdListening()
        {
            while (true)
            {
                UdpReceiveResult res = await inUDP.ReceiveAsync();
                if (res.RemoteEndPoint.Address.Equals(App.me.Ip))  // Ignore messages that I sent
                {
                    continue;
                }

                byte[] bytes = res.Buffer;
                Message m = JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(bytes), CLanJSON.settings());

                m.sender.Ip = res.RemoteEndPoint.Address;
                switch(m.messageType)
                {
                    case MessageType.HELLO:
                        Application.Current.Dispatcher.Invoke(new Action(() => App.AddUser(m.sender)));
                        Trace.WriteLine("RECEIVED HELLO UDP");
                        break;
                    case MessageType.BYE:
                        Application.Current.Dispatcher.Invoke(new Action(() => App.RemoveUser(m.sender)));
                        break;
                    case MessageType.SEND:
                        // Someone wants to send a file
                        // I do not have any information about the current file transfer yet because this is the first request i see about it
                        // I only have a file transfer request and i prompt the user about it
                        Trace.WriteLine("RECEIVED SEND UDP\n" + JsonConvert.SerializeObject(m.message, CLanJSON.settings()) + "\nEND UDP");
                        CLanFileTransferRequest req = JsonConvert.DeserializeObject<CLanFileTransferRequest>(m.message.ToString(), CLanJSON.settings());
                        // The prompt shows a blocking MessageBox
                        // While it is visible, no UDP packet is processed, so we also lose the list of users
                        // We have to launch it in an async way
                        Application.Current.Dispatcher.Invoke(() => req.Prompt());
                        break;
                    case MessageType.ACK:
                        // The other user accepted the file transfer
                        // It sent an ACK message with dummy content
                        Trace.WriteLine("RECEIVED ACK UDP\n" + JsonConvert.SerializeObject(m, CLanJSON.settings()) + "\nEND UDP");

                        // I have to start the pending file transfer to the user who just answered
                        // Note that this operation is non-blocking because the transfer is executed in a background worker
                        App.FileTransfers.Single(ft => ft.Other.Equals(m.sender)).Transfer();
                        break;
                    case MessageType.NACK:
                        // The other user denied the file transfer
                        // It sent a NACK message with dummy content
                        Trace.WriteLine("RECEIVED NACK UDP\n" + JsonConvert.SerializeObject(m.message, CLanJSON.settings()) + "\nEND UDP");

                        // I have to remove his file transfer from the list of file transfers
                        CLanFileTransfer delendum = App.FileTransfers.Single(ft => ft.Other.Equals(m.sender));
                        Application.Current.Dispatcher.Invoke(new Action(() => App.RemoveTransfer(delendum)));
                        break;
                    default:
                        Trace.WriteLine("Invalid message");
                        break;
                }
            }
        }
        public static void GoOffline()
        {
            Trace.WriteLine("Going Offline");
            Message bye = new Message(App.me, MessageType.BYE, "Farewell, cruel world!");
            byte[] bytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(bye, CLanJSON.settings()));
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, udpPort);
            outUDP.Send(bytes, bytes.Length, ip);
            App.DeactivateAdvertising();
        }
        public static void GoOnline()
        {
            Trace.WriteLine("Going Online");
            App.ActivateAdvertising();
        }

        public static void Send(User dest, object message)
        {
            byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(message, CLanJSON.settings()));
            Trace.WriteLine("CUDPM.CS - SENDING\n" + JsonConvert.SerializeObject(message, CLanJSON.settings()));
            outUDP.Send(data, data.Length, new IPEndPoint(dest.Ip, udpPort));
        }
    }
}
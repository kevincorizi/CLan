using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using System.Windows;

namespace CLanWPFTest
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
                    Console.WriteLine("Terminating advertisement" + oce.Message);
                    return;
                }
                catch (SocketException se)
                {
                    Console.WriteLine("Connection error: " + se.Message);
                }
            }
            while (!ct.WaitHandle.WaitOne(ADVERTISEMENT_INTERVAL));     // Sleeps for AD_IN seconds but wakes up if token is canceled
            Console.WriteLine("Exiting advertisement");
        }

        public static async Task StartAdListening()
        {
            while (true)
            {
                UdpReceiveResult res = await inUDP.ReceiveAsync();
                if (!res.RemoteEndPoint.Address.Equals(App.me.ip))  // Ignore messages that I sent
                {
                    byte[] bytes = res.Buffer;
                    Message m = JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(bytes), CLanJSON.settings());
                    m.sender.ip = res.RemoteEndPoint.Address;
                    switch(m.messageType)
                    {
                        case MessageType.HELLO:
                            await Application.Current.Dispatcher.BeginInvoke(new Action(() => App.AddUser(m.sender)));
                            break;
                        case MessageType.BYE:
                            await Application.Current.Dispatcher.BeginInvoke(new Action(() => App.RemoveUser(m.sender)));
                            break;
                        case MessageType.SEND:
                            Console.WriteLine("Someone wants to send a file");
                            break;
                        default:
                            Console.WriteLine("Invalid message");
                            break;
                    }
                    Console.WriteLine(JsonConvert.SerializeObject(m, CLanJSON.settings()));
                }
            }
        }
        public static void GoOffline()
        {
            Console.WriteLine("Going Offline");
            Message bye = new Message(App.me, MessageType.BYE, "Farewell, cruel world!");
            byte[] bytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(bye, CLanJSON.settings()));
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, udpPort);
            outUDP.Send(bytes, bytes.Length, ip);
            App.DeactivateAdvertising();
        }

        internal static void GoOnline()
        {
            Console.WriteLine("Going Online");
            App.ActivateAdvertising();
        }

        public static void SendFileRequest(User dest, string fileName)
        {
            Message req = new Message(dest, MessageType.SEND, fileName);
            byte[] bytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(req, CLanJSON.settings()));
            IPEndPoint ip = new IPEndPoint(dest.ip, udpPort);
            outUDP.Send(bytes, bytes.Length, ip);
        }
    }
}
using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using System.Windows;
using System.Diagnostics;

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
                if (!res.RemoteEndPoint.Address.Equals(App.me.Ip))  // Ignore messages that I sent
                {
                    byte[] bytes = res.Buffer;
                    Message m = JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(bytes), CLanJSON.settings());
                    m.sender.Ip = res.RemoteEndPoint.Address;
                    switch(m.messageType)
                    {
                        case MessageType.HELLO:
                            await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => App.AddUser(m.sender)));
                            break;
                        case MessageType.BYE:
                            await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => App.RemoveUser(m.sender)));
                            break;
                        default:
                            Trace.WriteLine("Invalid message");
                            break;
                    }
                    Trace.WriteLine(JsonConvert.SerializeObject(m, CLanJSON.settings()));
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

        internal static void GoOnline()
        {
            Trace.WriteLine("Going Online");
            App.ActivateAdvertising();
        }
    }
}
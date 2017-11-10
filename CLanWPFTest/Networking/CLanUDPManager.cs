using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CLanWPFTest.Networking
{
    public class CLanUDPManager
    {
        private static short udpPort = 20002;

        private static int ADVERTISEMENT_INTERVAL = 5000;
        private static UdpClient inUDP = new UdpClient(udpPort);
        private static UdpClient outUDP = new UdpClient();

        public static int GetAdvertisementInterval()
        {
            return ADVERTISEMENT_INTERVAL;
        }

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
                    continue;
            
                byte[] bytes = res.Buffer;
                Message m = JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(bytes), CLanJSON.settings());

                //Trace.WriteLine(Encoding.ASCII.GetString(bytes));

                m.sender.Ip = res.RemoteEndPoint.Address;
                switch(m.messageType)
                {
                    case MessageType.HELLO:
                        Application.Current.Dispatcher.Invoke(new Action(() => App.AddUser(m.sender)));
                        Trace.WriteLine("RECEIVED HELLO UDP FROM " + m.sender.Ip.ToString());
                        break;
                    case MessageType.BYE:
                        Application.Current.Dispatcher.Invoke(new Action(() => App.RemoveUser(m.sender)));
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
    }
}
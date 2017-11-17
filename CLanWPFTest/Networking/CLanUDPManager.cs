using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CLanWPFTest.Networking
{
    public class CLanUDPManager
    {
        private readonly short udpPort = 20002;
        public readonly int ADVERTISEMENT_INTERVAL;
        public readonly int KEEP_ALIVE_TIMER_MILLIS;

        private UdpClient inUDP;
        private UdpClient outUDP;

        private static CLanUDPManager instance = null;
        private static readonly object _lock = new object();

        private CLanUDPManager()
        {
            ADVERTISEMENT_INTERVAL = 5000;
            KEEP_ALIVE_TIMER_MILLIS = 2 * ADVERTISEMENT_INTERVAL;
            inUDP = new UdpClient(udpPort);
            outUDP = new UdpClient();
        }

        public static CLanUDPManager Instance
        {
            get
            {
                lock(_lock)
                {
                    if (instance == null)
                        instance = new CLanUDPManager();
                }
                return instance;
            }
        }

        public async Task StartAdvertisement(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, udpPort);
            do {
                try {
                    byte[] bytes = (new Message(App.me, MessageType.HELLO, "")).ToByteArray();
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

        public async Task StartListening(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            while(true) {
                try
                {
                    UdpReceiveResult res = await inUDP.ReceiveAsync();
                    if (res.RemoteEndPoint.Address.Equals(App.me.Ip))  // Ignore messages that I sent
                        continue;

                    Message m = Message.GetMessage(res.Buffer);
                    m.sender.Ip = res.RemoteEndPoint.Address;
                    switch (m.messageType)
                    {
                        case MessageType.HELLO:
                            OnUserJoin(m.sender);
                            Trace.WriteLine("RECEIVED HELLO UDP FROM " + m.sender.Ip.ToString());
                            break;
                        case MessageType.BYE:
                            OnUserLeave(m.sender);
                            break;
                        default:
                            Trace.WriteLine("Invalid message");
                            break;
                    }
                }
                catch (OperationCanceledException oce)
                {
                    Trace.WriteLine("Terminating listening" + oce.Message);
                    return;
                }
                catch (SocketException se)
                {
                    Trace.WriteLine("Connection error in UDP listener" + se.Message);
                    return;
                }
            }
        }

        public void GoOffline()
        {
            byte[] bytes = (new Message(App.me, MessageType.BYE, "Farewell, cruel world!")).ToByteArray();
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, udpPort);
            outUDP.Send(bytes, bytes.Length, ip);
            OnToggleOffline();
        }
        public void GoOnline()
        {
            OnToggleOnline();
        }

        #region Events
        public event EventHandler<User> UserJoin;
        public event EventHandler<User> UserLeave;
        public event EventHandler ToggleOnline;
        public event EventHandler ToggleOffline;

        public void OnUserJoin(User u)
        {
            UserJoin?.Invoke(this, u);
        }
        public void OnUserLeave(User u)
        {
            UserLeave?.Invoke(this, u);
        }
        public void OnToggleOnline()
        {
            ToggleOnline?.Invoke(this, EventArgs.Empty);
        }
        public void OnToggleOffline()
        {
            ToggleOffline?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
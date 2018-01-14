using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CLan.Networking
{
    public class CLanUDPManager
    {
        public readonly short udpPort = 20002;
        public readonly int ADVERTISEMENT_INTERVAL = 5000;
        public readonly int KEEP_ALIVE_TIMER_MILLIS = 10000;

        private static CLanUDPManager instance = null;
        private static readonly object _lock = new object();

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
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, udpPort);
            using (UdpClient outUDP = new UdpClient())
            {
                try
                {
                    do
                    {
                        byte[] bytes = (new Message(App.me, MessageType.HELLO, "")).ToByteArray();
                        await outUDP.SendAsync(bytes, bytes.Length, ip);
                    }
                    while (!ct.WaitHandle.WaitOne(ADVERTISEMENT_INTERVAL));

                    if(ct.IsCancellationRequested)
                    {
                        byte[] bytes = (new Message(App.me, MessageType.BYE, "Farewell, cruel world!")).ToByteArray();
                        outUDP.Send(bytes, bytes.Length, ip);
                        Trace.WriteLine("Terminating advertisement");
                    }
                }
                catch (SocketException se)
                {
                    Trace.WriteLine("Connection error: " + se.Message);
                }              
            }
        }

        public async Task StartListening(CancellationToken ct)
        {
            using (UdpClient inUDP = new UdpClient(udpPort))
            {
                ct.Register(() => inUDP.Close());
                try
                {
                    while (true)
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
                }
                catch (ObjectDisposedException ode)
                {
                    Trace.WriteLine("Terminating UDP Listener");
                }
                catch (SocketException se)
                {
                    Trace.WriteLine("Connection error in UDP listener" + se.Message);
                }
            }
        }

        public void GoOffline()
        {
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
using CLanWPFTest.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CLanWPFTest.Networking
{
    class CLanTCPManager
    {
        private int tcpListeningPort = 20001;
        private int secondInstancePort = 20003;
        private int BUFFER_SIZE = 1024;
        private Dictionary<User, Socket> sockets;

        private static CLanTCPManager instance = null;
        private static readonly object _lock = new object();
       
        private CLanTCPManager()
        {
            sockets = new Dictionary<User, Socket>();
        }
        public static CLanTCPManager Instance {
            get
            {
                lock (_lock)
                {
                    if (instance == null)
                        instance = new CLanTCPManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// Start listening for new TCP connections
        /// </summary>
        public void StartListening(CancellationToken ct)
        {            
            try
            {
                ct.ThrowIfCancellationRequested();
                TcpListener listener = new TcpListener(App.me.Ip, tcpListeningPort);
                listener.Start();
                while (true)
                {
                    Socket client = listener.AcceptSocket();

                    // Someone contacted me, i need to answer, but in a separate thread
                    Thread t = new Thread(() => HandleAccept(client));
                    t.Start();
                }
            }
            catch (OperationCanceledException oce)
            {
                Trace.WriteLine("Terminating TCP listening" + oce.Message);
                return;
            }
            catch (SocketException se)
            {
                Trace.WriteLine("Connection error in TCP listener" + se.Message);
                return;
            }
        }

        #region ACCEPT
        public void HandleAccept(Socket client)
        {
            // This method is already being executed in a separate thread
            Trace.WriteLine("New connection!");
            User source = App.OnlineUsers.SingleOrDefault(u => u.Ip.Equals((client.RemoteEndPoint as IPEndPoint).Address));
            if (source == null)
            {
                Trace.WriteLine("Incoming connection from non-listed user, probably in private mode");
                // There is going to be a socket not listed in the sockets list, for now
            }
            else if (!sockets.ContainsKey(source))
            {
                sockets.Add(source, client);
            }

            byte[] data = Receive(client);
            // Trace.WriteLine(Encoding.ASCII.GetString(data));
            Message m = Message.GetMessage(data);
            if (!sockets.ContainsValue(client))
            {
                // This means that the sender was probably in private mode and therefore not inserted yet
                sockets.Add(m.sender, client);
            }
            CLanFileTransferRequest req = CLanFileTransferRequest.GetRequest(m.message.ToString());
            req.TransferAccepted += HandleTransferAccepted;
            req.TransferRefused += HandleTransferRefused;
            req.Prompt();
        }
        public void HandleTransferAccepted(object sender, EventArgs e)
        {
            CLanFileTransferRequest req = sender as CLanFileTransferRequest;
            CLanFileTransfer cft = new CLanFileTransfer(req.From, req.Files, CLanTransferType.RECEIVE);
            cft.Start();    // Start the working thread, that will also ask for the save directory
        }
        public void HandleTransferRefused(object sender, EventArgs e)
        {
            // Decline the request
            CLanFileTransferRequest req = sender as CLanFileTransferRequest;
            byte[] toSend = new Message(App.me, MessageType.NACK, "Maybe next time :/").ToByteArray();
            Send(toSend, req.From);
        }
        #endregion

        public void SendFiles(CLanFileTransfer cft)
        {
            User other = cft.Other;
            List<CLanFile> files = cft.Files;
            BackgroundWorker bw = cft.BW;

            if (!sockets.ContainsKey(other) || !sockets[other].Connected)
            {
                Trace.WriteLine("Cannot send file to non-existing or disconnected client");
                return;
            }

            long totalSize = files.Sum(f => f.Size);
            long sentSize = 0;
            byte[] buffer = new byte[BUFFER_SIZE];

            using(sockets[other])
            {
                foreach (CLanFile f in files)
                {
                    // Update the View
                    cft.CurrentFile = f.Name;

                    using (NetworkStream stream = new NetworkStream(sockets[other]))
                    using (FileStream fstream = new FileStream(f.RelativePath, FileMode.Open, FileAccess.Read))
                    {
                        if (stream.CanWrite)
                        {
                            int oldProgress = 0;
                            while (sentSize < totalSize)
                            {
                                Array.Clear(buffer, 0, BUFFER_SIZE);
                                int size = fstream.Read(buffer, 0, BUFFER_SIZE);
                                stream.Write(buffer, 0, size);
                                sentSize += size;
                                int progress = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(sentSize) * 100 / Convert.ToDouble(totalSize)));
                                if (oldProgress != progress)
                                {
                                    oldProgress = progress;
                                    bw.ReportProgress(progress);
                                }
                            }
                        }
                    }
                }
            }

            sockets.Remove(other);
        }
        public void ReceiveFiles(CLanFileTransfer cft, string rootFolder)
        {
            User other = cft.Other;
            List<CLanFile> files = CLanFile.EnforceDuplicatePolicy(cft.Files, rootFolder);
            BackgroundWorker bw = cft.BW;

            if (!sockets.ContainsKey(other) || !sockets[other].Connected)
            {
                Trace.WriteLine("Cannot receive file from non-existing or disconnected client");
                return;
            }

            long totalSize = files.Sum(f => f.Size);
            long receivedSize = 0;
            byte[] buffer = new byte[BUFFER_SIZE];

            using (sockets[other])
            {
                // Here I already applied the renaming/overwriting policy for duplicate files, 
                // so I can simply receive them
                foreach (CLanFile f in files)
                {
                    // Update the View
                    cft.CurrentFile = f.Name;

                    using (NetworkStream stream = new NetworkStream(sockets[other]))
                    using (FileStream fstream = new FileStream(rootFolder + f.Name, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        if (stream.CanRead)
                        {
                            int oldProgress = 0;
                            while (receivedSize < totalSize)
                            {
                                Array.Clear(buffer, 0, BUFFER_SIZE);
                                int size = stream.Read(buffer, 0, BUFFER_SIZE);
                                fstream.Write(buffer, 0, size);
                                receivedSize += size;
                                int progress = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(receivedSize) * 100 / Convert.ToDouble(totalSize)));
                                if (oldProgress != progress)
                                {
                                    oldProgress = progress;
                                    bw.ReportProgress(progress);
                                }
                            }
                        }
                    }
                }
            }
            sockets.Remove(other);
        }

        #region UTILITIES
        public Socket GetConnection(User dest)
        {
            IPEndPoint i = new IPEndPoint(dest.Ip, tcpListeningPort);
            if (!sockets.ContainsKey(dest))
            {
                sockets.Add(dest, new Socket(SocketType.Stream, ProtocolType.IP));
            }
            if (!sockets[dest].Connected)
            {
                sockets[dest].Connect(i);
            }
            return sockets[dest];
        }
        public void Send(byte[] message, User dest)
        {
            if (!sockets.ContainsKey(dest) || !sockets[dest].Connected)
            {
                Trace.WriteLine("Cannot send TCP message to non-existing or disconnected client");
                return;
            }

            // Uses the GetStream public method to return the NetworkStream.
            NetworkStream netStream = new NetworkStream(sockets[dest]);

            if (netStream.CanWrite)
            {
                netStream.Write(message, 0, message.Length);
                netStream.Close();
            }
            else
            {
                Console.WriteLine("You cannot write data to this stream.");
                sockets[dest].Close();
                // Closing the tcpClient instance does not close the network stream.
                netStream.Close();
                return;
            }
        }
        public byte[] Receive(Socket source)
        {
            // Uses the GetStream public method to return the NetworkStream.
            NetworkStream netStream = new NetworkStream(source);
            if (netStream.CanRead)
            {
                // Reads NetworkStream into a byte buffer.
                byte[] bytes = new byte[source.ReceiveBufferSize];

                // Read can return anything from 0 to numBytesToRead. 
                // This method blocks until at least one byte is read.
                netStream.Read(bytes, 0, source.ReceiveBufferSize);
                netStream.Close();

                // Returns the data received from the host to the console.
                return bytes;
            }
            else
            {
                Console.WriteLine("You cannot read data from this stream.");
                source.Close();

                // Closing the tcpClient instance does not close the network stream.
                netStream.Close();
                return null;
            }
        }
        #endregion

        #region EVENTS
        public event EventHandler<string[]> FileListReceived;
        public void OnFileListReceived(string[] list)
        {
            FileListReceived?.Invoke(this, list);
        }
        #endregion  
    }
}
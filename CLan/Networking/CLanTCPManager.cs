using CLan.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CLan.Networking
{
    class CLanTCPManager
    {
        private int tcpListeningPort = 20001;
        private int BUFFER_SIZE = 1024;
        private Dictionary<User, Socket> socketBuffer;

        private static CLanTCPManager instance = null;
        private static readonly object _lock = new object();

        private CLanTCPManager() {
            socketBuffer = new Dictionary<User, Socket>();
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

        #region Listen
        public void StartListening(CancellationToken ct)
        {
            try
            {
                TcpListener listener = new TcpListener(App.me.Ip, tcpListeningPort);
                listener.Start();

                ct.Register(() => listener.Stop());

                while (true)
                {
                    Socket client = listener.AcceptSocket();

                    // Someone contacted me, i need to answer, but in a separate thread
                    Thread t = new Thread(() => HandleAccept(client));
                    t.Start();
                }
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.Interrupted)
                {
                    Trace.WriteLine("Terminating TCP Listener");
                }
                else
                {
                    Trace.WriteLine("Connection error in TCP listener" + se.ErrorCode);
                }
            }
        }
        #endregion

        #region Accept
        public void HandleAccept(Socket client)
        {
            // This method is already being executed in a separate thread
            Trace.WriteLine("New connection!");
            byte[] data = Receive(client);
            Message m = Message.GetMessage(data);

            User source = m.sender;
            socketBuffer.Add(source, client);

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
            Send(toSend, socketBuffer[req.From]);

            // Flush the buffer
            socketBuffer.Remove(req.From);
        }
        #endregion

        #region Files
        public void SendFiles(CLanFileTransfer cft)
        {
            Socket other = cft.currentSocket;
            List<CLanFile> files = cft.Files;
            BackgroundWorker bw = cft.BW;

            long totalSize = files.Sum(f => f.Size);
            long sentSize = 0;
            byte[] buffer = new byte[BUFFER_SIZE];
            Stopwatch sw = Stopwatch.StartNew();

            using (other)
            {
                foreach (CLanFile f in files)
                {
                    if (bw.CancellationPending)
                        break;

                    // Update the View
                    cft.CurrentFile = f.Name;
                    long currentSentSize = 0;

                    using (NetworkStream stream = new NetworkStream(other))
                    using (FileStream fstream = new FileStream(f.RelativePath, FileMode.Open, FileAccess.Read))
                    {
                        try
                        {
                            if (stream.CanWrite)
                            {
                                int oldProgress = 0;
                                long oldSecondsLeft = 0;
                                while (currentSentSize < f.Size && !bw.CancellationPending)
                                {
                                    Array.Clear(buffer, 0, BUFFER_SIZE);
                                    int size = fstream.Read(buffer, 0, BUFFER_SIZE);
                                    stream.Write(buffer, 0, size);
                                    currentSentSize += size;
                                    sentSize += size;

                                    int progress = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(sentSize) * 100 / Convert.ToDouble(totalSize)));

                                    long sizeUpToNow = sentSize;
                                    long timeUpToNow = sw.ElapsedMilliseconds;
                                    long sizeLeft = totalSize - sentSize;
                                    long timeLeft = (timeUpToNow * sizeLeft) / sizeUpToNow;
                                    long secondsLeft = timeLeft / 1000;

                                    if (oldProgress != progress)
                                    {
                                        oldProgress = progress;
                                        bw.ReportProgress(progress);
                                    }
                                    if (oldSecondsLeft != secondsLeft)
                                    {
                                        oldSecondsLeft = secondsLeft;
                                        cft.UpdateTimeLeft(secondsLeft);
                                    }
                                }

                                // I am the sender and I stopped the transfer
                                if (bw.CancellationPending)
                                {
                                    Trace.WriteLine("Transfer was cancelled by me");
                                    break;
                                }
                            }
                        }
                        catch (IOException ioe)
                        {
                            // I am the sender and the receiver stopped the transfer
                            Trace.WriteLine("The receiver stopped the transfer");
                            break;
                        }
                    }
                }
            }
        }
        public void ReceiveFiles(CLanFileTransfer cft, string rootFolder)
        {
            Socket other = cft.currentSocket;
            List<CLanFile> files = CLanFile.EnforceDuplicatePolicy(cft.Files, rootFolder);
            BackgroundWorker bw = cft.BW;

            long totalSize = files.Sum(f => f.Size);
            long receivedSize = 0;
            byte[] buffer = new byte[BUFFER_SIZE];
            Stopwatch sw = Stopwatch.StartNew();

            using (other)
            {
                // Here I already applied the renaming/overwriting policy for duplicate files, 
                // so I can simply receive them
                foreach (CLanFile f in files)
                {
                    if (bw.CancellationPending)
                        break;
                    // Update the View
                    cft.CurrentFile = f.Name;
                    long currentReceivedSize = 0;

                    using (NetworkStream stream = new NetworkStream(other))
                    using (FileStream fstream = new FileStream(rootFolder + f.Name, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        if (stream.CanRead)
                        {
                            int oldProgress = 0;
                            long oldSecondsLeft = 0;
                            while (currentReceivedSize < f.Size && !bw.CancellationPending)
                            {
                                Array.Clear(buffer, 0, BUFFER_SIZE);
                                int size = stream.Read(buffer, 0, (int)Math.Min(BUFFER_SIZE, f.Size - currentReceivedSize));
                                if (size <= 0)  // I am receiving and the sender closed the transfer
                                    break;
                                fstream.Write(buffer, 0, size);
                                currentReceivedSize += size;
                                receivedSize += size;
                                int progress = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(receivedSize) * 100 / Convert.ToDouble(totalSize)));

                                long sizeUpToNow = receivedSize;
                                long timeUpToNow = sw.ElapsedMilliseconds;
                                long sizeLeft = totalSize - receivedSize;
                                long timeLeft = (timeUpToNow * sizeLeft) / sizeUpToNow;
                                long secondsLeft = timeLeft / 1000;

                                if (oldProgress != progress)
                                {
                                    oldProgress = progress;
                                    bw.ReportProgress(progress);
                                }
                                if (oldSecondsLeft != secondsLeft)
                                {
                                    oldSecondsLeft = secondsLeft;
                                    cft.UpdateTimeLeft(secondsLeft);
                                }
                            }
                        }
                    }
                    if (bw.CancellationPending)
                    {
                        // I am the receiver and I stopped the transfer
                        // Delete pending file
                        File.Delete(rootFolder + f.Name);

                        // It will jump out of the foreach, closing the socket, causing an exception to occur on the other side,
                        // that will be catched to acknowledged the end of the transfer
                        break;
                    }
                    if (currentReceivedSize != f.Size)
                    {
                        // I am the receiver and the sender stopped the transfer
                        Trace.WriteLine("Transfer was cancelled by the sender");

                        // Delete pending file
                        File.Delete(rootFolder + f.Name);
                        break;
                    }
                    Trace.WriteLine("File received");
                }
            }
        }
        #endregion

        #region Utilities
        public Socket GetConnection(User dest)
        {
            IPEndPoint i = new IPEndPoint(dest.Ip, tcpListeningPort);
            if (socketBuffer.ContainsKey(dest))
            {                
                Socket tmp = socketBuffer[dest];
                if (!tmp.Connected)
                    tmp.Connect(i);
                socketBuffer.Remove(dest);
                return tmp;
            }
            
            Socket s = new Socket(SocketType.Stream, ProtocolType.IP);
            s.Connect(i);
            return s;
        }
        public void Send(byte[] message, Socket dest)
        {
            // Uses the GetStream public method to return the NetworkStream.
            NetworkStream netStream = new NetworkStream(dest);

            if (netStream.CanWrite)
            {
                netStream.Write(message, 0, message.Length);
                netStream.Close();
            }
            else
            {
                dest.Close();
                // Closing the socket instance does not close the network stream.
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
                source.Close();
                // Closing the tcpClient instance does not close the network stream.
                netStream.Close();
                return null;
            }
        }
        #endregion

        #region Events
        public event EventHandler<string[]> FileListReceived;
        public void OnFileListReceived(string[] list)
        {
            FileListReceived?.Invoke(this, list);
        }
        #endregion  
    }
}
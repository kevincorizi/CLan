using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
using System.Linq;
using CLanWPFTest.Objects;

namespace CLanWPFTest.Networking
{
    class CLanTCPManager
    {
        private static int tcpListeningPort = 20001;
        private static int BUFFER_SIZE = 1024;

        private static Dictionary<User, Socket> sockets = new Dictionary<User, Socket>();

        /// <summary>
        /// Start listening for new TCP connections
        /// </summary>
        public static void StartListening()
        {
            
            Trace.WriteLine("Starting TCP listener...");
            TcpListener listener = new TcpListener(App.me.Ip, tcpListeningPort);
            listener.Start();
            while(true)
            {
                Trace.WriteLine("Waiting for connections...");
                Socket client = listener.AcceptSocket();

                // Someone contacted me, i need to answer, but in a separate thread
                Thread t = new Thread(new ParameterizedThreadStart(HandleAccept));
                t.Start(client);
            }
        }


        public static void GetConnection(User dest)
        {
            IPEndPoint i = new IPEndPoint(dest.Ip, tcpListeningPort);
            if (!sockets.ContainsKey(dest))
            {
                sockets.Add(dest, new Socket(SocketType.Stream, ProtocolType.IP));
            }
            sockets[dest].Connect(i);
        }

        public static void Send(byte[] message, User dest)
        {
            if (!sockets.ContainsKey(dest) || !sockets[dest].Connected)
            {
                Trace.WriteLine("Cannot send TCP message to non-existing or disconnected client");
                return;
            }

            // If it is a valid source, we add it to the destinations to send him messages

            //destinations.Add(dest, sources[dest]);

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

        public static byte[] Receive(User source)
        {
            if (!sockets.ContainsKey(source) || !sockets[source].Connected)
            {
                Trace.WriteLine("Cannot receive TCP message from non-existing or disconnected client");
                return null;
            }

            // Uses the GetStream public method to return the NetworkStream.
            NetworkStream netStream = new NetworkStream(sockets[source]);
            if (netStream.CanRead)
            {
                // Reads NetworkStream into a byte buffer.
                byte[] bytes = new byte[sockets[source].ReceiveBufferSize];

                // Read can return anything from 0 to numBytesToRead. 
                // This method blocks until at least one byte is read.
                netStream.Read(bytes, 0, sockets[source].ReceiveBufferSize);
                netStream.Close();

                // Returns the data received from the host to the console.
                return bytes;
            }
            else
            {
                Console.WriteLine("You cannot read data from this stream.");
                sockets[source].Close();

                // Closing the tcpClient instance does not close the network stream.
                netStream.Close();
                return null;
            }
        }

        public static void HandleAccept(object obj)
        {
            // This method is already being executed in a separate thread
            Trace.WriteLine("New connection!");

            // retrieve client from parameter passed to thread
            Socket client = (Socket)obj;
            User source = App.OnlineUsers.SingleOrDefault(u => u.Ip.Equals((client.RemoteEndPoint as IPEndPoint).Address));
            if (source == null)
            {
                Trace.WriteLine("Cannot accept connection from non-listed user");
                client.Close();
                return;
            }
            if (!sockets.ContainsKey(source))
            {
                sockets.Add(source, client);
            }

            HandleIncomingRequest(source);
        }

        public static void HandleIncomingRequest(User source)
        {
            // This method is executed in the same thread as HandleAccept, which is a separate thread
            byte[] data = Receive(source);
            Trace.WriteLine(Encoding.ASCII.GetString(data));
            Message m = JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(data), CLanJSON.settings());
            CLanFileTransferRequest req = JsonConvert.DeserializeObject<CLanFileTransferRequest>(m.message.ToString(), CLanJSON.settings());

            req.Prompt();
        }

        public static void SendFiles(CLanFileTransfer cft)
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
            foreach (CLanFile f in files)
            {
                // Update the View
                cft.CurrentFile = f.Name;

                // Setup transfer
                byte[] buffer = null;

                NetworkStream stream = new NetworkStream(sockets[other]);
                FileStream fstream = new FileStream(f.RelativePath, FileMode.Open, FileAccess.Read);
                int packets = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(f.Size) / Convert.ToDouble(BUFFER_SIZE)));

                if(stream.CanWrite)
                {
                    for (int i = 0; i < packets; i++)
                    {
                        buffer = new byte[BUFFER_SIZE];
                        int size = fstream.Read(buffer, 0, BUFFER_SIZE);
                        stream.Write(buffer, 0, size);
                        sentSize += size;
                        bw.ReportProgress(Convert.ToInt32(Math.Ceiling(Convert.ToDouble(sentSize) / Convert.ToDouble(totalSize))));
                    }
                }

                fstream.Close();
            }

            sockets[other].Close();
            sockets.Remove(other);
        }

        public static void ReceiveFiles(CLanFileTransfer cft, string rootFolder = "")
        {
            User other = cft.Other;
            List<CLanFile> files = cft.Files;
            BackgroundWorker bw = cft.BW;

            if (!sockets.ContainsKey(other) || !sockets[other].Connected)
            {
                Trace.WriteLine("Cannot receive file from non-existing or disconnected client");
                return;
            }

            long totalSize = files.Sum(f => f.Size);
            long receivedSize = 0;

            foreach (CLanFile f in files)
            {
                // Update the View
                cft.CurrentFile = f.Name;

                // Setup transfer
                byte[] buffer = null;
                
                NetworkStream stream = new NetworkStream(sockets[other]);
                FileStream fstream = new FileStream(rootFolder + f.Name, FileMode.OpenOrCreate, FileAccess.Write);
                int packets = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(f.Size) / Convert.ToDouble(BUFFER_SIZE)));

                if (stream.CanRead)
                {
                    for (int i = 0; i < packets; i++)
                    {
                        buffer = new byte[BUFFER_SIZE];
                        int size = stream.Read(buffer, 0, BUFFER_SIZE);
                        fstream.Write(buffer, 0, size);
                        receivedSize += size;
                        bw.ReportProgress(Convert.ToInt32(Math.Ceiling(Convert.ToDouble(receivedSize) / Convert.ToDouble(totalSize))));
                    }
                }

                fstream.Close();
            }

            sockets[other].Close();
            sockets.Remove(other);
        }
    }
}

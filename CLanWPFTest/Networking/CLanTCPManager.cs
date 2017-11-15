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


        public static Socket GetConnection(User dest)
        {
            IPEndPoint i = new IPEndPoint(dest.Ip, tcpListeningPort);
            if (!sockets.ContainsKey(dest))
            {
                sockets.Add(dest, new Socket(SocketType.Stream, ProtocolType.IP));
            }
            sockets[dest].Connect(i);
            return sockets[dest];
        }

        public static void Send(byte[] message, User dest)
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

        public static byte[] Receive(Socket source)
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

        public static void HandleAccept(object obj)
        {
            // This method is already being executed in a separate thread
            Trace.WriteLine("New connection!");

            // retrieve client from parameter passed to thread
            Socket client = (Socket)obj;
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

            HandleIncomingRequest(client);
        }

        public static void HandleIncomingRequest(Socket source)
        {
            // This method is executed in the same thread as HandleAccept, which is a separate thread
            byte[] data = Receive(source);
            // Trace.WriteLine(Encoding.ASCII.GetString(data));
            Message m = Message.GetMessage(data);
            CLanFileTransferRequest req = CLanFileTransferRequest.GetRequest(m.message.ToString());

            if(!sockets.ContainsValue(source))
            {
                // This means that the sender was probably in private mode and therefore not inserted yet
                sockets.Add(m.sender, source);
            }

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

                if(stream.CanWrite)
                {
                    int oldProgress = 0;
                    while(sentSize < totalSize)
                    {
                        buffer = new byte[BUFFER_SIZE];
                        int size = fstream.Read(buffer, 0, BUFFER_SIZE);
                        stream.Write(buffer, 0, size);
                        sentSize += size;
                        int progress = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(sentSize) * 100 / Convert.ToDouble(totalSize)));
                        if(oldProgress != progress)
                        {
                            oldProgress = progress;
                            bw.ReportProgress(progress);
                        }                      
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

                string directoryName = Path.GetDirectoryName(f.Name);

                // If incoming file is in folder
                if (directoryName.Length > 0) {
                    // If there is no current folder with the same name, create it
                    // Note that this will be always valid for subfolders
                    // If there is a folder with that name, apply the policy selected by the user
                   
                    if(Directory.Exists(rootFolder + directoryName))
                    {
                        // A folder with that name already exists
                        if (Properties.Settings.Default.DefaultRenameFile)
                        {
                            // Apply renaming policy
                            // Need to change directoryName to something that does not exist
                            string newDirectoryName = directoryName;
                            for(int i = 1; ; i++)
                            {
                                newDirectoryName = newDirectoryName + " (" + i + ")";
                                if (!Directory.Exists(rootFolder + newDirectoryName))
                                    break;
                            }
                            directoryName = newDirectoryName;
                        }
                    }
                    Directory.CreateDirectory(rootFolder + directoryName);
                }

                // Check if the file already exists and apply duplicate policy
                if (File.Exists(rootFolder + f.Name))
                {
                    if (Properties.Settings.Default.DefaultRenameFile)
                    {
                        string newFileName = f.Name;
                        for (int i = 1; ; i++)
                        {
                            newFileName = Path.GetFileNameWithoutExtension(f.Name) + " (" + i + ")" + Path.GetExtension(f.Name);
                            if (!File.Exists(rootFolder + newFileName))
                                break;
                        }
                        f.Name = newFileName;
                    }
                }
                FileStream fstream = new FileStream(rootFolder + f.Name, FileMode.OpenOrCreate, FileAccess.Write);

                if (stream.CanRead)
                {
                    int oldProgress = 0;
                    while (receivedSize < totalSize)
                    {
                        buffer = new byte[BUFFER_SIZE];
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

                fstream.Close();
            }

            sockets[other].Close();
            sockets.Remove(other);
        }
    }
}

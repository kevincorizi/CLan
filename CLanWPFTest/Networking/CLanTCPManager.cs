using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace CLanWPFTest.Networking
{
    class CLanTCPManager
    {
        private static int tcpPort = 20001;

        private static Dictionary<User, TcpClient> destinations = new Dictionary<User, TcpClient>();

        /// <summary>
        /// Start listening for new TCP connections
        /// </summary>
        public static void StartListening()
        {
            
            Trace.WriteLine("Starting TCP listener...");
            TcpListener listener = new TcpListener(App.me.Ip, tcpPort);
            listener.Start();

            while(true)
            {
                Trace.WriteLine("Waiting for connections...");
                Socket client = listener.AcceptSocket();
            }
        }

        public static void Send(string message, User dest)
        {
            if(!destinations.ContainsKey(dest))
            {
                destinations.Add(dest, new TcpClient(new IPEndPoint(dest.Ip, tcpPort)));
            }
            Trace.WriteLine(message);
            byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
            NetworkStream stream = destinations[dest].GetStream();
            stream.Write(data, 0, data.Length);
        }
    }
}

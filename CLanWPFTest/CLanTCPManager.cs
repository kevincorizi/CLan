using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using Newtonsoft.Json;
using System.Net;

namespace CLanWPFTest
{
    class CLanTCPManager
    {
        private static int tcpPort = 20001;
        private static TcpListener server = new TcpListener(IPAddress.Any, tcpPort);
        private static TcpClient client = new TcpClient();

        public static void StartRequestListening()
        {
            server.Start();
            while(true)
            {
                Console.WriteLine("TCP is listening...");
                Socket ins = server.AcceptSocket();
                Console.WriteLine("TCP request incoming...");
                Task t = Task.Run(() => ServeFileRequest(ins));
                Console.WriteLine("Request sent to server...");
            }
        }

        public static void SendFileRequest(string fileName, User dest)
        {
            Message m = new Message(dest, MessageType.SEND, fileName);
            byte[] bytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(m, CLanJSON.settings()));
            IPEndPoint ip = new IPEndPoint(dest.ip, tcpPort);
            client.Connect(dest.ip, tcpPort);
            Console.WriteLine("Connected to " + dest.ip.ToString());
            client.Client.Send(bytes);
            Console.WriteLine("Request sent");
            // Update UI somehow
            byte[] response = new byte[256];
            client.Client.Receive(response);
            Console.Write("Response received: ");
            Console.WriteLine(Encoding.Default.GetString(response));
        }

        public static void ServeFileRequest(Socket s)
        {
            byte[] request = new byte[256];
            s.Receive(request);
            Console.WriteLine("Request received: \n" + Encoding.Default.GetString(request));
            s.Send(request);
            Console.WriteLine("Response sent (echoed)");
        }

        public static void ReceiveFile(string fileName)
        {

        }

        public static void SendFile(string fileName, User dest)
        {

        }
    }
}

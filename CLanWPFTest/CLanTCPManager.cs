using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using NetworkCommsDotNet.Tools;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.TCP;
using System.Threading;
using System.ComponentModel;

namespace CLanWPFTest
{
    class CLanTCPManager
    {
        private static int tcpPort = 20001;

        /// <summary>
        /// Start listening for new TCP connections
        /// </summary>
        public static void StartListening()
        {
            //Trigger IncomingPartialFileData method if we receive a packet of type 'PartialFileData'
            NetworkComms.AppendGlobalIncomingPacketHandler<byte[]>("PartialFileData", IncomingPartialFileData);
            //Trigger IncomingPartialFileDataInfo method if we receive a packet of type 'PartialFileDataInfo'
            NetworkComms.AppendGlobalIncomingPacketHandler<CLanFileInfo>("PartialFileDataInfo", IncomingPartialFileDataInfo);

            //Trigger the method OnConnectionClose so that we can do some clean-up
            NetworkComms.AppendGlobalConnectionCloseHandler(OnConnectionClose);

            //Start listening for TCP connections
            Connection.StartListening(ConnectionType.TCP, new IPEndPoint(IPAddress.Any, tcpPort));

            //Write out some useful debugging information the log window
            Console.WriteLine("Initialised WPF file transfer example. Accepting TCP connections on:");
            foreach (IPEndPoint listenEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
                Console.WriteLine(listenEndPoint.Address + ":" + listenEndPoint.Port);
        }


        /// <summary>
        /// Sends requested file to the remoteIP and port set in GUI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="bw">The background worker launched in order to complete the specific send operation</param>
        public static void SendFile(string fileName, User dest, BackgroundWorker bw)
        {
            //Parse the necessary remote information
            string filename = fileName;
            string remoteIP = dest.ip.ToString();
            string remotePort = tcpPort.ToString();

            //Set the send progress bar to 0
            bw.ReportProgress(0);

            try
            {
                //Create a fileStream from the selected file
                FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                //Wrap the fileStream in a threadSafeStream so that future operations are thread safe
                StreamTools.ThreadSafeStream safeStream = new StreamTools.ThreadSafeStream(stream);

                //Get the filename without the associated path information
                string shortFileName = System.IO.Path.GetFileName(filename);

                //Parse the remote connectionInfo
                //We have this in a separate try catch so that we can write a clear message to the log window
                //if there are problems
                ConnectionInfo remoteInfo;
                try
                {
                    remoteInfo = new ConnectionInfo(remoteIP, int.Parse(remotePort));
                }
                catch (Exception)
                {
                    throw new InvalidDataException("Failed to parse remote IP and port. Check and try again.");
                }

                //Get a connection to the remote side
                Connection connection = TCPConnection.GetConnection(remoteInfo);

                //Break the send into 20 segments. The less segments the less overhead 
                //but we still want the progress bar to update in sensible steps
                long sendChunkSizeBytes = (long)(stream.Length / 20.0) + 1;

                //Limit send chunk size to 500MB
                long maxChunkSizeBytes = 500L * 1024L * 1024L;
                if (sendChunkSizeBytes > maxChunkSizeBytes) sendChunkSizeBytes = maxChunkSizeBytes;

                long totalBytesSent = 0;
                do
                {
                    //Check the number of bytes to send as the last one may be smaller
                    long bytesToSend = (totalBytesSent + sendChunkSizeBytes < stream.Length ? sendChunkSizeBytes : stream.Length - totalBytesSent);

                    //Wrap the threadSafeStream in a StreamSendWrapper so that we can get NetworkComms.Net
                    //to only send part of the stream.
                    StreamTools.StreamSendWrapper streamWrapper = new StreamTools.StreamSendWrapper(safeStream, totalBytesSent, bytesToSend);

                    //We want to record the packetSequenceNumber
                    long packetSequenceNumber;
                    //Send the select data
                    connection.SendObject("PartialFileData", streamWrapper, out packetSequenceNumber);
                    //Send the associated SendInfo for this send so that the remote can correctly rebuild the data
                    connection.SendObject("PartialFileDataInfo", new CLanFileInfo(shortFileName, stream.Length, totalBytesSent, packetSequenceNumber));

                    totalBytesSent += bytesToSend;

                    //Update the GUI with our send progress
                    bw.ReportProgress((int)((double)totalBytesSent / stream.Length * 100));
                } while (totalBytesSent < stream.Length);

                //Clean up any unused memory
                GC.Collect();

                Console.WriteLine("Completed file send to '" + connection.ConnectionInfo.ToString() + "'.");
                
            }
            catch (CommunicationException)
            {
                //If there is a communication exception then we just write a connection
                //closed message to the log window
                Console.WriteLine("Failed to complete send as connection was closed.");
            }
            catch (Exception ex)
            {
                //If we get any other exception which is not an InvalidDataException
                //we log the error
                if (ex.GetType() != typeof(InvalidDataException))
                {
                    Console.WriteLine(ex.Message.ToString());
                    LogTools.LogException(ex, "SendFileError");
                }
            }
        }

        /// <summary>
        /// Handles an incoming packet of type 'PartialFileData'
        /// </summary>
        /// <param name="header">Header associated with incoming packet</param>
        /// <param name="connection">The connection associated with incoming packet</param>
        /// <param name="data">The incoming data</param>
        public static void IncomingPartialFileData(PacketHeader header, Connection connection, byte[] data)
        {
            Console.WriteLine("Incoming Partial File Data");
            try
            {
                CLanFileInfo info = null;
                CLanReceivedFile file = null;

                //Perform this in a thread safe way
                lock (Application.syncRoot)
                {
                    //Extract the packet sequence number from the header
                    //The header can also user defined parameters
                    long sequenceNumber = header.GetOption(PacketHeaderLongItems.PacketSequenceNumber);

                    if (Application.IncomingDataInfoCache.ContainsKey(connection.ConnectionInfo) && Application.IncomingDataInfoCache[connection.ConnectionInfo].ContainsKey(sequenceNumber))
                    {
                        //We have the associated SendInfo so we can add this data directly to the file
                        info = Application.IncomingDataInfoCache[connection.ConnectionInfo][sequenceNumber];
                        Application.IncomingDataInfoCache[connection.ConnectionInfo].Remove(sequenceNumber);

                        //Check to see if we have already received any files from this location
                        if (!Application.ReceivedFilesDict.ContainsKey(connection.ConnectionInfo))
                            Application.ReceivedFilesDict.Add(connection.ConnectionInfo, new Dictionary<string, CLanReceivedFile>());

                        //Check to see if we have already initialised this file
                        if (!Application.ReceivedFilesDict[connection.ConnectionInfo].ContainsKey(info.Filename))
                        {
                            Application.ReceivedFilesDict[connection.ConnectionInfo].Add(info.Filename, new CLanReceivedFile(info.Filename, connection.ConnectionInfo, info.TotalBytes));
                        }

                        file = Application.ReceivedFilesDict[connection.ConnectionInfo][info.Filename];
                    }
                    else
                    {
                        //We do not yet have the associated CLanFileInfo so we just add the data to the cache
                        if (!Application.IncomingDataCache.ContainsKey(connection.ConnectionInfo))
                            Application.IncomingDataCache.Add(connection.ConnectionInfo, new Dictionary<long, byte[]>());

                        Application.IncomingDataCache[connection.ConnectionInfo].Add(sequenceNumber, data);
                    }
                }

                //If we have everything we need we can add data to the CLanReceivedFile
                if (info != null && file != null && !file.IsCompleted)
                {
                    file.AddData(info.BytesStart, 0, data.Length, data);

                    //Perform a little clean-up
                    file = null;
                    data = null;
                    GC.Collect();
                }
                else if (info == null ^ file == null)
                    throw new Exception("Either both are null or both are set. Info is " + (info == null ? "null." : "set.") + " File is " + (file == null ? "null." : "set.") + " File is " + (file.IsCompleted ? "completed." : "not completed."));
            }
            catch (Exception ex)
            {
                //If an exception occurs we write to the log window and also create an error file
                Console.WriteLine("Exception - " + ex.ToString());
                LogTools.LogException(ex, "IncomingPartialFileDataError");
            }
        }

        /// <summary>
        /// Handles an incoming packet of type 'PartialFileDataInfo'
        /// </summary>
        /// <param name="header">Header associated with incoming packet</param>
        /// <param name="connection">The connection associated with incoming packet</param>
        /// <param name="data">The incoming data automatically converted to a SendInfo object</param>
        public static void IncomingPartialFileDataInfo(PacketHeader header, Connection connection, CLanFileInfo info)
        {
            Console.WriteLine("Incoming Partial File Data Info");
            try
            {
                byte[] data = null;
                CLanReceivedFile file = null;

                //Perform this in a thread safe way
                lock (Application.syncRoot)
                {
                    //Extract the packet sequence number from the header
                    //The header can also user defined parameters
                    long sequenceNumber = info.PacketSequenceNumber;

                    if (Application.IncomingDataCache.ContainsKey(connection.ConnectionInfo) && Application.IncomingDataCache[connection.ConnectionInfo].ContainsKey(sequenceNumber))
                    {
                        //We already have the associated data in the cache
                        data = Application.IncomingDataCache[connection.ConnectionInfo][sequenceNumber];
                        Application.IncomingDataCache[connection.ConnectionInfo].Remove(sequenceNumber);

                        //Check to see if we have already received any files from this location
                        if (!Application.ReceivedFilesDict.ContainsKey(connection.ConnectionInfo))
                            Application.ReceivedFilesDict.Add(connection.ConnectionInfo, new Dictionary<string, CLanReceivedFile>());

                        //Check to see if we have already initialised this file
                        if (!Application.ReceivedFilesDict[connection.ConnectionInfo].ContainsKey(info.Filename))
                        {
                            Application.ReceivedFilesDict[connection.ConnectionInfo].Add(info.Filename, new CLanReceivedFile(info.Filename, connection.ConnectionInfo, info.TotalBytes));
                        }

                        file = Application.ReceivedFilesDict[connection.ConnectionInfo][info.Filename];
                    }
                    else
                    {
                        //We do not yet have the necessary data corresponding with this SendInfo so we add the
                        //info to the cache
                        if (!Application.IncomingDataInfoCache.ContainsKey(connection.ConnectionInfo))
                            Application.IncomingDataInfoCache.Add(connection.ConnectionInfo, new Dictionary<long, CLanFileInfo>());

                        Application.IncomingDataInfoCache[connection.ConnectionInfo].Add(sequenceNumber, info);
                    }
                }

                //If we have everything we need we can add data to the ReceivedFile
                if (data != null && file != null && !file.IsCompleted)
                {
                    file.AddData(info.BytesStart, 0, data.Length, data);

                    //Perform a little clean-up
                    file = null;
                    data = null;
                    GC.Collect();
                }
                else if (data == null ^ file == null)
                    throw new Exception("Either both are null or both are set. Data is " + (data == null ? "null." : "set.") + " File is " + (file == null ? "null." : "set.") + " File is " + (file.IsCompleted ? "completed." : "not completed."));
            }
            catch (Exception ex)
            {
                //If an exception occurs we write to the log window and also create an error file
                Console.WriteLine("Exception - " + ex.ToString());
                LogTools.LogException(ex, "IncomingPartialFileDataInfo");
            }
        }

        /// <summary>
        /// If a connection is closed we clean-up any incomplete ReceivedFiles
        /// </summary>
        /// <param name="conn">The closed connection</param>
        public static void OnConnectionClose(Connection conn)
        {
            CLanReceivedFile[] filesToRemove = null;

            lock (Application.syncRoot)
            {
                //Remove any associated data from the caches
                Application.IncomingDataCache.Remove(conn.ConnectionInfo);
                Application.IncomingDataInfoCache.Remove(conn.ConnectionInfo);

                //Remove any non completed files
                if (Application.ReceivedFilesDict.ContainsKey(conn.ConnectionInfo))
                {
                    filesToRemove = (from current in Application.ReceivedFilesDict[conn.ConnectionInfo] where !current.Value.IsCompleted select current.Value).ToArray();
                    Application.ReceivedFilesDict[conn.ConnectionInfo] = (from current in Application.ReceivedFilesDict[conn.ConnectionInfo] where current.Value.IsCompleted select current).ToDictionary(entry => entry.Key, entry => entry.Value);
                }
            }

            //Write some useful information the log window
            Console.WriteLine("Connection closed with " + conn.ConnectionInfo.ToString());
        }
    }
}

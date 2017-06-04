using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanWPFTest
{
    /// <summary>
    /// Information class used to associate incoming data with the correct ReceivedFile
    /// </summary>
    [ProtoContract]
    public class CLanFileInfo
    {
        /// <summary>
        /// Corresponding filename
        /// </summary>
        [ProtoMember(1)]
        public string Filename { get; private set; }

        /// <summary>
        /// The starting point for the associated data
        /// </summary>
        [ProtoMember(2)]
        public long BytesStart { get; private set; }

        /// <summary>
        /// The total number of bytes expected for the whole ReceivedFile
        /// </summary>
        [ProtoMember(3)]
        public long TotalBytes { get; private set; }

        /// <summary>
        /// The packet sequence number corresponding to the associated data
        /// </summary>
        [ProtoMember(4)]
        public long PacketSequenceNumber { get; private set; }

        /// <summary>
        /// Private constructor required for deserialisation
        /// </summary>
        private CLanFileInfo() { }

        /// <summary>
        /// Create a new instance of SendInfo
        /// </summary>
        /// <param name="filename">Filename corresponding to data</param>
        /// <param name="totalBytes">Total bytes of the whole CLanReceivedFile</param>
        /// <param name="bytesStart">The starting point for the associated data</param>
        /// <param name="packetSequenceNumber">Packet sequence number corresponding to the associated data</param>
        public CLanFileInfo(string filename, long totalBytes, long bytesStart, long packetSequenceNumber)
        {
            this.Filename = filename;
            this.TotalBytes = totalBytes;
            this.BytesStart = bytesStart;
            this.PacketSequenceNumber = packetSequenceNumber;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanWPFTest
{
    public enum MessageType { HELLO, BYE, SEND, ACK, NACK };
    public class Message
    {
        public User sender { get; set; }
        public MessageType messageType { get; set; }
        public object message { get; set; }
        public DateTime timestamp { get; set; }

        public Message(User s, MessageType mt, object m)
        {
            sender = s;
            messageType = mt;
            timestamp = DateTime.Now;
            message = m;
        }
    }
}

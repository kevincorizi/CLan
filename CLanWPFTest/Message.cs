using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanWPFTest
{
    public enum MessageType { HELLO, BYE, SEND };
    class Message
    {
        public User sender { get; set; }
        public MessageType messageType { get; set; }
        public string message { get; set; }
        public DateTime timestamp { get; set; }

        public Message(User s, MessageType mt, string m)
        {
            sender = s;
            messageType = mt;
            timestamp = DateTime.Now;
            message = m;
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Text;

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

        public byte[] ToByteArray()
        {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this, CLanJSON.settings()));
        }

        public static Message GetMessage(byte[] data)
        {
            return JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(data), CLanJSON.settings());
        }
    }
}

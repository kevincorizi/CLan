using System;
using System.Net;
using System.Net.Sockets;
//
namespace CLanWPFTest
{
    public class User : IEquatable<User>
    {
        public string name { get; set; }
        public string picture { get; set; }
        public IPAddress ip { get; set; }

        public User(string name, string picture, IPAddress ip = null)
        {
            this.name = name;
            this.picture = picture;
            if(ip == null)
                this.ip = GetMyIPAddress();
            else
                this.ip = ip;
        }

        private static IPAddress GetMyIPAddress()
        {
            IPAddress[] hostAddresses = Dns.GetHostAddresses("");

            foreach (IPAddress hostAddress in hostAddresses)
            {
                if (hostAddress.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(hostAddress) &&  // ignore loopback addresses
                    !hostAddress.ToString().StartsWith("169.254."))  // ignore link-local addresses
                    return hostAddress;
            }
            return null; // or IPAddress.None if you prefer
        }

        public bool Equals(User other)
        {
            return this.ip.Equals(other.ip);
        }

        public override int GetHashCode()
        {
            return this.ip.GetHashCode();
        }
    }
}

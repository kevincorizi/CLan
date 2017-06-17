using System;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
//
namespace CLanWPFTest
{
    public class User : IEquatable<User>
    {
        public string Name { get; set; }
        public string Picture { get; set; }
        public IPAddress Ip { get; set; }

        [JsonIgnore]
        public DateTime lastKeepAlive { get; set; }

        public User(string name, string picture = "", IPAddress ip = null)
        {
            this.Name = name;
            if (picture == "")
                this.Picture = "/images/user.png";
            else
                this.Picture = picture;
            if(ip == null)
                this.Ip = GetMyIPAddress();
            else
                this.Ip = ip;
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
            return this.Ip.Equals(other.Ip);
        }

        public override int GetHashCode()
        {
            return this.Ip.GetHashCode();
        }
    }
}

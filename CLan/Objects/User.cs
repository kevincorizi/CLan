using CLan.Objects;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;

namespace CLan
{
    public class User : IEquatable<User>, INotifyPropertyChanged
    {
        private string name;
        public string Name {
            get
            {
                return name;
            }
            set
            {
                if(value != name)
                {
                    name = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private Uri picture;
        public Uri Picture
        {
            get
            {
                return picture;
            }
            set
            {
                picture = value;
                NotifyPropertyChanged();
            }
        }
        public IPAddress Ip { get; set; }

        [JsonIgnore]
        public DateTime lastKeepAlive { get; set; }

        public User(string n, string p = "", IPAddress i = null)
        {
            Name = n;
            if (p == null || p.CompareTo("") == 0)
                Picture = SettingsManager.UserPicture;
            else
                Picture = new Uri(p);
            if(i == null)   // initializing current user
                Ip = GetMyIPAddress();
            else            // initializing user from message
                Ip = i;
        }

        private static IPAddress GetMyIPAddress()
        {
            IPAddress[] hostAddresses = Dns.GetHostAddresses("");
            foreach (IPAddress hostAddress in hostAddresses)
            {
                if (hostAddress.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(hostAddress) &&            // ignore loopback addresses
                    !hostAddress.ToString().StartsWith("169.254."))  // ignore link-local addresses
                    return hostAddress;
            }
            return null;
        }

        public bool Equals(User other)
        {
            return Ip.Equals(other.Ip);
        }

        public override int GetHashCode()
        {
            return Ip.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

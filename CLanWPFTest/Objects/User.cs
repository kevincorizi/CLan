using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;

namespace CLanWPFTest
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

        private string picture;
        public string Picture
        {
            get
            {
                return picture;
            }
            set
            {
                if(value.CompareTo(picture) != 0)
                {
                    picture = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public IPAddress Ip { get; set; }

        [JsonIgnore]
        public DateTime lastKeepAlive { get; set; }

        public User(string n, string p = "", IPAddress i = null)
        {
            this.Name = n;
            if (p == null || p.CompareTo("") == 0)
                this.Picture = Properties.Settings.Default.PicturePath;
            else
                this.Picture = p;
            if(i == null)
                this.Ip = GetMyIPAddress();
            else
                this.Ip = i;
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

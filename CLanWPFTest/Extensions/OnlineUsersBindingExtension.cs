using System.Windows.Data;

namespace CLanWPFTest.Extensions
{
    public class OnlineUsersBindingExtension : Binding
    {
        public OnlineUsersBindingExtension()
        {
            Initialize();
        }

        public OnlineUsersBindingExtension(string path)
            : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Source = App.OnlineUsers;
            this.Mode = BindingMode.OneWay;
        }
    }
}
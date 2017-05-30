using Microsoft.Win32;

namespace CLanWPFTest
{
    class StartUpManager
    {
        public static void AddApplicationToCurrentUserStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if(key.GetValue("CLan") == null)
                    key.SetValue("CLan", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
            }
        }

        public static void RemoveApplicationFromCurrentUserStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if(key.GetValue("CLan") != null)
                    key.DeleteValue("CLan", false);
            }
        }
    }
}

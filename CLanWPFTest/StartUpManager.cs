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

            /*using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("*\\shell", true))
            {
                if (key.GetValue("Condividi con CLan") == null)
                    key.SetValue("Condividi con CLan", System.Reflection.Assembly.GetExecutingAssembly().Location + " %1");
            }*/
        }

        public static void RemoveApplicationFromCurrentUserStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if(key.GetValue("CLan") != null)
                    key.DeleteValue("CLan", false);
            }
        }

        //TODO registry key for right click context menu
    }
}

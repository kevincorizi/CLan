using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Linq;
using System.Collections;

namespace CLan.Objects
{
    public class SettingsManager
    {
        public static bool SaveInDefaultPath {
            get {
                return !Properties.Settings.Default.DefaultAskSavePath && Properties.Settings.Default.DefaultSavePath != "";
            }
            set
            {
                Properties.Settings.Default.DefaultAskSavePath = value;
            }
        }
        public static bool DefaultAcceptTransfer {
            get {
                return Properties.Settings.Default.DefaultAcceptTransfer == true;
            }
            set
            {
                Properties.Settings.Default.DefaultAcceptTransfer = value;
            }
        }
        public static string DefaultSavePath {
            get {
                return Properties.Settings.Default.DefaultSavePath;
            }
            set
            {
                Properties.Settings.Default.DefaultSavePath = value;
            }
        }
        public static bool DefaultRenameOnDuplicate
        {
            get
            {
                return Properties.Settings.Default.DefaultRenameFile;
            }
            set
            {
                Properties.Settings.Default.DefaultRenameFile = value;
            }
        }

        public static bool DefaultHideNotifications
        {
            get
            {
                return Properties.Settings.Default.HideNotification;
            }
            set
            {
                Properties.Settings.Default.HideNotification = value;
            }
        }

        public static bool DefaultPrivateMode
        {
            get
            {
                return Properties.Settings.Default.DefaultPrivate;
            }
            set
            {
                Properties.Settings.Default.DefaultPrivate = value;
            }
        }

        public static bool DefaultPublicMode
        {
            get
            {
                return !Properties.Settings.Default.DefaultPrivate;
            }
            set
            {
                
            }
        }

        public static string Username
        {
            get
            {
                if (System.DirectoryServices.AccountManagement.UserPrincipal.Current.DisplayName == null)
                    return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                return System.DirectoryServices.AccountManagement.UserPrincipal.Current.DisplayName;
            }
        }

        public static Uri UserPicture
        {
            get
            {
                return new Uri(Properties.Settings.Default.PicturePath);
            }
            set
            {
                Properties.Settings.Default.PicturePath = value.ToString();
            }
        }

        public static Uri BackgroundPicture
        {
            get
            {
                return new Uri(Properties.Settings.Default.BackgroundPath);
            }
            set
            {
                Properties.Settings.Default.BackgroundPath = value.ToString();
            }
        }

        public static void Save()
        {
            Properties.Settings.Default.Save();
        }
        public static void Undo()
        {
            Properties.Settings.Default.Reload();
        }

        public static string[] GetResourcesUnder(string folder)
        {
            folder = folder.ToLower() + "/";

            var assembly = Assembly.GetCallingAssembly();
            var resourcesName = assembly.GetName().Name + ".g.resources";
            var stream = assembly.GetManifestResourceStream(resourcesName);
            var resourceReader = new ResourceReader(stream);

            var resources =
                from p in resourceReader.OfType<DictionaryEntry>()
                let theme = (string)p.Key
                where theme.StartsWith(folder)
                select theme.Substring(folder.Length);

            return resources.ToArray();
        }
    }
}

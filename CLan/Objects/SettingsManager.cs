using System;
using System.Reflection;
using System.Resources;
using System.Linq;
using System.Collections;

namespace CLan.Objects
{
    public class SettingsManager
    {
        #region Files
        public static string DefaultSavePath
        {
            get
            {
                return Properties.Settings.Default.DefaultSavePath;
            }
            set
            {
                Properties.Settings.Default.DefaultSavePath = value;
            }
        }
        public static bool SaveInDefaultPath {
            get {
                return !Properties.Settings.Default.DefaultAskSavePath && Properties.Settings.Default.DefaultSavePath != "";
            }
            set
            {
                Properties.Settings.Default.DefaultAskSavePath = value;
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
        #endregion   

        #region Privacy
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
        #endregion

        #region Users
        public static string Username
        {
            get
            {
                if (System.DirectoryServices.AccountManagement.UserPrincipal.Current.DisplayName == null)
                {
                    return System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(System.IO.Path.PathSeparator)[1];
                }
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
        #endregion

        #region Transfers
        public static bool DefaultAcceptTransfer
        {
            get
            {
                return Properties.Settings.Default.DefaultAcceptTransfer == true;
            }
            set
            {
                Properties.Settings.Default.DefaultAcceptTransfer = value;
            }
        }
        #endregion

        #region Resources (User pictures and backgrounds)
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
        #endregion

        public static void Save()
        {
            Properties.Settings.Default.Save();
        }
        public static void Undo()
        {
            Properties.Settings.Default.Reload();
        }      
    }
}

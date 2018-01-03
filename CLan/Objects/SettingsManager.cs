using System;
using System.ComponentModel;

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
                return System.DirectoryServices.AccountManagement.UserPrincipal.Current.DisplayName;
            }
        }

        public static string UserPicture
        {
            get
            {
                return Properties.Settings.Default.PicturePath;
            }
            set
            {
                Properties.Settings.Default.PicturePath = value;
            }
        }

        public static string BackgroundPicture
        {
            get
            {
                return Properties.Settings.Default.BackgroundPath;
            }
            set
            {
                Properties.Settings.Default.BackgroundPath = value;
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
    }
}

namespace CLan.Objects
{
    class SettingsManager
    {
        public static bool SaveInDefaultPath {
            get {
                return !Properties.Settings.Default.DefaultAskSavePath && Properties.Settings.Default.DefaultSavePath != "";
            }
        }
        public static bool DefaultAcceptTransfer {
            get {
                return Properties.Settings.Default.DefaultAcceptTransfer == true;
            }
        }
        public static string DefaultSavePath {
            get {
                return Properties.Settings.Default.DefaultSavePath;
            }
        }
        public static bool DefaultRenameOnDuplicate
        {
            get
            {
                return Properties.Settings.Default.DefaultRenameFile;
            }
        }
    }
}

using CLan.Extensions;
using Newtonsoft.Json;

namespace CLan
{
    class CLanJSON
    {
        private static JsonSerializerSettings settings = null; 

        private static void initSettings()
        {
            settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPAddressConverter());
            settings.Converters.Add(new IPEndPointConverter());
            settings.Formatting = Formatting.Indented;
        }

        public static JsonSerializerSettings Settings()
        {
            if (settings == null)
            {
                initSettings();
            }
            return settings;
        }
    }
}

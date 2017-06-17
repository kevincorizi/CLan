using CLanWPFTest.Extensions;
using Newtonsoft.Json;
using System;

namespace CLanWPFTest
{
    class CLanJSON
    {
        private static JsonSerializerSettings _settings = null; 

        private static void initSettings()
        {
            _settings = new JsonSerializerSettings();
            _settings.Converters.Add(new IPAddressConverter());
            _settings.Converters.Add(new IPEndPointConverter());
            _settings.Formatting = Formatting.Indented;
        }

        public static JsonSerializerSettings settings()
        {
            if (_settings == null)
            {
                Console.WriteLine("Initializing JSON settings");
                initSettings();
            }
            return _settings;
        }
    }
}

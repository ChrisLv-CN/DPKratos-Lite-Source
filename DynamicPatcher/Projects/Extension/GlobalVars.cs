using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension
{
    public static partial class GlobalVars
    {
        public static string ExtensionVersion { get; } = typeof(GlobalVars).Assembly.GetName().Version.ToString();
        public static string RootDirectory => AppDomain.CurrentDomain.BaseDirectory;
        public static string DynamicPatcherDirectory => Path.Combine(RootDirectory, "DynamicPatcher");

        public static SerializationConfiguration Serialization;
    }

    public static partial class GlobalVars
    {
        public class SerializationConfiguration
        {

            public bool SerializationCheck { get; set; }
            public bool LogSerializerDetail { get; set; }

        }
    }

    public static partial class GlobalVars
    {
        static GlobalVars()
        {
            LoadGlobalConfiguration();
        }

        private static void LoadGlobalConfiguration()
        {
            Serialization = new SerializationConfiguration()
            {
                SerializationCheck = false,
                LogSerializerDetail = false
            };
        }
    }


}

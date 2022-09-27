using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Utilities
{

    public static class ConfigHelper
    {

        public static bool TryGetImageConfig<T>(this Pointer<TechnoClass> pTechno, out T imgConfig) where T : INIConfig, new()
        {
            string section = pTechno.Ref.Type.Ref.Base.Base.ID;
            return TryGetConfig(section, out imgConfig);
        }

        private static bool TryGetConfig<T>(string section, out T imgConfig) where T : INIConfig, new()
        {
            imgConfig = default(T);

            // 获取Image的Section
            string image = Ini.GetSection(Ini.RulesDependency, section).Get<string>("Image");
            if (!string.IsNullOrEmpty(image))
            {
                section = image;
            }
            return TryGetImageConfig(section, out imgConfig);
        }

        public static bool TryGetImageConfig<T>(string imageSection, out T imgConfig) where T : INIConfig, new()
        {
            imgConfig = Ini.GetConfig<T>(Ini.ArtDependency, imageSection).Data;
            return null != imgConfig;
        }

        public static T GetImageConfig<T>(this Pointer<TechnoClass> pTechno) where T : INIConfig, new()
        {
            string section = pTechno.Ref.Type.Ref.Base.Base.ID;
            return GetConfig<T>(section);
        }

        private static T GetConfig<T>(string section) where T : INIConfig, new()
        {
            // 获取Image的Section
            string image = Ini.GetSection(Ini.RulesDependency, section).Get<string>("Image");
            if (!string.IsNullOrEmpty(image))
            {
                section = image;
            }
            return GetImageConfig<T>(section);
        }

        public static T GetImageConfig<T>(string imageSection) where T : INIConfig, new()
        {
            return Ini.GetConfig<T>(Ini.ArtDependency, imageSection).Data;
        }

    }

}
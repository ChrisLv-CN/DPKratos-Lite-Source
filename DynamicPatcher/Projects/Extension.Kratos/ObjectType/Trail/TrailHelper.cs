using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public static class TrailHelper
    {
        public static bool TryGetTrails(string section, out List<Trail> trails)
        {
            trails = null;

            // 获取Image的Section
            string image = Ini.GetSection(Ini.RulesDependency, section).Get<string>("Image");
            if (!string.IsNullOrEmpty(image))
            {
                section = image;
            }
            // 获取尾巴
            ISectionReader reader = Ini.GetSection(Ini.ArtDependency, section);
            int i = -1;
            do
            {
                string title = "Trail" + (i >= 0 ? i : "") + ".";
                string typeName = reader.Get<string>(title + "Type");
                if (!string.IsNullOrEmpty(typeName) && Ini.HasSection(Ini.ArtDependency, typeName))
                {
                    // 读取Section
                    IConfigWrapper<TrailType> type = Ini.GetConfig<TrailType>(Ini.ArtDependency, typeName);
                    Trail trail = new Trail(type);
                    trail.Read(reader, title);
                    if (null == trails)
                    {
                        trails = new List<Trail>();
                    }
                    trails.Add(trail);
                }
                i++;
            } while (i < 11);
            return null != trails && trails.Count() > 0;
        }

    }

}


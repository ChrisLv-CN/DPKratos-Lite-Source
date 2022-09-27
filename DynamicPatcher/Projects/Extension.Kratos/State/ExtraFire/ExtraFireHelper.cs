using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public static class ExtraFireHelper
    {

        public static bool TryGetFLHData(Pointer<TechnoClass> pTechno, out ExtraFireFLHData fireFLHData)
        {
            string section = pTechno.Ref.Type.Ref.Base.Base.ID;
            return TryGetFLHData(section, out fireFLHData);
        }

        public static bool TryGetFLHData(string section, out ExtraFireFLHData fireFLHData)
        {
            fireFLHData = null;

            // 获取Image的Section
            string image = Ini.GetSection(Ini.RulesDependency, section).Get<string>("Image");
            if (!string.IsNullOrEmpty(image))
            {
                section = image;
            }
            return TryGetImageFLHData(section, out fireFLHData);
        }

        public static bool TryGetImageFLHData(string imageSection, out ExtraFireFLHData fireFLHData)
        {
            fireFLHData = Ini.GetConfig<ExtraFireFLHData>(Ini.ArtDependency, imageSection).Data;
            return null != fireFLHData;
        }


    }
}

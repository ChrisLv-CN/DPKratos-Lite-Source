using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Utilities
{

    public static class WeaponTypeHelper
    {

        public static WeaponTypeData GetData(this Pointer<WeaponTypeClass> pWeapon)
        {
            return pWeapon.GetData(out WeaponTypeExt ext);
        }

        public static WeaponTypeData GetData(this Pointer<WeaponTypeClass> pWeapon, out WeaponTypeExt ext)
        {
            ext = WeaponTypeExt.ExtMap.Find(pWeapon);
            if (null != ext)
            {
                IConfigWrapper<WeaponTypeData> data = (IConfigWrapper<WeaponTypeData>)ext.WeaponTypeData;
                if (null == data)
                {
                    data = Ini.GetConfig<WeaponTypeData>(Ini.RulesDependency, pWeapon.Ref.Base.ID);
                    ext.WeaponTypeData = (INIComponent)data;
                }
                return data.Data;
            }
            return new WeaponTypeData();
        }

    }
}

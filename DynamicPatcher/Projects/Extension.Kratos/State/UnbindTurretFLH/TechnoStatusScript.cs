using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class TechnoStatusScript
    {

        private IConfigWrapper<UnbindTurretFLHData> _unbindTurretFLHData;
        private UnbindTurretFLHData unbindTurretFLHData
        {
            get
            {
                if (null == _unbindTurretFLHData)
                {
                    // 获取Image
                    string imgSection = this.section;
                    string image = Ini.GetSection(Ini.RulesDependency, imgSection).Get<string>("Image");
                    if (!string.IsNullOrEmpty(image))
                    {
                        imgSection = image;
                    }
                    _unbindTurretFLHData = Ini.GetConfig<UnbindTurretFLHData>(Ini.ArtDependency, imgSection);
                }
                return _unbindTurretFLHData.Data;
            }
        }

        public bool IsUnbindTurret(int weaponIdx)
        {
            if (unbindTurretFLHData.Enable)
            {
                Pointer<TechnoTypeClass> pType = pTechno.Ref.Type;
                bool isElite = pTechno.Ref.Veterancy.IsElite();
                // 是否盖特或者FV
                if (pType.Ref.IsGattling || pType.Ref.Gunner)
                {
                    if (isElite)
                    {
                        return null != unbindTurretFLHData.EliteWeaponIndexs && unbindTurretFLHData.EliteWeaponIndexs.Contains(weaponIdx);
                    }
                    return null != unbindTurretFLHData.WeaponIndexs && unbindTurretFLHData.WeaponIndexs.Contains(weaponIdx);
                }
                else if (weaponIdx == 0)
                {
                    if (isElite)
                    {
                        return !unbindTurretFLHData.ElitePrimaryFireOnTurret;
                    }
                    return !unbindTurretFLHData.PrimaryFireOnTurret;
                }
                else if (weaponIdx == 1)
                {
                    if (isElite)
                    {
                        return !unbindTurretFLHData.EliteSecondaryFireOnTurret;
                    }
                    return !unbindTurretFLHData.SecondaryFireOnTurret;
                }
            }
            return false;
        }

    }
}

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
        public int FLHIndex;

        private IConfigWrapper<FireFLHOnBodyData> _fireFLHOnBodyData;
        private FireFLHOnBodyData fireFLHOnBodyData
        {
            get
            {
                if (null == _fireFLHOnBodyData)
                {
                    // 获取Image
                    string imgSection = this.section;
                    string image = Ini.GetSection(Ini.RulesDependency, imgSection).Get<string>("Image");
                    if (!string.IsNullOrEmpty(image))
                    {
                        imgSection = image;
                    }
                    _fireFLHOnBodyData = Ini.GetConfig<FireFLHOnBodyData>(Ini.ArtDependency, imgSection);
                }
                return _fireFLHOnBodyData.Data;
            }
        }

        private IConfigWrapper<FireFLHOnTargetData> _fireFLHOnTargetData;
        private FireFLHOnTargetData fireFLHOnTargetData
        {
            get
            {
                if (null == _fireFLHOnTargetData)
                {
                    // 获取Image
                    string imgSection = this.section;
                    string image = Ini.GetSection(Ini.RulesDependency, imgSection).Get<string>("Image");
                    if (!string.IsNullOrEmpty(image))
                    {
                        imgSection = image;
                    }
                    _fireFLHOnTargetData = Ini.GetConfig<FireFLHOnTargetData>(Ini.ArtDependency, imgSection);
                }
                return _fireFLHOnTargetData.Data;
            }
        }

        public bool IsFLHOnBody(int weaponIdx)
        {
            if (fireFLHOnBodyData.Enable)
            {
                Pointer<TechnoTypeClass> pType = pTechno.Ref.Type;
                bool isElite = pTechno.Ref.Veterancy.IsElite();
                // 是否盖特或者FV
                if (pType.Ref.IsGattling || pType.Ref.Gunner)
                {
                    if (isElite)
                    {
                        return null != fireFLHOnBodyData.EliteWeaponIndexs && fireFLHOnBodyData.EliteWeaponIndexs.Contains(weaponIdx);
                    }
                    return null != fireFLHOnBodyData.WeaponIndexs && fireFLHOnBodyData.WeaponIndexs.Contains(weaponIdx);
                }
                else if (weaponIdx == 0)
                {
                    if (isElite)
                    {
                        return fireFLHOnBodyData.ElitePrimaryFireOnBody;
                    }
                    return fireFLHOnBodyData.PrimaryFireOnBody;
                }
                else if (weaponIdx == 1)
                {
                    if (isElite)
                    {
                        return fireFLHOnBodyData.EliteSecondaryFireOnBody;
                    }
                    return fireFLHOnBodyData.SecondaryFireOnBody;
                }
            }
            return false;
        }

        public bool IsFLHOnTarget()
        {
            if (fireFLHOnTargetData.Enable)
            {
                Pointer<TechnoTypeClass> pType = pTechno.Ref.Type;
                bool isElite = pTechno.Ref.Veterancy.IsElite();
                // 是否盖特或者FV
                if (pType.Ref.IsGattling || pType.Ref.Gunner)
                {
                    if (isElite)
                    {
                        return null != fireFLHOnTargetData.EliteWeaponIndexs && fireFLHOnTargetData.EliteWeaponIndexs.Contains(FLHIndex);
                    }
                    return null != fireFLHOnTargetData.WeaponIndexs && fireFLHOnTargetData.WeaponIndexs.Contains(FLHIndex);
                }
                else if (FLHIndex == 0)
                {
                    if (isElite)
                    {
                        return fireFLHOnTargetData.ElitePrimaryFireOnTarget;
                    }
                    return fireFLHOnTargetData.PrimaryFireOnTarget;
                }
                else if (FLHIndex == 1)
                {
                    if (isElite)
                    {
                        return fireFLHOnTargetData.EliteSecondaryFireOnTarget;
                    }
                    return fireFLHOnTargetData.SecondaryFireOnTarget;
                }
            }
            return false;
        }

    }
}

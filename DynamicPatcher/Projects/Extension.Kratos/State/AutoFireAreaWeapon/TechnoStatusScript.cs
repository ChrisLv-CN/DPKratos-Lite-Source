using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.EventSystems;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class TechnoStatusScript
    {

        public bool SkipROF;

        private IConfigWrapper<AutoFireAreaWeaponData> _autoFireAreaWeaponData;
        private AutoFireAreaWeaponData autoFireAreaWeaponData
        {
            get
            {
                if (null == _autoFireAreaWeaponData)
                {
                    _autoFireAreaWeaponData = Ini.GetConfig<AutoFireAreaWeaponData>(Ini.RulesDependency, section);
                }
                return _autoFireAreaWeaponData.Data;
            }
        }

        private TimerStruct reloadTimer;
        private TimerStruct initialDelayTimer;

        public void OnUpdate_AutoFireAreaWeapon()
        {
            Pointer<WeaponStruct> pWeapon = IntPtr.Zero;
            if (autoFireAreaWeaponData.Enable && !(pWeapon = pTechno.Ref.GetWeapon(autoFireAreaWeaponData.WeaponIndex)).IsNull)
            {
                if (initialDelayTimer.Expired() && reloadTimer.Expired())
                {
                    // 检查弹药消耗
                    int technoAmmo = pTechno.Ref.Ammo;
                    int weaponAmmo = Ini.GetConfig<WeaponTypeData>(Ini.RulesDependency, pWeapon.Ref.WeaponType.Ref.Base.ID).Data.Ammo;
                    if (technoAmmo >= 0)
                    {
                        int leftAmmo = technoAmmo - weaponAmmo;
                        if (autoFireAreaWeaponData.CheckAmmo || autoFireAreaWeaponData.UseAmmo)
                        {
                            if (leftAmmo < 0)
                            {
                                return;
                            }
                        }
                        if (autoFireAreaWeaponData.UseAmmo)
                        {
                            pTechno.Ref.Ammo = leftAmmo;
                            pTechno.Ref.ReloadNow();
                        }
                    }
                    int rof = pWeapon.Ref.WeaponType.Ref.ROF;
                    if (pTechno.TryGetAEManager(out AttachEffectScript aeManager))
                    {
                        rof = (int)(rof * aeManager.CountAttachStatusMultiplier().ROFMultiplier);
                    }
                    reloadTimer.Start(rof);
                    if (autoFireAreaWeaponData.TargetToGround)
                    {
                        CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                        if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell) && !pCell.IsNull)
                        {
                            SkipROF = true;
                            pTechno.Ref.Fire_IgnoreType(pCell.Convert<AbstractClass>(), autoFireAreaWeaponData.WeaponIndex);
                            SkipROF = false;
                        }
                    }
                    else
                    {
                        SkipROF = true;
                        pTechno.Ref.Fire_IgnoreType(pTechno.Convert<AbstractClass>(), autoFireAreaWeaponData.WeaponIndex);
                        SkipROF = false;
                    }

                }
            }
        }

    }
}
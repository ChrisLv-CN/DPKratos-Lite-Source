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

    public partial class TechnoStatusScript
    {

        public OverrideWeaponState OverrideWeaponState = new OverrideWeaponState();


        public void OnPut_OverrideWeapon()
        {
            // 初始化状态机
            OverrideWeaponData data = Ini.GetConfig<OverrideWeaponData>(Ini.RulesDependency, section).Data;
            if (data.Enable)
            {
                OverrideWeaponState.Enable(data);
            }
        }

        public void OnFire_OverrideWeapon(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            OverrideWeaponState.WeaponIndex = weaponIndex;
        }

    }
}

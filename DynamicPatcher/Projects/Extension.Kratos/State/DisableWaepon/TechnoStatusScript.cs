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

        public State<DisableWeaponData> DisableWeaponState = new State<DisableWeaponData>();

        public bool CanFire_DisableWeapon(Pointer<AbstractClass> pTarget, Pointer<WeaponTypeClass> pWeapon)
        {
            return DisableWeaponState.IsActive();
        }

    }
}

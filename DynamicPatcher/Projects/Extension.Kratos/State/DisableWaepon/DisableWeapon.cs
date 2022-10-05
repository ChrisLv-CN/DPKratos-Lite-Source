using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class AttachEffect
    {
        public DisableWeapon DisableWeapon;

        private void InitDisableWeapon()
        {
            this.DisableWeapon = AEData.DisableWeaponData.CreateEffect<DisableWeapon>();
            RegisterEffect(DisableWeapon);
        }
    }


    [Serializable]
    public class DisableWeapon : StateEffect<DisableWeapon, DisableWeaponData>
    {
        public override State<DisableWeaponData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.DisableWeaponState;
        }
        
        public override State<DisableWeaponData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

    }
}

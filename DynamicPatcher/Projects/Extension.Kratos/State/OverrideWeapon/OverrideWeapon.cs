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
        public OverrideWeapon OverrideWeapon;

        private void InitOverrideWeapon()
        {
            this.OverrideWeapon = AEData.OverrideWeaponData.CreateEffect<OverrideWeapon>();
            RegisterEffect(OverrideWeapon);
        }
    }


    [Serializable]
    public class OverrideWeapon : StateEffect<OverrideWeapon, OverrideWeaponData>
    {
        public override State<OverrideWeaponData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.OverrideWeaponState;
        }
        
        public override State<OverrideWeaponData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

    }
}

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
        public AttackBeacon AttackBeacon;

        private void InitAttackBeacon()
        {
            this.AttackBeacon = AEData.AttackBeaconData.CreateEffect<AttackBeacon>();
            RegisterEffect(AttackBeacon);
        }
    }


    [Serializable]
    public class AttackBeacon : StateEffect<AttackBeacon, AttackBeaconData>
    {
        public override State<AttackBeaconData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.AttackBeaconState;
        }
        
        public override State<AttackBeaconData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

    }
}

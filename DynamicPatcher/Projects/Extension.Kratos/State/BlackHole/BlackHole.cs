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
        public BlackHole BlackHole;

        private void InitBlackHole()
        {
            this.BlackHole = AEData.BlackHoleData.CreateEffect<BlackHole>();
            RegisterEffect(BlackHole);
        }
    }


    [Serializable]
    public class BlackHole : StateEffect<BlackHole, BlackHoleData>
    {
        public override State<BlackHoleData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.BlackHoleState;
        }
        
        public override State<BlackHoleData> GetState(BulletStatusScript statusScript)
        {
            return statusScript.BlackHoleState;
        }

    }
}

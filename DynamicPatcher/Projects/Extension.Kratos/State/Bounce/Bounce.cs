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
        public Bounce Bounce;

        private void InitBounce()
        {
            this.Bounce = AEData.BounceData.CreateEffect<Bounce>();
            RegisterEffect(Bounce);
        }
    }


    [Serializable]
    public class Bounce : StateEffect<Bounce, BounceData>
    {
        public override State<BounceData> GetState(TechnoStatusScript statusScript)
        {
            return null;
        }

        public override State<BounceData> GetState(BulletStatusScript statusScript)
        {
            return statusScript.BounceState;
        }

    }
}

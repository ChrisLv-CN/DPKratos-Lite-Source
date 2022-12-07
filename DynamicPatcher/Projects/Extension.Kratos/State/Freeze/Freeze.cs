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
        public Freeze Freeze;

        private void InitFreeze()
        {
            this.Freeze = AEData.FreezeData.CreateEffect<Freeze>();
            RegisterEffect(Freeze);
        }
    }


    [Serializable]
    public class Freeze : StateEffect<Freeze, FreezeData>
    {
        public override State<FreezeData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.FreezeState;
        }

        public override State<FreezeData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

    }
}

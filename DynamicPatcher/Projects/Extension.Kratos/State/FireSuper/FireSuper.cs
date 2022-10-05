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
        public FireSuper FireSuper;

        private void InitFireSuper()
        {
            this.FireSuper = AEData.FireSuperData.CreateEffect<FireSuper>();
            RegisterEffect(FireSuper);
        }
    }


    [Serializable]
    public class FireSuper : StateEffect<FireSuper, FireSuperData>
    {
        public override State<FireSuperData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.FireSuperState;
        }
        
        public override State<FireSuperData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

    }
}

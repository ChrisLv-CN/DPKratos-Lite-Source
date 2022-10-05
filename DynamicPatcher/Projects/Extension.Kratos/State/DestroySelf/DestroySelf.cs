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
        public DestroySelf DestroySelf;

        private void InitDestroySelf()
        {
            this.DestroySelf = AEData.DestroySelfData.CreateEffect<DestroySelf>();
            RegisterEffect(DestroySelf);
        }
    }


    [Serializable]
    public class DestroySelf : StateEffect<DestroySelf, DestroySelfData>
    {
        public override State<DestroySelfData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.DestroySelfState;
        }
        
        public override State<DestroySelfData> GetState(BulletStatusScript statusScript)
        {
            return statusScript.DestroySelfState;
        }

    }
}

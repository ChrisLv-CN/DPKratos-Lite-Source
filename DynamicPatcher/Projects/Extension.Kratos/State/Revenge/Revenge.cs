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
        public Revenge Revenge;

        private void InitRevenge()
        {
            this.Revenge = AEData.RevengeData.CreateEffect<Revenge>();
            RegisterEffect(Revenge);
        }
    }


    [Serializable]
    public class Revenge : StateEffect<Revenge, RevengeData>
    {
        public override State<RevengeData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.RevengeState;
        }

        public override State<RevengeData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

    }
}

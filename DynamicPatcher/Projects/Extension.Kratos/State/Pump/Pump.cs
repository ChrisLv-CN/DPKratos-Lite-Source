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
        public Pump Pump;

        private void InitPump()
        {
            this.Pump = AEData.PumpData.CreateEffect<Pump>();
            RegisterEffect(Pump);
        }
    }


    [Serializable]
    public class Pump : StateEffect<Pump, PumpData>
    {
        public override State<PumpData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.PumpState;
        }

        public override State<PumpData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

    }
}

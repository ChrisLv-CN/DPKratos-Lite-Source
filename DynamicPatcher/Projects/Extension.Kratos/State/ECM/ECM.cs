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
        public ECM ECM;

        private void InitECM()
        {
            this.ECM = AEData.ECMData.CreateEffect<ECM>();
            RegisterEffect(ECM);
        }
    }


    [Serializable]
    public class ECM : StateEffect<ECM, ECMData>
    {
        public override State<ECMData> GetState(TechnoStatusScript statusScript)
        {
            return null;
        }

        public override State<ECMData> GetState(BulletStatusScript statusScript)
        {
            return statusScript.ECMState;
        }

    }
}

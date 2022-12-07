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
        public Deactive Deactive;

        private void InitDeactive()
        {
            this.Deactive = AEData.DeactiveData.CreateEffect<Deactive>();
            RegisterEffect(Deactive);
        }
    }


    [Serializable]
    public class Deactive : StateEffect<Deactive, DeactiveData>
    {
        public override State<DeactiveData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.DeactiveState;
        }

        public override State<DeactiveData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

    }
}

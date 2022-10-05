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
        public ExtraFire ExtraFire;

        private void InitExtraFire()
        {
            this.ExtraFire = AEData.ExtraFireData.CreateEffect<ExtraFire>();
            RegisterEffect(ExtraFire);
        }
    }


    [Serializable]
    public class ExtraFire : StateEffect<ExtraFire, ExtraFireData>
    {
        public override State<ExtraFireData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.ExtraFireState;
        }
        
        public override State<ExtraFireData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

    }
}

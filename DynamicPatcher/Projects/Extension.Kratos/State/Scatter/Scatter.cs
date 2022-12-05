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
        public Scatter Scatter;

        private void InitScatter()
        {
            this.Scatter = AEData.ScatterData.CreateEffect<Scatter>();
            RegisterEffect(Scatter);
        }
    }


    [Serializable]
    public class Scatter : StateEffect<Scatter, ScatterData>
    {
        public override State<ScatterData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.ScatterState;
        }

        public override State<ScatterData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

    }
}

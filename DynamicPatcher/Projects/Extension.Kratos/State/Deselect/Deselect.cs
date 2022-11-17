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
        public Deselect Deselect;

        private void InitDeselect()
        {
            this.Deselect = AEData.DeselectData.CreateEffect<Deselect>();
            RegisterEffect(Deselect);
        }
    }


    [Serializable]
    public class Deselect : StateEffect<Deselect, DeselectData>
    {
        public override State<DeselectData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.DeselectState;
        }

        public override State<DeselectData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

    }
}

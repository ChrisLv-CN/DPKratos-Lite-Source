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
        public Paintball Paintball;

        private void InitPaintball()
        {
            this.Paintball = AEData.PaintballData.CreateEffect<Paintball>();
            RegisterEffect(Paintball);
        }
    }


    [Serializable]
    public class Paintball : StateEffect<Paintball, PaintballData>
    {
        public override State<PaintballData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.PaintballState;
        }
        
        public override State<PaintballData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

    }
}

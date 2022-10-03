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

        public Stand Stand;

        private void InitStand()
        {
            this.Stand = AEData.StandData.CreateEffect<Stand>();
            RegisterEffect(Stand);
        }
    }


    [Serializable]
    public class Stand : Effect<StandData>
    {

        public void UpdateLocation(LocationMark mark)
        {
            
        }

    }
}

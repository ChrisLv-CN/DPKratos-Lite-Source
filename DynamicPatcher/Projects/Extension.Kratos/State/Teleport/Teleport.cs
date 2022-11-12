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
        public Teleport Teleport;

        private void InitTeleport()
        {
            this.Teleport = AEData.TeleportData.CreateEffect<Teleport>();
            RegisterEffect(Teleport);
        }
    }


    [Serializable]
    public class Teleport : StateEffect<Teleport, TeleportData>
    {
        public override State<TeleportData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.TeleportState;
        }

        public override State<TeleportData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

    }
}

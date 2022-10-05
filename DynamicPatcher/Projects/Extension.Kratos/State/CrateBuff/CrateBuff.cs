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
        public CrateBuff CrateBuff;

        private void InitCrateBuff()
        {
            this.CrateBuff = AEData.CrateBuffData.CreateEffect<CrateBuff>();
            RegisterEffect(CrateBuff);
        }
    }


    [Serializable]
    public class CrateBuff : Effect<CrateBuffData>
    {

        public override void OnEnable()
        {
            if (pOwner.TryGetTechnoStatus(out TechnoStatusScript status))
            {
                status.RecalculateStatus();
            }
        }

        public override void OnDisable(CoordStruct location)
        {
            if (pOwner.TryGetTechnoStatus(out TechnoStatusScript status))
            {
                status.RecalculateStatus();
            }
        }

    }
}

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
        public DestroyAnim DestroyAnim;

        private void InitDestroyAnim()
        {
            this.DestroyAnim = AEData.DestroyAnimData.CreateEffect<DestroyAnim>();
            RegisterEffect(DestroyAnim);
        }
    }


    [Serializable]
    public class DestroyAnim : StateEffect<DestroyAnim, DestroyAnimData>
    {
        public override State<DestroyAnimData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.DestroyAnimState;
        }

        public override State<DestroyAnimData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

    }
}

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
        public Transform Transform;

        private void InitTransform()
        {
            this.Transform = AEData.TransformData.CreateEffect<Transform>();
            RegisterEffect(Transform);
        }
    }


    [Serializable]
    public class Transform : StateEffect<Transform, TransformData>
    {
        public override State<TransformData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.TransformState;
        }

        public override State<TransformData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }
    }
}

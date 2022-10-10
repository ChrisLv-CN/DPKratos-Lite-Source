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
    public class Transform : Effect<TransformData>
    {
        public override void OnEnable()
        {
            TransformScript script = null;
            if (!pOwner.IsNull
                && pOwner.CastToTechno(out Pointer<TechnoClass> pTechno)
                && null != (script = pTechno.FindOrAllocate<TransformScript>()))
            {
                script.TryConvertTypeTo(Data.TransformToType);
            }
        }

        public override void OnDisable(CoordStruct location)
        {
            TransformScript script = null;
            if (!pOwner.IsNull
                && pOwner.CastToTechno(out Pointer<TechnoClass> pTechno)
                && null != (script = pTechno.FindOrAllocate<TransformScript>()))
            {
                script.CancelConverType(Data.TransformToType);
            }
        }

    }
}

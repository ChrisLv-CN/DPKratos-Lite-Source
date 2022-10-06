using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class ConvertTypeStatus
    {
        public SwizzleablePointer<TechnoTypeClass> pSourceType;

        public SwizzleablePointer<TechnoTypeClass> pTargetType;

        public bool HasBeenChanged;
        public bool Locked;

        public ConvertTypeStatus(Pointer<TechnoTypeClass> pType)
        {
            pSourceType = new SwizzleablePointer<TechnoTypeClass>(pType);
            pTargetType = new SwizzleablePointer<TechnoTypeClass>(IntPtr.Zero);
            HasBeenChanged = false;
            Locked = false;
        }

        public void ChangeTypeTo(Pointer<TechnoTypeClass> pNewType)
        {
            pTargetType.Pointer = pNewType;
            HasBeenChanged = false;
        }

        public void ResetType()
        {
            pTargetType.Pointer = IntPtr.Zero;
        }

        public override string ToString()
        {
            return string.Format("{{\"SourceType\":{0}, \"TargetType\":{1}, \"HasBeenChanged\":{2}}}",
                pSourceType.IsNull ? "null" : pSourceType.Ref.Base.Base.ID,
                pTargetType.IsNull ? "null" : pTargetType.Ref.Base.Base.ID,
                HasBeenChanged
            );
        }
    }

}

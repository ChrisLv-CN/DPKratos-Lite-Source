using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using PatcherYRpp;

namespace Extension.Ext
{
    [Obsolete("CellExt is disable because performance problem in serialization.", true)]
    [Serializable]
    public partial class CellExt : GOInstanceExtension<CellExt, CellClass>
    {
        public CellExt(Pointer<CellClass> OwnerObject) : base(OwnerObject)
        {
        }

        public override void SaveToStream(IStream stream)
        {
            base.SaveToStream(stream);
        }

        public override void LoadFromStream(IStream stream)
        {
            base.LoadFromStream(stream);
        }

        //[Hook(HookType.AresHook, Address = 0x47BDA1, Size = 5)]
        public static unsafe UInt32 CellClass_CTOR(REGISTERS* R)
        {
            var pItem = (Pointer<CellClass>)R->ESI;

            CellExt.ExtMap.FindOrAllocate(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x47BB60, Size = 6)]
        public static unsafe UInt32 CellClass_DTOR(REGISTERS* R)
        {
            var pItem = (Pointer<CellClass>)R->ECX;

            CellExt.ExtMap.Remove(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x483C10, Size = 5)]
        //[Hook(HookType.AresHook, Address = 0x4839F0, Size = 7)]
        public static unsafe UInt32 CellClass_SaveLoad_Prefix(REGISTERS* R)
        {
            var pItem = R->Stack<Pointer<CellClass>>(0x4);
            var pStm = R->Stack<Pointer<IStream>>(0x8);
            IStream stream = Marshal.GetObjectForIUnknown(pStm) as IStream;

            CellExt.ExtMap.PrepareStream(pItem, stream);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x483C00, Size = 5)]
        public static unsafe UInt32 CellClass_Load_Suffix(REGISTERS* R)
        {
            CellExt.ExtMap.LoadStatic();
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x483C79, Size = 6)]
        public static unsafe UInt32 CellClass_Save_Suffix(REGISTERS* R)
        {
            CellExt.ExtMap.SaveStatic();
            return 0;
        }
    }
}

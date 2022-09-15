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
    [Serializable]
    public partial class HouseExt : CommonInstanceExtension<HouseExt, HouseClass, HouseTypeExt, HouseTypeClass>
    {
        public HouseExt(Pointer<HouseClass> OwnerObject) : base(OwnerObject)
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

        //[Hook(HookType.AresHook, Address = 0x4F6532, Size = 5)]
        public static unsafe UInt32 HouseClass_CTOR(REGISTERS* R)
        {
            var pItem = (Pointer<HouseClass>)R->EAX;

            HouseExt.ExtMap.FindOrAllocate(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x4F7140, Size = 6)]
        public static unsafe UInt32 HouseClass_DTOR(REGISTERS* R)
        {
            var pItem = (Pointer<HouseClass>)R->ECX;

            HouseExt.ExtMap.Remove(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x504080, Size = 5)]
        //[Hook(HookType.AresHook, Address = 0x503040, Size = 5)]
        public static unsafe UInt32 HouseClass_SaveLoad_Prefix(REGISTERS* R)
        {
            var pItem = R->Stack<Pointer<HouseClass>>(0x4);
            var pStm = R->Stack<Pointer<IStream>>(0x8);
            IStream stream = Marshal.GetObjectForIUnknown(pStm) as IStream;

            HouseExt.ExtMap.PrepareStream(pItem, stream);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x504069, Size = 7)]
        public static unsafe UInt32 HouseClass_Load_Suffix(REGISTERS* R)
        {
            HouseExt.ExtMap.LoadStatic();
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x5046DE, Size = 7)]
        public static unsafe UInt32 HouseClass_Save_Suffix(REGISTERS* R)
        {
            HouseExt.ExtMap.SaveStatic();
            return 0;
        }
    }
}

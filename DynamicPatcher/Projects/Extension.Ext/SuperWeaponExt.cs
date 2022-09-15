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
    public partial class SuperWeaponExt : CommonInstanceExtension<SuperWeaponExt, SuperClass, SWTypeExt, SuperWeaponTypeClass>
    {
        public SuperWeaponExt(Pointer<SuperClass> OwnerObject) : base(OwnerObject)
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

        //[Hook(HookType.AresHook, Address = 0x6CB10E, Size = 7)]
        public static unsafe UInt32 SuperClass_CTOR(REGISTERS* R)
        {
            var pItem = (Pointer<SuperClass>)R->ESI;

            SuperWeaponExt.ExtMap.FindOrAllocate(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x6CB120, Size = 7)]
        public static unsafe UInt32 SuperClass_DTOR(REGISTERS* R)
        {
            var pItem = (Pointer<SuperClass>)R->ECX;

            SuperWeaponExt.ExtMap.Remove(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x6CDEF0, Size = 5)]
        //[Hook(HookType.AresHook, Address = 0x6CDFD0, Size = 8)]
        public static unsafe UInt32 SuperClass_SaveLoad_Prefix(REGISTERS* R)
        {
            var pItem = R->Stack<Pointer<SuperClass>>(0x4);
            var pStm = R->Stack<Pointer<IStream>>(0x8);
            IStream stream = Marshal.GetObjectForIUnknown(pStm) as IStream;

            SuperWeaponExt.ExtMap.PrepareStream(pItem, stream);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x6CDFC7, Size = 5)]
        public static unsafe UInt32 SuperClass_Load_Suffix(REGISTERS* R)
        {
            SuperWeaponExt.ExtMap.LoadStatic();
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x6CDFEA, Size = 5)]
        public static unsafe UInt32 SuperClass_Save_Suffix(REGISTERS* R)
        {
            SuperWeaponExt.ExtMap.SaveStatic();
            return 0;
        }
    }
}

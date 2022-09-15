using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using Extension.INI;
using PatcherYRpp;

namespace Extension.Ext
{
    [Serializable]
    public partial class HouseTypeExt : CommonTypeExtension<HouseTypeExt, HouseTypeClass>
    {
        public HouseTypeExt(Pointer<HouseTypeClass> OwnerObject) : base(OwnerObject)
        {
        }

        protected override void LoadFromINI(INIReader reader)
        {
            base.LoadFromINI(reader);

            string section = OwnerObject.Ref.Base.ID;

        }

        public override void SaveToStream(IStream stream)
        {
            base.SaveToStream(stream);
        }
        public override void LoadFromStream(IStream stream)
        {
            base.LoadFromStream(stream);
        }

        //[Hook(HookType.AresHook, Address = 0x511635, Size = 5)]
        //[Hook(HookType.AresHook, Address = 0x511643, Size = 5)]
        public static unsafe UInt32 HouseTypeClass_CTOR(REGISTERS* R)
        {
            var pItem = (Pointer<HouseTypeClass>)R->EAX;

            HouseTypeExt.ExtMap.FindOrAllocate(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x512760, Size = 6)]
        public static unsafe UInt32 HouseTypeClass_DTOR(REGISTERS* R)
        {
            var pItem = (Pointer<HouseTypeClass>)R->ECX;

            HouseTypeExt.ExtMap.Remove(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x51215A, Size = 5)]
        //[Hook(HookType.AresHook, Address = 0x51214F, Size = 5)]
        public static unsafe UInt32 HouseTypeClass_LoadFromINI(REGISTERS* R)
        {
            var pItem = (Pointer<HouseTypeClass>)R->EBX;
            var pINI = R->Base<Pointer<CCINIClass>>(0x8);

            HouseTypeExt.ExtMap.LoadFromINI(pItem, pINI);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x512480, Size = 5)]
        //[Hook(HookType.AresHook, Address = 0x512290, Size = 5)]
        public static unsafe UInt32 HouseTypeClass_SaveLoad_Prefix(REGISTERS* R)
        {
            var pItem = R->Stack<Pointer<HouseTypeClass>>(0x4);
            var pStm = R->Stack<Pointer<IStream>>(0x8);
            IStream stream = Marshal.GetObjectForIUnknown(pStm) as IStream;

            HouseTypeExt.ExtMap.PrepareStream(pItem, stream);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x51246D, Size = 5)]
        public static unsafe UInt32 HouseTypeClass_Load_Suffix(REGISTERS* R)
        {
            HouseTypeExt.ExtMap.LoadStatic();
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x51255C, Size = 5)]
        public static unsafe UInt32 HouseTypeClass_Save_Suffix(REGISTERS* R)
        {
            HouseTypeExt.ExtMap.SaveStatic();
            return 0;
        }
    }
}

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public partial class WarheadTypeExt : CommonTypeExtension<WarheadTypeExt, WarheadTypeClass>
    {
        public WarheadTypeExt(Pointer<WarheadTypeClass> OwnerObject) : base(OwnerObject)
        {

        }

        protected override void LoadFromINI(INIReader reader)
        {
            base.LoadFromINI(reader);

            string section = OwnerObject.Ref.Base.ID;

        }

        //[Hook(HookType.AresHook, Address = 0x75D1A9, Size = 7)]
        public static unsafe UInt32 WarheadTypeClass_CTOR(REGISTERS* R)
        {
            var pItem = (Pointer<WarheadTypeClass>)R->EBP;

            WarheadTypeExt.ExtMap.FindOrAllocate(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x75E5C8, Size = 6)]
        public static unsafe UInt32 WarheadTypeClass_SDDTOR(REGISTERS* R)
        {
            var pItem = (Pointer<WarheadTypeClass>)R->ESI;

            WarheadTypeExt.ExtMap.Remove(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x75DEAF, Size = 5)]
        //[Hook(HookType.AresHook, Address = 0x75DEA0, Size = 5)]
        public static unsafe UInt32 WarheadTypeClass_LoadFromINI(REGISTERS* R)
        {
            var pItem = (Pointer<WarheadTypeClass>)R->ESI;
            var pINI = R->Stack<Pointer<CCINIClass>>(0x150);

            WarheadTypeExt.ExtMap.LoadFromINI(pItem, pINI);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x75E2C0, Size = 5)]
        //[Hook(HookType.AresHook, Address = 0x75E0C0, Size = 8)]
        public static unsafe UInt32 WarheadTypeClass_SaveLoad_Prefix(REGISTERS* R)
        {
            var pItem = R->Stack<Pointer<WarheadTypeClass>>(0x4);
            var pStm = R->Stack<Pointer<IStream>>(0x8);
            IStream stream = Marshal.GetObjectForIUnknown(pStm) as IStream;

            WarheadTypeExt.ExtMap.PrepareStream(pItem, stream);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x75E2AE, Size = 7)]
        public static unsafe UInt32 WarheadTypeClass_Load_Suffix(REGISTERS* R)
        {
            WarheadTypeExt.ExtMap.LoadStatic();
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x75E39C, Size = 5)]
        public static unsafe UInt32 WarheadTypeClass_Save_Suffix(REGISTERS* R)
        {
            WarheadTypeExt.ExtMap.SaveStatic();
            return 0;
        }
    }
}

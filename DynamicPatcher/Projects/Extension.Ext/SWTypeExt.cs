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
    public partial class SWTypeExt : CommonTypeExtension<SWTypeExt, SuperWeaponTypeClass>
    {
        public SWTypeExt(Pointer<SuperWeaponTypeClass> OwnerObject) : base(OwnerObject)
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

        //[Hook(HookType.AresHook, Address = 0x6CE6F6, Size = 5)]
        static public unsafe UInt32 SuperWeaponTypeClass_CTOR(REGISTERS* R)
        {
            var pItem = (Pointer<SuperWeaponTypeClass>)R->EAX;

            SWTypeExt.ExtMap.FindOrAllocate(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x6CEFE0, Size = 8)]
        static public unsafe UInt32 SuperWeaponTypeClass_DTOR(REGISTERS* R)
        {
            var pItem = (Pointer<SuperWeaponTypeClass>)R->ECX;

            SWTypeExt.ExtMap.Remove(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x6CEE50, Size = 0xA)]
        //[Hook(HookType.AresHook, Address = 0x6CEE43, Size = 0xA)]
        static public unsafe UInt32 SuperWeaponTypeClass_LoadFromINI(REGISTERS* R)
        {
            var pItem = (Pointer<SuperWeaponTypeClass>)R->EBP;
            var pINI = R->Stack<Pointer<CCINIClass>>(0x3FC);

            SWTypeExt.ExtMap.LoadFromINI(pItem, pINI);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x6CE8D0, Size = 8)]
        //[Hook(HookType.AresHook, Address = 0x6CE800, Size = 0xA)]
        static public unsafe UInt32 SuperWeaponTypeClass_SaveLoad_Prefix(REGISTERS* R)
        {
            var pItem = R->Stack<Pointer<SuperWeaponTypeClass>>(0x4);
            var pStm = R->Stack<Pointer<IStream>>(0x8);
            IStream stream = Marshal.GetObjectForIUnknown(pStm) as IStream;

            SWTypeExt.ExtMap.PrepareStream(pItem, stream);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x6CE8BE, Size = 7)]
        static public unsafe UInt32 SuperWeaponTypeClass_Load_Suffix(REGISTERS* R)
        {
            SWTypeExt.ExtMap.LoadStatic();
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x6CE8EA, Size = 5)]
        static public unsafe UInt32 SuperWeaponTypeClass_Save_Suffix(REGISTERS* R)
        {
            SWTypeExt.ExtMap.SaveStatic();
            return 0;
        }
    }
}

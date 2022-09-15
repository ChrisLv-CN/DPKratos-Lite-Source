
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;

namespace ExtensionHooks
{
    public class HouseExtHooks
    {
        [Hook(HookType.AresHook, Address = 0x4F6532, Size = 5)]
        public static unsafe UInt32 HouseClass_CTOR(REGISTERS* R)
        {
            return HouseExt.HouseClass_CTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x4F7140, Size = 6)]
        public static unsafe UInt32 HouseClass_DTOR(REGISTERS* R)
        {
            return HouseExt.HouseClass_DTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x504080, Size = 5)]
        [Hook(HookType.AresHook, Address = 0x503040, Size = 5)]
        public static unsafe UInt32 HouseClass_SaveLoad_Prefix(REGISTERS* R)
        {
            return HouseExt.HouseClass_SaveLoad_Prefix(R);
        }

        [Hook(HookType.AresHook, Address = 0x504069, Size = 7)]
        public static unsafe UInt32 HouseClass_Load_Suffix(REGISTERS* R)
        {
            return HouseExt.HouseClass_Load_Suffix(R);
        }

        [Hook(HookType.AresHook, Address = 0x5046DE, Size = 7)]
        public static unsafe UInt32 HouseClass_Save_Suffix(REGISTERS* R)
        {
            return HouseExt.HouseClass_Save_Suffix(R);
        }
    }
}
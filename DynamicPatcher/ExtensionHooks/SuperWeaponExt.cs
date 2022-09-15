
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
    public class SuperWeaponExtHooks
    {
        [Hook(HookType.AresHook, Address = 0x6CB10E, Size = 7)]
        public static unsafe UInt32 SuperClass_CTOR(REGISTERS* R)
        {
            return SuperWeaponExt.SuperClass_CTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x6CB120, Size = 7)]
        public static unsafe UInt32 SuperClass_DTOR(REGISTERS* R)
        {
            return SuperWeaponExt.SuperClass_DTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x6CDEF0, Size = 5)]
        [Hook(HookType.AresHook, Address = 0x6CDFD0, Size = 8)]
        public static unsafe UInt32 SuperClass_SaveLoad_Prefix(REGISTERS* R)
        {
            return SuperWeaponExt.SuperClass_SaveLoad_Prefix(R);
        }

        [Hook(HookType.AresHook, Address = 0x6CDFC7, Size = 5)]
        public static unsafe UInt32 SuperClass_Load_Suffix(REGISTERS* R)
        {
            return SuperWeaponExt.SuperClass_Load_Suffix(R);
        }

        [Hook(HookType.AresHook, Address = 0x6CDFEA, Size = 5)]
        public static unsafe UInt32 SuperClass_Save_Suffix(REGISTERS* R)
        {
            return SuperWeaponExt.SuperClass_Save_Suffix(R);
        }
    }
}

using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;

namespace ExtensionHooks
{
    public class SuperWeaponTypeExtHooks
    {
        [Hook(HookType.AresHook, Address = 0x6CE6F6, Size = 5)]
        public static unsafe UInt32 SuperWeaponTypeClass_CTOR(REGISTERS* R)
        {
            return SuperWeaponTypeExt.SuperWeaponTypeClass_CTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x6CEFE0, Size = 8)]
        public static unsafe UInt32 SuperWeaponTypeClass_DTOR(REGISTERS* R)
        {
            return SuperWeaponTypeExt.SuperWeaponTypeClass_DTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x6CEE50, Size = 0xA)]
        [Hook(HookType.AresHook, Address = 0x6CEE43, Size = 0xA)]
        public static unsafe UInt32 SuperWeaponTypeClass_LoadFromINI(REGISTERS* R)
        {
            return SuperWeaponTypeExt.SuperWeaponTypeClass_LoadFromINI(R);
        }

        [Hook(HookType.AresHook, Address = 0x6CE8D0, Size = 8)]
        [Hook(HookType.AresHook, Address = 0x6CE800, Size = 0xA)]
        public static unsafe UInt32 SuperWeaponTypeClass_SaveLoad_Prefix(REGISTERS* R)
        {
            return SuperWeaponTypeExt.SuperWeaponTypeClass_SaveLoad_Prefix(R);
        }

        [Hook(HookType.AresHook, Address = 0x6CE8BE, Size = 7)]
        public static unsafe UInt32 SuperWeaponTypeClass_Load_Suffix(REGISTERS* R)
        {
            return SuperWeaponTypeExt.SuperWeaponTypeClass_Load_Suffix(R);
        }

        [Hook(HookType.AresHook, Address = 0x6CE8EA, Size = 5)]
        public static unsafe UInt32 SuperWeaponTypeClass_Save_Suffix(REGISTERS* R)
        {
            return SuperWeaponTypeExt.SuperWeaponTypeClass_Save_Suffix(R);
        }
    }
}
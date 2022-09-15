
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;

namespace ExtensionHooks
{
    public class HouseTypeExtHooks
    {
        [Hook(HookType.AresHook, Address = 0x511635, Size = 5)]
        [Hook(HookType.AresHook, Address = 0x511643, Size = 5)]
        public static unsafe UInt32 HouseTypeClass_CTOR(REGISTERS* R)
        {
            return HouseTypeExt.HouseTypeClass_CTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x512760, Size = 6)]
        public static unsafe UInt32 HouseTypeClass_DTOR(REGISTERS* R)
        {
            return HouseTypeExt.HouseTypeClass_DTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x51215A, Size = 5)]
        [Hook(HookType.AresHook, Address = 0x51214F, Size = 5)]
        public static unsafe UInt32 HouseTypeClass_LoadFromINI(REGISTERS* R)
        {
            return HouseTypeExt.HouseTypeClass_LoadFromINI(R);
        }

        [Hook(HookType.AresHook, Address = 0x512480, Size = 5)]
        [Hook(HookType.AresHook, Address = 0x512290, Size = 5)]
        public static unsafe UInt32 HouseTypeClass_SaveLoad_Prefix(REGISTERS* R)
        {
            return HouseTypeExt.HouseTypeClass_SaveLoad_Prefix(R);
        }

        [Hook(HookType.AresHook, Address = 0x51246D, Size = 5)]
        public static unsafe UInt32 HouseTypeClass_Load_Suffix(REGISTERS* R)
        {
            return HouseTypeExt.HouseTypeClass_Load_Suffix(R);
        }

        [Hook(HookType.AresHook, Address = 0x51255C, Size = 5)]
        public static unsafe UInt32 HouseTypeClass_Save_Suffix(REGISTERS* R)
        {
            return HouseTypeExt.HouseTypeClass_Save_Suffix(R);
        }
    }
}
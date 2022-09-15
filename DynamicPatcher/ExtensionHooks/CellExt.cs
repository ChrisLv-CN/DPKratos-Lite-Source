
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
    public class CellExtHooks
    {
#if USE_CELL_EXT
        [Hook(HookType.AresHook, Address = 0x47BDA1, Size = 5)]
        public static unsafe UInt32 CellClass_CTOR(REGISTERS* R)
        {
            return CellExt.CellClass_CTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x47BB60, Size = 6)]
        public static unsafe UInt32 CellClass_DTOR(REGISTERS* R)
        {
            return CellExt.CellClass_DTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x483C10, Size = 5)]
        [Hook(HookType.AresHook, Address = 0x4839F0, Size = 7)]
        public static unsafe UInt32 CellClass_SaveLoad_Prefix(REGISTERS* R)
        {
            return CellExt.CellClass_SaveLoad_Prefix(R);
        }

        [Hook(HookType.AresHook, Address = 0x483C00, Size = 5)]
        public static unsafe UInt32 CellClass_Load_Suffix(REGISTERS* R)
        {
            return CellExt.CellClass_Load_Suffix(R);
        }

        [Hook(HookType.AresHook, Address = 0x483C79, Size = 6)]
        public static unsafe UInt32 CellClass_Save_Suffix(REGISTERS* R)
        {
            return CellExt.CellClass_Save_Suffix(R);
        }
#endif
    }
}
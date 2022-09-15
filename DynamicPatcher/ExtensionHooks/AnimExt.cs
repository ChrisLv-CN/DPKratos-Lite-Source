using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using Extension.Ext;

namespace ExtensionHooks
{
    public class AnimExtHooks
    {
#if USE_ANIM_EXT
        [Hook(HookType.AresHook, Address = 0x422126, Size = 5)]
        [Hook(HookType.AresHook, Address = 0x4228D2, Size = 5)]
        [Hook(HookType.AresHook, Address = 0x422707, Size = 5)]
        public static unsafe UInt32 AnimClass_CTOR(REGISTERS* R)
        {
            return AnimExt.AnimClass_CTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x4228E0, Size = 5)]
        public static unsafe UInt32 AnimClass_DTOR(REGISTERS* R)
        {
            return AnimExt.AnimClass_DTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x425280, Size = 5)]
        [Hook(HookType.AresHook, Address = 0x4253B0, Size = 5)]
        public static unsafe UInt32 AnimClass_SaveLoad_Prefix(REGISTERS* R)
        {
            return AnimExt.AnimClass_SaveLoad_Prefix(R);
        }

        [Hook(HookType.AresHook, Address = 0x4253A2, Size = 7)]
        [Hook(HookType.AresHook, Address = 0x425358, Size = 7)]
        [Hook(HookType.AresHook, Address = 0x425391, Size = 7)]
        public static unsafe UInt32 AnimClass_Load_Suffix(REGISTERS* R)
        {
            return AnimExt.AnimClass_Load_Suffix(R);
        }

        [Hook(HookType.AresHook, Address = 0x4253FF, Size = 5)]
        public static unsafe UInt32 AnimClass_Save_Suffix(REGISTERS* R)
        {
            return AnimExt.AnimClass_Save_Suffix(R);
        }
#endif
    }
}

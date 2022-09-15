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
#if !USE_ANIM_EXT
    [Obsolete("AnimExt is disable because performance consideration. Define symbol 'USE_ANIM_EXT' to use it.", true)]
#endif
    [Serializable]
    public partial class AnimExt : CommonInstanceExtension<AnimExt, AnimClass, AnimTypeExt, AnimTypeClass>
    {
        public AnimExt(Pointer<AnimClass> OwnerObject) : base(OwnerObject)
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

        //[Hook(HookType.AresHook, Address = 0x422126, Size = 5)]
        //[Hook(HookType.AresHook, Address = 0x4228D2, Size = 5)]
        //[Hook(HookType.AresHook, Address = 0x422707, Size = 5)]
        public static unsafe UInt32 AnimClass_CTOR(REGISTERS* R)
        {
            var pItem = (Pointer<AnimClass>)R->ESI;

            AnimExt.ExtMap.FindOrAllocate(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x4228E0, Size = 5)]
        public static unsafe UInt32 AnimClass_DTOR(REGISTERS* R)
        {
            var pItem = (Pointer<AnimClass>)R->ECX;

            AnimExt.ExtMap.Remove(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x425280, Size = 5)]
        //[Hook(HookType.AresHook, Address = 0x4253B0, Size = 5)]
        public static unsafe UInt32 AnimClass_SaveLoad_Prefix(REGISTERS* R)
        {
            var pItem = R->Stack<Pointer<AnimClass>>(0x4);
            var pStm = R->Stack<Pointer<IStream>>(0x8);
            IStream stream = Marshal.GetObjectForIUnknown(pStm) as IStream;

            AnimExt.ExtMap.PrepareStream(pItem, stream);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x4253A2, Size = 7)]
        //[Hook(HookType.AresHook, Address = 0x425358, Size = 7)]
        //[Hook(HookType.AresHook, Address = 0x425391, Size = 7)]
        public static unsafe UInt32 AnimClass_Load_Suffix(REGISTERS* R)
        {
            AnimExt.ExtMap.LoadStatic();
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x4253FF, Size = 5)]
        public static unsafe UInt32 AnimClass_Save_Suffix(REGISTERS* R)
        {
            AnimExt.ExtMap.SaveStatic();
            return 0;
        }
    }
}

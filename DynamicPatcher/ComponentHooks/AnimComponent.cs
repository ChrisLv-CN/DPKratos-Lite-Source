using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;

namespace ComponentHooks
{
    public class AnimComponentHooks
    {
#if USE_ANIM_EXT
        [Hook(HookType.AresHook, Address = 0x423AC0, Size = 6)]
        public static unsafe UInt32 AnimClass_Update_Components(REGISTERS* R)
        {
            try
            {
                Pointer<AnimClass> pAnim = (IntPtr)R->ECX;

                AnimExt ext = AnimExt.ExtMap.Find(pAnim);
                ext.GameObject.Foreach(c => c.OnUpdate());

                return 0;
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
                return 0;
            }
        }

        [Hook(HookType.AresHook, Address = 0x42429E, Size = 6)]
        [Hook(HookType.AresHook, Address = 0x42437E, Size = 6)]
        [Hook(HookType.AresHook, Address = 0x4243A6, Size = 6)]
        [Hook(HookType.AresHook, Address = 0x424567, Size = 6)]
        [Hook(HookType.AresHook, Address = 0x4246DC, Size = 6)]
        [Hook(HookType.AresHook, Address = 0x424B42, Size = 6)]
        [Hook(HookType.AresHook, Address = 0x4247EB, Size = 6)]
        [Hook(HookType.AresHook, Address = 0x42492A, Size = 6)]
        [Hook(HookType.AresHook, Address = 0x424B29, Size = 6)]
        [Hook(HookType.AresHook, Address = 0x424B1B, Size = 6)]
        public static unsafe UInt32 AnimClass_LateUpdate_Components(REGISTERS* R)
        {
            try
            {
                Pointer<AnimClass> pAnim = (IntPtr)R->ESI;

                AnimExt ext = AnimExt.ExtMap.Find(pAnim);
                ext.GameObject.Foreach(c => c.OnLateUpdate());

                return 0;
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
                return 0;
            }
        }

        [Hook(HookType.AresHook, Address = 0x425530, Size = 6)]
        public static unsafe UInt32 AnimClass_Remove_Components(REGISTERS* R)
        {
            try
            {
                Pointer<AnimClass> pAnim = (IntPtr)R->ECX;

                AnimExt ext = AnimExt.ExtMap.Find(pAnim);
                ext.GameObject.Foreach(c => (c as IObjectScriptable)?.OnRemove());

                return 0;
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
                return 0;
            }
        }

        [Hook(HookType.AresHook, Address = 0x422CA0, Size = 5)]
        public static unsafe UInt32 AnimClass_Render_Components(REGISTERS* R)
        {
            try
            {
                Pointer<AnimClass> pAnim = (IntPtr)R->ECX;

                AnimExt ext = AnimExt.ExtMap.Find(pAnim);
                ext.GameObject.Foreach(c => c.OnRender());

                return 0;
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
                return 0;
            }
        }
#endif
    }
}

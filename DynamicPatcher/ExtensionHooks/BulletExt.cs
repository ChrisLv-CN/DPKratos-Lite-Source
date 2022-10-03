
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;

namespace ExtensionHooks
{
    public class BulletExtHooks
    {
        [Hook(HookType.AresHook, Address = 0x4664BA, Size = 5)]
        public static unsafe UInt32 BulletClass_CTOR(REGISTERS* R)
        {
            return BulletExt.BulletClass_CTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x4665E9, Size = 0xA)]
        public static unsafe UInt32 BulletClass_DTOR(REGISTERS* R)
        {
            try
            {
                Pointer<BulletClass> pBullet = (IntPtr)R->ESI;

                BulletExt ext = BulletExt.ExtMap.Find(pBullet);
                ext.GameObject.Foreach(c => (c as IObjectScriptable)?.OnUnInit());
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return BulletExt.BulletClass_DTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x46AFB0, Size = 8)]
        [Hook(HookType.AresHook, Address = 0x46AE70, Size = 5)]
        public static unsafe UInt32 BulletClass_SaveLoad_Prefix(REGISTERS* R)
        {
            return BulletExt.BulletClass_SaveLoad_Prefix(R);
        }

        [Hook(HookType.AresHook, Address = 0x46AF97, Size = 7)]
        [Hook(HookType.AresHook, Address = 0x46AF9E, Size = 7)]
        public static unsafe UInt32 BulletClass_Load_Suffix(REGISTERS* R)
        {
            return BulletExt.BulletClass_Load_Suffix(R);
        }

        [Hook(HookType.AresHook, Address = 0x46AFC4, Size = 5)]
        public static unsafe UInt32 BulletClass_Save_Suffix(REGISTERS* R)
        {
            return BulletExt.BulletClass_Save_Suffix(R);
        }

        // 导弹类抛射体当高度低于地面高度时强制引爆
        [Hook(HookType.AresHook, Address = 0x466E18, Size = 6)]
        public static unsafe UInt32 BulletClass_CheckHight_UnderGround(REGISTERS* R)
        {
            try
            {
                Pointer<BulletClass> pBullet = (IntPtr)R->ECX;
                BulletExt ext = BulletExt.ExtMap.Find(pBullet);

                if (pBullet.Ref.Base.GetHeight() <= 0
                    && pBullet.TryGetStatus(out BulletStatusScript bulletStatus)
                    && !bulletStatus.SubjectToGround)
                {
                    R->Stack<Bool>(0x18, false);
                    R->Stack<uint>(0x20, 0);
                    // Logger.Log($"{Game.CurrentFrame} - ooxx v135={R->Stack<Bool>(0x18)} pos={R->Stack<uint>(0x20)}.");
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

    }
}
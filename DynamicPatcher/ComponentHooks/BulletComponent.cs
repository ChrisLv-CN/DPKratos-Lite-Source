using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentHooks
{
    public class BulletComponentHooks
    {
        [Hook(HookType.AresHook, Address = 0x4666F7, Size = 6)]
        public static unsafe UInt32 BulletClass_Update_Components(REGISTERS* R)
        {
            try
            {
                Pointer<BulletClass> pBullet = (IntPtr)R->EBP;

                BulletExt ext = BulletExt.ExtMap.Find(pBullet);
                ext.GameObject.Foreach(c => c.OnUpdate());
                // 接管手动引爆，直接删除抛射体
                if (!pBullet.Ref.Base.IsAlive)
                {
                    // Logger.Log($"{Game.CurrentFrame} [{pBullet.Ref.Type.Ref.Base.Base.ID}]{pBullet} 手动引爆，跳过update");
                    return 0x467FEE;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x467FEE, Size = 6)]
        [Hook(HookType.AresHook, Address = 0x466781, Size = 6)]
        public static unsafe UInt32 BulletClass_LateUpdate_Components(REGISTERS* R)
        {
            try
            {
                Pointer<BulletClass> pBullet = (IntPtr)R->EBP;

                BulletExt ext = BulletExt.ExtMap.Find(pBullet);
                ext.GameObject.Foreach(c => c.OnLateUpdate());

                return 0;
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
                return 0;
            }
        }

        [Hook(HookType.AresHook, Address = 0x466556, Size = 6)]
        public static unsafe UInt32 BulletClass_Init(REGISTERS* R)
        {
            try
            {
                Pointer<BulletClass> pBullet = (IntPtr)R->ECX;
                BulletExt ext = BulletExt.ExtMap.Find(pBullet);
                // Logger.Log("BulletExt init {0}", ext == null?"Ext is null":"is ready.");
                ext.GameObject.Foreach(c => (c as IBulletScriptable)?.OnInit());
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x468B5D, Size = 6)]
        public static unsafe UInt32 BulletClass_Put(REGISTERS* R)
        {
            try
            {
                Pointer<BulletClass> pBullet = (IntPtr)R->EBX;
                BulletExt ext = BulletExt.ExtMap.Find(pBullet);
                Pointer<CoordStruct> pCoord = R->Stack<IntPtr>(-0x20);
                // Logger.Log("BulletExt init {0} {1}", ext == null?"Ext is null":"is ready.", pCoord.Data);
                DirType dirType = DirType.N;
                ext.GameObject.Foreach(c => (c as IBulletScriptable)?.OnPut(pCoord, ref dirType));
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x4690C1, Size = 8)]
        public static unsafe UInt32 BulletClass_Detonate_Components(REGISTERS* R)
        {
            try
            {
                Pointer<BulletClass> pBullet = (IntPtr)R->ECX;
                var pCoords = R->Stack<Pointer<CoordStruct>>(0x4);

                BulletExt ext = BulletExt.ExtMap.Find(pBullet);
                bool skip = false;
                if (!skip)
                {
                    ext.GameObject.Foreach(c => (c as IBulletScriptable)?.OnDetonate(pCoords, ref skip));
                }
                if (skip)
                {
                    return 0x46A2FB; // skip
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x468090, Size = 6)]
        public static unsafe UInt32 BulletClass_Render_Components(REGISTERS* R)
        {
            try
            {
                Pointer<BulletClass> pBullet = (IntPtr)R->ECX;

                BulletExt ext = BulletExt.ExtMap.Find(pBullet);
                ext.GameObject.Foreach(c => c.OnRender());
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
    }
}

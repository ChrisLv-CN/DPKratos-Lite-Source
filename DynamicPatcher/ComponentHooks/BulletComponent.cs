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

                return 0;
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
                return 0;
            }
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
            return (uint)0;
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
                ext.GameObject.Foreach(c => (c as IBulletScriptable)?.OnPut(pCoord, default));
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return (uint)0;
        }

        [Hook(HookType.AresHook, Address = 0x4690B0, Size = 6)]
        public static unsafe UInt32 BulletClass_Detonate_Components(REGISTERS* R)
        {
            try
            {
                Pointer<BulletClass> pBullet = (IntPtr)R->ECX;
                var pCoords = R->Stack<Pointer<CoordStruct>>(0x4);

                BulletExt ext = BulletExt.ExtMap.Find(pBullet);
                ext.GameObject.Foreach(c => (c as IBulletScriptable)?.OnDetonate(pCoords));

                return 0;
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
                return 0;
            }
        }

        [Hook(HookType.AresHook, Address = 0x468090, Size = 6)]
        public static unsafe UInt32 BulletClass_Render_Components(REGISTERS* R)
        {
            try
            {
                Pointer<BulletClass> pBullet = (IntPtr)R->ECX;

                BulletExt ext = BulletExt.ExtMap.Find(pBullet);
                ext.GameObject.Foreach(c => c.OnRender());

                return 0;
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
                return 0;
            }
        }
    }
}

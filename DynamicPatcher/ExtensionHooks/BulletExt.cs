
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
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

        // 除 ROT>0 和 Vertical 之外的抛射体会在此根据重力对储存的向量变量进行运算
        // 对Arcing抛射体的重力进行削减
        [Hook(HookType.AresHook, Address = 0x46745C, Size = 7)]
        public static unsafe UInt32 BulletClass_Update_ChangeVelocity(REGISTERS* R)
        {
            try
            {
                Pointer<BulletClass> pBullet = (IntPtr)R->EBP;
                if (pBullet.AmIArcing() && pBullet.TryGetStatus(out BulletStatusScript status) && status.SpeedChanged)
                {
                    Pointer<BulletVelocity> pVelocity = R->lea_Stack<IntPtr>(0x90); // 已经算过重力的速度
                    BulletVelocity velocity = pBullet.Ref.Velocity;
                    // Logger.Log("Arcing当前车速 {0} - {1}, {2}", pBullet.Ref.Speed, velocity, pVelocity.IsNull ? "null" : pVelocity.Data);
                    // velocity *= 0;
                    pVelocity.Ref.X = velocity.X;
                    pVelocity.Ref.Y = velocity.Y;
                    pVelocity.Ref.Z = status.LocationLocked ? 0 : velocity.Z; // 锁定状态，竖直方向向量0
                    // Logger.Log(" - Arcing当前车速 {0} - {1}, {2}", pBullet.Ref.Speed, velocity, pVelocity.IsNull ? "null" : pVelocity.Data);
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        // 除 ROT>0 和 Vertical 之外的抛射体会在Label_158根据速度向量获取坐标
        // Arcing抛射体即使向量非常小也会试图移动1点
        [Hook(HookType.AresHook, Address = 0x4677C2, Size = 5)]
        public static unsafe UInt32 BulletClass_Update_ChangeVelocity_Locked(REGISTERS* R)
        {
            try
            {
                Pointer<BulletClass> pBullet = (IntPtr)R->EBP;
                if (pBullet.AmIArcing() && pBullet.TryGetStatus(out BulletStatusScript status))
                {
                    if (status.SpeedChanged && status.LocationLocked)
                    {
                        // Logger.Log("Label_158 当前坐标 {0} - 坐标 {{\"X\":{1}, \"Y\":{2}, \"Z\":{3}}}", pBullet.Ref.Base.Location, R->ESI, R->EDI, R->EAX);
                        CoordStruct location = pBullet.Ref.Base.Location;
                        R->ESI = (uint)location.X;
                        R->EDI = (uint)location.Y;
                        R->EAX = (uint)location.Y; // 不要问为什么不是Z
                    }
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        // 抛射体爆炸时读取发射者的阵营所属，当发射者死亡时，变中立弹头
        [Hook(HookType.AresHook, Address = 0x469A75, Size = 7)]
        public static unsafe UInt32 BulletClass_Detonate_GetHouse(REGISTERS* R)
        {
            Pointer<BulletClass> pBullet = (IntPtr)R->ESI;
            Pointer<HouseClass> pHouse = (IntPtr)R->ECX;
            if (pHouse.IsNull && pBullet.TryGetStatus(out BulletStatusScript status) && !status.pSourceHouse.IsNull)
            {
                // Logger.Log($"{Game.CurrentFrame} 抛射体的发射者死了");
                R->ECX = (uint)status.pSourceHouse;
            }
            return 0;
        }

        // Take over to create Warhead Anim
        [Hook(HookType.AresHook, Address = 0x469C4E, Size = 5)]
        public static unsafe UInt32 BulletClass_Detonate_WHAnim_Remap(REGISTERS* R)
        {
            try
            {
                Pointer<BulletClass> pBullet = (IntPtr)R->ESI;
                Pointer<AnimTypeClass> pAnimType = (IntPtr)R->EBX;
                CoordStruct location = R->Stack<CoordStruct>(0x64);
                // Logger.Log($"{Game.CurrentFrame} - 抛射体 {pBullet} [{pBullet.Ref.Type.Ref.Base.Base.ID}] 所属 {(pBullet.Ref.Owner.IsNull ? "null" : pBullet.Ref.Owner.Ref.Owner)} 播放弹头动画 {pAnimType} [{pAnimType.Ref.Base.Base.ID}], 位置 {location}");
                // 播放弹头动画
                // Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, location);
                Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, location, 0, 1, BlitterFlags.Flat | BlitterFlags.bf_400 | BlitterFlags.Centered, -15, false);
                pAnim.SetAnimOwner(pBullet);
                pAnim.SetCreater(pBullet);
                return 0x469D06;
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x469D5C, Size = 6)]
        public static unsafe UInt32 BulletClass_Detonate_WHVxlDebris_Remap(REGISTERS* R)
        {
            try
            {
                if (AudioVisual.Data.AllowMakeVoxelDebrisByKratos)
                {
                    Pointer<BulletClass> pBullet = (IntPtr)R->ESI;
                    Pointer<WarheadTypeClass> pWH = (IntPtr)R->EAX;
                    int times = (int)R->EBX;
                    Pointer<HouseClass> pHouse = pBullet.GetSourceHouse();
                    Pointer<TechnoClass> pCreater = pBullet.Ref.Owner;
                    CoordStruct location = pBullet.Ref.Base.Base.GetCoords();
                    ExpandAnims.PlayExpandDebirs(pWH.Ref.DebrisTypes, pWH.Ref.DebrisMaximums, times, location, pHouse, pCreater);
                    R->EBX = 0;
                    return 0x469E18;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x469EB4, Size = 6)]
        public static unsafe UInt32 BulletClass_Detonate_WHDebris_Remap(REGISTERS* R)
        {
            try
            {
                Pointer<BulletClass> pBullet = (IntPtr)R->ESI;
                Pointer<AnimClass> pAnim = (IntPtr)R->EDI;
                if (!pAnim.IsNull)
                {
                    pAnim.SetAnimOwner(pBullet);
                    pAnim.SetCreater(pBullet);
                }
                int i = (int)R->EBX;
                // Logger.Log($"{Game.CurrentFrame} 剩余碎片数量 {i}");
                if (i > 0)
                {
                    return 0x469E34;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
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

        [Hook(HookType.AresHook, Address = 0x4683E2, Size = 5)]
        public static unsafe UInt32 BulletClass_DrawSHP_Colour(REGISTERS* R)
        {
            Pointer<BulletClass> pBullet = (IntPtr)R->ESI;
            if (pBullet.TryGetStatus(out BulletStatusScript status) && status.PaintballState.NeedPaint(out bool changeColor, out bool changeBright) && changeColor)
            {
                R->Stack(0, status.PaintballState.Data.GetColor());
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x4683E7, Size = 9)]
        public static unsafe UInt32 BulletClass_DrawSHP_Bright(REGISTERS* R)
        {
            Pointer<BulletClass> pBullet = (IntPtr)R->ESI;
            if (pBullet.TryGetStatus(out BulletStatusScript status) && status.PaintballState.NeedPaint(out bool changeColor, out bool changeBright) && changeBright)
            {
                R->Stack(0, status.PaintballState.Data.GetBright(1000));
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x46B201, Size = 7)]
        public static unsafe UInt32 BulletClass_DrawVXL_Colour(REGISTERS* R)
        {
            try
            {
                // Logger.Log($"{Game.CurrentFrame} {R->Stack<IntPtr>(0x10)} {R->Stack<IntPtr>(0x14)} {R->Stack<IntPtr>(0x10 - 0x14)} {R->Stack<IntPtr>(0x10 - 0x4)} {R->Stack<IntPtr>(0x10 - 0x8)} {R->Stack<uint>(0)} {(BlitterFlags)R->EDI}");
                R->EDI = (uint)BlitterFlags.None;
                // R->Stack<uint>(0x118, 2000);
                R->Stack<uint>(0, ColorStruct.Red.ToColorAdd().Add2RGB565());
                Pointer<BulletClass> pBullet = R->Stack<IntPtr>(0x10 - 0x4);
                // Logger.Log($"{Game.CurrentFrame} [{pBullet.Ref.Type.Ref.Base.Base.ID}] {ColorStruct.Red.ToColorAdd().Add2RGB565()}");
                if (pBullet.TryGetStatus(out BulletStatusScript status))
                {
                    status.BulletClass_DrawVXL_Paintball(R);
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

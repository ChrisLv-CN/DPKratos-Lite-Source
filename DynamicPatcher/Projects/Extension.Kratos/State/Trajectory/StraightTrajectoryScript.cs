using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{
    [Serializable]
    public class StraightBullet
    {
        public CoordStruct sourcePos;
        public CoordStruct targetPos;
        public BulletVelocity Velocity;

        public StraightBullet(CoordStruct sourcePos, CoordStruct targetPos, BulletVelocity bulletVelocity)
        {
            this.sourcePos = sourcePos;
            this.targetPos = targetPos;
            this.Velocity = bulletVelocity;
        }

        public void ResetVelocity(Pointer<BulletClass> pBullet)
        {
            // Logger.Log($"{Game.CurrentFrame} 重新设置直线导弹的向量，当前记录 {this.Velocity}");
            this.Velocity = pBullet.RecalculateBulletVelocity(sourcePos, targetPos);
        }
    }


    public partial class BulletStatusScript
    {

        private StraightBullet straightBullet;
        private bool resetTargetFlag;


        public void InitState_Trajectory_Straight()
        {
            // 直线弹道
            CoordStruct sourcePos = pBullet.Ref.SourceCoords;
            CoordStruct targetPos = pBullet.Ref.TargetCoords;

            // 绝对直线，重设目标坐标
            if (trajectoryData.AbsolutelyStraight && !pBullet.Ref.Owner.IsNull)
            {
                // Logger.Log("{0} 绝对直线弹道", pBullet.Ref.Type.Ref.Base.Base.ID);
                double distance = targetPos.DistanceFrom(sourcePos);
                DirStruct facing = pBullet.Ref.Owner.Ref.GetRealFacing().current();
                targetPos = FLHHelper.GetFLHAbsoluteCoords(sourcePos, new CoordStruct((int)distance, 0, 0), facing);
                pBullet.Ref.TargetCoords = targetPos;

                // BulletEffectHelper.BlueLine(pBullet.Ref.SourceCoords, pBullet.Ref.TargetCoords, 1, 90);
            }
            // 重设速度
            BulletVelocity velocity = pBullet.Ref.Velocity;
            // Logger.Log($"{Game.CurrentFrame} 抛射体原始速度 {velocity}");
            bool reset = true;
            if (!pBullet.Ref.WeaponType.IsNull)
            {
                WeaponTypeData weaponTypeData = pBullet.Ref.WeaponType.GetData();
                if (weaponTypeData.RadialFire)
                {
                    SingleVector3D sourceV = sourcePos.ToSingleVector3D();
                    SingleVector3D targetV = sourceV + velocity.ToSingleVector3D();
                    CoordStruct forward = FLHHelper.GetForwardCoords(sourceV, targetV, pBullet.Ref.Speed);
                    velocity = (forward - sourcePos).ToBulletVelocity();
                    if (pBullet.Ref.Type.Ref.Level)
                    {
                        velocity.Z = 0;
                    }
                    reset = false;
                }
            }
            if (reset)
            {
                velocity = pBullet.RecalculateBulletVelocity(sourcePos, targetPos);
            }
            straightBullet = new StraightBullet(sourcePos, targetPos, velocity);

            // Logger.Log($"{Game.CurrentFrame} 计算直线导弹[{section}]{pBullet}的向量，记录向量 {velocity}");

            // 设置触碰引信
            if (pBullet.Ref.Type.Ref.Proximity)
            {
                ActiveProximity();
            }
        }

        public void OnUpdate_Trajectory_Straight()
        {
            if (null != straightBullet && !CaptureByBlackHole)
            {
                // 强制修正速度
                pBullet.Ref.Velocity = straightBullet.Velocity;
            }
            // 看不懂西木的做法，自己来
            Pointer<AbstractClass> pTarget = pBullet.Ref.Target;
            if (!pTarget.IsNull && !resetTargetFlag)
            {
                CoordStruct targetCoord = pBullet.Ref.TargetCoords;
                CoordStruct targetPos = pTarget.Ref.GetCoords();
                if (pTarget.Ref.IsInAir())
                {
                    // 目标在空中，目标当前的距离和目标位置相差一个格子，则取消目标
                    if (MapClass.Instance.TryGetCellAt(targetPos, out Pointer<CellClass> pCell) && MapClass.Instance.TryGetCellAt(targetCoord, out Pointer<CellClass> pTargetCell))
                    {
                        if (pCell != pTargetCell)
                        {
                            pBullet.Ref.SetTarget(IntPtr.Zero);
                            resetTargetFlag = true;
                        }
                    }
                    else if (targetCoord.DistanceFrom(targetPos) >= 128)
                    {
                        pBullet.Ref.SetTarget(IntPtr.Zero);
                        resetTargetFlag = true;
                    }
                }
                else
                {
                    // 目标离开所在的格子，就设定目标位置的格子为目标
                    if (MapClass.Instance.TryGetCellAt(targetPos, out Pointer<CellClass> pCell) && MapClass.Instance.TryGetCellAt(targetCoord, out Pointer<CellClass> pTargetCell))
                    {
                        if (pCell != pTargetCell)
                        {
                            pBullet.Ref.SetTarget(pTargetCell.Convert<AbstractClass>());
                            resetTargetFlag = true;
                        }
                    }
                }
            }
        }

        public bool IsStraight()
        {
            return isRocket && null != straightBullet;
        }

        public void ResetStraightMissileVelocity()
        {
            if (null != straightBullet)
            {
                straightBullet.ResetVelocity(pBullet);
            }
        }

    }
}

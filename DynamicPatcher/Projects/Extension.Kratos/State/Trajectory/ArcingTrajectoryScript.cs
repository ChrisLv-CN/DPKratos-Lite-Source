using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class BulletStatusScript
    {
        private bool arcingTrajectoryInitFlag;

        public void OnUpdate_Trajectory_Arcing()
        {
            ResetArcingVelocity();
        }

        public void ResetArcingVelocity(float speedMultiple = 1f, bool force = false)
        {
            if (force || !arcingTrajectoryInitFlag && trajectoryData.AdvancedBallistics)
            {
                arcingTrajectoryInitFlag = true;
                CoordStruct sourcePos = pBullet.Ref.Base.Base.GetCoords();
                CoordStruct targetPos = pBullet.Ref.TargetCoords;

                // Logger.Log($"{Game.CurrentFrame} 炮弹 [{section}]{pBullet} 出膛初速度 {pBullet.Ref.Velocity}");

                // 速度控制
                if (trajectoryData.ArcingFixedSpeed > 0)
                {
                    // Logger.Log($"{Game.CurrentFrame} 原抛射体[{section}]{pBullet} 速度{pBullet.Ref.Speed}, 高级弹道学, 改使用恒定速度{trajectoryData.ArcingFixedSpeed}");
                    pBullet.Ref.Speed = trajectoryData.ArcingFixedSpeed;
                }
                else
                {
                    // Logger.Log($"{Game.CurrentFrame} 原抛射体[{section}]{pBullet} 速度{pBullet.Ref.Speed}, 高级弹道学, 加速度{pBullet.Ref.Type.Ref.Acceleration}");
                    pBullet.Ref.Speed += pBullet.Ref.Type.Ref.Acceleration;
                }
                int speed = (int)(pBullet.Ref.Speed * speedMultiple);
                int gravity = RulesClass.Global().Gravity;
                if (trajectoryData.Gravity > 0)
                {
                    gravity = trajectoryData.Gravity;
                }
                bool lobber = !pBullet.Ref.WeaponType.IsNull ? pBullet.Ref.WeaponType.Ref.Lobber : false;
                bool inaccurate = trajectoryData.Inaccurate;
                float min = trajectoryData.BallisticScatterMin;
                float max = trajectoryData.BallisticScatterMax;

                BulletVelocity velocity = WeaponHelper.GetBulletArcingVelocity(sourcePos, ref targetPos, speed, gravity, lobber, inaccurate, min, max, pBullet.Ref.Velocity.ToCoordStruct().Z, out double straightDistance, out double realSpeed, out Pointer<CellClass> pTargetCell);
                pBullet.Ref.Speed = (int)realSpeed;
                pBullet.Ref.Velocity = velocity;
                pBullet.Ref.TargetCoords = targetPos;
                if (inaccurate && !pTargetCell.IsNull)
                {
                    pBullet.Ref.Target = pTargetCell.Convert<AbstractClass>();
                }

                /*
                // 高抛弹道
                if (!pBullet.Ref.WeaponType.IsNull && pBullet.Ref.WeaponType.Ref.Lobber)
                {
                    pBullet.Ref.Speed = (int)(pBullet.Ref.Speed * 0.5);
                    // Logger.Log("高抛弹道, 削减速度{0}", pBullet.Ref.Speed);
                }

                // 不精确
                if (trajectoryData.Inaccurate)
                {
                    // 不精确, 需要修改目标坐标
                    int min = (int)(trajectoryData.BallisticScatterMin * 256);
                    int max = trajectoryData.BallisticScatterMax > 0 ? (int)(trajectoryData.BallisticScatterMax * 256) : RulesClass.Global().BallisticScatter;
                    // Logger.Log("炮弹[{0}]不精确, 需要重新计算目标位置, 散布范围=[{1}, {2}]", pBullet.Ref.Type.Convert<AbstractTypeClass>().Ref.ID, min, max);
                    if (min > max)
                    {
                        int temp = min;
                        min = max;
                        max = temp;
                    }
                    // 随机
                    double r = MathEx.Random.Next(min, max);
                    var theta = MathEx.Random.NextDouble() * 2 * Math.PI;
                    CoordStruct offset = new CoordStruct((int)(r * Math.Cos(theta)), (int)(r * Math.Sin(theta)), 0);
                    targetPos += offset;
                    pBullet.Ref.TargetCoords = targetPos;
                    // Logger.Log("计算结果, 随机半径{0}[{1},{2}], 随机角度{3}, 偏移{4}", r, min, max, theta, offset);
                    // if (MapClass.Instance.TryGetCellAt(targetPos, out Pointer<CellClass> pCell))
                    // {
                    //     CoordStruct cellPos = pCell.Ref.GetCoordsWithBridge();
                    //     targetPos.Z = cellPos.Z; // 修正高度差
                    // }
                }

                // 重算抛物线弹道
                int zDiff = targetPos.Z - sourcePos.Z + pBullet.Ref.Velocity.ToCoordStruct().Z; // 修正高度差
                targetPos.Z = 0;
                sourcePos.Z = 0;
                double distance = targetPos.DistanceFrom(sourcePos);
                // Logger.Log("位置和目标的水平距离{0}", distance);
                double speed = pBullet.Ref.Speed;
                // Logger.Log("重新计算初速度, 当前速度{0}", speed);
                double vZ = (zDiff * speed) / distance + (0.5 * RulesClass.Global().Gravity * distance) / speed;
                // Logger.Log("计算Z方向的初始速度{0}", vZ);
                BulletVelocity v = new BulletVelocity(targetPos.X - sourcePos.X, targetPos.Y - sourcePos.Y, 0);
                v *= speed / distance;
                v.Z = vZ;
                pBullet.Ref.Velocity = v;
                */
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Utilities
{

    public static class BulletTypeHelper
    {
        public static unsafe BulletVelocity GetVelocity(this Pointer<BulletClass> pBullet)
        {
            return GetVelocity(pBullet.Ref.SourceCoords, pBullet.Ref.TargetCoords, pBullet.Ref.Speed);
        }

        public static unsafe BulletVelocity GetVelocity(CoordStruct sourcePos, CoordStruct targetPos, int speed)
        {
            BulletVelocity velocity = new BulletVelocity(targetPos.X - sourcePos.X, targetPos.Y - sourcePos.Y, targetPos.Z - sourcePos.Z);
            velocity *= speed / targetPos.DistanceFrom(sourcePos);
            return velocity;
        }

        public static unsafe BulletVelocity RecalculateBulletVelocity(this Pointer<BulletClass> pBullet)
        {
            CoordStruct targetPos = pBullet.Ref.TargetCoords;
            Pointer<AbstractClass> pTarget = pBullet.Ref.Target;
            if (!pTarget.IsNull)
            {
                targetPos = pTarget.Ref.GetCoords();
            }
            return pBullet.RecalculateBulletVelocity(targetPos);
        }

        public static unsafe BulletVelocity RecalculateBulletVelocity(this Pointer<BulletClass> pBullet, CoordStruct targetPos)
        {
            return pBullet.RecalculateBulletVelocity(pBullet.Ref.Base.Base.GetCoords(), targetPos);
        }

        public static unsafe BulletVelocity RecalculateBulletVelocity(this Pointer<BulletClass> pBullet, CoordStruct sourcePos, CoordStruct targetPos)
        {
            BulletVelocity velocity = new BulletVelocity(targetPos.X - sourcePos.X, targetPos.Y - sourcePos.Y, targetPos.Z - sourcePos.Z);
            velocity *= pBullet.Ref.Speed / targetPos.DistanceFrom(sourcePos);
            pBullet.Ref.Velocity = velocity;
            pBullet.Ref.SourceCoords = sourcePos;
            pBullet.Ref.TargetCoords = targetPos;
            return velocity;
        }


        public static unsafe Pointer<HouseClass> GetHouse(this Pointer<BulletClass> pBullet)
        {
            Pointer<TechnoClass> pOwner = pBullet.Ref.Owner;
            if (!pOwner.IsNull)
            {
                return pOwner.Ref.Owner;
            }
            return pBullet.GetSourceHouse();
        }

        public static unsafe Pointer<HouseClass> GetSourceHouse(this Pointer<BulletClass> pBullet)
        {
            if (pBullet.TryGetStatus(out BulletStatusScript status))
            {
                return status.pSourceHouse;
            }
            Pointer<TechnoClass> pOwner = pBullet.Ref.Owner;
            if (!pOwner.IsNull)
            {
                return pOwner.Ref.Owner;
            }
            return IntPtr.Zero;
        }

        public static unsafe void SetSourceHouse(this Pointer<BulletClass> pBullet, Pointer<HouseClass> pHouse)
        {
            if (!pHouse.IsNull && pBullet.TryGetStatus(out BulletStatusScript status))
            {
                status.pSourceHouse.Pointer = pHouse;
            }
        }

        public static unsafe bool AmIArcing(this Pointer<BulletClass> pBullet)
        {
            return pBullet.Ref.Type.Ref.Arcing || pBullet.Ref.Type.Ref.ROT <= 0;
        }

        public static unsafe bool AmIMissile(this Pointer<BulletClass> pBullet)
        {
            return !pBullet.Ref.Type.Ref.Arcing && pBullet.Ref.Type.Ref.ROT > 1;
        }

        public static unsafe bool AmIRocket(this Pointer<BulletClass> pBullet)
        {
            return !pBullet.Ref.Type.Ref.Arcing && pBullet.Ref.Type.Ref.ROT == 1;
        }

        // 高级弹道学
        public static BulletVelocity GetBulletArcingVelocity(CoordStruct sourcePos, CoordStruct targetPos, double speed, int gravity, bool lobber, bool inaccurate, float scatterMin, float scatterMax, out double straightDistance, out double realSpeed)
        {
            // 不精确
            if (inaccurate)
            {
                targetPos += GetInaccurateOffset(scatterMin, scatterMax);
            }

            // 重算抛物线弹道
            if (gravity == 0)
            {
                gravity = RulesClass.Global().Gravity;
            }
            int zDiff = targetPos.Z - sourcePos.Z + gravity; // 修正高度差
            targetPos.Z = 0;
            sourcePos.Z = 0;
            straightDistance = targetPos.DistanceFrom(sourcePos);
            // Logger.Log("位置和目标的水平距离{0}", straightDistance);
            realSpeed = speed;
            if (straightDistance == 0 || double.IsNaN(straightDistance))
            {
                // 直上直下
                return new BulletVelocity(0, 0, gravity);
            }
            if (realSpeed == 0)
            {
                // realSpeed = WeaponTypeClass.GetSpeed((int)straightDistance, gravity);
                realSpeed = Math.Sqrt(straightDistance * gravity * 1.2);
                // Logger.Log($"YR计算的速度{realSpeed}, 距离 {(int)straightDistance}, 重力 {gravity}");
            }
            // 高抛弹道
            if (lobber)
            {
                realSpeed = (int)(realSpeed * 0.5);
                // Logger.Log("高抛弹道, 削减速度{0}", pBullet.Ref.Speed);
            }
            // Logger.Log("重新计算初速度, 当前速度{0}", realSpeed);
            double vZ = (zDiff * realSpeed) / straightDistance + 0.5 * gravity * straightDistance / realSpeed;
            // Logger.Log("计算Z方向的初始速度{0}", vZ);
            BulletVelocity v = new BulletVelocity(targetPos.X - sourcePos.X, targetPos.Y - sourcePos.Y, 0);
            v *= realSpeed / straightDistance;
            v.Z = vZ;
            return v;
        }

        public static CoordStruct GetInaccurateOffset(float scatterMin, float scatterMax)
        {
            // 不精确, 需要修改目标坐标
            int min = (int)(scatterMin * 256);
            int max = scatterMax > 0 ? (int)(scatterMax * 256) : RulesClass.Global().BallisticScatter;
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
            return offset;
        }
    }
}

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
        // 热诱弹设置
        public bool IsDecoy;
        public CoordStruct LaunchPos;
        public TimerStruct LifeTimer;
        private int _weaponRange;
        private int WeaponRange
        {
            get
            {
                if (0 == _weaponRange && !pBullet.Ref.WeaponType.IsNull)
                {
                    _weaponRange = pBullet.Ref.WeaponType.Ref.Range;
                }
                return _weaponRange;
            }
        }

        public void InitState_Trajectory_Missile()
        {
            if (isMissile)
            {
                // 高抛导弹
                if (!pBullet.Ref.WeaponType.IsNull && pBullet.Ref.WeaponType.Ref.Lobber)
                {
                    if (pBullet.Ref.Velocity.Z < 0)
                    {
                        pBullet.Ref.Velocity.Z *= -1;
                    }
                    pBullet.Ref.Velocity.Z += RulesClass.Global().Gravity;
                }

                // 翻转发射方向
                if (trajectoryData.ReverseVelocity)
                {
                    BulletVelocity velocity = pBullet.Ref.Velocity;
                    pBullet.Ref.Velocity *= -1;
                    if (!trajectoryData.ReverseVelocityZ)
                    {
                        pBullet.Ref.Velocity.Z = velocity.Z;
                    }
                }

                // 晃动的出膛方向
                if (trajectoryData.ShakeVelocity != 0)
                {
                    BulletVelocity velocity = pBullet.Ref.Velocity;
                    double shakeX = MathEx.Random.NextDouble() * trajectoryData.ShakeVelocity;
                    double shakeY = MathEx.Random.NextDouble() * trajectoryData.ShakeVelocity;
                    double shakeZ = MathEx.Random.NextDouble();
                    pBullet.Ref.Velocity.X *= shakeX;
                    pBullet.Ref.Velocity.Y *= shakeY;
                    pBullet.Ref.Velocity.Z *= shakeZ;
                }
            }
        }

        public void OnUpdate_Trajectory_Decroy()
        {
            if (IsDecoy)
            {
                // 检查存活时间
                if (LifeTimer.Expired())
                {
                    LifeData.Detonate(true);
                }
                else
                {
                    // 执行热诱弹轨迹变化
                    // Check distance to Change speed and target point
                    int speed = pBullet.Ref.Speed - 5;
                    pBullet.Ref.Speed = speed < 10 ? 10 : speed;
                    if (speed > 10 && LaunchPos.DistanceFrom(pBullet.Ref.Base.Base.GetCoords()) <= WeaponRange)
                    {
                        pBullet.Ref.Base.Location += new CoordStruct(0, 0, 64);
                    }
                }
            }
        }

    }
}

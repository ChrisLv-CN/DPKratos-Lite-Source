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

        public RecordBulletStatus RecordBulletStatus;
        public bool SpeedChanged = false;
        public bool LocationLocked = false;

        public unsafe void OnUpdate_RecalculateStatus()
        {
            if (!SpeedChanged)
            {
                RecordBulletStatus = new RecordBulletStatus(pBullet.Ref.Base.Health, pBullet.Ref.Speed, pBullet.Ref.Velocity, pBullet.Ref.CourseLocked);
            }
            // 计算AE伤害加成
            if (pBullet.TryGetAEManager(out AttachEffectScript aeManager))
            {
                CrateBuffData aeBuff = aeManager.CountAttachStatusMultiplier();
                int newHealth = RecordBulletStatus.Health;
                if (aeBuff.FirepowerMultiplier != 1)
                {
                    // 调整参数
                    newHealth = (int)Math.Ceiling(newHealth * aeBuff.FirepowerMultiplier);
                }
                // 重设伤害值
                pBullet.Ref.Base.Health = newHealth;
                if (null != DamageData)
                {
                    DamageData.Damage = newHealth;
                }
                // 计算AE速度加成
                if (aeBuff.SpeedMultiplier != 1)
                {
                    SpeedChanged = true;
                }
                // Logger.Log("方向向量 - {0}，速度系数{1}，记录向量{2}", pBullet.Ref.Velocity, aeMultiplier.SpeedMultiplier, RecordBulletStatus.Velocity);
                // 还原
                if (SpeedChanged && aeBuff.SpeedMultiplier == 1.0)
                {
                    SpeedChanged = false;
                    LocationLocked = false;
                    pBullet.Ref.Speed = RecordBulletStatus.Speed;
                    if (IsStraight(out StraightTrajectoryScript straight))
                    {
                        // 恢复直线弹道的向量
                        straight.ResetVelocity();
                    }
                    else if (pBullet.Ref.Type.Ref.Arcing)
                    {
                        // 抛物线类型的向量，只恢复方向向量，即X和Y
                        double x = RecordBulletStatus.Velocity.X;
                        double y = RecordBulletStatus.Velocity.Y;
                        BulletVelocity nowVelocity = pBullet.Ref.Velocity;
                        if (nowVelocity.X < 0 && x > 0)
                        {
                            x *= -1;
                        }
                        if (nowVelocity.Y < 0 && y > 0)
                        {
                            y *= -1;
                        }
                        pBullet.Ref.Velocity.X = x;
                        pBullet.Ref.Velocity.Y = y;
                    }
                    return;
                }

                // 更改运动向量
                if (SpeedChanged)
                {
                    double multiplier = aeBuff.SpeedMultiplier;
                    if (multiplier == 0.0)
                    {
                        LocationLocked = true;
                        pBullet.Ref.Speed = 1;
                        multiplier = 1E-19;
                    }
                    // 导弹类需要每帧更改一次运动向量
                    if (IsStraight(out StraightTrajectoryScript straight))
                    {
                        // 直线导弹用保存的向量覆盖，每次都要重新计算
                        pBullet.Ref.Velocity *= multiplier;
                    }
                    else if (pBullet.Ref.Type.Ref.Arcing)
                    {
                        // Arcing类，重算方向上向量，即X和Y
                        BulletVelocity recVelocity = RecordBulletStatus.Velocity;
                        recVelocity.Z = pBullet.Ref.Velocity.Z;
                        BulletVelocity newVelocity = recVelocity * multiplier;
                        pBullet.Ref.Velocity = newVelocity;
                    }
                    else
                    {
                        pBullet.Ref.Velocity *= multiplier;
                    }

                    // Logger.Log(" - 方向向量{0}，速度系数{1}，记录向量{2}", pBullet.Ref.Velocity, aeMultiplier.SpeedMultiplier, RecordBulletStatus.Velocity);
                }
            }
        }

        private bool IsStraight(out StraightTrajectoryScript straight)
        {
            straight = null;
            if (pBullet.TryGetComponent<StraightTrajectoryScript>(out straight))
            {
                return straight.IsStraight();
            }
            return false;
        }

    }
}

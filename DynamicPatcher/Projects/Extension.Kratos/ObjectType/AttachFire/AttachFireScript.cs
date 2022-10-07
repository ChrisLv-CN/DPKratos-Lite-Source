using System.ComponentModel;
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

    // Shooter发射属于Attacker的Weapon朝Target
    [Serializable]
    [GlobalScriptable(typeof(TechnoExt), typeof(BulletExt))]
    public class AttachFireScript : ObjectScriptable
    {
        public AttachFireScript(IExtension owner) : base(owner) { }

        // 发射自身武器的待发射的队列
        private Queue<DelayFireWeapon> delayFires = new Queue<DelayFireWeapon>();

        // Burst发射模式下剩余待发射的队列
        private Queue<SimulateBurst> simulateBurstQueue = new Queue<SimulateBurst>();

        public override void OnUpdate()
        {
            if (pObject.IsDead() || pObject.IsInvisible())
            {
                delayFires.Clear();
                simulateBurstQueue.Clear();
                return;
            }
            Pointer<ObjectClass> pShooter = pObject;
            // 发射自身武器
            if (pShooter.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                for (int i = 0; i < delayFires.Count; i++)
                {
                    DelayFireWeapon delayFire = delayFires.Dequeue();
                    if (delayFire.TimesUp())
                    {
                        // 发射武器
                        if (delayFire.FireOwnWeapon)
                        {
                            pTechno.Ref.Fire_IgnoreType(delayFire.pTarget, delayFire.WeaponIndex);
                        }
                        else
                        {
                            double rofMult = pTechno.GetROFMult();
                            FireCustomWeapon(pTechno, delayFire.pTarget, pTechno.Ref.Owner, delayFire.pWeapon, delayFire.weaponTypeData, delayFire.FLH, rofMult);
                        }
                        delayFire.ReduceOnce();
                    }
                    if (delayFire.NotDone())
                    {
                        delayFires.Enqueue(delayFire);
                    }
                }
            }
            else
            {
                delayFires.Clear();
            }
            // 模拟Burst发射
            for (int i = 0; i < simulateBurstQueue.Count; i++)
            {
                SimulateBurst burst = simulateBurstQueue.Dequeue();
                // 检查是否还需要发射
                if (burst.Index < burst.Burst)
                {
                    // 检查延迟
                    if (burst.CanFire())
                    {
                        Pointer<TechnoClass> pAttacker = burst.pAttacker; // 武器所属对象，可以为null
                        Pointer<AbstractClass> pTarget = burst.pTarget; // 武器的目标
                        Pointer<HouseClass> pAttackingHouse = burst.pAttackingHouse; // 武器所属阵营
                        Pointer<WeaponTypeClass> pWeaponType = burst.pWeaponType;
                        // 检查目标幸存和射程
                        if (!pWeaponType.IsNull // 武器存在
                            && !pTarget.IsNull && (!pTarget.CastToTechno(out Pointer<TechnoClass> pTemp) || !pTemp.IsDeadOrInvisible()) // 目标存在
                            && (!burst.WeaponTypeData.CheckRange || InRange(pTarget, burst)) // 射程之内
                        )
                        {
                            // 发射
                            SimulateBurstFire(burst);
                        }
                        else
                        {
                            // 武器失效
                            continue;
                        }
                    }
                    // 归队
                    simulateBurstQueue.Enqueue(burst);
                }
            }
        }

        private bool InRange(Pointer<AbstractClass> pTarget, Pointer<WeaponTypeClass> pWeaponType)
        {
            CoordStruct location = pObject.Ref.Base.GetCoords();
            switch (pObject.Ref.Base.WhatAmI())
            {
                case AbstractType.Bullet:
                    CoordStruct targetPos = pTarget.Ref.GetCoords();
                    double distance = targetPos.DistanceFrom(location);
                    double minRange = pWeaponType.Ref.MinimumRange;
                    double maxRange = pWeaponType.Ref.Range;
                    return distance <= pWeaponType.Ref.Range && distance >= minRange;
                default:
                    return pObject.Convert<TechnoClass>().Ref.InRange(location, pTarget, pWeaponType);
            }
        }

        private bool InRange(Pointer<AbstractClass> pTarget, SimulateBurst burst)
        {
            return InRange(pTarget, burst.pWeaponType);
        }

        public void FireOwnWeapon(int weaponIndex, Pointer<AbstractClass> pTarget, int delay = 0, int count = 1)
        {
            DelayFireWeapon delayFireWeapon = new DelayFireWeapon(weaponIndex, pTarget, delay, count);
            delayFires.Enqueue(delayFireWeapon);
        }

        /// <summary>
        /// 下订单发射自定义武器
        /// </summary>
        /// <param name="pAttacker"></param>
        /// <param name="pTarget"></param>
        /// <param name="pAttackingHouse"></param>
        /// <param name="weaponId"></param>
        /// <param name="flh"></param>
        /// <param name="rofMult"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool FireCustomWeapon(Pointer<TechnoClass> pAttacker, Pointer<AbstractClass> pTarget, Pointer<HouseClass> pAttackingHouse,
            string weaponId, CoordStruct flh,
            double rofMult = 1, FireBulletToTarget callback = null)
        {
            bool isFire = false;
            Pointer<WeaponTypeClass> pWeapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(weaponId);
            if (!pWeapon.IsNull)
            {
                WeaponTypeData weaponTypeData = Ini.GetConfig<WeaponTypeData>(Ini.RulesDependency, weaponId).Data;
                isFire = FireCustomWeapon(pAttacker, pTarget, pAttackingHouse, pWeapon, weaponTypeData, flh, rofMult, callback);
            }
            return isFire;
        }

        /// <summary>
        /// 下订单发射自定义武器
        /// </summary>
        /// <param name="pAttacker"></param>
        /// <param name="pTarget"></param>
        /// <param name="pAttackingHouse"></param>
        /// <param name="pWeapon"></param>
        /// <param name="weaponTypeData"></param>
        /// <param name="flh"></param>
        /// <param name="rofMult"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool FireCustomWeapon(Pointer<TechnoClass> pAttacker, Pointer<AbstractClass> pTarget, Pointer<HouseClass> pAttackingHouse,
            Pointer<WeaponTypeClass> pWeapon, WeaponTypeData weaponTypeData, CoordStruct flh,
            double rofMult = 1, FireBulletToTarget callback = null)
        {
            bool isFire = false;
            int burst = pWeapon.Ref.Burst;
            int minRange = pWeapon.Ref.MinimumRange;
            int range = pWeapon.Ref.Range;
            if (pTarget.Ref.IsInAir() && pObject.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                range += pTechno.Ref.Type.Ref.AirRangeBonus;
            }
            // 检查射程
            if (!weaponTypeData.CheckRange || InRange(pTarget, pWeapon))
            {
                // 发射武器
                if (burst > 1 && weaponTypeData.SimulateBurst)
                {
                    int flipY = 1;
                    Pointer<BulletTypeClass> pBulletType = pWeapon.Ref.Projectile;
                    if (!pBulletType.IsNull)
                    {
                        // 翻转抛射体的速度，左右对调
                        TrajectoryData trajectoryData = Ini.GetConfig<TrajectoryData>(Ini.RulesDependency, pBulletType.Ref.Base.Base.ID).Data;
                        if (trajectoryData.ReverseVelocity)
                        {
                            flipY = -1;
                        }
                    }
                    // 模拟burst发射武器
                    SimulateBurst newBurst = new SimulateBurst(pAttacker, pTarget, pAttackingHouse, pWeapon, flh, burst, minRange, range, weaponTypeData, flipY, callback);
                    // Logger.Log("{0} - {1}{2}添加订单模拟Burst发射{3}发，目标类型{4}，入队", Game.CurrentFrame, pAttacker.IsNull ? "null" : pAttacker.Ref.Type.Ref.Base.Base.ID, pAttacker, burst, pAttacker.Ref.Target.IsNull ? "null" : pAttacker.Ref.Target.Ref.WhatAmI());
                    // 发射武器
                    SimulateBurstFire(newBurst);
                    // 入队
                    simulateBurstQueue.Enqueue(newBurst);
                    isFire = true;
                }
                else
                {
                    // 直接发射武器
                    CoordStruct sourcePos = GetSourcePos(flh, out DirStruct facingDir);
                    CoordStruct targetPos = pTarget.Ref.GetCoords();
                    // 扇形攻击
                    RadialFireHelper radialFireHelper = new RadialFireHelper(facingDir, burst, weaponTypeData.RadialAngle);
                    BulletVelocity bulletVelocity = ExHelper.GetBulletVelocity(sourcePos, targetPos);
                    for (int i = 0; i < burst; i++)
                    {
                        if (weaponTypeData.RadialFire)
                        {
                            bulletVelocity = radialFireHelper.GetBulletVelocity(i);
                        }
                        // 发射武器，全射出去
                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pObject} 发射自定义武器 [{pWeapon.Ref.Base.ID}], 攻击者 [{(pAttacker.IsNull ? "Null" : pAttacker.Ref.Type.Ref.Base.Base.ID)}]{pAttacker}, 目标 [{(pTarget.CastToObject(out Pointer<ObjectClass> pTargetObject) ? pTarget.Ref.WhatAmI() : pTargetObject.Ref.Type.Ref.Base.ID)}]{pTarget}");
                        Pointer<BulletClass> pBullet = ExHelper.FireBulletTo(pObject, pAttacker, pTarget, pAttackingHouse, pWeapon, sourcePos, targetPos, bulletVelocity);
                        if (null != callback)
                        {
                            callback(i, burst, pBullet, pTarget);
                        }
                    }
                    isFire = true;
                }
            }
            return isFire;
        }

        private void SimulateBurstFire(SimulateBurst burst)
        {
            // 模拟Burst发射武器
            if (burst.WeaponTypeData.SimulateBurstMode == 3)
            {
                // 模式3，双发
                SimulateBurst b2 = burst.Clone();
                b2.FlipY *= -1;
                SimulateBurstFireOnce(b2);
            }
            // 单发
            SimulateBurstFireOnce(burst);
        }

        private void SimulateBurstFireOnce(SimulateBurst burst)
        {
            // Pointer<TechnoClass> pShooter = WhoIsShooter(pShooter);
            CoordStruct sourcePos = GetSourcePos(burst, out DirStruct facingDir);
            CoordStruct targetPos = burst.pTarget.Ref.GetCoords();
            BulletVelocity bulletVelocity = default;
            // 扇形攻击
            if (burst.WeaponTypeData.RadialFire)
            {
                RadialFireHelper radialFireHelper = new RadialFireHelper(facingDir, burst.Burst, burst.WeaponTypeData.RadialAngle);
                bulletVelocity = radialFireHelper.GetBulletVelocity(burst.Index);
            }
            else
            {
                bulletVelocity = ExHelper.GetBulletVelocity(sourcePos, targetPos);
            }
            // 发射武器
            // Logger.Log($"{Game.CurrentFrame} [{section}]{pObject} 发射自定义武器 [{burst.pWeaponType.Ref.Base.ID}], 攻击者 [{(burst.pAttacker.IsNull ? "Null" : burst.pAttacker.Ref.Type.Ref.Base.Base.ID)}]{burst.pAttacker}, 目标 [{(burst.pTarget.Pointer.CastToObject(out Pointer<ObjectClass> pTargetObject) ? burst.pTarget.Ref.WhatAmI() : pTargetObject.Ref.Type.Ref.Base.ID)}]{burst.pTarget.Pointer}");
            Pointer<BulletClass> pBullet = ExHelper.FireBulletTo(pObject, burst.pAttacker, burst.pTarget, burst.pAttackingHouse, burst.pWeaponType, sourcePos, targetPos, bulletVelocity);
            if (null != burst.Callback)
            {
                burst.Callback(burst.Index, burst.Burst, pBullet, burst.pTarget);
            }
            burst.CountOne();
        }

        private CoordStruct GetSourcePos(SimulateBurst burst, out DirStruct facingDir)
        {
            return GetSourcePos(burst.FLH, out facingDir, burst.FlipY);
        }

        private CoordStruct GetSourcePos(CoordStruct flh, out DirStruct facingDir, int flipY = 1)
        {
            CoordStruct sourcePos = pObject.Ref.Base.GetCoords();
            facingDir = default;
            switch (pObject.Ref.Base.WhatAmI())
            {
                case AbstractType.Bullet:
                    Pointer<BulletClass> pBullet = pObject.Convert<BulletClass>();
                    sourcePos = pBullet.GetFLHAbsoluteCoords(flh, flipY);
                    facingDir = pBullet.Facing();
                    break;
                default:
                    Pointer<TechnoClass> pTechno = pObject.Convert<TechnoClass>();
                    sourcePos = pTechno.GetFLHAbsoluteCoords(flh, true, flipY);
                    facingDir = pTechno.Ref.GetRealFacing().current();
                    break;
            }
            return sourcePos;
        }

    }
}

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

    public static class WeaponHelper
    {

        public static unsafe Pointer<TechnoClass> WhoIsShooter(this Pointer<TechnoClass> pAttacker)
        {
            Pointer<TechnoClass> pTransporter = pAttacker.Ref.Transporter;
            if (!pTransporter.IsNull)
            {
                // I'm a passengers
                pAttacker = WhoIsShooter(pTransporter); // 递归查找到最外层的载具
            }
            return pAttacker;
        }

        public static unsafe BulletVelocity GetBulletVelocity(CoordStruct sourcePos, CoordStruct targetPos)
        {
            CoordStruct bulletFLH = new CoordStruct(1, 0, 0);
            DirStruct bulletDir = FLHHelper.Point2Dir(sourcePos, targetPos);
            SingleVector3D bulletV = FLHHelper.GetFLHAbsoluteOffset(bulletFLH, bulletDir, default);
            return bulletV.ToBulletVelocity();
        }

        // 高级弹道学
        public static unsafe BulletVelocity GetBulletArcingVelocity(CoordStruct sourcePos, ref CoordStruct targetPos,
            double speed, int gravity, bool lobber, bool inaccurate, float scatterMin, float scatterMax,
            int zOffset, out double straightDistance, out double realSpeed, out Pointer<CellClass> pTargetCell)
        {
            // 不精确
            if (inaccurate)
            {
                targetPos += GetInaccurateOffset(scatterMin, scatterMax);
            }
            // 不潜地
            if (MapClass.Instance.TryGetCellAt(targetPos, out pTargetCell))
            {
                targetPos.Z = pTargetCell.Ref.GetCoordsWithBridge().Z;
            }

            // 重算抛物线弹道
            if (gravity == 0)
            {
                gravity = RulesClass.Global().Gravity;
            }
            CoordStruct tempSourcePos = sourcePos;
            CoordStruct tempTargetPos = targetPos;
            int zDiff = tempTargetPos.Z - tempSourcePos.Z + zOffset; // 修正高度差
            tempTargetPos.Z = 0;
            tempSourcePos.Z = 0;
            straightDistance = tempTargetPos.DistanceFrom(tempSourcePos);
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
                // Logger.Log("高抛弹道, 削减速度{0}", realSpeed);
            }
            // Logger.Log("重新计算初速度, 当前速度{0}", realSpeed);
            double vZ = (zDiff * realSpeed) / straightDistance + 0.5 * gravity * straightDistance / realSpeed;
            // Logger.Log("计算Z方向的初始速度{0}", vZ);
            BulletVelocity v = new BulletVelocity(tempTargetPos.X - tempSourcePos.X, tempTargetPos.Y - tempSourcePos.Y, 0);
            v *= realSpeed / straightDistance;
            v.Z = vZ;
            return v;
        }

        public static unsafe CoordStruct GetInaccurateOffset(float scatterMin, float scatterMax)
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

        public static unsafe void FireWeaponTo(Pointer<TechnoClass> pShooter, Pointer<TechnoClass> pAttacker, Pointer<AbstractClass> pTarget, Pointer<HouseClass> pAttackingHouse,
            Pointer<WeaponTypeClass> pWeapon, CoordStruct flh,
            FireBulletToTarget callback = null, CoordStruct bulletSourcePos = default, bool radialFire = false, int splitAngle = 180, bool radialZ = true)
        {
            if (pTarget.IsNull)
            {
                return;
            }
            CoordStruct targetPos = pTarget.Ref.GetCoords();
            // radial fire
            int burst = pWeapon.Ref.Burst;
            RadialFireHelper radialFireHelper = new RadialFireHelper(pShooter, burst, splitAngle);
            int flipY = -1;
            for (int i = 0; i < burst; i++)
            {
                BulletVelocity bulletVelocity = default;
                if (radialFire)
                {
                    flipY = (i < burst / 2f) ? -1 : 1;
                    bulletVelocity = radialFireHelper.GetBulletVelocity(i, radialZ);
                }
                else
                {
                    flipY *= -1;
                }
                CoordStruct sourcePos = bulletSourcePos;
                if (default == bulletSourcePos)
                {
                    // get flh
                    sourcePos = FLHHelper.GetFLHAbsoluteCoords(pShooter, flh, true, flipY, default);
                }
                if (default == bulletVelocity)
                {
                    bulletVelocity = GetBulletVelocity(sourcePos, targetPos);
                }
                Pointer<BulletClass> pBullet = FireBulletTo(pShooter.Convert<ObjectClass>(), pAttacker, pTarget, pAttackingHouse, pWeapon, sourcePos, targetPos, bulletVelocity);

                // Logger.Log("发射{0}，抛射体{1}，回调{2}", pWeapon.Ref.Base.ID, pBullet.IsNull ? " is null" : pBullet.Ref.Type.Ref.Base.Base.ID, null == callback);
                if (null != callback && !pBullet.IsNull)
                {
                    callback(i, burst, pBullet, pTarget);
                }
            }
        }

        public static unsafe Pointer<BulletClass> FireBulletTo(Pointer<ObjectClass> pShooter, Pointer<TechnoClass> pAttacker, Pointer<AbstractClass> pTarget, Pointer<HouseClass> pAttackingHouse,
            Pointer<WeaponTypeClass> pWeapon,
            CoordStruct sourcePos, CoordStruct targetPos,
            BulletVelocity bulletVelocity = default)
        {
            if (pTarget.IsNull || (pTarget.CastToTechno(out Pointer<TechnoClass> pTechno) && !pTechno.Ref.Base.IsAlive))
            {
                return IntPtr.Zero;
            }
            // Fire weapon
            Pointer<BulletClass> pBullet = FireBullet(pAttacker, pTarget, pAttackingHouse, pWeapon, sourcePos, targetPos, bulletVelocity);
            // Draw bullet effect
            DrawBulletEffect(pWeapon, sourcePos, targetPos, pAttacker, pTarget, pAttackingHouse);
            // Draw particle system
            AttachedParticleSystem(pWeapon, sourcePos, pTarget, pAttacker, targetPos);
            // Play report sound
            PlayReportSound(pWeapon, sourcePos);
            if (!pShooter.IsNull)
            {
                // Draw weapon anim
                DrawWeaponAnim(pShooter, pWeapon, sourcePos, targetPos);
                // FeedbackAttachEffects
                AttachEffectScript.FeedbackAttach(pShooter, pWeapon);
            }
            return pBullet;
        }

        public static unsafe Pointer<BulletClass> FireBullet(Pointer<TechnoClass> pAttacker, Pointer<AbstractClass> pTarget, Pointer<HouseClass> pAttackingHouse,
            Pointer<WeaponTypeClass> pWeapon,
            CoordStruct sourcePos, CoordStruct targetPos,
            BulletVelocity bulletVelocity = default)
        {
            double fireMult = 1;
            if (!pAttacker.IsDead())
            {
                // check spawner
                Pointer<SpawnManagerClass> pSpawn = pAttacker.Ref.SpawnManager;
                if (pWeapon.Ref.Spawner && !pSpawn.IsNull)
                {
                    pSpawn.Ref.SetTarget(pTarget);
                    return Pointer<BulletClass>.Zero;
                }
                // check Abilities FIREPOWER
                fireMult = pAttacker.GetDamageMult(); //GetDamageMult(pAttacker);
            }

            Pointer<BulletClass> pBullet = FireBullet(pWeapon, fireMult, pTarget, pAttacker, pAttackingHouse, sourcePos, targetPos, bulletVelocity);
            // Logger.Log("{0}发射武器{1}，创建抛射体，目标类型{2}", pAttacker, pWeapon.Ref.Base.ID, pTarget.Ref.WhatAmI());
            return pBullet;
        }

        public static Pointer<BulletClass> FireBullet(Pointer<WeaponTypeClass> pWeapon, double fireMult,
            Pointer<AbstractClass> pTarget, Pointer<TechnoClass> pAttacker, Pointer<HouseClass> pAttackingHouse,
            CoordStruct sourcePos, CoordStruct targetPos,
            BulletVelocity bulletVelocity = default)
        {
            Pointer<BulletTypeClass> pBulletType = pWeapon.Ref.Projectile;
            int damage = (int)(pWeapon.Ref.Damage * fireMult);
            Pointer<WarheadTypeClass> pWH = pWeapon.Ref.Warhead;
            int speed = pWeapon.Ref.GetSpeed(sourcePos, targetPos);
            bool bright = pWeapon.Ref.Bright; // 原游戏中弹头上的bright是无效的

            Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(pTarget, pAttacker, damage, pWH, speed, bright);
            // Logger.Log($"{Game.CurrentFrame} {pAttacker}发射武器，创建抛射体，目标类型{(!pTarget.IsNull ? pTarget.Ref.WhatAmI() : "UNKNOW")} {pTarget}");
            pBullet.Ref.WeaponType = pWeapon;
            // 设置所属
            pBullet.SetSourceHouse(pAttackingHouse);
            if (default == bulletVelocity)
            {
                bulletVelocity = GetBulletVelocity(sourcePos, targetPos);
            }
            // pBullet.Ref.SetTarget(pTarget);
            pBullet.Ref.MoveTo(sourcePos, bulletVelocity);
            if (default != targetPos)
            {
                pBullet.Ref.TargetCoords = targetPos;
            }
            // if (pBulletType.Ref.Inviso && !pBulletType.Ref.Airburst)
            // {
            //     pBullet.Ref.Detonate(targetPos);
            //     pBullet.Ref.Base.UnInit();
            // }
            return pBullet;
        }

        private static unsafe void DrawBulletEffect(Pointer<WeaponTypeClass> pWeapon, CoordStruct sourcePos, CoordStruct targetPos,
            Pointer<TechnoClass> pAttacker, Pointer<AbstractClass> pTarget, Pointer<HouseClass> pAttackingHouse)
        {
            // IsLaser
            if (pWeapon.Ref.IsLaser)
            {
                LaserType laserType = new LaserType(false);
                ColorStruct houseColor = default;
                if (pWeapon.Ref.IsHouseColor)
                {
                    if (!pAttacker.IsNull)
                    {
                        houseColor = pAttacker.Ref.Owner.Ref.LaserColor;
                    }
                    else if (!pAttackingHouse.IsNull)
                    {
                        houseColor = pAttackingHouse.Ref.LaserColor;
                    }
                }
                laserType.InnerColor = pWeapon.Ref.LaserInnerColor;
                laserType.OuterColor = pWeapon.Ref.LaserOuterColor;
                laserType.OuterSpread = pWeapon.Ref.LaserOuterSpread;
                laserType.IsHouseColor = pWeapon.Ref.IsHouseColor;
                laserType.Duration = pWeapon.Ref.LaserDuration;
                // get thickness and fade
                WeaponTypeData ext = Ini.GetConfig<WeaponTypeData>(Ini.RulesDependency, pWeapon.Ref.Base.ID).Data;
                if (ext.LaserThickness > 0)
                {
                    laserType.Thickness = ext.LaserThickness;
                }
                laserType.Fade = ext.LaserFade;
                laserType.IsSupported = ext.IsSupported;
                BulletEffectHelper.DrawLine(sourcePos, targetPos, laserType, houseColor);
            }
            // IsRadBeam
            if (pWeapon.Ref.IsRadBeam)
            {
                RadBeamType radBeamType = RadBeamType.RadBeam;
                if (!pWeapon.Ref.Warhead.IsNull && pWeapon.Ref.Warhead.Ref.Temporal)
                {
                    radBeamType = RadBeamType.Temporal;
                }
                BeamType beamType = new BeamType(radBeamType);
                BulletEffectHelper.DrawBeam(sourcePos, targetPos, beamType);
                // RadBeamType beamType = RadBeamType.RadBeam;
                // ColorStruct beamColor = RulesClass.Global().RadColor;
                // if (!pWeapon.Ref.Warhead.IsNull && pWeapon.Ref.Warhead.Ref.Temporal)
                // {
                //     beamType = RadBeamType.Temporal;
                //     beamColor = RulesClass.Global().ChronoBeamColor;
                // }
                // Pointer<RadBeam> pRadBeam = RadBeam.Allocate(beamType);
                // if (!pRadBeam.IsNull)
                // {
                //     pRadBeam.Ref.SetCoordsSource(sourcePos);
                //     pRadBeam.Ref.SetCoordsTarget(targetPos);
                //     pRadBeam.Ref.Color = beamColor;
                //     pRadBeam.Ref.Period = 15;
                //     pRadBeam.Ref.Amplitude = 40.0;
                // }
            }
            //IsElectricBolt
            if (pWeapon.Ref.IsElectricBolt)
            {
                if (!pAttacker.IsNull && !pTarget.IsNull)
                {
                    BulletEffectHelper.DrawBolt(pAttacker, pTarget, pWeapon, sourcePos);
                }
                else
                {
                    BulletEffectHelper.DrawBolt(sourcePos, targetPos, pWeapon.Ref.IsAlternateColor);
                }
            }
        }

        private static unsafe void AttachedParticleSystem(Pointer<WeaponTypeClass> pWeapon, CoordStruct sourcePos, Pointer<AbstractClass> pTarget, Pointer<TechnoClass> pAttacker, CoordStruct targetPos)
        {
            //ParticleSystem
            Pointer<ParticleSystemTypeClass> psType = pWeapon.Ref.AttachedParticleSystem;
            if (!psType.IsNull)
            {
                BulletEffectHelper.DrawParticele(psType, sourcePos, pTarget, pAttacker, targetPos);
            }
        }

        private static unsafe void PlayReportSound(Pointer<WeaponTypeClass> pWeapon, CoordStruct sourcePos)
        {
            if (pWeapon.Ref.Report.Count > 0)
            {
                int index = MathEx.Random.Next(0, pWeapon.Ref.Report.Count - 1);
                int soundIndex = pWeapon.Ref.Report.Get(index);
                if (soundIndex != -1)
                {
                    VocClass.PlayAt(soundIndex, sourcePos, IntPtr.Zero);
                }
            }
        }

        private static unsafe void DrawWeaponAnim(Pointer<ObjectClass> pShooter, Pointer<WeaponTypeClass> pWeapon, CoordStruct sourcePos, CoordStruct targetPos)
        {
            // Anim
            if (pWeapon.Ref.Anim.Count > 0)
            {
                int facing = pWeapon.Ref.Anim.Count;
                int index = 0;
                if (facing % 8 == 0)
                {
                    CoordStruct tempSourcePos = sourcePos;
                    tempSourcePos.Z = 0;
                    CoordStruct tempTargetPos = targetPos;
                    tempTargetPos.Z = 0;
                    DirStruct dir = FLHHelper.Point2Dir(tempSourcePos, tempTargetPos);
                    index = dir.Dir2FrameIndex(facing);
                }
                Pointer<AnimTypeClass> pAnimType = pWeapon.Ref.Anim.Get(index);
                // Logger.Log($"{Game.CurrentFrame} 获取第{index}个动画[{(pAnimType.IsNull ? "不存在" : pAnimType.Convert<AbstractTypeClass>().Ref.ID)}]");
                if (!pAnimType.IsNull)
                {
                    Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, sourcePos);
                    pAnim.Ref.SetOwnerObject(pShooter);
                }
            }
        }


    }

}
using System;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{
    [Serializable]
    public enum CumulativeMode
    {
        NO = 0, YES = 1, ATTACKER = 2
    }
    public class CumulativeModeParser : KEnumParser<CumulativeMode>
    {
        public override bool ParseInitials(string t, ref CumulativeMode buffer)
        {
            switch (t)
            {
                case "1":
                case "T": // true
                case "Y": // yes
                    buffer = CumulativeMode.YES;
                    return true;
                case "0":
                case "F": // false
                case "N": // no
                    buffer = CumulativeMode.NO;
                    return true;
                case "A": // attacker
                    buffer = CumulativeMode.ATTACKER;
                    return true;
            }
            return false;
        }
    }

    [Serializable]
    public enum AttachOwnerType
    {
        TECHONO = 0, BULLET = 1
    }

    [Serializable]
    public partial class AttachEffectData : INIConfig
    {
        static AttachEffectData()
        {
            new CumulativeModeParser().Register();
        }

        public string Name;

        public bool Enable;

        public int Duration; // 持续时间
        public bool HoldDuration; // 无限时间

        public int Delay; // 不可获得同名的延迟
        public Point2D RandomDelay; // 随机最小值

        public int InitialDelay; // 生效前的初始延迟
        public Point2D InitialRandomDelay; // 随机初始延迟

        public bool DiscardOnEntry; // 离开地图则失效
        public bool PenetratesIronCurtain; // 弹头附加，影响铁幕
        public bool FromTransporter; // 弹头附加，乘客附加时，视为载具
        public bool OwnerTarget; // 弹头附加，属于被赋予对象

        public CumulativeMode Cumulative; // 可叠加
        public bool ResetDurationOnReapply; // 不可叠加时，重复获得时是否重置计时器
        public int Group; // 分组，同一个分组的效果互相影响，削减或增加持续时间
        public bool OverrideSameGroup; // 是否覆盖同一个分组
        public string Next; // 结束后播放下一个AE

        public bool AttachOnceInTechnoType; // 写在TechnoType上只在创建时赋予一次
        public bool AttachWithDamage; // 弹头附加，随着伤害附加，而不是按弹头爆炸位置附加，如在使用AmbientDamage时

        // 赋予对象过滤
        public string[] AffectTypes; // 可影响的单位
        public string[] NotAffectTypes; // 不可影响的单位

        public bool AffectTechno; // 可赋予单位
        public bool AffectBuilding; // 可赋予建筑类型
        public bool AffectInfantry; // 可赋予步兵类型
        public bool AffectUnit; // 可赋予载具类型
        public bool AffectAircraft; // 可赋予飞机类型

        public bool AffectBullet; // 可赋予抛射体
        public bool AffectMissile; // 可赋予 ROT > 1
        public bool AffectTorpedo; // 可赋予 Level = yes
        public bool AffectCannon; // 可赋予 Arcing = yes
        public bool AffectBomb; // 可赋予 Vertical = yes

        public AttachEffectData()
        {
            this.Name = null;

            this.Enable = false;

            this.Duration = -1;
            this.HoldDuration = true;

            this.Delay = 0;
            this.RandomDelay = default;
            this.InitialDelay = -1;
            this.InitialRandomDelay = default;

            this.DiscardOnEntry = false;
            this.PenetratesIronCurtain = false;
            this.FromTransporter = true;
            this.OwnerTarget = false;

            this.Cumulative = CumulativeMode.NO;
            this.ResetDurationOnReapply = false;
            this.Group = -1;
            this.OverrideSameGroup = false;
            this.Next = null;

            this.AttachOnceInTechnoType = false;
            this.AttachWithDamage = false;

            // 赋予对象过滤
            this.AffectTypes = null;
            this.NotAffectTypes = null;

            this.AffectTechno = true;
            this.AffectBuilding = true;
            this.AffectInfantry = true;
            this.AffectUnit = true;
            this.AffectAircraft = true;

            this.AffectBullet = false;
            this.AffectMissile = true;
            this.AffectTorpedo = true;
            this.AffectCannon = true;
            this.AffectBomb = true;
        }

        public int GetDuration()
        {
            return this.HoldDuration ? -1 : this.Duration;
        }

        public override void Read(IConfigReader reader)
        {
            this.Name = reader.Section;

            //TODO 增加新的AE类型

            ReadAnimationData(reader);
            ReadAttackBeaconData(reader);
            // ReadAutoWeaponData(reader);
            ReadBlackHoleData(reader);
            ReadCrateBuffData(reader);
            // ReadDamageReactionData(reader);
            // ReadDamageSelfData(reader);
            ReadDestroySelfData(reader);
            ReadExtraFireData(reader);
            ReadFireSuperData(reader);
            ReadGiftBoxData(reader);
            ReadPaintballData(reader);
            ReadStandData(reader);
            // ReadTransformData(reader);
            ReadDisableWeaponData(reader);
            ReadDeselectData(reader);
            ReadOverrideWeaponData(reader);

            if (Enable)
            {
                this.Duration = reader.Get("Duration", this.Duration);
                this.HoldDuration = Duration <= 0;
                this.HoldDuration = reader.Get("HoldDuration", this.HoldDuration);

                this.Delay = reader.Get("Delay", this.Delay);
                this.RandomDelay = reader.Get("RandomDelay", this.RandomDelay);
                this.InitialDelay = reader.Get("InitialDelay", this.InitialDelay);
                this.InitialRandomDelay = reader.Get("InitialRandomDelay", this.InitialRandomDelay);

                this.DiscardOnEntry = reader.Get("DiscardOnEntry", this.DiscardOnEntry);
                this.PenetratesIronCurtain = reader.Get("PenetratesIronCurtain", this.PenetratesIronCurtain);
                this.FromTransporter = reader.Get("FromTransporter", this.FromTransporter);
                this.OwnerTarget = reader.Get("OwnerTarget", this.OwnerTarget);

                this.Cumulative = reader.Get("Cumulative", this.Cumulative);
                this.ResetDurationOnReapply = reader.Get("ResetDurationOnReapply", this.ResetDurationOnReapply);
                this.Group = reader.Get("Group", this.Group);
                this.OverrideSameGroup = reader.Get("OverrideSameGroup", this.OverrideSameGroup);
                this.Next = reader.Get("Next", this.Next);

                this.AttachOnceInTechnoType = reader.Get("AttachOnceInTechnoType", this.AttachOnceInTechnoType);
                this.AttachWithDamage = reader.Get("AttachWithDamage", this.AttachWithDamage);

                // 赋予对象过滤
                this.AffectTypes = reader.GetList<string>("AffectTypes", this.AffectTypes);
                this.NotAffectTypes = reader.GetList<string>("NotAffectTypes", this.NotAffectTypes);

                this.AffectTechno = reader.Get("AffectTechno", this.AffectTechno);
                this.AffectBuilding = reader.Get("AffectBuilding", this.AffectBuilding);
                this.AffectInfantry = reader.Get("AffectInfantry", this.AffectInfantry);
                this.AffectUnit = reader.Get("AffectUnit", this.AffectUnit);
                this.AffectAircraft = reader.Get("AffectAircraft", this.AffectAircraft);
                if (!AffectBuilding && !AffectInfantry && !AffectUnit && !AffectAircraft)
                {
                    this.AffectTechno = false;
                }

                this.AffectBullet = reader.Get("AffectBullet", this.AffectBullet);
                this.AffectMissile = reader.Get("AffectMissile", this.AffectMissile);
                this.AffectTorpedo = reader.Get("AffectTorpedo", this.AffectTorpedo);
                this.AffectCannon = reader.Get("AffectCannon", this.AffectCannon);
                this.AffectBomb = reader.Get("AffectBomb", this.AffectBomb);
                if (!AffectMissile && !AffectCannon && !AffectBomb)
                {
                    this.AffectBullet = false;
                }
            }

        }

        public bool CanAffectType(Pointer<ObjectClass> pOwner)
        {
            if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                return CanAffectType(pTechno);
            }
            else if (pOwner.CastToBullet(out Pointer<BulletClass> pBullet))
            {
                return CanAffectType(pBullet);
            }
            return false;
        }


        private bool CanAffectType(string ID)
        {
            if (null != NotAffectTypes && NotAffectTypes.Length > 0 && NotAffectTypes.Contains(ID))
            {
                return false;
            }
            return null == AffectTypes || AffectTypes.Length <= 0 || AffectTypes.Contains(ID);
        }

        public bool CanAffectType(Pointer<BulletClass> pBullet)
        {
            if (CanAffectType(pBullet.Ref.Type.Ref.Base.Base.ID))
            {
                if (pBullet.AmIArcing())
                {
                    return AffectCannon;
                }
                else
                {
                    if (pBullet.Ref.Type.Ref.Level)
                    {
                        return AffectTorpedo;
                    }
                    return AffectMissile;
                }
            }
            return false;
        }

        public bool CanAffectType(Pointer<TechnoClass> pTechno)
        {
            if (CanAffectType(pTechno.Ref.Type.Ref.Base.Base.ID))
            {
                switch (pTechno.Ref.Base.Base.WhatAmI())
                {
                    case AbstractType.Building:
                        return AffectBuilding;
                    case AbstractType.Infantry:
                        return AffectInfantry;
                    case AbstractType.Unit:
                        return AffectUnit;
                    case AbstractType.Aircraft:
                        return AffectAircraft;
                }
            }
            return false;
        }

    }

    public static class AttachEffectDataHelper
    {
        public static AttachEffect CreateAE(this IConfigWrapper<AttachEffectData> effectData)
        {
            if (null != effectData && effectData.Data.Enable)
            {
                return new AttachEffect(effectData);
            }
            return null;
        }
    }
}

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
        NO = 0, YES = 1, ATTACKER = 2, HOUSE = 3
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
                case "H": // house
                    buffer = CumulativeMode.HOUSE;
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
    public partial class AttachEffectData : EffectData
    {
        static AttachEffectData()
        {
            new CumulativeModeParser().Register();
        }

        public string Name;

        public int Duration; // 持续时间
        public bool HoldDuration; // 无限时间

        public int Delay; // 不可获得同名的延迟
        public Point2D RandomDelay; // 随机最小值

        public int InitialDelay; // 生效前的初始延迟
        public Point2D InitialRandomDelay; // 随机初始延迟

        public bool DiscardOnEntry; // 离开地图则失效
        public bool DiscardOnTransform; // 发生类型改变时失效
        public bool PenetratesIronCurtain; // 弹头附加，影响铁幕
        public bool FromTransporter; // 弹头附加，乘客附加时，视为载具
        public bool ReceiverOwn; // 弹头附加，属于被赋予对象

        public CumulativeMode Cumulative; // 可叠加
        public int MaxStack; // 叠加上限
        public bool ResetDurationOnReapply; // 不可叠加时，重复获得时是否重置计时器
        public int Group; // 分组，同一个分组的效果互相影响，削减或增加持续时间
        public bool OverrideSameGroup; // 是否覆盖同一个分组
        public string Next; // 结束后播放下一个AE

        public string[] AttachWithOutTypes; // 有这些AE存在则不可赋予
        public bool AttachOnceInTechnoType; // 写在TechnoType上只在创建时赋予一次
        public bool Inheritable; // 是否可以被礼盒礼物继承

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
            this.DiscardOnTransform = true;
            this.PenetratesIronCurtain = false;
            this.FromTransporter = true;
            this.ReceiverOwn = false;

            this.Cumulative = CumulativeMode.NO;
            this.MaxStack = -1;
            this.ResetDurationOnReapply = false;
            this.Group = -1;
            this.OverrideSameGroup = false;
            this.Next = null;

            this.AttachWithOutTypes = null;
            this.AttachOnceInTechnoType = false;
            this.Inheritable = true;

            // 赋予对象过滤
            this.AffectBullet = false;
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
            ReadAutoWeaponData(reader);
            ReadBlackHoleData(reader);
            ReadBounceData(reader);
            ReadBroadcastData(reader);
            ReadCrateBuffData(reader);
            ReadDamageReactionData(reader);
            ReadDamageSelfData(reader);
            ReadDestroyAnimData(reader);
            ReadDestroySelfData(reader);
            ReadDeselectData(reader);
            ReadDisableWeaponData(reader);
            ReadECMData(reader);
            ReadExtraFireData(reader);
            ReadFireSuperData(reader);
            ReadFreezeData(reader);
            ReadGiftBoxData(reader);
            ReadImmuneData(reader);
            ReadInfoData(reader);
            ReadMarkData(reader);
            ReadPaintballData(reader);
            ReadPumpData(reader);
            ReadRevengeData(reader);
            ReadScatterData(reader);
            ReadStackData(reader);
            ReadStandData(reader);
            ReadTeleportData(reader);
            ReadTransformData(reader);
            ReadOverrideWeaponData(reader);

            if (Enable)
            {
                int druation = reader.Get("Duration", 0);
                if (druation != 0)
                {
                    this.Duration = druation;
                }
                this.HoldDuration = Duration <= 0;
                this.HoldDuration = reader.Get("HoldDuration", this.HoldDuration);

                this.Delay = reader.Get("Delay", this.Delay);
                this.RandomDelay = reader.Get("RandomDelay", this.RandomDelay);
                this.InitialDelay = reader.Get("InitialDelay", this.InitialDelay);
                this.InitialRandomDelay = reader.Get("InitialRandomDelay", this.InitialRandomDelay);

                this.DiscardOnEntry = reader.Get("DiscardOnEntry", this.DiscardOnEntry);
                this.DiscardOnTransform = reader.Get("DiscardOnTransform", this.DiscardOnTransform);
                this.PenetratesIronCurtain = reader.Get("PenetratesIronCurtain", this.PenetratesIronCurtain);
                this.FromTransporter = reader.Get("FromTransporter", this.FromTransporter);
                this.ReceiverOwn = reader.Get("ReceiverOwn", this.ReceiverOwn);

                this.Cumulative = reader.Get("Cumulative", this.Cumulative);
                this.MaxStack = reader.Get("MaxStack", this.MaxStack);
                this.ResetDurationOnReapply = reader.Get("ResetDurationOnReapply", this.ResetDurationOnReapply);
                this.Group = reader.Get("Group", this.Group);
                this.OverrideSameGroup = reader.Get("OverrideSameGroup", this.OverrideSameGroup);
                this.Next = reader.Get("Next", this.Next);

                this.AttachWithOutTypes = reader.GetList("AttachWithOutTypes", this.AttachWithOutTypes);
                this.AttachOnceInTechnoType = reader.Get("AttachOnceInTechnoType", this.AttachOnceInTechnoType);
                this.Inheritable = reader.Get("Inheritable", this.Inheritable);

                base.Read(reader, null);
            }
        }

        public bool Contradiction(AttachEffectScript aeManager)
        {
            return null != AttachWithOutTypes && AttachWithOutTypes.Any()
                && null != aeManager.AEStacks && aeManager.AEStacks.Any()
                && AttachWithOutTypes.Intersect(aeManager.AEStacks.Keys).Count() > 0;
        }
    }

    public static class AttachEffectDataHelper
    {
        public static AttachEffect CreateAE(this AttachEffectData effectData)
        {
            if (null != effectData && effectData.Enable)
            {
                return new AttachEffect(effectData);
            }
            return null;
        }
    }
}

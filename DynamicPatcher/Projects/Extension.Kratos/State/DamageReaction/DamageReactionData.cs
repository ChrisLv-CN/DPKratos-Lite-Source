using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public partial class AttachEffectData
    {
        public DamageReactionData DamageReactionData;

        private void ReadDamageReactionData(IConfigReader reader)
        {
            DamageReactionData data = new DamageReactionData();
            data.Read(reader);
            if (data.Enable)
            {
                this.DamageReactionData = data;
                this.Enable = true;
            }
        }
    }


    [Serializable]
    public enum DamageReactionMode
    {
        NONE = 0, EVASION = 1, REDUCE = 2, FORTITUDE = 3, PREVENT = 4
    }
    public class DamageReactionModeParser : KEnumParser<DamageReactionMode>
    {
        public override bool ParseInitials(string t, ref DamageReactionMode buffer)
        {
            switch (t)
            {
                case "R":
                    buffer = DamageReactionMode.REDUCE;
                    // this.DefaultText = LongText.HIT; // 击中
                    return true;
                case "F":
                    buffer = DamageReactionMode.FORTITUDE;
                    // this.DefaultText = LongText.GLANCING; // 偏斜
                    return true;
                case "P":
                    buffer = DamageReactionMode.PREVENT;
                    // this.DefaultText = LongText.BLOCK; // 格挡
                    return true;
                case "E":
                    buffer = DamageReactionMode.EVASION;
                    // this.DefaultText = LongText.MISS; // 未命中
                    return true;
            }
            return false;
        }
    }

    [Serializable]
    public enum DamageTextStyle
    {
        AUTO = 0, DAMAGE = 1, REPAIR = 2
    }
    public class DamageTextStyleParser : KEnumParser<DamageTextStyle>
    {
        public override bool ParseInitials(string t, ref DamageTextStyle buffer)
        {
            switch (t)
            {
                case "D":
                    buffer = DamageTextStyle.DAMAGE;
                    return true;
                case "R":
                    buffer = DamageTextStyle.REPAIR;
                    return true;
                default:
                    buffer = DamageTextStyle.AUTO;
                    return true;
            }
        }
    }

    [Serializable]
    public class DamageReactionEntity
    {
        static DamageReactionEntity()
        {
            new DamageReactionModeParser().Register();
            new DamageTextStyleParser().Register();
        }

        public DamageReactionMode Mode;
        public double Chance;
        public int Delay;
        public bool ActiveOnce; // 触发效果之后结束
        public int TriggeredTimes; // 触发次数够就结束
        public bool ResetTimes;

        public string[] TriggeredAttachEffects; // 触发后附加AE
        public double[] TriggeredAttachEffectChances; // 附加效果的成功率
        public bool TriggeredAttachEffectsFromAttacker; // 触发后附加的AE来源是攻击者

        public string[] OnlyReactionWarheads; // 只响应某些弹头

        public string Anim;
        public CoordStruct AnimFLH;
        public int AnimDelay;

        public double ReducePercent; // 伤害调整比例
        public int MaxDamage; // 伤害上限

        public bool ActionText; // 显示响应DamageText
        public DamageTextStyle TextStyle;
        public LongText DefaultText; // 默认显示的内容
        public string CustomText;
        public string CustomSHP;
        public int CustomSHPIndex;


        public DamageReactionEntity()
        {
            this.Mode = DamageReactionMode.EVASION;
            this.Chance = 0;
            this.Delay = 0;
            this.ActiveOnce = false;
            this.TriggeredTimes = -1;
            this.ResetTimes = false;

            this.TriggeredAttachEffects = null;
            this.TriggeredAttachEffectChances = null;
            this.TriggeredAttachEffectsFromAttacker = false;

            this.OnlyReactionWarheads = null;

            this.Anim = null;
            this.AnimFLH = default;
            this.AnimDelay = 0;

            this.ReducePercent = 1;
            this.MaxDamage = 10;

            this.ActionText = true;
            this.TextStyle = DamageTextStyle.AUTO;
            this.DefaultText = LongText.MISS;
            this.CustomText = null;
            this.CustomSHP = null;
            this.CustomSHPIndex = 0;
        }

        public DamageReactionEntity Clone()
        {
            DamageReactionEntity data = new DamageReactionEntity();
            data.Mode = this.Mode;
            data.Chance = this.Chance;
            data.Delay = this.Delay;
            data.ActiveOnce = this.ActiveOnce;
            data.TriggeredTimes = this.TriggeredTimes;
            data.ResetTimes = this.ResetTimes;

            data.TriggeredAttachEffects = null != this.TriggeredAttachEffects ? (string[])this.TriggeredAttachEffects.Clone() : null;
            data.TriggeredAttachEffectChances = null != this.TriggeredAttachEffectChances ? (double[])this.TriggeredAttachEffectChances.Clone() : null;
            data.TriggeredAttachEffectsFromAttacker = this.TriggeredAttachEffectsFromAttacker;

            data.OnlyReactionWarheads = null != this.OnlyReactionWarheads ? (string[])this.OnlyReactionWarheads.Clone() : null;

            data.Anim = this.Anim;
            data.AnimFLH = this.AnimFLH;
            data.AnimDelay = this.AnimDelay;

            data.ReducePercent = this.ReducePercent;
            data.MaxDamage = this.MaxDamage;

            data.ActionText = this.ActionText;
            data.TextStyle = this.TextStyle;
            data.DefaultText = this.DefaultText;
            data.CustomText = this.CustomText;
            data.CustomSHP = this.CustomSHP;
            data.CustomSHPIndex = this.CustomSHPIndex;

            return data;
        }

        public void Read(ISectionReader reader, string title)
        {
            this.Mode = reader.Get(title + "Mode", this.Mode);
            switch (Mode)
            {
                case DamageReactionMode.REDUCE:
                    this.DefaultText = LongText.HIT; // 击中
                    break;
                case DamageReactionMode.FORTITUDE:
                    this.DefaultText = LongText.GLANCING; // 偏斜
                    break;
                case DamageReactionMode.PREVENT:
                    this.DefaultText = LongText.BLOCK; // 格挡
                    break;
                case DamageReactionMode.EVASION:
                    this.DefaultText = LongText.MISS; // 未命中
                    break;
                default:
                    return;
            }
            this.Chance = reader.GetChance(title + "Chance", this.Chance);
            this.Delay = reader.Get(title + "Delay", this.Delay);
            this.ActiveOnce = reader.Get(title + "ActiveOnce", this.ActiveOnce);
            this.TriggeredTimes = reader.Get(title + "TriggeredTimes", this.TriggeredTimes);
            this.ResetTimes = reader.Get(title + "ResetTimes", this.ResetTimes);

            this.TriggeredAttachEffects = reader.GetList(title + "TriggeredAttachEffects", this.TriggeredAttachEffects);
            this.TriggeredAttachEffectChances = reader.GetChanceList(title + "TriggeredAttachEffectChances", this.TriggeredAttachEffectChances);
            this.TriggeredAttachEffectsFromAttacker = reader.Get(title + "TriggeredAttachEffectsFromAttacker", this.TriggeredAttachEffectsFromAttacker);

            this.OnlyReactionWarheads = reader.GetList(title + "OnlyReactionWarheads", this.OnlyReactionWarheads);
            if (null != OnlyReactionWarheads && OnlyReactionWarheads.Count() == 1 && OnlyReactionWarheads[0].IsNullOrEmptyOrNone())
            {
                OnlyReactionWarheads = null;
            }

            this.Anim = reader.Get(title + "Anim", this.Anim);
            this.AnimFLH = reader.Get(title + "AnimFLH", this.AnimFLH);
            this.AnimDelay = reader.Get(title + "AnimDelay", this.AnimDelay);

            this.ReducePercent = reader.GetPercent(title + "ReducePercent", this.ReducePercent);
            if (Mode == DamageReactionMode.REDUCE)
            {
                double mult = Math.Abs(ReducePercent);
                if (mult > 1.0)
                {
                    this.DefaultText = LongText.CRIT; // 暴击
                }
                else if (mult < 1.0)
                {
                    this.DefaultText = LongText.GLANCING; // 偏斜
                }
            }

            this.MaxDamage = reader.Get(title + "FortitudeMax", this.MaxDamage);

            this.ActionText = reader.Get(title + "ActionText", this.ActionText);
            this.TextStyle = reader.Get(title + "ActionTextStyle", this.TextStyle);

            this.CustomText = reader.Get(title + "ActionTextCustom", this.CustomText);
            this.CustomSHP = reader.Get(title + "ActionTextSHP", this.CustomSHP);
            this.CustomSHPIndex = reader.Get(title + "ActionTextSHPIndex", this.CustomSHPIndex);
        }

        public bool IsOnMark(Pointer<WarheadTypeClass> pWH)
        {
            return IsOnMark(pWH.Ref.Base.ID);
        }

        public bool IsOnMark(string warheadId)
        {
            return null == OnlyReactionWarheads || OnlyReactionWarheads.Contains(warheadId);
        }

    }

    [Serializable]
    public class DamageReactionData : EffectData, IStateData
    {
        public const string TITLE = "DamageReaction.";

        public DamageReactionEntity Data;
        public DamageReactionEntity EliteData;

        public DamageReactionData()
        {
            this.Data = null;
            this.EliteData = null;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            DamageReactionEntity data = new DamageReactionEntity();
            data.Read(reader, TITLE);
            if (data.Chance > 0)
            {
                this.Data = data;
            }

            DamageReactionEntity elite = null != this.Data ? Data.Clone() : new DamageReactionEntity();
            elite.Read(reader, TITLE + "Elite");
            if (elite.Chance > 0)
            {
                this.EliteData = elite;
            }

            this.Enable = null != this.Data || null != this.EliteData;

        }
    }

}

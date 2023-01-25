using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{
    public interface IEffectData
    {

    }

    [Serializable]
    public enum AffectWho
    {
        MASTER = 0, STAND = 1, ALL = 2,
    }

    public class AffectWhoParser : KEnumParser<AffectWho>
    {
        public override bool ParseInitials(string t, ref AffectWho buffer)
        {
            switch (t)
            {
                case "M":
                    buffer = AffectWho.MASTER;
                    break;
                case "S":
                    buffer = AffectWho.STAND;
                    break;
                default:
                    buffer = AffectWho.ALL;
                    break;
            }
            return true;
        }
    }

    [Serializable]
    public class EffectData : FilterData, IEffectData
    {
        public bool Enable;

        public int TriggeredTimes; // 触发次数

        public bool Powered; // 建筑需要使用电力

        public AffectWho AffectWho;

        public bool DeactiveWhenCivilian;

        static EffectData()
        {
            new AffectWhoParser().Register();
        }

        public EffectData()
        {
            this.Enable = false;
            this.TriggeredTimes = -1;
            this.Powered = false;
            this.AffectWho = AffectWho.MASTER;
            this.DeactiveWhenCivilian = false;
        }

        public override void Read(IConfigReader reader) { }

        public override void Read(ISectionReader reader, string title)
        {
            base.Read(reader, title);

            this.Enable = reader.Get(title + "Enable", this.Enable);
            this.TriggeredTimes = reader.Get(title + "TriggeredTimes", this.TriggeredTimes);
            this.Powered = reader.Get(title + "Powered", this.Powered);
            this.AffectWho = reader.Get(title + "AffectWho", this.AffectWho);
            this.DeactiveWhenCivilian = reader.Get(title + "DeactiveWhenCivilian", this.DeactiveWhenCivilian);
        }

    }

    public static class EffectDataHelper
    {
        public static E CreateEffect<E>(this EffectData effectData) where E : IEffect, new()
        {
            if (null != effectData && effectData.Enable)
            {
                E effect = new E();
                effect.SetData(effectData);
                return effect;
            }
            return default(E);
        }
    }

}

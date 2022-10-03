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
        ALL = 0, MASTER = 1, STAND = 2
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
    public class EffectData : INIConfig, IEffectData
    {
        public bool Enable;

        public AffectWho AffectWho;

        public bool DeactiveWhenCivilian;

        static EffectData()
        {
            new AffectWhoParser().Register();
        }

        public EffectData()
        {
            this.Enable = false;
            this.AffectWho = AffectWho.ALL;
            this.DeactiveWhenCivilian = false;
        }

        public EffectData CopyTo(EffectData data)
        {
            data.Enable = this.Enable;
            data.AffectWho = this.AffectWho;
            data.DeactiveWhenCivilian = this.DeactiveWhenCivilian;
            return data;
        }

        public override void Read(IConfigReader reader) { }

        public void Read(IConfigReader reader, string title)
        {
            this.Enable = reader.Get(title + "Enable", this.Enable);
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

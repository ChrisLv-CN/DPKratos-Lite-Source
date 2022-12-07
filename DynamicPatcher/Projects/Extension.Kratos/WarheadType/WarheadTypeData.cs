using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class WarheadTypeData : INIConfig
    {
        // YR
        // Ares
        public bool AffectsOwner;
        public bool AffectsAllies;
        public bool AffectsEnemies;
        public bool EffectsRequireDamage;
        public bool EffectsRequireVerses;
        public bool AllowZeroDamage;
        public string PreImpactAnim;

        // Kratos
        public bool AffectInAir;

        public bool ClearTarget;

        
        // 弹头传送标记
        public bool Teleporter;
        // 弹头捕获标记
        public bool Capturer;
        // 不触发伤害响应
        public bool IgnoreDamageReaction;
        // 替身不分摊伤害
        public bool IgnoreStandShareDamage;

        public WarheadTypeData()
        {
            // Ares
            this.AffectsOwner = true;
            this.AffectsAllies = true;
            this.AffectsEnemies = true;
            this.EffectsRequireDamage = false;
            this.EffectsRequireVerses = true;
            this.AllowZeroDamage = false;
            this.PreImpactAnim = null;

            // Kratos
            this.AffectInAir = true;

            this.ClearTarget = false;

            this.Teleporter = false;
            this.Capturer = false;
            this.IgnoreDamageReaction = false;
            this.IgnoreStandShareDamage = false;
        }

        public override void Read(IConfigReader reader)
        {
            // Ares
            this.AffectsAllies = reader.Get("AffectsAllies", this.AffectsAllies);
            this.AffectsOwner = reader.Get("AffectsOwner", this.AffectsAllies);

            this.AffectsEnemies = reader.Get("AffectsEnemies", this.AffectsEnemies);
            this.EffectsRequireDamage = reader.Get("EffectsRequireDamage", this.EffectsRequireDamage);
            this.EffectsRequireVerses = reader.Get("EffectsRequireVerses", this.EffectsRequireVerses);
            this.AllowZeroDamage = reader.Get("AllowZeroDamage", this.AllowZeroDamage);
            this.PreImpactAnim = reader.Get("PreImpactAnim", this.PreImpactAnim);

            // Kratos
            this.AffectInAir = reader.Get("AffectInAir", this.AffectInAir);

            this.ClearTarget = reader.Get("ClearTarget", this.ClearTarget);

            this.Teleporter = reader.Get("Teleporter", this.Teleporter);
            this.Capturer = reader.Get("Capturer", this.Capturer);
            this.IgnoreDamageReaction = reader.Get("IgnoreDamageReaction", this.IgnoreDamageReaction);
            this.IgnoreStandShareDamage = reader.Get("IgnoreStandShareDamage", this.IgnoreStandShareDamage);
        }

    }


}

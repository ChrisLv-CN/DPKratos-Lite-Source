using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public class WarheadTypeData : INIAutoConfig
    {
        // YR
        // Ares
        public bool AffectsOwner = true;
        public bool AffectsAllies = true;
        public bool AffectsEnemies = true;
        public bool EffectsRequireDamage = false;
        public bool EffectsRequireVerses = true;
        public bool AllowZeroDamage = false;
        public string PreImpactAnim = null;

        // Kratos
        public bool AffectsAir = true;
        public bool AffectsBullet = false;

    }


}

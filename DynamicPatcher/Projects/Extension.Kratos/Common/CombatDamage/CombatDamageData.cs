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
    public class CombatDamageData : INIAutoConfig
    {
        // 动画
        public bool AllowAnimDamageTakeOverByKratos = true;
        public bool AllowDamageIfDebrisHitWater = true;

        // 替身
        public bool AllowAutoPickStandAsTarget = true;
    }
}

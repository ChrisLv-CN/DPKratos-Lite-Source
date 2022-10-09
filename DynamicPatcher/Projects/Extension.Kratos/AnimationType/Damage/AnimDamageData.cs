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
    public class AnimDamageData : INIConfig
    {
        public bool AllowAnimDamageTakeOverByKratos;
        public bool AllowDamageIfDebrisHitWater;


        public int Damage; // 动画伤害
        public int InitDelay; // 动画伤害初始延迟
        public int Delay; // 动画伤害延迟

        public bool KillByCreater; // 动画制造伤害传递攻击者为动画的创建者

        public string Warhead; // 使用弹头制造伤害
        public bool PlayWarheadAnim; // 播放弹头动画

        public string Weapon; // 使用武器制造伤害
        public bool UseWeaponDamage; // 使用武器的伤害而不是动画的伤害

        public AnimDamageData()
        {
            this.AllowAnimDamageTakeOverByKratos = true;
            this.AllowDamageIfDebrisHitWater = true;

            this.Damage = 0;
            this.InitDelay = 0;
            this.Delay = 0;

            this.KillByCreater = false;

            this.Warhead = null;
            this.PlayWarheadAnim = false;

            this.Weapon = null;
            this.UseWeaponDamage = false;
        }

        public override void Read(IConfigReader reader)
        {
            ISectionReader combat = Ini.GetSection(Ini.RulesDependency, RulesClass.SectionCombatDamage);
            this.AllowAnimDamageTakeOverByKratos = combat.Get("AllowAnimDamageTakeOverByKratos", this.AllowAnimDamageTakeOverByKratos);
            this.AllowDamageIfDebrisHitWater = combat.Get("AllowDamageIfDebrisHitWater", this.AllowDamageIfDebrisHitWater);

            this.Damage = reader.Get("Damage", this.Damage);
            this.Delay =  reader.Get("Damage.Delay", this.Delay);
            // Ares 习惯，InitDelay 与 Delay相同
            this.InitDelay =  reader.Get("Damage.InitDelay", this.Delay);

            this.KillByCreater =  reader.Get("Damage.KillByCreater", this.KillByCreater);

            this.Warhead =  reader.Get("Warhead", this.Warhead);
            this.PlayWarheadAnim =  reader.Get("Warhead.PlayAnim", this.PlayWarheadAnim);

            this.Weapon =  reader.Get("Weapon", this.Weapon);
            this.UseWeaponDamage =  reader.Get("Weapon.AllowDamage", this.UseWeaponDamage);
        }

    }


}

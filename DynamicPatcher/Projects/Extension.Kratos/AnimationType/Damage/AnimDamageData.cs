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
        public int Damage;
        public int InitDelay;
        public int Delay;

        public bool KillByCreater;

        public string Warhead;
        public bool PlayWarheadAnim;

        public string Weapon;
        public bool UseWeaponDamage;

        public AnimDamageData()
        {
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
            this.Damage = reader.Get("Damage", this.Damage);
            this.InitDelay =  reader.Get("Damage.InitDelay", this.InitDelay);
            this.Delay =  reader.Get("Damage.Delay", this.Delay);

            this.KillByCreater =  reader.Get("Damage.KillByCreater", this.KillByCreater);

            this.Warhead =  reader.Get("Warhead", this.Warhead);
            this.PlayWarheadAnim =  reader.Get("Warhead.PlayAnim", this.PlayWarheadAnim);

            this.Weapon =  reader.Get("Weapon", this.Weapon);
            this.UseWeaponDamage =  reader.Get("Weapon.AllowDamage", this.UseWeaponDamage);
        }

    }


}

using System;
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
        public DamageSelfData DamageSelfData;

        private void ReadDamageSelfData(IConfigReader reader)
        {
            DamageSelfData data = new DamageSelfData(reader);
            if (data.Enable)
            {
                this.DamageSelfData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class DamageSelfData : EffectData
    {

        public const string TITLE = "DamageSelf.";

        public int Damage;
        public int ROF;
        public string Warhead;
        public bool WarheadAnim;
        public bool Decloak;
        public bool IgnoreArmor;

        public bool Peaceful;


        public DamageSelfData()
        {
            this.Damage = 0;
            this.ROF = 0;
            this.Warhead = null;
            this.IgnoreArmor = true;
            this.Decloak = false;
            this.Peaceful = false;

            this.AffectWho = AffectWho.MASTER;
        }

        public DamageSelfData(IConfigReader reader) : this()
        {
            Read(reader);
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Damage = reader.Get(TITLE + "Damage", this.Damage);
            this.Enable = Damage != 0;

            this.ROF = reader.Get(TITLE + "ROF", this.ROF);
            this.Warhead = reader.Get(TITLE + "Warhead", this.Warhead);
            this.IgnoreArmor = reader.Get(TITLE + "IgnoreArmor", this.IgnoreArmor);
            this.Decloak = reader.Get(TITLE + "Decloak", this.Decloak);
            this.Peaceful = reader.Get(TITLE + "Peaceful", this.Peaceful);
        }

    }


}

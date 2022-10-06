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
        public CrateBuffData CrateBuffData;

        private void ReadCrateBuffData(IConfigReader reader)
        {
            CrateBuffData data = new CrateBuffData();
            data.Read(reader);
            if (data.Enable)
            {
                this.CrateBuffData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class CrateBuffData : EffectData, IStateData
    {
        public const string TITLE = "Status.";


        public double FirepowerMultiplier;
        public double ArmorMultiplier;
        public double SpeedMultiplier;
        public double ROFMultiplier;
        public bool Cloakable;
        public bool ForceDecloak;


        public CrateBuffData()
        {
            this.FirepowerMultiplier = 1.0;
            this.ArmorMultiplier = 1.0;
            this.SpeedMultiplier = 1.0;
            this.ROFMultiplier = 1.0;
            this.Cloakable = false;
            this.ForceDecloak = false;

            this.AffectWho = AffectWho.ALL;
        }

        public static CrateBuffData operator +(CrateBuffData a, CrateBuffData b)
        {
            a.FirepowerMultiplier += b.FirepowerMultiplier;
            a.ArmorMultiplier += b.ArmorMultiplier;
            a.SpeedMultiplier += b.SpeedMultiplier;
            a.ROFMultiplier += b.ROFMultiplier;
            a.Cloakable = b.Cloakable;
            a.ForceDecloak = b.ForceDecloak;
            return a;
        }

        public CrateBuffData Clone()
        {
            CrateBuffData data = new CrateBuffData();
            data.FirepowerMultiplier = this.FirepowerMultiplier;
            data.ArmorMultiplier = this.ArmorMultiplier;
            data.SpeedMultiplier = this.SpeedMultiplier;
            data.ROFMultiplier = this.ROFMultiplier;
            data.Cloakable = this.Cloakable;
            data.ForceDecloak = this.ForceDecloak;
            return data;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.FirepowerMultiplier = reader.Get(TITLE + "FirepowerMultiplier", this.FirepowerMultiplier);
            this.ArmorMultiplier = reader.Get(TITLE + "ArmorMultiplier", this.ArmorMultiplier);
            this.SpeedMultiplier = reader.Get(TITLE + "SpeedMultiplier", this.SpeedMultiplier);
            this.ROFMultiplier = reader.Get(TITLE + "ROFMultiplier", this.ROFMultiplier);
            this.Cloakable = reader.Get(TITLE + "Cloakable", this.Cloakable);
            this.ForceDecloak = reader.Get(TITLE + "ForceDecloak", this.ForceDecloak);

            this.Enable = FirepowerMultiplier != 1 || ArmorMultiplier != 1 || SpeedMultiplier != 1 || ROFMultiplier != 1 || Cloakable || ForceDecloak;
        }

    }


}

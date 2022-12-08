using System;
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
        public ImmuneData ImmuneData;

        private void ReadImmuneData(IConfigReader reader)
        {
            ImmuneData data = new ImmuneData(reader);
            if (data.Enable)
            {
                this.ImmuneData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class ImmuneData : EffectData
    {

        public const string TITLE = "Immune.";

        public bool Psionics; // 免疫心灵控制
        public bool PsionicWeapons; // 免疫狂暴和心灵震爆
        public bool Radiation; // 免疫辐射
        public bool Poison; // 免疫病毒
        public bool EMP; // 免疫EMP
        public bool Parasite; // 免疫寄生
        public bool Temporal; // 免疫超时空
        public bool IsLocomotor; // 免疫磁电


        public ImmuneData()
        {
            this.Psionics = false;
            this.PsionicWeapons = false;
            this.Radiation = false;
            this.Poison = false;
            this.EMP = false;
            this.Parasite = false;
            this.Temporal = false;
            this.IsLocomotor = false;
        }

        public ImmuneData(IConfigReader reader) : this()
        {
            Read(reader);
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Psionics = reader.Get(TITLE + "Psionics", this.Psionics);
            this.PsionicWeapons = reader.Get(TITLE + "PsionicWeapons", this.PsionicWeapons);
            this.Radiation = reader.Get(TITLE + "Radiation", this.Radiation);
            this.Poison = reader.Get(TITLE + "Poison", this.Poison);
            this.EMP = reader.Get(TITLE + "EMP", this.EMP);
            this.Parasite = reader.Get(TITLE + "Parasite", this.Parasite);
            this.Temporal = reader.Get(TITLE + "Temporal", this.Temporal);
            this.IsLocomotor = reader.Get(TITLE + "IsLocomotor", this.IsLocomotor);

            this.Enable = Psionics || PsionicWeapons || Radiation || Poison || EMP || Parasite || Temporal || IsLocomotor;
        }

    }


}

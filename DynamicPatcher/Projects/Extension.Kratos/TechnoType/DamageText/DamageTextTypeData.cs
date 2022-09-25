using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public class DamageTextTypeData : INIConfig
    {
        public const string TITLE = "DamageText.";

        public bool Hidden;

        public DamageTextData Damage;
        public DamageTextData Repair;

        public DamageTextTypeData()
        {
            this.Hidden = false;
            this.Damage = new DamageTextData(true);
            this.Repair = new DamageTextData(false);
        }

        public override void Read(IConfigReader reader)
        {
            int infDeath = reader.Get("InfDeath", 0);
            ISectionReader avReader = Ini.GetSection(Ini.RulesDependency, RulesExt.SectionAudioVisual);
            Read(avReader);
            Read(avReader, TITLE + infDeath + ".");
            Read(reader);
        }

        public void Read(ISectionReader reader, string title = TITLE)
        {
            this.Hidden = reader.Get(title + "Hidden", Hidden);

            this.Damage.Read(reader, title);
            this.Damage.Read(reader, title + "Damage.");

            this.Repair.Read(reader, title);
            this.Repair.Read(reader, title + "Repair.");
        }
    }



}
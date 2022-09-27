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
    public class DamageText : PrintTextData
    {
        public bool Hidden;
        public bool Detail;
        public int Rate;
        public Point2D XOffset;
        public Point2D YOffset;
        public int RollSpeed;
        public int Duration;

        public DamageText(bool isDamage) : base()
        {
            this.Hidden = false;
            this.Detail = true;
            this.Rate = 0;
            this.XOffset = new Point2D(-15, 15);
            this.YOffset = new Point2D(-12, 12);
            this.RollSpeed = 1;
            this.Duration = 75;

            this.SHPFileName = "pipsnum.shp";
            this.ImageSize = new Point2D(5, 8);
            if (isDamage)
            {
                this.Color = new ColorStruct(252, 0, 0);
                this.ZeroFrameIndex = 30;

                this.HitIndex = 1;
            }
            else
            {
                this.Color = new ColorStruct(0, 252, 0);
                this.ZeroFrameIndex = 0;

                this.HitIndex = 0;
            }
        }

        public override void Read(ISectionReader reader, string title)
        {
            base.Read(reader, title);
            this.Hidden = reader.Get(title + "Hidden", Hidden);
            this.Detail = reader.Get(title + "Detail", Detail);
            this.Rate = reader.Get(title + "Rate", Rate);
            this.XOffset = reader.Get(title + "XOffset", XOffset);
            this.YOffset = reader.Get(title + "YOffset", YOffset);
            this.RollSpeed = reader.Get(title + "RollSpeed", RollSpeed);
            this.Duration = reader.Get(title + "Duration", Duration);
        }
    }

    public class DamageTextData : INIConfig
    {
        public const string TITLE = "DamageText.";

        public bool Hidden;

        public DamageText Damage;
        public DamageText Repair;

        public DamageTextData()
        {
            this.Hidden = false;
            this.Damage = new DamageText(true);
            this.Repair = new DamageText(false);
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

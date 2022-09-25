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
    public class HealthTextTypeData
    {

        public const string TITLE = "HealthText.";

        public bool Hidden;

        public HealthTextData Green;
        public HealthTextData Yellow;
        public HealthTextData Red;

        public HealthTextTypeData() { }

        public HealthTextTypeData(AbstractType type)
        {
            this.Hidden = false;
            this.Green = new HealthTextData(HealthState.Green);
            this.Yellow = new HealthTextData(HealthState.Yellow);
            this.Red = new HealthTextData(HealthState.Red);
            switch (type)
            {
                case AbstractType.Building:
                    Green.Style = HealthTextStyle.FULL;
                    Yellow.Style = HealthTextStyle.FULL;
                    Red.Style = HealthTextStyle.FULL;
                    break;
                case AbstractType.Infantry:
                    Green.Style = HealthTextStyle.SHORT;
                    Yellow.Style = HealthTextStyle.SHORT;
                    Red.Style = HealthTextStyle.SHORT;
                    break;
                case AbstractType.Unit:
                    Green.Style = HealthTextStyle.FULL;
                    Yellow.Style = HealthTextStyle.FULL;
                    Red.Style = HealthTextStyle.FULL;
                    break;
                case AbstractType.Aircraft:
                    Green.Style = HealthTextStyle.FULL;
                    Yellow.Style = HealthTextStyle.FULL;
                    Red.Style = HealthTextStyle.FULL;
                    break;
            }
        }

        public HealthTextTypeData Clone()
        {
            HealthTextTypeData data = new HealthTextTypeData();
            data.Hidden = this.Hidden;
            data.Green = this.Green.Clone();
            data.Yellow = this.Yellow.Clone();
            data.Red = this.Red.Clone();
            return data;
        }

        public void Read(ISectionReader reader, string title = TITLE)
        {
            this.Hidden = reader.Get(title + "Hidden", Hidden);

            this.Green.Read(reader, title);
            this.Yellow.Read(reader, title);
            this.Red.Read(reader, title);

            this.Green.Read(reader, title + "Green.");
            this.Yellow.Read(reader, title + "Yellow.");
            this.Red.Read(reader, title + "Red.");
        }

    }


}
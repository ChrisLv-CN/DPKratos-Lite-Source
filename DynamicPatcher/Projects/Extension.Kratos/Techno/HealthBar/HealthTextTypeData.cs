using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using System.Threading.Tasks;
using Extension.INI;

namespace Extension.Ext
{
    
    [Serializable]
    public class HealthTextTypeData : INIConfig
    {
        public bool Hidden;

        public HealthTextData Green;
        public HealthTextData Yellow;
        public HealthTextData Red;

        private HealthTextTypeData() { }

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

        public override void Read(IConfigReader reader)
        {
            // Logger.Log($"{Game.CurrentFrame} 读取全局设置");
            this.Hidden = reader.Get("HealthText.Hidden", Hidden);

            Read(reader, "HealthText.");
        }

        public void Read(ISectionReader reader, string title)
        {
            this.Hidden = reader.Get(title + "Hidden", Hidden);

            this.Green.Read(reader, title);
            this.Yellow.Read(reader, title);
            this.Red.Read(reader, title);

            this.Green.Read(reader, title + "Green.");
            this.Yellow.Read(reader, title + "Yellow.");
            this.Red.Read(reader, title + "Red.");
        }

        [Obsolete]
        public bool TryReadHealthTextType(INIReader reader, string section, string title)
        {
            bool isRead = false;

            bool hidden = false;
            if (reader.Read(section, title + "Hidden", ref hidden))
            {
                isRead = true;
                this.Hidden = hidden;
            }

            if (!Hidden)
            {

                if (this.Green.TryReadHealthText(reader, section, title))
                {
                    isRead = true;
                }
                if (this.Yellow.TryReadHealthText(reader, section, title))
                {
                    isRead = true;
                }
                if (this.Red.TryReadHealthText(reader, section, title))
                {
                    isRead = true;
                }

                if (this.Green.TryReadHealthText(reader, section, title + "Green."))
                {
                    isRead = true;
                }
                if (this.Yellow.TryReadHealthText(reader, section, title + "Yellow."))
                {
                    isRead = true;
                }
                if (this.Red.TryReadHealthText(reader, section, title + "Red."))
                {
                    isRead = true;
                }
            }
            
            return isRead;
        }

    }


}
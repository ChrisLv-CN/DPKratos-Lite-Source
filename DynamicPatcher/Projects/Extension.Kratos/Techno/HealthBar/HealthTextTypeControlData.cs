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
    public class HealthTextTypeControlData : INIConfig
    {
        public bool Hidden;

        public HealthTextTypeData Building;
        public HealthTextTypeData Infantry;
        public HealthTextTypeData Unit;
        public HealthTextTypeData Aircraft;

        public HealthTextTypeControlData()
        {
            this.Hidden = false;
            this.Building = new HealthTextTypeData(AbstractType.Building);
            this.Infantry = new HealthTextTypeData(AbstractType.Infantry);
            this.Unit = new HealthTextTypeData(AbstractType.Unit);
            this.Aircraft = new HealthTextTypeData(AbstractType.Aircraft);
        }

        public override void Read(IConfigReader reader)
        {
            Logger.Log($"{Game.CurrentFrame} 读取全局设置");
            this.Hidden = reader.Get("HealthText.Hidden", Hidden);

            this.Building.Read(reader, "HealthText.");
            this.Infantry.Read(reader, "HealthText.");
            this.Unit.Read(reader, "HealthText.");
            this.Aircraft.Read(reader, "HealthText.");

            this.Building.Read(reader, "HealthText.Building.");
            this.Infantry.Read(reader, "HealthText.Infantry.");
            this.Unit.Read(reader, "HealthText.Unit.");
            this.Aircraft.Read(reader, "HealthText.Aircraft.");
        }

        [Obsolete]
        public bool TryReadHealthText(INIReader reader, string section)
        {
            bool isRead = false;

            bool hidden = false;
            if (reader.Read(section, "HealthText.Hidden", ref hidden))
            {
                isRead = true;
                this.Hidden = hidden;
            }

            if (!Hidden)
            {
                if (Building.TryReadHealthTextType(reader, section, "HealthText."))
                {
                    isRead = true;
                }
                if (Infantry.TryReadHealthTextType(reader, section, "HealthText."))
                {
                    isRead = true;
                }
                if (Unit.TryReadHealthTextType(reader, section, "HealthText."))
                {
                    isRead = true;
                }
                if (Aircraft.TryReadHealthTextType(reader, section, "HealthText."))
                {
                    isRead = true;
                }

                if (Building.TryReadHealthTextType(reader, section, "HealthText.Building."))
                {
                    isRead = true;
                }
                if (Infantry.TryReadHealthTextType(reader, section, "HealthText.Infantry."))
                {
                    isRead = true;
                }
                if (Unit.TryReadHealthTextType(reader, section, "HealthText.Unit."))
                {
                    isRead = true;
                }
                if (Aircraft.TryReadHealthTextType(reader, section, "HealthText.Aircraft."))
                {
                    isRead = true;
                }
            }

            return isRead;
        }

    }



}
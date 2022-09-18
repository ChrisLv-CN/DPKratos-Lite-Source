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
    public class HealthTextTypeControlData
    {
        public bool Hidden;

        public HealthTextTypeData Building;
        public HealthTextTypeData Infantry;
        public HealthTextTypeData Unit;
        public HealthTextTypeData Aircraft;

        public HealthTextTypeControlData()
        {
            ISectionReader sectionReader = Ini.GetSection(Ini.GetDependency(INIConstant.RulesName), RulesExt.SectionAudioVisual);

            this.Hidden = sectionReader.Get(HealthTextTypeData.TITLE + "Hidden", false);

            this.Building = new HealthTextTypeData(AbstractType.Building);
            this.Building.Read(sectionReader, HealthTextTypeData.TITLE);
            this.Building.Read(sectionReader, HealthTextTypeData.TITLE + "Building.");

            this.Infantry = new HealthTextTypeData(AbstractType.Infantry);
            this.Infantry.Read(sectionReader, HealthTextTypeData.TITLE);
            this.Infantry.Read(sectionReader, HealthTextTypeData.TITLE + "Infantry.");

            this.Unit = new HealthTextTypeData(AbstractType.Unit);
            this.Unit.Read(sectionReader, HealthTextTypeData.TITLE);
            this.Unit.Read(sectionReader, HealthTextTypeData.TITLE + "Unit.");

            this.Aircraft = new HealthTextTypeData(AbstractType.Aircraft);
            this.Aircraft.Read(sectionReader, HealthTextTypeData.TITLE);
            this.Aircraft.Read(sectionReader, HealthTextTypeData.TITLE + "Aircraft.");
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
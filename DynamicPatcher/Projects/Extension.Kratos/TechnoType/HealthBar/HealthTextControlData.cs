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
    public class HealthTextControlData
    {
        public bool Hidden;

        public HealthTextData Building;
        public HealthTextData Infantry;
        public HealthTextData Unit;
        public HealthTextData Aircraft;

        public HealthTextControlData()
        {
            ISectionReader sectionReader = Ini.GetSection(Ini.GetDependency(INIConstant.RulesName), RulesExt.SectionAudioVisual);

            this.Hidden = sectionReader.Get(HealthTextData.TITLE + "Hidden", false);

            this.Building = new HealthTextData(AbstractType.Building);
            this.Building.Read(sectionReader, HealthTextData.TITLE);
            this.Building.Read(sectionReader, HealthTextData.TITLE + "Building.");

            this.Infantry = new HealthTextData(AbstractType.Infantry);
            this.Infantry.Read(sectionReader, HealthTextData.TITLE);
            this.Infantry.Read(sectionReader, HealthTextData.TITLE + "Infantry.");

            this.Unit = new HealthTextData(AbstractType.Unit);
            this.Unit.Read(sectionReader, HealthTextData.TITLE);
            this.Unit.Read(sectionReader, HealthTextData.TITLE + "Unit.");

            this.Aircraft = new HealthTextData(AbstractType.Aircraft);
            this.Aircraft.Read(sectionReader, HealthTextData.TITLE);
            this.Aircraft.Read(sectionReader, HealthTextData.TITLE + "Aircraft.");
        }

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class SupportSpawnsData : INIConfig
    {
        public string TITLE = "SupportSpawns.";

        public bool Enable = false;

        public string[] Weapons = null;
        public string[] EliteWeapons = null;

        public bool SwitchFLH = false;
        public bool Always = false;

        public SupportSpawnsData()
        {
            this.Enable = false;
            this.Weapons = null;
            this.EliteWeapons = null;
            this.SwitchFLH = false;
            this.Always = false;
        }

        public override void Read(IConfigReader reader)
        {
            this.Weapons = reader.GetList(TITLE + "Weapons", this.Weapons);
            this.EliteWeapons = reader.GetList(TITLE + "EliteWeapons", this.EliteWeapons);
            this.Enable = (null != Weapons && Weapons.Any()) || (null != EliteWeapons && EliteWeapons.Any());

            this.SwitchFLH = reader.Get(TITLE + "SwitchFLH", this.SwitchFLH);
            this.Always = reader.Get(TITLE + "Always", this.Always);
        }

    }

}

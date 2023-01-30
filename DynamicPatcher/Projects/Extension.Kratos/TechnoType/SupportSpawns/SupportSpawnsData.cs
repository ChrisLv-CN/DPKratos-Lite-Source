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

        public bool Enable;

        public string[] Weapons;
        public string[] EliteWeapons;

        public bool SwitchFLH;
        public bool AlwaysFire;

        public SupportSpawnsData()
        {
            this.Enable = false;
            this.Weapons = null;
            this.EliteWeapons = null;
            this.SwitchFLH = false;
            this.AlwaysFire = false;
        }

        public override void Read(IConfigReader reader)
        {
            this.Weapons = reader.GetList(TITLE + "Weapons", this.Weapons);
            this.EliteWeapons = reader.GetList(TITLE + "EliteWeapons", this.Weapons);
            this.Enable = (null != Weapons && Weapons.Any()) || (null != EliteWeapons && EliteWeapons.Any());

            this.SwitchFLH = reader.Get(TITLE + "SwitchFLH", this.SwitchFLH);
            this.AlwaysFire = reader.Get(TITLE + "AlwaysFire", this.AlwaysFire);
        }

    }

}

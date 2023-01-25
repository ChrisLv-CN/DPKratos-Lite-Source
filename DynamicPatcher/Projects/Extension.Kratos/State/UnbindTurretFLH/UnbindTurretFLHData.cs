using System.Collections.Generic;
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
    public class UnbindTurretFLHData : INIConfig
    {

        public bool Enable;

        public bool PrimaryFireOnTurret;
        public bool ElitePrimaryFireOnTurret;

        public bool SecondaryFireOnTurret;
        public bool EliteSecondaryFireOnTurret;

        public List<int> WeaponIndexs;
        public List<int> EliteWeaponIndexs;



        public UnbindTurretFLHData()
        {
            this.Enable = false;

            this.PrimaryFireOnTurret = true;
            this.ElitePrimaryFireOnTurret = true;

            this.SecondaryFireOnTurret = true;
            this.EliteSecondaryFireOnTurret = true;

            this.WeaponIndexs = null;
            this.EliteWeaponIndexs = null;
        }

        public override void Read(IConfigReader reader)
        {
            this.PrimaryFireOnTurret = reader.Get("PrimaryFireOnTurret", this.PrimaryFireOnTurret);
            this.ElitePrimaryFireOnTurret = reader.Get("ElitePrimaryFireOnTurret", this.PrimaryFireOnTurret);

            this.SecondaryFireOnTurret = reader.Get("SecondaryFireOnTurret", this.SecondaryFireOnTurret);
            this.EliteSecondaryFireOnTurret = reader.Get("EliteSecondaryFireOnTurret", this.SecondaryFireOnTurret);

            for (int i = 0; i < 127; i++)
            {
                int t = i + 1;
                bool bind = reader.Get("Weapon" + t + "OnTurret", true);
                if (!bind)
                {
                    if (null == WeaponIndexs)
                    {
                        WeaponIndexs = new List<int>();
                    }
                    WeaponIndexs.Add(i);
                }
                bind = reader.Get("EliteWeapon" + t + "OnTurret", bind);
                if (!bind)
                {
                    if (null == EliteWeaponIndexs)
                    {
                        EliteWeaponIndexs = new List<int>();
                    }
                    EliteWeaponIndexs.Add(i);
                }
            }

            this.Enable = !PrimaryFireOnTurret || !ElitePrimaryFireOnTurret || !SecondaryFireOnTurret || !EliteSecondaryFireOnTurret
                        || null != WeaponIndexs || null != EliteWeaponIndexs;
        }

    }


}

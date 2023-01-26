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
    public class FireFLHOnBodyData : INIConfig
    {

        public bool Enable;

        public bool PrimaryFireOnBody;
        public bool ElitePrimaryFireOnBody;

        public bool SecondaryFireOnBody;
        public bool EliteSecondaryFireOnBody;

        public List<int> WeaponIndexs;
        public List<int> EliteWeaponIndexs;



        public FireFLHOnBodyData()
        {
            this.Enable = false;

            this.PrimaryFireOnBody = false;
            this.ElitePrimaryFireOnBody = false;

            this.SecondaryFireOnBody = false;
            this.EliteSecondaryFireOnBody = false;

            this.WeaponIndexs = null;
            this.EliteWeaponIndexs = null;
        }

        public override void Read(IConfigReader reader)
        {
            this.PrimaryFireOnBody = reader.Get("PrimaryFireOnBody", this.PrimaryFireOnBody);
            this.ElitePrimaryFireOnBody = reader.Get("ElitePrimaryFireOnBody", this.PrimaryFireOnBody);

            this.SecondaryFireOnBody = reader.Get("SecondaryFireOnBody", this.SecondaryFireOnBody);
            this.EliteSecondaryFireOnBody = reader.Get("EliteSecondaryFireOnBody", this.SecondaryFireOnBody);

            for (int i = 0; i < 127; i++)
            {
                int t = i + 1;
                bool unbind = reader.Get("Weapon" + t + "OnBody", false);
                if (unbind)
                {
                    if (null == WeaponIndexs)
                    {
                        WeaponIndexs = new List<int>();
                    }
                    WeaponIndexs.Add(i);
                }
                unbind = reader.Get("EliteWeapon" + t + "OnBody", unbind);
                if (unbind)
                {
                    if (null == EliteWeaponIndexs)
                    {
                        EliteWeaponIndexs = new List<int>();
                    }
                    EliteWeaponIndexs.Add(i);
                }
            }

            this.Enable = PrimaryFireOnBody || ElitePrimaryFireOnBody || SecondaryFireOnBody || EliteSecondaryFireOnBody
                        || null != WeaponIndexs || null != EliteWeaponIndexs;
        }

    }


}

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
    public class FireFLHOnTargetData : INIConfig
    {

        public bool Enable;

        public bool PrimaryFireOnTarget;
        public bool ElitePrimaryFireOnTarget;

        public bool SecondaryFireOnTarget;
        public bool EliteSecondaryFireOnTarget;

        public List<int> WeaponIndexs;
        public List<int> EliteWeaponIndexs;



        public FireFLHOnTargetData()
        {
            this.Enable = false;

            this.PrimaryFireOnTarget = false;
            this.ElitePrimaryFireOnTarget = false;

            this.SecondaryFireOnTarget = false;
            this.EliteSecondaryFireOnTarget = false;

            this.WeaponIndexs = null;
            this.EliteWeaponIndexs = null;
        }

        public override void Read(IConfigReader reader)
        {
            this.PrimaryFireOnTarget = reader.Get("PrimaryFireOnTarget", this.PrimaryFireOnTarget);
            this.ElitePrimaryFireOnTarget = reader.Get("ElitePrimaryFireOnTarget", this.PrimaryFireOnTarget);

            this.SecondaryFireOnTarget = reader.Get("SecondaryFireOnTarget", this.SecondaryFireOnTarget);
            this.EliteSecondaryFireOnTarget = reader.Get("EliteSecondaryFireOnTarget", this.SecondaryFireOnTarget);

            for (int i = 0; i < 127; i++)
            {
                int t = i + 1;
                bool unbind = reader.Get("Weapon" + t + "OnTarget", false);
                if (unbind)
                {
                    if (null == WeaponIndexs)
                    {
                        WeaponIndexs = new List<int>();
                    }
                    WeaponIndexs.Add(i);
                }
                unbind = reader.Get("EliteWeapon" + t + "OnTarget", unbind);
                if (unbind)
                {
                    if (null == EliteWeaponIndexs)
                    {
                        EliteWeaponIndexs = new List<int>();
                    }
                    EliteWeaponIndexs.Add(i);
                }
            }

            this.Enable = PrimaryFireOnTarget || ElitePrimaryFireOnTarget || SecondaryFireOnTarget || EliteSecondaryFireOnTarget
                        || null != WeaponIndexs || null != EliteWeaponIndexs;
        }

    }


}

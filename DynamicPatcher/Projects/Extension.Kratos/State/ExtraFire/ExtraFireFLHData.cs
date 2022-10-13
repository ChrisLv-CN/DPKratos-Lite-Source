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
    public class ExtraFireFLH
    {

        public CoordStruct PrimaryFLH;
        public CoordStruct SecondaryFLH;
        public Dictionary<int, CoordStruct> WeaponXFLH;


        public ExtraFireFLH()
        {
            this.PrimaryFLH = default;
            this.SecondaryFLH = default;
            this.WeaponXFLH = null;
        }

        public ExtraFireFLH Clone()
        {
            ExtraFireFLH data = new ExtraFireFLH();
            data.PrimaryFLH = this.PrimaryFLH;
            data.SecondaryFLH = this.SecondaryFLH;
            data.WeaponXFLH = null != this.WeaponXFLH ? new Dictionary<int, CoordStruct>(this.WeaponXFLH) : null;
            return data;
        }

        public void Read(ISectionReader reader, string title)
        {
            this.PrimaryFLH = reader.Get(title + "PrimaryFLH", this.PrimaryFLH);
            this.SecondaryFLH = reader.Get(title + "SecondaryFLH", this.SecondaryFLH);

            for (int i = 0; i < 128; i++)
            {
                CoordStruct weaponFLH = reader.Get<CoordStruct>(title + "Weapon" + (i + 1) + "FLH", default);
                if (default != weaponFLH)
                {
                    if (null == this.WeaponXFLH)
                    {
                        this.WeaponXFLH = new Dictionary<int, CoordStruct>();
                    }
                    if (this.WeaponXFLH.ContainsKey(i))
                    {
                        this.WeaponXFLH[i] = weaponFLH;
                    }
                    else
                    {
                        this.WeaponXFLH.Add(i, weaponFLH);
                    }
                }
            }

        }
    }

    [Serializable]
    public class ExtraFireFLHData : INIConfig
    {
        public const string TITLE = "ExtraFire.";

        public ExtraFireFLH Data;
        public ExtraFireFLH EliteData;

        public ExtraFireFLHData()
        {
            this.Data = null;
            this.EliteData = null;
        }

        public override void Read(IConfigReader reader)
        {
            this.Data = new ExtraFireFLH();
            Data.Read(reader, TITLE);

            this.EliteData = this.Data.Clone();
            EliteData.Read(reader, TITLE + "Elite");
        }

    }


}

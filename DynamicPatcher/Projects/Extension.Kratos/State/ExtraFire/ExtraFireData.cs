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

    public partial class AttachEffectData
    {
        public ExtraFireData ExtraFireData;

        private void ReadExtraFireData(IConfigReader reader)
        {
            ExtraFireData data = new ExtraFireData();
            data.Read(reader);
            if (data.Enable)
            {
                this.ExtraFireData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class ExtraFireEntity
    {

        public string[] Primary;
        public string[] Secondary;
        public Dictionary<int, string[]> WeaponX;


        public ExtraFireEntity()
        {
            this.Primary = null;
            this.Secondary = null;
            this.WeaponX = null;
        }

        public ExtraFireEntity Clone()
        {
            ExtraFireEntity data = new ExtraFireEntity();
            data.Primary = null != this.Primary ? (string[])this.Primary.Clone() : null;
            data.Secondary = null != this.Secondary ? (string[])this.Secondary.Clone() : null;
            data.WeaponX = null != this.WeaponX ? new Dictionary<int, string[]>(this.WeaponX) : null;
            return data;
        }

        public void Read(ISectionReader reader, string title)
        {
            this.Primary = reader.GetList(title + "Primary", this.Primary);
            this.Secondary = reader.GetList(title + "Secondary", this.Secondary);

            for (int i = 0; i < 127; i++)
            {
                string[] weapons = reader.GetList<string>(title + "Weapon" + (i + 1), null);
                if (null != weapons && weapons.Length > 0)
                {
                    if (null == this.WeaponX)
                    {
                        this.WeaponX = new Dictionary<int, string[]>();
                    }
                    if (this.WeaponX.ContainsKey(i))
                    {
                        this.WeaponX[i] = weapons;
                    }
                    else
                    {
                        this.WeaponX.Add(i, weapons);
                    }
                }
            }

        }

        public bool IsEnable()
        {
            return (null != this.Primary && this.Primary.Length > 0)
                || (null != this.Secondary && this.Secondary.Length > 0)
                || (null != this.WeaponX && this.WeaponX.Count > 0);
        }
    }

    [Serializable]
    public class ExtraFireData : EffectData, IStateData
    {
        public const string TITLE = "ExtraFire.";

        public ExtraFireEntity Data;
        public ExtraFireEntity EliteData;

        public ExtraFireData()
        {
            this.Data = null;
            this.EliteData = null;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            ExtraFireEntity data = new ExtraFireEntity();
            data.Read(reader, TITLE);
            if (null != data && data.IsEnable())
            {
                this.Data = data;
            }
            // 精英武器不克隆，需要单独写
            ExtraFireEntity elite = new ExtraFireEntity();
            elite.Read(reader, TITLE + "Elite");
            if (null != elite && elite.IsEnable())
            {
                this.EliteData = elite;
            }

            this.Enable = null != this.Data || null != this.EliteData;

        }

    }


}

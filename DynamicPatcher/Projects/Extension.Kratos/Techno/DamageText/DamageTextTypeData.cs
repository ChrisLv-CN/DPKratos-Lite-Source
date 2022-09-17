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
    public class DamageTextTypeData
    {
        public const string TITLE = "DamageText.";

        public bool Hidden;

        public DamageTextData Damage;
        public DamageTextData Repair;

        public DamageTextTypeData()
        {
            this.Hidden = false;
            this.Damage = new DamageTextData(true);
            this.Repair = new DamageTextData(false);
        }

        public DamageTextTypeData Clone()
        {
            DamageTextTypeData data = new DamageTextTypeData();
            data.Hidden = this.Hidden;
            data.Damage = this.Damage.Clone();
            data.Repair = this.Repair.Clone();
            return data;
        }

        public void Read(ISectionReader reader, string title = TITLE)
        {
            this.Hidden = reader.Get(title + "Hidden", Hidden);

            this.Damage.Read(reader, title);
            this.Damage.Read(reader, title + "Damage.");

            this.Repair.Read(reader, title);
            this.Repair.Read(reader, title + "Repair.");
        }

        [Obsolete]
        public bool TryReadDamageTextType(INIReader reader, string section, string title)
        {
            bool isRead = false;

            bool hidden = false;
            if (reader.Read(section, title + "Hidden", ref hidden))
            {
                isRead = true;
                this.Hidden = hidden;
            }

            if (!Hidden)
            {
                if (Damage.TryReadDamageText(reader, section, title))
                {
                    isRead = true;
                }
                if (Repair.TryReadDamageText(reader, section, title))
                {
                    isRead = true;
                }

                if (Damage.TryReadDamageText(reader, section, title + "Damage."))
                {
                    isRead = true;
                }
                if (Repair.TryReadDamageText(reader, section, title + "Repair."))
                {
                    isRead = true;
                }
            }

            return isRead;
        }

    }



}
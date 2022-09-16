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
    public enum DamageTextStyle
    {
        AUTO = 0, DAMAGE = 1, REPAIR = 2
    }

    [Serializable]
    public class DamageTextTypeControlData
    {
        public bool Hidden;

        public Dictionary<int, DamageTextTypeData> Types;

        public DamageTextTypeControlData()
        {
            ISectionReader sectionReader = Ini.GetSection(Ini.GetDependency(INIConstant.RulesName), RulesExt.SectionAudioVisual);

            this.Hidden = sectionReader.Get(DamageTextTypeData.TITLE + "Hidden", false);
            this.Types = new Dictionary<int, DamageTextTypeData>();
            for (int i = 0; i <= 10; i++)
            {
                // 0 是unknow类型，默认设置
                DamageTextTypeData data = new DamageTextTypeData();
                data.Read(sectionReader);
                Types.Add(i, data);
            }
        }

        [Obsolete]
        public bool TryReadDamageText(INIReader reader, string section)
        {
            bool isRead = false;

            bool hidden = false;
            if (reader.Read(section, "DamageText.Hidden", ref hidden))
            {
                isRead = true;
                this.Hidden = hidden;
            }

            if (!Hidden)
            {

                foreach (KeyValuePair<int, DamageTextTypeData> type in Types)
                {
                    if (type.Value.TryReadDamageTextType(reader, section, "DamageText."))
                    {
                        isRead = true;
                    }

                    if (type.Value.TryReadDamageTextType(reader, section, "DamageText." + type.Key + "."))
                    {
                        isRead = true;
                    }
                }
            }

            return isRead;
        }
    }



}
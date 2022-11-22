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
    public class HotKeyAttachEffectTypeData : INIConfig
    {
        public bool Enable;

        public Dictionary<int, HotKeyAttachEffectData> Datas;

        public HotKeyAttachEffectTypeData()
        {
            this.Enable = false;
            this.Datas = null;
        }

        public override void Read(IConfigReader reader)
        {
            ISectionReader cdReader = Ini.GetSection(Ini.RulesDependency, RulesClass.SectionCombatDamage);
            string title = "HotKey.";
            ReadAndAddData(0, title, cdReader, reader);
            for (int i = 0; i < 127; i++)
            {
                title = "HotKey" + i + ".";
                ReadAndAddData(i, title, cdReader, reader);
            }

            this.Enable = null != Datas && Datas.Any();
        }

        private void ReadAndAddData(int i, string title, ISectionReader generalReader, ISectionReader privateReader)
        {
            HotKeyAttachEffectData data = new HotKeyAttachEffectData();
            data.Read(generalReader, title);
            if (data.Enable)
            {
                FindAndAdd(i, data);
            }
            HotKeyAttachEffectData data2 = new HotKeyAttachEffectData();
            data2.Read(privateReader, title);
            if (data2.Enable)
            {
                FindAndAdd(i, data2);
            }
        }

        private void FindAndAdd(int i, HotKeyAttachEffectData data)
        {
            if (null == Datas)
            {
                Datas = new Dictionary<int, HotKeyAttachEffectData>();
            }
            if (Datas.ContainsKey(i))
            {
                Datas[i] = data;
            }
            else
            {
                Datas.Add(i, data);
            }
        }

    }

}

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
    public partial class AttachEffectTypeTypeData : INIConfig
    {
        public bool Enable;

        public Dictionary<int, AttachEffectTypeData> Datas;

        public AttachEffectTypeTypeData()
        {
            this.Enable = false;
            this.Datas = null;
        }

        public override void Read(IConfigReader reader)
        {
            for (int i = 1; i < 128; i++)
            {
                AttachEffectTypeData data = new AttachEffectTypeData();
                data.Read(reader, i);
                if (null != data.AttachEffectTypes && data.AttachEffectTypes.Any())
                {
                    if (null == Datas)
                    {
                        Datas = new Dictionary<int, AttachEffectTypeData>();
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

            this.Enable = null != Datas && Datas.Any();
        }

    }

}

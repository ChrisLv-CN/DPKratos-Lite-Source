using System.Data.Common;
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
    public class SHPFVTurretTypeData : INIConfig
    {
        public Dictionary<int, SHPFVTurretData> Datas;

        public SHPFVTurretTypeData()
        {
            this.Datas = null;
        }

        public override void Read(IConfigReader reader)
        {
            for (int i = 1; i < 128; i++)
            {
                SHPFVTurretData data = new SHPFVTurretData();
                data.Read(reader, i);
                if (data.WeaponTurretFrameIndex > -1)
                {
                    if (null == Datas)
                    {
                        Datas = new Dictionary<int, SHPFVTurretData>();
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

        public bool TryGetData(int ifvMode, out SHPFVTurretData data)
        {
            data = null;
            if (null != Datas && Datas.ContainsKey(ifvMode))
            {
                data = Datas[ifvMode];
                return true;
            }
            return false;
        }

    }

}

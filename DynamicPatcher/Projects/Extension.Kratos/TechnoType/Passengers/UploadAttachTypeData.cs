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
    public class UploadAttachTypeData : INIConfig
    {
        public bool Enable;

        public Dictionary<int, UploadAttachData> Datas;

        public UploadAttachTypeData()
        {
            this.Enable = false;
            this.Datas = null;
        }

        public override void Read(IConfigReader reader)
        {
            string title = "Upload.";
            UploadAttachData data = new UploadAttachData();
            data.Read(reader, title);
            if (data.Enable)
            {
                FindAndAdd(0, data);
            }

            for (int i = 0; i < 127; i++)
            {
                title = "Upload" + i + ".";
                UploadAttachData data2 = new UploadAttachData();
                data2.Read(reader);
                if (data2.Enable)
                {
                    FindAndAdd(i, data2);
                }
            }

            this.Enable = null != Datas && Datas.Any();
        }

        private void FindAndAdd(int i, UploadAttachData data)
        {
            if (null == Datas)
            {
                Datas = new Dictionary<int, UploadAttachData>();
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

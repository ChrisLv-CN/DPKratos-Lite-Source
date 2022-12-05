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
    public class FeedbackAttachTypeData : INIConfig
    {
        public bool Enable;

        public Dictionary<int, FeedbackAttachData> Datas;

        public FeedbackAttachTypeData()
        {
            this.Enable = false;
            this.Datas = null;
        }

        public override void Read(IConfigReader reader)
        {
            string title = "Feedback.";
            FeedbackAttachData data = new FeedbackAttachData();
            data.Read(reader, title);
            if (data.Enable)
            {
                FindAndAdd(0, data);
            }

            for (int i = 0; i < 127; i++)
            {
                title = "Feedback" + i + ".";
                FeedbackAttachData data2 = new FeedbackAttachData();
                data2.Read(reader, title);
                if (data2.Enable)
                {
                    FindAndAdd(i, data2);
                }
            }

            this.Enable = null != Datas && Datas.Any();
        }

        private void FindAndAdd(int i, FeedbackAttachData data)
        {
            if (null == Datas)
            {
                Datas = new Dictionary<int, FeedbackAttachData>();
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

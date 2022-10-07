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
    public class DeployToTransformData : GiftBoxData
    {
        public new const string TITLE = "DeployToTransform.";

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            // 兼容旧版语句
            if (null == Data)
            {
                string[] gifts = reader.GetList<string>("DeployToTransform", null);
                if (null != gifts && gifts.Length > 0)
                {
                    GiftBox data = new GiftBox();
                    data.Read(reader, TITLE);
                    data.Gifts = gifts;
                    this.Data = data;
                    if (null == EliteData)
                    {
                        EliteData = Data.Clone();
                    }
                }
                this.Enable = null != Data || null != EliteData;
            }
            ForTransform(); // 强制修改部分属性
        }

    }

}

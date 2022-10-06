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
            ForTransform(); // 强制修改部分属性
        }

    }

}

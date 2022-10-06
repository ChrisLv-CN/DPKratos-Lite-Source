using System;
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
        public TransformData TransformData;

        private void ReadTransformData(IConfigReader reader)
        {
            TransformData data = new TransformData(reader);
            if (data.Enable)
            {
                this.TransformData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class TransformData : EffectData
    {

        public const string TITLE = "Transform.";

        public string TransformToType;


        public TransformData()
        {
            this.TransformToType = null;
        }

        public TransformData(IConfigReader reader) : this()
        {
            Read(reader);
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.TransformToType = reader.Get(TITLE + "Type", this.TransformToType);
            this.Enable = !TransformToType.IsNullOrEmptyOrNone();
        }

    }


}

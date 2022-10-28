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
        public MarkData MarkData;

        private void ReadMarkData(IConfigReader reader)
        {
            MarkData data = new MarkData(reader);
            if (data.Enable)
            {
                this.MarkData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class MarkData : EffectData
    {

        public const string TITLE = "Mark.";

        public string Name;


        public MarkData()
        {
            this.Name = null;
        }

        public MarkData(IConfigReader reader) : this()
        {
            Read(reader);
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Name = reader.Get(TITLE + "Name", this.Name);
            this.Enable = !Name.IsNullOrEmptyOrNone();
        }

    }


}

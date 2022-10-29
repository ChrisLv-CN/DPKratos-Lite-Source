using System;
using System.Linq;
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

        public string[] Names;


        public MarkData()
        {
            this.Names = null;
        }

        public MarkData(IConfigReader reader) : this()
        {
            Read(reader);
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Names = reader.GetList(TITLE + "Names", this.Names);
            this.Enable = null != this.Names && this.Names.Any();
        }

    }


}

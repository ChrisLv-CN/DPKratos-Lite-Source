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
        public ScatterData ScatterData;

        private void ReadScatterData(IConfigReader reader)
        {
            ScatterData data = new ScatterData();
            data.Read(reader);
            if (data.Enable)
            {
                this.ScatterData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class ScatterData : EffectData, IStateData
    {
        public const string TITLE = "Scatter.";

        public CoordStruct MoveToFLH;
        public bool Panic;


        public ScatterData()
        {
            this.MoveToFLH = default;
            this.Panic = false;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.MoveToFLH = reader.Get(TITLE + "MoveToFLH", MoveToFLH);
            this.Panic = reader.Get(TITLE + "Panic", Panic);
        }

    }


}

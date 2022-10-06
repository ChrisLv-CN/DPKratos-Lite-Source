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
        public DeselectData DeselectData;

        private void ReadDeselectData(IConfigReader reader)
        {
            DeselectData data = new DeselectData();
            data.Read(reader);
            if (data.Enable)
            {
                this.DeselectData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class DeselectData : EffectData, IStateData
    {
        public const string TITLE = "Select.";

        public bool Disable;

        public DeselectData()
        {
            this.Disable = false;

            this.AffectWho = AffectWho.ALL;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Disable = reader.Get(TITLE + "Disable", false);
            this.Enable = this.Disable;
        }

    }


}

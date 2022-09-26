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
    public class DeselectData : EffectData, IStateData
    {
        public const string TITLE = "Select.";

        public bool Disable;

        public DeselectData()
        {
            this.Disable = false;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Disable = reader.Get(TITLE + "Disable", false);
            this.Enable = this.Disable;
        }

    }


}

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
        public FreezeData FreezeData;

        private void ReadFreezeData(IConfigReader reader)
        {
            FreezeData data = new FreezeData();
            data.Read(reader);
            if (data.Enable)
            {
                this.FreezeData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class FreezeData : EffectData, IStateData
    {
        public const string TITLE = "Freeze.";

        public FreezeData()
        {
            this.AffectWho = AffectWho.ALL;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);
        }

    }


}

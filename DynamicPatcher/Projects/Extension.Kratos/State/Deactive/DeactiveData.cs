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
        public DeactiveData DeactiveData;

        private void ReadDeactiveData(IConfigReader reader)
        {
            DeactiveData data = new DeactiveData();
            data.Read(reader);
            if (data.Enable)
            {
                this.DeactiveData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class DeactiveData : EffectData, IStateData
    {
        public const string TITLE = "Deactive.";

        public DeactiveData()
        {
            this.AffectWho = AffectWho.ALL;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);
        }

    }


}

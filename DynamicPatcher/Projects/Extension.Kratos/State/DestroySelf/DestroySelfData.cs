using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public class DestroySelfData : EffectData, IStateData
    {
        public const string TITLE = "DestroySelf.";

        public int Delay;
        public bool Peaceful;

        public DestroySelfData()
        {
            this.Delay = -1;
            this.Peaceful = false;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            if (reader.Get("DestroySelf", false))
            {
                this.Delay = 0;
            }
            else
            {
                string delay = null;
                delay = reader.Get<string>(TITLE + "Delay", null);
                if (string.IsNullOrEmpty(delay))
                {
                    this.Delay = -1;
                }
                else if (ExHelper.Number.IsMatch(delay))
                {
                    this.Delay = Convert.ToInt32(delay);
                }
                else if (delay.ToUpper().StartsWith("Y") || delay.ToUpper().StartsWith("T"))
                {
                    this.Delay = 0;
                }
            }
            this.Enable = Delay >= 0;
            this.Peaceful = reader.Get(TITLE + "Peaceful", this.Peaceful);
        }

    }


}

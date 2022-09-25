using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public class DestroySelfData : INIConfig, IStateData
    {
        public int Delay;
        public bool Peaceful;

        public DestroySelfData()
        {
            this.Delay = -1;
            this.Peaceful = false;
        }

        public override void Read(IConfigReader reader)
        {
            string delay = null;
            delay = reader.Get<string>("DestroySelf.Delay", null);
            if (string.IsNullOrEmpty(delay))
            {
                this.Delay = -1;
            }
            else if (ExHelper.Number.IsMatch(delay))
            {
                this.Delay = Convert.ToInt32(delay);
            }
            else if (delay.ToUpper().StartsWith("y") || delay.ToUpper().StartsWith("t"))
            {
                this.Delay = 0;
            }
            this.Peaceful = reader.Get("DestroySelf.Peaceful", this.Peaceful);
        }

    }


}

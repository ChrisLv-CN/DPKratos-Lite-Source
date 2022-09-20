using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public class AntiBulletData : INIConfig
    {
        public bool Enable;
        public bool OneShotOneKill;
        public bool Harmless;
        public bool Self;
        public bool ForPassengers;
        public bool ScanAll;
        public int Range;
        public int EliteRange;
        public int Rate;

        public AntiBulletData()
        {
            this.Enable = false;
            this.OneShotOneKill = true;
            this.Harmless = false;
            this.Self = true;
            this.ForPassengers = false;
            this.ScanAll = false;
            this.Range = 0;
            this.EliteRange = this.Range;
            this.Rate = 0;
        }

        public override void Read(IConfigReader reader)
        {
            this.Enable = reader.Get("AntiMissile.Enable", this.Enable);
            this.OneShotOneKill = reader.Get("AntiMissile.OneShotOneKill", this.OneShotOneKill);
            this.Harmless = reader.Get("AntiMissile.Harmless", this.Harmless);
            this.Self = reader.Get("AntiMissile.Self", this.Self);
            this.ForPassengers = reader.Get("AntiMissile.ForPassengers", this.ForPassengers);
            this.ScanAll = reader.Get("AntiMissile.ScanAll", this.ScanAll);
            this.Range = reader.Get("AntiMissile.Range", this.Range);
            if (Range > 0)
            {
                this.Range *= 256;
            }
            this.EliteRange = reader.Get("AntiMissile.EliteRange", this.Range);
            if (EliteRange > 0 && EliteRange != Range)
            {
                this.EliteRange *= 256;
            }
            this.Rate = reader.Get("AntiMissile.Rate", this.Rate);
        }

        public override string ToString()
        {
            return string.Format("{{\"Enable\":{0}, \"OneShotOneKill\":{1}, \"Harmless\":{2}, \"Self\":{3}, \"ForPassengers\":{4}, \"ScanAll\":{5}, \"Range\":{6}, \"EliteRange\":{7}, \"Rate\":{8}}}",
                Enable, OneShotOneKill, Harmless, Self, ForPassengers, ScanAll, Range, EliteRange, Rate
            );
        }

    }


}

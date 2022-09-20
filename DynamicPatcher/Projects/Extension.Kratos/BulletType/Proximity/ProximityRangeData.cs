using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public class ProximityRangeData : INIConfig
    {
        public int Range;
        public bool Random;
        public int MaxRange;
        public int MinRange;

        public ProximityRangeData()
        {
            this.Range = -1;
            this.Random = false;
            this.MinRange = 0;
            this.MaxRange = 0;
        }

        public override void Read(IConfigReader reader)
        {
            this.Range = reader.Get("ProximityRange", -1);
            if (Range > 0)
            {
                this.Range *= 256;
            }
            this.MaxRange = Range;
            Point2D randomRange = reader.Get<Point2D>("ProximityRange.Random", default);
            if (default != randomRange)
            {
                this.Range = 0;
                this.Random = true;
                this.MinRange = randomRange.X * 256;
                this.MaxRange = randomRange.Y * 256;
                if (MinRange > MaxRange)
                {
                    int temp = MaxRange;
                    MaxRange = MinRange;
                    MinRange = temp;
                }
            }
        }
    }

}

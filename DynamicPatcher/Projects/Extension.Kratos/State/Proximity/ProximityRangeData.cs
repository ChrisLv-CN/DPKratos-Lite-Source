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
        public int Range; // *256

        public bool Random;
        public Point2D RandomRange;

        public ProximityRangeData()
        {
            this.Range = 0;
            this.Random = false;
            this.RandomRange = default;
        }

        public override void Read(IConfigReader reader)
        {
            double range = reader.Get("ProximityRange", -1d);
            if (range > 0)
            {
                this.Range = (int)(range * 256);
            }

            SingleVector2D randomRange = reader.Get<SingleVector2D>("ProximityRange.Random", default);
            if (0 != randomRange.X && 0 != randomRange.Y)
            {
                int x = (int)(randomRange.X * 256);
                int y = (int)(randomRange.Y * 256);
                if (x < 0)
                {
                    x = -x;
                }
                if (y < 0)
                {
                    y = -x;
                }
                if (x > y)
                {
                    int t = x;
                    x = y;
                    y = t;
                }
                this.RandomRange = new Point2D(x, y);
                this.Random = 0 != RandomRange.X && 0 != RandomRange.Y;
            }
        }

        public int GetRange()
        {
            if (Random && default != RandomRange)
            {
                return RandomRange.GetRandomValue(0);
            }
            return Range;
        }
    }

}

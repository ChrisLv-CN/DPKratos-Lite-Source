using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Ext
{

    public partial class TrailType
    {
        public LaserType LaserType;

        private void InitLaserType()
        {
            LaserType = new LaserType(true);
            LaserType.IsHouseColor = false;
            LaserType.Fade = true;
        }

        private void ReadLaserType(ISectionReader reader)
        {

            this.LaserType.InnerColor = reader.Get("Laser.InnerColor", this.LaserType.InnerColor);
            this.LaserType.OuterColor = reader.Get("Laser.OuterColor", this.LaserType.OuterColor);
            this.LaserType.OuterSpread = reader.Get("Laser.OuterSpread", this.LaserType.OuterSpread);

            this.LaserType.IsHouseColor = reader.Get("Laser.IsHouseColor", this.LaserType.IsHouseColor);
            this.LaserType.IsSupported = reader.Get("Laser.IsSupported", this.LaserType.IsSupported);
            this.LaserType.Fade = reader.Get("Laser.Fade", this.LaserType.Fade);

            this.LaserType.Duration = reader.Get("Laser.Duration", this.LaserType.Duration);
            this.LaserType.Thickness = reader.Get("Laser.Thickness", this.LaserType.Thickness);

            List<ColorStruct> colorList = new List<ColorStruct>();
            for (int i = 0; i < 128; i++)
            {
                string key = "Laser.Color" + i;
                ColorStruct color = reader.Get<ColorStruct>(key, default);
                if (default != color)
                {
                    colorList.Add(color);
                }
            }
            if (null != colorList && colorList.Any())
            {
                this.LaserType.ColorList = colorList.ToArray();
            }
            this.LaserType.ColorListRandom = reader.Get("Laser.ColorListRandom", this.LaserType.ColorListRandom);
        }
    }
}

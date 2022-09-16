using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using System.Threading.Tasks;
using Extension.INI;

namespace Extension.Ext
{
    
    [Serializable]
    public class DamageTextData : PrintTextData
    {
        public bool Hidden;
        public bool Detail;
        public int Rate;
        public Point2D XOffset;
        public Point2D YOffset;
        public int RollSpeed;
        public int Duration;

        public DamageTextData(bool isDamage) : base()
        {
            this.Hidden = false;
            this.Detail = true;
            this.Rate = 0;
            this.XOffset = new Point2D(-15, 15);
            this.YOffset = new Point2D(-12, 12);
            this.RollSpeed = 1;
            this.Duration = 75;

            this.SHPFileName = "pipsnum.shp";
            this.ImageSize = new Point2D(5, 8);
            if (isDamage)
            {
                this.Color = new ColorStruct(252, 0, 0);
                this.ZeroFrameIndex = 30;

                this.HitIndex = 1;
            }
            else
            {
                this.Color = new ColorStruct(0, 252, 0);
                this.ZeroFrameIndex = 0;

                this.HitIndex = 0;
            }
        }

        public DamageTextData Clone()
        {
            DamageTextData data = new DamageTextData(true);
            CopyTo(data);
            data.Hidden = this.Hidden;
            data.Detail = this.Detail;
            data.Rate = this.Rate;
            data.XOffset = this.XOffset;
            data.YOffset = this.YOffset;
            data.RollSpeed = this.RollSpeed;
            data.Duration = this.Duration;
            return data;
        }

        public override void Read(ISectionReader reader, string title)
        {
            base.Read(reader, title);
            this.Hidden = reader.Get(title + "Hidden", Hidden);
            this.Detail = reader.Get(title + "Detail", Detail);
            this.Rate = reader.Get(title + "Rate", Rate);
            this.XOffset = reader.Get(title + "XOffset", XOffset);
            this.YOffset = reader.Get(title + "YOffset", YOffset);
            this.RollSpeed = reader.Get(title + "RollSpeed", RollSpeed);
            this.Duration = reader.Get(title + "Duration", Duration);
        }

        [Obsolete]
        public bool TryReadDamageText(INIReader reader, string section, string title)
        {
            bool isRead = false;

            bool hidden = false;
            if (reader.Read(section, title + "Hidden", ref hidden))
            {
                isRead = true;
                this.Hidden = hidden;
            }

            if (!Hidden)
            {
                isRead = TryReadPrintText(reader, section, title);

                bool detail = false;
                if (reader.Read(section, title + "Detail", ref detail))
                {
                    isRead = true;
                    this.Detail = detail;
                }

                int rate = 0;
                if (reader.Read(section, title + "Rate", ref rate))
                {
                    isRead = true;
                    this.Rate = rate;
                }

                Point2D xOffset = default;
                if (ExHelper.ReadPoint2D(reader, section, title + "XOffset", ref xOffset))
                {
                    isRead = true;
                    Point2D offset = xOffset;
                    if (xOffset.X > xOffset.Y)
                    {
                        offset.X = xOffset.Y;
                        offset.Y = xOffset.X;
                    }
                    this.XOffset = offset;
                }

                Point2D yOffset = default;
                if (ExHelper.ReadPoint2D(reader, section, title + "YOffset", ref yOffset))
                {
                    isRead = true;
                    Point2D offset = yOffset;
                    if (yOffset.X > yOffset.Y)
                    {
                        offset.X = yOffset.Y;
                        offset.Y = yOffset.X;
                    }
                    this.YOffset = offset;
                }

                int roll = 1;
                if (reader.Read(section, title + "RollSpeed", ref roll))
                {
                    isRead = true;
                    this.RollSpeed = roll;
                }

                int duration = 0;
                if (reader.Read(section, title + "Duration", ref duration))
                {
                    isRead = true;
                    this.Duration = duration;
                }
            }

            return isRead;
        }
    }

}
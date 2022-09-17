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
    public class RollingText : PrintText
    {
        public int RollSpeed;

        public RollingText(string text, CoordStruct location, Point2D offset, int rollSpeed, int duration, PrintTextData data) : base(text, location, offset, duration, data)
        {
            this.Text = text;
            this.Location = location;
            this.Offset = offset;
            this.RollSpeed = rollSpeed;
            this.Duration = duration;
            this.LifeTimer.Start(duration);
            this.Data = data;
        }

        public new bool CanPrint(out Point2D offset, out Point2D pos, out RectangleStruct bound)
        {
            if (base.CanPrint(out offset, out pos, out bound))
            {
                this.Offset -= new Point2D(0, RollSpeed);
                return true;
            }
            return false;
        }

    }

}

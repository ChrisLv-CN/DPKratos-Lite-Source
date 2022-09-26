using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class PaintballState : State<PaintballData>
    {
        
        public bool NeedPaint(out bool changeColor, out bool changeBright)
        {
            changeColor = false;
            changeBright = false;
            bool active = IsActive() && null != Data;
            if (active)
            {
                changeColor = default != Data.Color;
                changeBright = 1.0f != Data.BrightMultiplier;
            }
            return active;
        }

        public uint GetColor()
        {
            ColorStruct colorAdd = Data.Color.ToColorAdd();
            return colorAdd.Add2RGB565();
        }

        public uint GetBright(uint bright)
        {
            double b = bright * Data.BrightMultiplier;
            if (b < 0)
            {
                b = 0;
            }
            else if (b > 2000)
            {
                b = 2000;
            }
            return (uint)b;
        }
    }

}

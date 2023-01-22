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
        private int lastFrame;

        public override void StartTimer(int duration)
        {
            base.StartTimer(duration + 1);
        }

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

    }

}

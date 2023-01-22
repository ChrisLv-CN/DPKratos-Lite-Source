using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class BulletStatusScript
    {

        public PaintballState PaintballState = new PaintballState();

        public void InitState_Paintball()
        {
            PaintballData data = Ini.GetConfig<PaintballData>(Ini.RulesDependency, section).Data;
            if (data.Enable)
            {
                PaintballState.Enable(data);
            }
        }

        public unsafe void BulletClass_DrawVXL_Paintball(REGISTERS* R)
        {

            if (!PaintballState.NeedPaint(out bool changeColor, out bool changeBright))
            {
                return;
            }
            if (changeColor)
            {
                uint color = PaintballState.Data.GetColor();
                // Logger.Log("RGB888 = {0}, RGB565 = {1}, RGB565 = {2}", Paintball.Color, colorAdd, ExHelper.ColorAdd2RGB565(colorAdd));
                R->Stack<uint>(0, color);
            }
            if (changeBright)
            {
                uint bright = R->Stack<uint>(0x118);
                R->Stack<uint>(0x118, PaintballState.Data.GetBright(bright));
            }
        }


    }
}

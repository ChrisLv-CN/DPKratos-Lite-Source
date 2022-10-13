using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.Utilities;
using Extension.Script;

namespace ExtensionHooks
{
    public class LaserDrawExtHooks
    {
        private static ColorStruct maxColor;

        [Hook(HookType.AresHook, Address = 0x550D1F, Size = 6)]
        public static unsafe UInt32 LaserDrawClass_DrawInHouseColor_Context_Set(REGISTERS* R)
        {
            maxColor = R->Stack<ColorStruct>(0x14);
            // Logger.Log($"{Game.CurrentFrame} Draw laser color {maxColor}");
            return 0;
        }

        // better drawing from Phobos
        [Hook(HookType.AresHook, Address = 0x550F47, Size = 5)]
        public static unsafe UInt32 LaserDrawClass_DrawInHouseColor_BetterDrawing(REGISTERS* R)
        {
            bool doNot_quickDraw = R->Stack<bool>(0x13);
            R->ESI = doNot_quickDraw ? 8u : 64u;

            Pointer<LaserDrawClass> pLaser = (IntPtr)R->EBX;
            int thickness = pLaser.Ref.Thickness;
            // faster
            if (thickness <= 5)
            {
                R->EAX = (uint)(maxColor.R >> 1);
                R->ECX = (uint)(maxColor.G >> 1);
                R->EDX = (uint)(maxColor.B >> 1);
            }
            else
            {
                double mult = 1.0;
                int currentThickness = R->Stack<int>(0x5C);
                double falloffStep = 1.0 / thickness;
                double falloffMult = (1.0 - falloffStep).FastPow(currentThickness);
                mult = (1.0 - falloffStep * currentThickness) * falloffMult;

                R->EAX = (uint)(maxColor.R * mult);
                R->ECX = (uint)(maxColor.G * mult);
                R->EDX = (uint)(maxColor.B * mult);
            }
            return 0x550F9D;
        }



        // [Hook(HookType.AresHook, Address = 0x550F6A, Size = 8)]
        // public static unsafe UInt32 LaserDrawClass_Fade(REGISTERS* R)
        // {
        //     return LaserDrawExt.LaserDrawClass_Fade(R);
        // }
    }
}


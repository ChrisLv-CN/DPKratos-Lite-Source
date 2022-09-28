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

    public partial class TechnoStatusScript
    {

        public PaintballState PaintballState = new PaintballState();

        private float deactivateDimEMP = 0.8f;
        private float deactivateDimPowered = 0.5f;

        public void OnPut_Paintball()
        {
            ISectionReader reader = Ini.GetSection(Ini.RulesDependency, RulesExt.SectionAudioVisual);
            deactivateDimEMP = reader.Get("DeactivateDimEMP", deactivateDimEMP);
            deactivateDimPowered = reader.Get("DeactivateDimPowered", deactivateDimPowered);

            PaintballData data = Ini.GetConfig<PaintballData>(Ini.RulesDependency, section).Data;
            if (data.Enable)
            {
                PaintballState.Enable(data);
            }
        }

        public void OnUpdate_Paintball()
        {
            if (pTechno.Convert<AbstractClass>().Ref.WhatAmI() == AbstractType.Building)
            {
                if (PaintballState.IsActive())
                {
                    // Logger.Log($"{Game.CurrentFrame} - {pTechno.Ref.Type.Ref.Base.Base.ID} change color {PaintballState.Color} {changeColor}, change bright {changeBright}, ForceShilded {pTechno.Ref.IsForceShilded}");
                    pTechno.Ref.Base.Mark(MarkType.CHANGE);
                }

            }
        }

        public unsafe void TechnoClass_DrawSHP_Paintball(REGISTERS* R)
        {
            // int b = (int)R->EBP;
            // int nb = pTechno.Ref.ApplyEffectBright(b);
            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} bright = {R->EBP} b = {b} nb = {nb}");
            // R->EBP = (uint)nb;
            // if (pTechno.Convert<AbstractClass>().Ref.WhatAmI() == AbstractType.Building)
            // {
            //     return;
            // }
            if (!PaintballState.NeedPaint(out bool changeColor, out bool changeBright) || pTechno.Ref.Berzerk || pTechno.Ref.IsForceShilded || pTechno.Ref.Base.IsIronCurtained())
            {
                return;
            }

            // Logger.Log($"{Game.CurrentFrame} - {pTechno} {pTechno.Ref.Type.Ref.Base.Base.ID} change color {PaintballState.Color} {changeColor}, change bright {changeBright}");
            if (changeColor)
            {
                // Logger.Log("RGB888 = {0}, RGB565 = {1}, RGB565 = {2}", Paintball.Color, colorAdd, ExHelper.ColorAdd2RGB565(colorAdd));
                R->EAX = PaintballState.GetColor();
            }
            if (changeBright)
            {
                // uint bright = R->EBP;
                R->EBP = PaintballState.GetBright(R->EBP);
            }
        }

        /// <summary>
        /// SHP载具染色
        /// </summary>
        /// <param name="R"></param>
        public unsafe void UnitClass_DrawSHP_Colour(REGISTERS* R)
        {
            if (!pTechno.IsNull && !pTechno.Ref.IsVoxel() && pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Unit)
            {
                //== Colour ==
                // ForceShield
                // LaserTarget
                // Berzerk
                if (pTechno.Ref.Berzerk)
                {
                    R->EAX = RulesClass.BerserkColor.Add2RGB565();
                    // R->EAX = Drawing.Color16bit(color);
                    // R->EBP = 200;
                    // R->EAX = 0x42945536;
                }
                // add PowerUp

                //== Darker ==
                // IronCurtain
                // EMP
                // NoPower
                if (pTechno.Ref.Base.IsIronCurtained())
                {
                    // SHP载具支持铁幕染色
                    // R->EBP = (uint)pTechno.Ref.ApplyEffectBright((int)R->EBP);
                }
                else if (pTechno.Ref.IsUnderEMP() && pTechno.Ref.EMPLockRemaining > 0)
                {
                    R->EBP = GetBright(R->EBP, deactivateDimEMP);
                }
                else if (!pTechno.Ref.Base.IsActive() && pTechno.Ref.Type.Ref.PoweredUnit)
                {
                    R->EBP = GetBright(R->EBP, deactivateDimPowered);
                }
            }
        }

        private uint GetBright(uint bright, float mult)
        {
            double b = bright * mult;
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

        public unsafe void TechnoClass_DrawSHP_Paintball_BuildAnim(REGISTERS* R)
        {
            if (!PaintballState.NeedPaint(out bool changeColor, out bool changeBright) || pTechno.Ref.Berzerk || pTechno.Ref.IsForceShilded || pTechno.Ref.Base.IsIronCurtained())
            {
                return;
            }

            // Logger.Log($"{Game.CurrentFrame} - {pTechno} {pTechno.Ref.Type.Ref.Base.Base.ID} change color {PaintballState.Color} {changeColor}, change bright {changeBright}");
            if (changeColor)
            {
                R->EBP = PaintballState.GetColor();
            }
            if (changeBright)
            {
                uint bright = R->Stack<uint>(0x38);
                R->Stack<uint>(0x38, PaintballState.GetBright(bright));
            }
        }

        public unsafe void TechnoClass_DrawVXL_Paintball(REGISTERS* R, bool isBuilding)
        {

            if (!PaintballState.NeedPaint(out bool changeColor, out bool changeBright) || pTechno.Ref.Berzerk || pTechno.Ref.IsForceShilded || pTechno.Ref.Base.IsIronCurtained())
            {
                return;
            }
            if (changeColor)
            {
                uint color = PaintballState.GetColor();
                // Logger.Log("RGB888 = {0}, RGB565 = {1}, RGB565 = {2}", Paintball.Color, colorAdd, ExHelper.ColorAdd2RGB565(colorAdd));
                if (isBuilding)
                {
                    // vxl turret
                    R->Stack<uint>(0x24, color);
                }
                else
                {
                    R->ESI = color;
                }
            }
            if (changeBright)
            {
                if (isBuilding)
                {
                    // Vxl turret
                    uint bright = R->Stack<uint>(0x20);
                    R->Stack<uint>(0x20, PaintballState.GetBright(bright));
                }
                else
                {
                    uint bright = R->Stack<uint>(0x1E0);
                    R->Stack<uint>(0x1E0, PaintballState.GetBright(bright));
                }
            }
        }


    }
}
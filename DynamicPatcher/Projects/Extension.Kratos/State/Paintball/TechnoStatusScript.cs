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

        public AnimExt ExtraSparkleAnimExt;
        private Pointer<AnimClass> pExtraSparkleAnim => null != ExtraSparkleAnimExt ? ExtraSparkleAnimExt.OwnerObject : default;

        private float deactivateDimEMP = 0.8f;
        private float deactivateDimPowered = 0.5f;

        private bool buildingWasColor = false;
        private bool buildingWasBerzerk = false;
        private bool buildingWasEMP = false;

        public void InitState_Paintball()
        {
            ISectionReader reader = Ini.GetSection(Ini.RulesDependency, RulesClass.SectionAudioVisual);
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
            if (isBuilding && !AmIStand())
            {
                // 检查状态有所变化，则重新渲染
                if (pTechno.Ref.Berzerk)
                {
                    if (!buildingWasBerzerk)
                    {
                        buildingWasBerzerk = true;
                        pTechno.Ref.Base.NeedsRedraw = true;
                    }
                }
                else
                {
                    if (buildingWasBerzerk)
                    {
                        buildingWasBerzerk = false;
                        pTechno.Ref.Base.NeedsRedraw = true;
                    }
                }

                if (pTechno.Ref.IsUnderEMP())
                {
                    if (!buildingWasEMP)
                    {
                        buildingWasEMP = true;
                        pTechno.Ref.Base.NeedsRedraw = true;
                    }
                }
                else
                {
                    if (buildingWasEMP)
                    {
                        buildingWasEMP = false;
                        pTechno.Ref.Base.NeedsRedraw = true;
                    }
                }

                if (PaintballState.IsActive())
                {
                    // Logger.Log($"{Game.CurrentFrame} - {pTechno.Ref.Type.Ref.Base.Base.ID} change color {PaintballState.Color} {changeColor}, change bright {changeBright}, ForceShilded {pTechno.Ref.IsForceShilded}");
                    if (!buildingWasColor || PaintballState.IsReset())
                    {
                        buildingWasColor = true;
                        pTechno.Ref.Base.NeedsRedraw = true;
                    }
                }
                else
                {
                    if (buildingWasColor)
                    {
                        buildingWasColor = false;
                        pTechno.Ref.Base.NeedsRedraw = true;
                    }
                }
            }

            // 移除额外的EMP动画
            if (!pExtraSparkleAnim.IsNull && !pTechno.Ref.IsUnderEMP())
            {
                pExtraSparkleAnim.Ref.Loops = 0;
                ExtraSparkleAnimExt = null;
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

            // Logger.Log($"{Game.CurrentFrame} - [{section}] {pTechno} change color {PaintballState.Data.Color} {changeColor}, change bright {changeBright}, stats.Token {PaintballState.Token}");
            if (changeColor)
            {
                // Logger.Log("RGB888 = {0}, RGB565 = {1}, RGB565 = {2}", Paintball.Color, colorAdd, ExHelper.ColorAdd2RGB565(colorAdd));
                R->EAX = PaintballState.Data.GetColor();
            }
            if (changeBright)
            {
                // uint bright = R->EBP;
                R->EBP = PaintballState.Data.GetBright(R->EBP);
            }
        }

        /// <summary>
        /// SHP载具染色
        /// </summary>
        /// <param name="R"></param>
        public unsafe void TechnoClass_DrawSHP_Colour(REGISTERS* R)
        {
            if (!pTechno.IsNull && !pTechno.Ref.IsVoxel())
            {
                //== Colour ==
                // ForceShield
                // LaserTarget
                // Berzerk, 建筑不支持狂暴染色
                if (pTechno.Ref.Berzerk) // paint color to building
                {
                    R->EAX = RulesClass.BerserkColor.Add2RGB565();
                    // R->EAX = Drawing.Color16bit(color);
                    // R->EBP = 200;
                    // R->EAX = 0x42945536;
                }

                //== Darker ==
                // IronCurtain, SHP载具支持铁幕染色
                // EMP, 建筑不支持变黑
                // NoPower
                if (pTechno.Ref.IsUnderEMP())
                {
                    R->EBP = GetBright(R->EBP, deactivateDimEMP);
                }
                else
                {
                    if (pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Unit)
                    {
                        if (!pTechno.Ref.Base.IsActive() && pTechno.Ref.Type.Ref.PoweredUnit)
                        {
                            R->EBP = GetBright(R->EBP, deactivateDimPowered);
                        }
                    }
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
            uint bright = R->Stack<uint>(0x38);
            if (pTechno.Ref.Berzerk) // paint color to building's Anim
            {
                R->EBP = RulesClass.BerserkColor.Add2RGB565();
            }

            if (pTechno.Ref.IsUnderEMP())
            {
                R->Stack<uint>(0x38, (uint)GetBright(bright, deactivateDimEMP));
            }

            if (!PaintballState.NeedPaint(out bool changeColor, out bool changeBright) || pTechno.Ref.Berzerk || pTechno.Ref.IsForceShilded || pTechno.Ref.Base.IsIronCurtained())
            {
                return;
            }

            // Logger.Log($"{Game.CurrentFrame} - {pTechno} {pTechno.Ref.Type.Ref.Base.Base.ID} change color {PaintballState.Data.Color} {changeColor}, change bright {changeBright}");
            if (changeColor)
            {
                R->EBP = PaintballState.Data.GetColor();
            }
            if (changeBright)
            {
                R->Stack<uint>(0x38, PaintballState.Data.GetBright(bright));
            }
        }

        public unsafe void TechnoClass_DrawVXL_Paintball(REGISTERS* R, bool isBuilding)
        {
            if (isBuilding)
            {
                // Vxl turret
                uint bright = R->Stack<uint>(0x20);
                if (pTechno.Ref.Berzerk) // paint color to building's Anim
                {
                    R->Stack<uint>(0x24, RulesClass.BerserkColor.Add2RGB565());
                }

                if (pTechno.Ref.IsUnderEMP())
                {
                    R->Stack<uint>(0x20, (uint)GetBright(bright, deactivateDimEMP));
                }
            }

            if (!PaintballState.NeedPaint(out bool changeColor, out bool changeBright) || pTechno.Ref.Berzerk || pTechno.Ref.IsForceShilded || pTechno.Ref.Base.IsIronCurtained())
            {
                return;
            }
            if (changeColor)
            {
                uint color = PaintballState.Data.GetColor();
                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} RGB888 = {PaintballState.Data.Color}, RGB565 = {PaintballState.Data.Color.ToColorAdd()}, RGB565 = {color}");
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
                    R->Stack<uint>(0x20, PaintballState.Data.GetBright(bright));
                }
                else
                {
                    uint bright = R->Stack<uint>(0x1E0);
                    R->Stack<uint>(0x1E0, PaintballState.Data.GetBright(bright));
                }
            }
        }


    }
}

using System.ComponentModel;
using System;
using System.Reflection;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.EventSystems;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public class Kratos
    {
        static Kratos()
        {
            bool disable = false;
            if (Ini.Read<bool>(Ini.GetDependency("uimd.ini"), "UISettings", "DisableKratosVersionText", ref disable))
            {
                disableVersionText = disable;
            }
        }

        public const string Label = "Kratos";

        private static string version;
        public static string Version
        {
            get
            {
                if (string.IsNullOrEmpty(version))
                {
                    version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    string[] v = version.Split('.');
                    if (v.Length > 2)
                    {
                        version = v[2] + "." + v[3];
                    }
                }
                return version;
            }
        }

        private static bool disableVersionText;
        private static bool disableAntiModifyMO;

        private static TimerStruct showTextTimer = new TimerStruct(150);

        private static int antiModifyMessageIndex = 7;
        private static TimerStruct antiModifyDelay;
        private static string[] supers = new string[] { "NukeSpecial", "LightningStormSpecial", "GeneticConverterSpecial" };
        private static GiftBoxData giftBoxData = new GiftBoxData(new string[] { "STARDUSTB" });
        public static void SendActiveMessage(object sender, EventArgs args)
        {
            string message = "Lite version " + Version + " is active, have fun.";
            MessageListClass.Instance.PrintMessage(Label, message, ColorSchemeIndex.Red, 150, true);
            EventSystem.GScreen.RemovePermanentHandler(EventSystem.GScreen.GScreenRenderEvent, SendActiveMessage);
            if (!disableAntiModifyMO && CCINIClass.INI_Ruels_FileName == "RULESMO.INI")
            {
                EventSystem.GScreen.AddPermanentHandler(EventSystem.GScreen.GScreenRenderEvent, IAmModifyMO);
            }
        }

        public static void DrawVersionText(object sender, EventArgs args)
        {
            string text = "Kratos-Lite Ver." + Version;
            RectangleStruct textRect = Drawing.GetTextDimensions(text, new Point2D(0, 0), 0, 2, 0);
            RectangleStruct sidebarRect = Surface.Sidebar.Ref.GetRect();
            int x = sidebarRect.Width / 2 - textRect.Width / 2;
            int y = sidebarRect.Height - textRect.Height;
            Point2D pos = new Point2D(x, y);

            Surface.Sidebar.Ref.DrawText(text, Pointer<Point2D>.AsPointer(ref pos), Drawing.TooltipColor);
            // Surface.Current.Ref.DrawText(text, Pointer<Point2D>.AsPointer(ref pos), Drawing.TooltipColor);
            // Surface.Primary.Ref.DrawText(text, Pointer<Point2D>.AsPointer(ref pos), Drawing.TooltipColor);

            if (disableVersionText && showTextTimer.Expired())
            {
                EventSystem.GScreen.RemovePermanentHandler(EventSystem.GScreen.SidebarRenderEvent, DrawVersionText);
            }
        }

        public static unsafe void IAmModifyMO(object sender, EventArgs args)
        {
            string message;
            switch (antiModifyMessageIndex)
            {
                case 7:
                    message = "Detected that you are modifying \"Mental Omega\" without authorization.";
                    VocClass.Speak("EVA_NuclearSiloDetected");
                    break;
                case 6:
                    message = "Self-Destruction countdown...";
                    VocClass.Speak("Mis_A12_EvaCountdown");
                    break;
                case 5:
                    message = antiModifyMessageIndex.ToString();
                    int nukeSiren = VocClass.FindIndex("NukeSiren");
                    if (nukeSiren > -1)
                    {
                        VocClass.PlayGlobal(nukeSiren, 0x2000, 1.0f);
                    }
                    break;
                default:
                    message = antiModifyMessageIndex.ToString();
                    break;
            }
            if (antiModifyDelay.Expired())
            {
                antiModifyDelay.Start(90);
                if (antiModifyMessageIndex > 0)
                {
                    MessageListClass.Instance.PrintMessage(Label, message, ColorSchemeIndex.Red, 450, true);
                }
                if (antiModifyMessageIndex == 0)
                {
                    MessageListClass.Instance.PrintMessage(Label, "Happy Mode Active!!!", ColorSchemeIndex.Red, -1, true);
                }
                antiModifyMessageIndex--;
            }
            if (antiModifyMessageIndex < 0 && 0.01d.Bingo())
            {
                // var func = (delegate* unmanaged[Thiscall]<int, IntPtr, void>)ASM.FastCallTransferStation;
                // func(0x7DC720, IntPtr.Zero);
                HouseClass.Array.FindIndex((pHouse, i) =>
                {
                    if (!pHouse.IsNull && pHouse.Ref.ControlledByPlayer())
                    {
                        Pointer<TechnoClass> pTarget = pHouse.GetTechnoRandom();
                        if (!pTarget.IsNull)
                        {
                            int typeIndex = MathEx.Random.Next(4);
                            CoordStruct location = pTarget.Ref.Base.Base.GetCoords();
                            switch (typeIndex)
                            {
                                case 0:
                                    int superIndex = MathEx.Random.Next(supers.Length);
                                    FireSuperEntity superEntity = new FireSuperEntity();
                                    superEntity.Supers = new string[] { supers[superIndex] };
                                    FireSuperManager.Launch(pHouse, location, superEntity);
                                    BulletEffectHelper.RedCell(location, 128, 1, 450);
                                    BulletEffectHelper.RedCrosshair(location, 1024, 1, 450);
                                    BulletEffectHelper.RedLineZ(location, 2048, 1, 450);
                                    break;
                                case 1:
                                    pTarget.Ref.FirepowerMultiplier = 4.0;
                                    pTarget.Ref.Berzerk = true;
                                    pTarget.Ref.BerzerkDurationLeft = 750;
                                    if (pTarget.CastToFoot(out var pFoot))
                                    {
                                        pTarget.Convert<MissionClass>().Ref.ForceMission(Mission.Hunt);
                                    }
                                    break;
                                case 2:
                                    pTarget.Ref.EMPLockRemaining = 450;
                                    Pointer<AnimTypeClass> pSparkles = RulesClass.Global().EMPulseSparkles;
                                    if (!pSparkles.IsNull)
                                    {
                                        Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pSparkles, pTarget.Ref.Base.Location);
                                        pAnim.Ref.Loops = 0xFF;
                                        pAnim.Ref.SetOwnerObject(pTarget.Convert<ObjectClass>());
                                        if (pTarget.Ref.Base.Base.WhatAmI() == AbstractType.Building)
                                        {
                                            pAnim.Ref.ZAdjust = -1024;
                                        }
                                        if (pTarget.TryGetStatus(out var paint))
                                        {
                                            paint.pExtraSparkleAnim.Pointer = pAnim;
                                        }
                                    }
                                    break;
                                case 3:
                                    if (pTarget.TryGetStatus(out var gift))
                                    {
                                        gift.GiftBoxState.Enable(giftBoxData);
                                    }
                                    break;
                            }
                        }
                    }
                    return false;
                });
            }
        }
    }

}

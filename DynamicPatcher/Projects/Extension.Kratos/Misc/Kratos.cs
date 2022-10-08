using System.ComponentModel;
using System;
using System.Reflection;
using DynamicPatcher;
using PatcherYRpp;
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
                    MessageListClass.Instance.PrintMessage(Label, message, ColorSchemeIndex.Red, -1, true);
                }
                if (antiModifyMessageIndex == 0)
                {
                    VocClass.Speak("EVA_NuclearMissileLaunched");
                    MessageListClass.Instance.PrintMessage(Label, "KABOOOOOOOOOOOOOOM!!!", ColorSchemeIndex.Red, -1, true);
                }
                antiModifyMessageIndex--;
            }
            if (antiModifyMessageIndex < -2)
            {
                var func = (delegate* unmanaged[Thiscall]<int, IntPtr, void>)ASM.FastCallTransferStation;
                func(0x7DC720, IntPtr.Zero);
            }
        }
    }

}

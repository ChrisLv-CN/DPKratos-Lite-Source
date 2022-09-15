
using System;
using System.Reflection;
using DynamicPatcher;
using Extension.EventSystems;
using Extension.INI;
using PatcherYRpp;

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

        private static string version;
        public static string Version
        {
            get
            {
                if (string.IsNullOrEmpty(version))
                {
                    object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
                    if (attributes.Length > 0)
                    {
                        version = ((AssemblyFileVersionAttribute)attributes[0]).Version; // DP.Kratos.x.x
                        string[] v = version.Split('.');
                        if (v.Length > 2)
                        {
                            version = v[1] + "." + v[2];
                        }
                    }
                }
                return version;
            }
        }

        private static bool disableVersionText;

        private static TimerStruct showTextTimer = new TimerStruct(150);

        public static void SendActiveMessage(object sender, EventArgs args)
        {
            string label = "DP-Kratos";
            string message = "build " + Version + " is active, have fun.";
            MessageListClass.Instance.PrintMessage(label, message, ColorSchemeIndex.Red, 150, true);
            EventSystem.GScreen.RemovePermanentHandler(EventSystem.GScreen.GScreenRenderEvent, SendActiveMessage);
        }

        public static void DrawVersionText(object sender, EventArgs args)
        {
            string text = "DP-Kratos build " + Version;
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
    }

}
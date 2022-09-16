
using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.EventSystems;

namespace ExtensionHooks
{
    public class GScreenExtHook
    {

        static GScreenExtHook()
        {
            EventSystem.GScreen.AddPermanentHandler(EventSystem.GScreen.GScreenRenderEvent, Kratos.SendActiveMessage);
            EventSystem.GScreen.AddPermanentHandler(EventSystem.GScreen.SidebarRenderEvent, Kratos.DrawVersionText);

            EventSystem.GScreen.AddPermanentHandler(EventSystem.GScreen.GScreenRenderEvent, PrintTextManager.PrintText);
        }

        // [Hook(HookType.AresHook, Address = 0x4F4497, Size = 6)] // GScreenClass_Render
        [Hook(HookType.AresHook, Address = 0x4F4583, Size = 6)] // GScreenClass_Render
        public static unsafe UInt32 GScreenClass_Render(REGISTERS* R)
        {
            EventSystem.GScreen.Broadcast(EventSystem.GScreen.GScreenRenderEvent, EventArgs.Empty);

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x6A70AE, Size = 5)] // SidebarClass_Draw_It
        public static unsafe UInt32 SidebarClass_Draw_It(REGISTERS* R)
        {
            // Logger.Log($"{Game.CurrentFrame} SidebarClass_Draw_It call"); // before GScreen
            EventSystem.GScreen.Broadcast(EventSystem.GScreen.SidebarRenderEvent, EventArgs.Empty);

            return 0;
        }

    }
}


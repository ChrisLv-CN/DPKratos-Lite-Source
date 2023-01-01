using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.EventSystems
{
    public class GScreenRenderEvent : EventBase
    {
        public override string Name => "GScreenClass_Render";
        public override string Description => "Raised when GScreen is Render";
    }
    public class SidebarRenderEvent : EventBase
    {
        public override string Name => "SidebarClass_Draw_It";
        public override string Description => "Raised when Sidebar is Render";
    }
    public class GScreenEventArgs : EventArgs
    {
        public GScreenEventArgs(bool isBeginRender)
        {
            IsBeginRender = isBeginRender;
        }

        public bool IsBeginRender { get; }
        public bool IsLateRender => !IsBeginRender;
    }
    public class GScreenEventSystem : EventSystem
    {

        public GScreenEventSystem()
        {
            GScreenRenderEvent = new GScreenRenderEvent();
            SidebarRenderEvent = new SidebarRenderEvent();
        }

        public GScreenRenderEvent GScreenRenderEvent { get; }
        public SidebarRenderEvent SidebarRenderEvent { get; }
    }
}

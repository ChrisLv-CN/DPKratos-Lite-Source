using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PatcherYRpp;

namespace Extension.EventSystems
{
    public class TechnoTypeChangeEvent : EventBase
    {
        public override string Name => "TechnoClass_TypeChange";
        public override string Description => "Techno's type is changed";
    }
    public class TechnoTypeChangeEventArgs : EventArgs
    {
        public TechnoTypeChangeEventArgs(Pointer<TechnoClass> pTechno, bool isTransform)
        {
            this.pTechno = pTechno;
            this.IsTransform = isTransform;
        }

        public Pointer<TechnoClass> pTechno { get; }
        public bool IsTransform { get; }
        public bool IsReset => !IsTransform;
    }
    public class TechnoEventSystem : EventSystem
    {

        public TechnoEventSystem()
        {
            TypeChangeEvent = new TechnoTypeChangeEvent();
        }

        public TechnoTypeChangeEvent TypeChangeEvent { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.EventSystems
{
    public class ScenarioClearClassesEvent : EventBase
    {
        public override string Name => "ScenarioClearClasses";
        public override string Description => "Raised when scenario is cleaning classes";
    }
    public class ScenarioStartEvent : EventBase
    {
        public override string Name => "ScenarioStart";
        public override string Description => "Raised when scenario start";
    }

    public class LogicClassUpdateEvent : EventBase
    {
        public override string Name => "LogicClassUpdate";
        public override string Description => "Raised when LogicClass update";
    }

    public class LogicClassUpdateEventArgs : EventArgs
    {
        public LogicClassUpdateEventArgs(bool isBeginUpdate)
        {
            IsBeginUpdate = isBeginUpdate;
        }

        public bool IsBeginUpdate { get; }
        public bool IsLateUpdate => !IsBeginUpdate;
    }

    public class GeneralEventSystem : EventSystem
    {

        public GeneralEventSystem()
        {
            ScenarioClearClassesEvent = new ScenarioClearClassesEvent();
            ScenarioStartEvent = new ScenarioStartEvent();
            LogicClassUpdateEvent = new LogicClassUpdateEvent();
        }

        public ScenarioClearClassesEvent ScenarioClearClassesEvent { get; }
        public ScenarioStartEvent ScenarioStartEvent { get; }
        public LogicClassUpdateEvent LogicClassUpdateEvent { get; }
    }
}

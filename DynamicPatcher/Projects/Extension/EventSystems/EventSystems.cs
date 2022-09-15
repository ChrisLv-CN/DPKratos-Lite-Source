using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.EventSystems
{
    // Register Systems here


    public abstract partial class EventSystem
    {
        public static GeneralEventSystem General { get; }
        public static PointerExpireEventSystem PointerExpire { get; }
        public static SaveGameEventSystem SaveGame { get; }

        public static GScreenEventSystem GScreen { get; }


        private static event EventHandler OnClearTemporaryHandler;

        static EventSystem()
        {
            General = new GeneralEventSystem();
            General.AddPermanentHandler(General.ScenarioClearClassesEvent, (sender, e) =>
            {
                OnClearTemporaryHandler?.Invoke(sender, e);
            });
            PointerExpire = new PointerExpireEventSystem();
            SaveGame = new SaveGameEventSystem();
            GScreen = new GScreenEventSystem();
        }


        private void Register()
        {
            OnClearTemporaryHandler += ClearTemporaryHandler;
        }
        private void Unregister()
        {
            OnClearTemporaryHandler -= ClearTemporaryHandler;
        }
    }
}

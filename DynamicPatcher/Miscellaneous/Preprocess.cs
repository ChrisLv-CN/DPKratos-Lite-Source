using DynamicPatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Extension.Components;
using Extension.EventSystems;
using Extension.INI;
using Extension.Serialization;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Script;

namespace Miscellaneous
{
    [RunClassConstructorFirst]
    public class Preprocess
    {
        static Preprocess()
        {
            // add 500MB pressure
            GC.AddMemoryPressure(500 * 1024 * 1024);
            Logger.Log("Add 500MB pressure to GC.");

            RunSomeClassConstructor();
            AddEventSystemHandlers();
        }

        static void RunClassConstructor(Type type)
        {
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            Logger.Log("Run class constructor for {0}.", type.FullName);
        }

        static void RunSomeClassConstructor()
        {
            RunClassConstructor(typeof(MainSerializer));

            RunClassConstructor(typeof(INIConstant));
            RunClassConstructor(typeof(INIComponent));
        }

        static void AddEventSystemHandlers()
        {
            EventSystem.PointerExpire.AddPermanentHandler(EventSystem.PointerExpire.AnnounceExpiredPointerEvent, ObjectFinderHandler);

            EventSystem.SaveGame.AddPermanentHandler(EventSystem.SaveGame.LoadGameEvent, ScriptManagerHandler);
        }

        private static void ObjectFinderHandler(object sender, EventArgs e)
        {
            var args = (AnnounceExpiredPointerEventArgs)e;
            var pAbstract = args.ExpiredPointer;

            if (pAbstract.CastToObject(out Pointer<ObjectClass> pObject))
            {
                ObjectFinder.ObjectContainer.PointerExpired(pObject);
                if (pAbstract.CastToTechno(out var _))
                {
                    ObjectFinder.TechnoContainer.PointerExpired(pObject);
                }
            }

            //Logger.Log("invoke AnnounceExpiredPointer({0}, {1})", DebugUtilities.GetAbstractID(pAbstract), removed);
        }

        private static void ScriptManagerHandler(object sender, EventArgs e)
        {
            var args = (LoadGameEventArgs)e;

            if (args.IsStart)
            {
                RunClassConstructor(typeof(ScriptManager));
                EventSystem.SaveGame.RemovePermanentHandler(EventSystem.SaveGame.LoadGameEvent, ScriptManagerHandler);
            }
        }
    }
}

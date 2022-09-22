using Extension.EventSystems;
using Extension.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.INI
{
    public static class INIBufferPreloader
    {
        static INIBufferPreloader()
        {
            EventSystem.General.AddPermanentHandler(EventSystem.General.ScenarioStartEvent, (_, _) =>
            {
                Preload();
            });

            EventSystem.SaveGame.AddPermanentHandler(EventSystem.SaveGame.LoadGameEvent, (_, e) =>
            {
                var args = (LoadGameEventArgs)e;
                if (args.IsEnd)
                {
                    Preload();
                }
            });
        }

        private static void Preload()
        {
            Task.Delay(1000)
                .ContinueWith(_ => Preload(INIConstant.RulesName))
                .ContinueWith(_ => Preload(INIConstant.MapName))
                .ContinueWith(_ => Preload(INIConstant.ArtName))
                .ContinueWith(_ => Preload(INIConstant.AiName));
        }

        private static void Preload(string file)
        {
            DynamicPatcher.Logger.Log("INIBufferPreloader::preload {0}", file);
            INIComponentManager.FindFile(file);
        }
    }
}

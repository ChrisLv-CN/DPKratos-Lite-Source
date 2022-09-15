using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.EventSystems;
using Extension.Serialization;
using Extension.Utilities;
using PatcherYRpp;

namespace Extension.INI
{
    public static class INIConstant
    {
        public static string AiName { get; set; } = "aimd.ini";
        public static string ArtName { get; set; } = "artmd.ini";
        public static string RulesName { get; set; } = "rulesmd.ini";
        public static string Ra2md { get; set; } = "ra2md.ini";

        public static string GameModeName => _gameModeName;
        public static string MapName => ScenarioClass.Instance.FileName;

        static INIConstant()
        {
            EventSystem.General.AddPermanentHandler(EventSystem.General.ScenarioStartEvent, (_, _) =>
            {
                _gameModeName = MPGameModeClass.Instance.INIFilename;
            });

            EventSystem.SaveGame.AddPermanentHandler(EventSystem.SaveGame.SaveGameEvent, (_, e) =>
            {
                var args = (SaveGameEventArgs)e;
                if (args.IsStartInStream)
                {
                    args.Stream.WriteObject(_gameModeName);
                }
            });

            EventSystem.SaveGame.AddPermanentHandler(EventSystem.SaveGame.LoadGameEvent, (_, e) =>
            {
                var args = (LoadGameEventArgs)e;
                if (args.IsStartInStream)
                {
                    args.Stream.ReadObject(out _gameModeName);
                }
            });
        }

        private static string _gameModeName;
    }
}

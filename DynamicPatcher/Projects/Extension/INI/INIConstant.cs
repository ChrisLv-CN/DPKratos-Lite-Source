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
        public static string AiName => CCINIClass.INI_AI_FileName; //"aimd.ini";
        public static string ArtName => CCINIClass.INI_Art_FileName; //"artmd.ini";
        public static string RulesName => CCINIClass.INI_Ruels_FileName; //"rulesmd.ini";
        public static string Ra2md => "ra2md.ini";

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

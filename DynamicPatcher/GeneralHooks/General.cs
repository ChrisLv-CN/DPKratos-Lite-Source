using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Components;
using Extension.EventSystems;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;

namespace GeneralHooks
{
    public class General
    {
        static General()
        {
            EventSystem.General.AddPermanentHandler(EventSystem.General.ScenarioStartEvent, MathExHandler);
            // Kartos event
            EventSystem.General.AddPermanentHandler(EventSystem.General.ScenarioStartEvent, CombatDamage.Reload);
            EventSystem.General.AddPermanentHandler(EventSystem.General.ScenarioStartEvent, AudioVisual.Reload);
            EventSystem.General.AddPermanentHandler(EventSystem.General.ScenarioStartEvent, TechnoStatusScript.Clear);
            EventSystem.General.AddPermanentHandler(EventSystem.General.ScenarioStartEvent, BulletStatusScript.Clear);
            EventSystem.General.AddPermanentHandler(EventSystem.General.ScenarioStartEvent, PrintTextManager.Clear);
        }

        private static void MathExHandler(object sender, EventArgs e)
        {
            // ensure network synchronization
            int seed = ScenarioClass.Seed;
            Logger.Log($"Scenario start, set random seed = {seed}");
            MathEx.SetRandomSeed(seed);
            //Logger.Log("set random seed!");
        }

        [Hook(HookType.AresHook, Address = 0x52BA60, Size = 5)]
        public static unsafe UInt32 YR_Boot(REGISTERS* R)
        {

            return 0;
        }

        // in progress: Initializing Tactical display
        [Hook(HookType.AresHook, Address = 0x6875F3, Size = 6)]
        public static unsafe UInt32 Scenario_Start1(REGISTERS* R)
        {
            EventSystem.General.Broadcast(EventSystem.General.ScenarioStartEvent, EventArgs.Empty);

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x55AFB3, Size = 6)]
        public static unsafe UInt32 LogicClass_Update(REGISTERS* R)
        {
            EventSystem.General.Broadcast(EventSystem.General.LogicClassUpdateEvent, new LogicClassUpdateEventArgs(true));

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x55B719, Size = 5)]
        public static unsafe UInt32 LogicClass_Update_Late(REGISTERS* R)
        {
            EventSystem.General.Broadcast(EventSystem.General.LogicClassUpdateEvent, new LogicClassUpdateEventArgs(false));
            // foreach (Pointer<HouseClass> pHouse in HouseClass.Array)
            // {
            //     BaseClass baseClass = pHouse.Ref.Base;
            //     foreach (CellStruct cellStruct in baseClass.Cells_24)
            //     {
            //         if (MapClass.Instance.TryGetCellAt(cellStruct, out var pCell))
            //         {
            //             BulletEffectHelper.GreenCell(pCell.Ref.GetCoordsWithBridge(), 128, 1, 1, true);
            //         }
            //     }
            //     foreach (BaseNodeClass baseNode in baseClass.BaseNodes)
            //     {
            //         if (MapClass.Instance.TryGetCellAt(baseNode.MapCoords, out var pCell))
            //         {
            //             BulletEffectHelper.RedCell(pCell.Ref.GetCoordsWithBridge(), 128, 1, 1, true);
            //             BulletEffectHelper.RedLineZ(pCell.Ref.GetCoordsWithBridge(), 2048, 1, 1);
            //         }
            //     }
            // }

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x685659, Size = 0xA)]
        public static unsafe UInt32 Scenario_ClearClasses(REGISTERS* R)
        {
            EventSystem.General.Broadcast(EventSystem.General.ScenarioClearClassesEvent, EventArgs.Empty);

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x7CD8EF, Size = 9)]
        public static unsafe UInt32 ExeTerminate(REGISTERS* R)
        {

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x533066, Size = 6)]
        public static unsafe UInt32 CommandClassCallback_Register(REGISTERS* R)
        {
            Commands.MakeCommand<AttachEffect1Command>();
            Commands.MakeCommand<AttachEffect2Command>();
            Commands.MakeCommand<AttachEffect3Command>();
            Commands.MakeCommand<AttachEffect4Command>();
            Commands.MakeCommand<AttachEffect5Command>();
            Commands.MakeCommand<AttachEffect6Command>();
            Commands.MakeCommand<AttachEffect7Command>();
            Commands.MakeCommand<AttachEffect8Command>();
            Commands.MakeCommand<AttachEffect9Command>();
            return 0;
        }
    }
}

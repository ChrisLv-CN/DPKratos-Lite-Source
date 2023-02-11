using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class TechnoStatusScript
    {

        public State<ScatterData> ScatterState = new State<ScatterData>();

        private bool forceMoving;
        private bool panic;

        public void InitState_Scatter()
        {
            // 初始化状态机
            if (!isBuilding)
            {
                // 初始化状态机
                ScatterData data = Ini.GetConfig<ScatterData>(Ini.RulesDependency, section).Data;
                if (data.Enable)
                {
                    ScatterState.Enable(data);
                }
            }
        }

        public void OnUpdate_Scatter()
        {
            if (!isBuilding && ScatterState.IsActive())
            {
                if (ScatterState.IsReset())
                {
                    this.forceMoving = false;
                    this.panic = false;
                }
                ScatterData data = ScatterState.Data;
                this.panic = data.Panic;
                if (panic && pTechno.CastToInfantry(out Pointer<InfantryClass> pInfantry))
                {
                    if (pInfantry.Ref.PanicDurationLeft <= 200)
                    {
                        pInfantry.Ref.PanicDurationLeft = 300;
                    }
                }
                // 向目标移动，再散开
                CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                if (!forceMoving && default != data.MoveToFLH)
                {
                    forceMoving = true;
                    CoordStruct moveTo = pTechno.GetFLHAbsoluteCoords(data.MoveToFLH, false);
                    if (MapClass.Instance.TryGetCellAt(moveTo, out Pointer<CellClass> pTargetCell))
                    {
                        pTechno.Ref.SetDestination(pTargetCell);
                        pTechno.Convert<MissionClass>().Ref.QueueMission(Mission.Move, true);
                    }
                }
                if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
                {
                    pTechno.Ref.Base.Scatter(pCell.Ref.GetCoordsWithBridge(), true, false);
                }
            }
            else
            {
                this.forceMoving = false;
                this.panic = false;
            }
        }

    }
}

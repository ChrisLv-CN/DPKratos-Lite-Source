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


    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateAfter(typeof(TechnoStatusScript))]
    public class DeployFireOnceScript : TechnoScriptable
    {
        public DeployFireOnceScript(TechnoExt owner) : base(owner) { }

        public override void Awake()
        {
            if (!pTechno.CastToUnit(out Pointer<UnitClass> pUnit) || pUnit.Ref.Type.Ref.Base.DeployFire)
            {
                GameObject.RemoveComponent(this);
                return;
            }
        }

        public void UnitDeployFireOnce()
        {
            Pointer<MissionClass> pMission = pTechno.Convert<MissionClass>();
            if (pMission.Ref.CurrentMission == Mission.Unload)
            {
                pMission.Ref.QueueMission(Mission.Stop, true);
            }
        }
    }
}
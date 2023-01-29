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
    [UpdateAfter(typeof(AircraftAttitudeScript))]
    public class MissileHomingScript : TechnoScriptable
    {
        public MissileHomingScript(TechnoExt owner) : base(owner) { }

        public bool IsHoming;
        public CoordStruct HomingTargetLocation;

        private IConfigWrapper<MissileHomingData> _data;
        private MissileHomingData data
        {
            get
            {
                if (null == _data)
                {
                    _data = Ini.GetConfig<MissileHomingData>(Ini.RulesDependency, section);
                }
                return _data.Data;
            }
        }

        private bool initFlag;

        public override void Awake()
        {
            ILocomotion locomotion = null;
            if (!pTechno.Ref.Type.Ref.MissileSpawn || !pTechno.CastIf<AircraftClass>(AbstractType.Aircraft, out Pointer<AircraftClass> pAircraft)
                || (locomotion = pAircraft.Convert<FootClass>().Ref.Locomotor).ToLocomotionClass().Ref.GetClassID() != LocomotionClass.Rocket)
            {
                GameObject.RemoveComponent(this);
                return;
            }
            this.IsHoming = data.Homing;
        }

        public override void OnUpdate()
        {
            if (!initFlag)
            {
                initFlag = true;
                if (data.FacingTarget)
                {
                    CoordStruct sourcePos = pTechno.Ref.Base.Base.GetCoords();
                    CoordStruct targetPos = pTechno.Convert<FootClass>().Ref.Locomotor.Destination();
                    DirStruct dir = FLHHelper.Point2Dir(sourcePos, targetPos);
                    pTechno.Ref.Facing.set(dir);
                    pTechno.Ref.TurretFacing.set(dir);
                }
            }
            if (IsHoming && !pTechno.IsDeadOrInvisible())
            {
                Pointer<AbstractClass> pTarget = pTechno.Ref.Target;
                if (!pTarget.IsNull)
                {
                    if (pTarget.CastToTechno(out Pointer<TechnoClass> pTargetTechno))
                    {
                        // 如果目标消失，导弹会追到最后一个位置然后爆炸
                        if (!pTargetTechno.IsDeadOrInvisibleOrCloaked())
                        {
                            HomingTargetLocation = pTarget.Ref.GetCoords();
                            // Logger.Log($"{Game.CurrentFrame} 更新导弹 {OwnerObject} [{OwnerObject.Ref.Type.Ref.Base.Base.ID}] 目的地 {HomingTargetLocation}");
                        }
                    }
                }

                if (default != HomingTargetLocation)
                {
                    ILocomotion loco = pTechno.Convert<FootClass>().Ref.Locomotor;
                    Pointer<LocomotionClass> pLoco = loco.ToLocomotionClass();
                    if (LocomotionClass.Rocket == pLoco.Ref.GetClassID())
                    {
                        Pointer<RocketLocomotionClass> pRLoco = pLoco.Convert<RocketLocomotionClass>();
                        if (pRLoco.Ref.Timer34.Step > 2)
                        {
                            // Logger.Log($"{Game.CurrentFrame} 重设导弹 {OwnerObject} [{OwnerObject.Ref.Type.Ref.Base.Base.ID}] 目的地 {HomingTargetLocation}");
                            pRLoco.Ref.Destination = HomingTargetLocation;
                        }
                    }
                }
            }
        }
    }
}
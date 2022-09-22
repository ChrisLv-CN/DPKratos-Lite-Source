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
    public class AircraftAttitudeScript : TechnoScriptable
    {
        public AircraftAttitudeScript(TechnoExt owner) : base(owner) { }

        private static float angelStep = (float)Math.Atan2(20, Game.CellSize);

        public float PitchAngle;

        private float targetAngle;
        private bool smooth; // 平滑的改变角度
        private bool lockAngle; // 由其他地方算角度
        private TechnoStatusScript technoStatus => GameObject.GetComponent<TechnoStatusScript>();
        private CoordStruct lastLocation;

        public override void Awake()
        {
            ILocomotion locomotion = null;
            if (!pTechno.Ref.IsVoxel() || !pTechno.CastToFoot(out Pointer<FootClass> pFoot) || !pFoot.CastIf<AircraftClass>(AbstractType.Aircraft, out Pointer<AircraftClass> pAircraft)
                || (locomotion = pFoot.Ref.Locomotor).ToLocomotionClass().Ref.GetClassID() != LocomotionClass.Fly)
            {
                GameObject.RemoveComponent(this);
                return;
            }
            this.PitchAngle = 0f;
            this.smooth = true;
            this.lockAngle = false;
            this.lastLocation = pTechno.Ref.Base.Base.GetCoords();
        }

        public override void OnUpdate()
        {
            // 角度差比Step大
            if (smooth && PitchAngle != targetAngle && Math.Abs(targetAngle - PitchAngle) > angelStep)
            {
                // 只调整一个step
                if (targetAngle > PitchAngle)
                {
                    PitchAngle += angelStep;
                }
                else
                {
                    PitchAngle -= angelStep;
                }
            }
            else
            {
                PitchAngle = targetAngle;
            }
            // 关闭图像缓存
            technoStatus.DisableVoxelCache = PitchAngle != 0;
            // Logger.Log($"{Game.CurrentFrame} 飞机[{section}]{pTechno} 高度 {pTechno.Ref.Base.GetHeight()} 记录的倾斜角度 Angle = {PitchAngle} 位置 {pTechno.Ref.Base.Base.GetCoords()}");
            CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
            if (!lockAngle)
            {
                UpdateHeadToCoord(location);
            }
            lastLocation = location;
        }

        public override void OnLateUpdate()
        {
            // 子机在降落时调整鸡头的朝向
            Pointer<TechnoClass> pSpawnOwner = IntPtr.Zero;
            if (!(pSpawnOwner = pTechno.Ref.SpawnOwner).IsDeadOrInvisible() &&
                pTechno.Convert<FootClass>().Ref.Locomotor.ToLocomotionClass<FlyLocomotionClass>().Ref.IsLanding)
            {
                DirStruct dir = pSpawnOwner.Ref.Facing.current();
                pTechno.Ref.Facing.turn(dir);
                pTechno.Ref.TurretFacing.turn(dir);
            }
        }

        public void UnLock()
        {
            this.smooth = true;
            this.lockAngle = false;
        }

        public void UpdateHeadToCoord(CoordStruct headTo, bool lockAngle = false)
        {
            if (lockAngle)
            {
                this.smooth = false;
                this.lockAngle = true;
            }
            if (pTechno.IsDeadOrInvisibleOrCloaked())
            {
                PitchAngle = 0f;
            }
            else if (headTo != default)
            {
                Pointer<FlyLocomotionClass> pFly = pTechno.Convert<FootClass>().Ref.Locomotor.ToLocomotionClass<FlyLocomotionClass>();
                // 检查状态
                if (!pTechno.Ref.Base.Base.IsInAir() || pFly.Ref.IsTakingOff || pFly.Ref.IsLanding || !pFly.Ref.IsMoving)
                {
                    PitchAngle = 0f;
                }
                else
                {
                    // 计算俯仰角度
                    double dist = lastLocation.DistanceFrom(headTo);
                    double deltaZ = lastLocation.Z - headTo.Z;
                    double angle = Math.Asin(Math.Abs(deltaZ) / dist);
                    // Logger.Log($"{Game.CurrentFrame} 计算俯冲角度 {angle}， 距离 {dist}， 高度差 {deltaZ}， 正弦值 {deltaZ / dist} location {lastLocation} headTo = {headTo}");
                    int z = pTechno.Ref.Base.GetHeight() - pFly.Ref.FlightLevel;
                    if (z > 0)
                    {
                        // 俯冲
                        targetAngle = (float)angle;
                    }
                    else if (z < 0)
                    {
                        // 爬升
                        targetAngle = -(float)angle;
                    }
                    else
                    {
                        // 平飞
                        targetAngle = 0f;
                    }
                }
            }
        }
    }
}
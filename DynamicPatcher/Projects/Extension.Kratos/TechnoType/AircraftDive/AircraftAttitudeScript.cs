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

        private AircraftAttitudeData attitudeData => Ini.GetConfig<AircraftAttitudeData>(Ini.RulesDependency, section).Data;
        private bool disable; // 关闭俯仰姿态自动调整，但不会影响俯冲
        private float targetAngle;
        private bool smooth; // 平滑的改变角度
        private bool lockAngle; // 由其他地方算角度
        private TechnoStatusScript technoStatus => GameObject.GetComponent<TechnoStatusScript>();
        private CoordStruct lastLocation;

        public override void Awake()
        {
            ILocomotion locomotion = null;
            if (!pTechno.Ref.IsVoxel() || !pTechno.CastIf<AircraftClass>(AbstractType.Aircraft, out Pointer<AircraftClass> pAircraft)
                || (locomotion = pAircraft.Ref.Base.Locomotor).ToLocomotionClass().Ref.GetClassID() != LocomotionClass.Fly)
            {
                GameObject.RemoveComponent(this);
                return;
            }

            this.disable = attitudeData.Disable;
            this.PitchAngle = 0f;
            this.smooth = true;
            this.lockAngle = false;
            this.lastLocation = pTechno.Ref.Base.Base.GetCoords();
        }

        public override void OnUpdate()
        {
            if (!pTechno.IsDeadOrInvisible())
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
                if (!disable && !lockAngle)
                {
                    UpdateHeadToCoord(location);
                }
                lastLocation = location;
            }
        }

        public override void OnLateUpdate()
        {
            if (!pTechno.IsDeadOrInvisible())
            {
                // 子机在降落时调整鸡头的朝向
                Pointer<TechnoClass> pSpawnOwner = IntPtr.Zero;
                Pointer<FlyLocomotionClass> pFly = IntPtr.Zero;
                if (!(pSpawnOwner = pTechno.Ref.SpawnOwner).IsDeadOrInvisible()
                    && !(pFly = pTechno.Convert<FootClass>().Ref.Locomotor.ToLocomotionClass<FlyLocomotionClass>()).IsNull)
                {
                    int dir = 0;
                    if (pFly.Ref.IsLanding)
                    {
                        dir = attitudeData.SpawnLandDir;
                        // Logger.Log($"{Game.CurrentFrame} Landing dir {dir}");
                        DirStruct targetDir = GetAngle(dir, pSpawnOwner);
                        pTechno.Ref.Facing.turn(targetDir);
                        pTechno.Ref.TurretFacing.turn(targetDir);
                    }
                    else if (pFly.Ref.IsMoving && !pTechno.InAir() && pSpawnOwner.Ref.Type.Ref.RadialFireSegments <= 1)
                    {
                        switch (pTechno.Convert<MissionClass>().Ref.CurrentMission)
                        {
                            case Mission.Guard:
                            case Mission.Area_Guard:
                                dir = attitudeData.SpawnTakeoffDir;
                                // Logger.Log($"{Game.CurrentFrame} Takeoff dir {dir}");
                                DirStruct targetDir = GetAngle(dir, pSpawnOwner);
                                pTechno.Ref.Facing.set(targetDir);
                                pTechno.Ref.TurretFacing.set(targetDir);
                                break;
                        }
                    }
                }
            }
        }

        private DirStruct GetAngle(int dir, Pointer<TechnoClass> pSpawnOwner)
        {
            DirStruct targetDir = ExHelper.DirNormalized(dir, 16);
            double targetRad = targetDir.radians();
            DirStruct sourceDir = pSpawnOwner.Ref.Facing.current();
            double sourceRad = sourceDir.radians();
            float angle = (float)(sourceRad - targetRad);
            targetDir = ExHelper.Radians2Dir(angle);
            return targetDir;
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
                if (!pTechno.InAir() || pFly.Ref.IsTakingOff || pFly.Ref.IsLanding || !pFly.Ref.IsMoving)
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
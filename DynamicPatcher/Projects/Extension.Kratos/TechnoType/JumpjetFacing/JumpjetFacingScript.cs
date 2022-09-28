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
    public class JumpjetFacingScript : TechnoScriptable
    {

        public JumpjetFacingScript(TechnoExt owner) : base(owner) { }

        private JumpjetFacingData data => Ini.GetConfig<JumpjetFacingData>(Ini.RulesDependency, section).Data;

        private bool needToTurn;
        private DirStruct toDir;

        public override void Awake()
        {
            ILocomotion locomotion = null;
            if (!data.Enable
                || !pTechno.CastToFoot(out Pointer<FootClass> pFoot)
                || (locomotion = pFoot.Ref.Locomotor).ToLocomotionClass().Ref.GetClassID() != LocomotionClass.Jumpjet)
            {
                GameObject.RemoveComponent(this);
                return;
            }
        }

        public override void OnUpdate()
        {
            if (!pTechno.IsDeadOrInvisible())
            {
                if (pTechno.Convert<AbstractClass>().Ref.IsInAir())
                {
                    if (needToTurn)
                    {
                        Pointer<FootClass> pFoot = pTechno.Convert<FootClass>();
                        // Logger.Log("需要转向, 当前速度{0}, {1}", pFoot.Ref.GetCurrentSpeed(), pFoot.Ref.FacingChanging ? "正在转向" : "没有转向");
                        if (pFoot.Ref.GetCurrentSpeed() == 0)
                        {
                            Turning();
                            pFoot.Ref.StopMoving();
                            Pointer<JumpjetLocomotionClass> jjLoco = pFoot.Ref.Locomotor.ToLocomotionClass<JumpjetLocomotionClass>();
                            jjLoco.Ref.LocomotionFacing.turn(toDir); // Discovery by Trsdy
                        }
                        else
                        {
                            Cancel();
                            // Logger.Log("当前速度{0}, 没有停稳, 取消转向", pFoot.Ref.GetCurrentSpeed());
                        }
                    }
                    else if (!pTechno.Ref.Target.IsNull)
                    {
                        Pointer<AbstractClass> pTarget = pTechno.Ref.Target;
                        bool canFire = false;
                        int weaponIdx = pTechno.Ref.SelectWeapon(pTarget);
                        FireError fireError = pTechno.Ref.GetFireError(pTarget, weaponIdx, true);
                        switch (fireError)
                        {
                            case FireError.ILLEGAL:
                            case FireError.CANT:
                            case FireError.MOVING:
                            case FireError.RANGE:
                                break;
                            default:
                                canFire = pTechno.Ref.IsCloseEnough(pTarget, weaponIdx);
                                break;
                        }
                        if (canFire)
                        {
                            CoordStruct sourcePos = pTechno.Ref.Base.Location;
                            CoordStruct targetPos = pTechno.Ref.Target.Ref.GetCoords();
                            DirStruct toDir = ExHelper.Point2Dir(sourcePos, targetPos);
                            DirStruct selfDir = pTechno.Ref.Facing.current();
                            int facing = data.Facing;
                            int toIndex = ExHelper.Dir2FacingIndex(toDir, facing);
                            int selfIndex = ExHelper.Dir2FacingIndex(selfDir, facing);
                            if (selfIndex != toIndex)
                            {
                                DirStruct targetDir = ExHelper.DirNormalized(toIndex, facing);
                                TurnTo(targetDir);
                                // Logger.Log("需要转向, 当前朝向{0}, 目标朝向{1}", ExHelper.Dir2FacingIndex(selfDir, facing), ExHelper.Dir2FacingIndex(toDir, facing));
                            }
                            else
                            {
                                Cancel();
                                // Logger.Log("无需转向, 当前朝向{0}, 目标朝向{1}", ExHelper.Dir2FacingIndex(selfDir, facing), ExHelper.Dir2FacingIndex(toDir, facing));
                            }
                        }
                        else
                        {
                            Cancel();
                        }
                    }
                }
            }
        }

        private void TurnTo(DirStruct toDir)
        {
            this.needToTurn = true;
            this.toDir = toDir;
        }

        private void Turning()
        {
            this.needToTurn = false;
        }

        private void Cancel()
        {
            this.needToTurn = false;
        }

    }
}

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
    public class DestroyAnimsScript : TechnoScriptable
    {
        public DestroyAnimsScript(TechnoExt owner) : base(owner) { }

        public bool IAmWreak = false;

        private DestroyAnimsData data => Ini.GetConfig<DestroyAnimsData>(Ini.RulesDependency, section).Data;
        private SwizzleablePointer<HouseClass> pKillerHouse = new SwizzleablePointer<HouseClass>(IntPtr.Zero);

        public override void Awake()
        {
            this.IAmWreak = data.Wreck;
        }

        public override void OnUpdate()
        {
            // 被单位打死的残骸会自动恢复激活状态，强行再改回去，被弹头超武直接打死就不会
            if (IAmWreak && !pTechno.Ref.Deactivated)
            {
                pTechno.Ref.Deactivate();
                // Logger.Log($"{Game.CurrentFrame} 我{pTechno}是个残骸 Deactivated = {pTechno.Ref.Deactivated}");
            }
        }

        public unsafe bool PlayDestroyAnims()
        {
            if (!pTechno.Ref.Base.Base.IsInAir())
            {
                CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                if (!data.WreckType.IsNullOrEmptyOrNone())
                {
                    Pointer<HouseClass> pHouse = pTechno.Ref.Owner;
                    switch (data.WreckOwner)
                    {
                        case WreckOwner.KILLER:
                            pHouse = pKillerHouse;
                            if (pHouse.IsNull)
                            {
                                pHouse = HouseClass.FindSpecial();
                            }
                            break;
                        case WreckOwner.NEUTRAL:
                            pHouse = HouseClass.FindSpecial();
                            break;
                    }
                    // 生成单位替代死亡动画
                    Pointer<TechnoClass> pWreak = GiftBoxHelper.CreateAndPutTechno(data.WreckType, pHouse, location);
                    if (!pWreak.IsNull)
                    {
                        // 调整朝向
                        pWreak.Ref.Facing.set(pTechno.Ref.Facing.current());
                        pWreak.Ref.TurretFacing.set(pTechno.Ref.TurretFacing.current());
                        // 调整任务
                        pWreak.Convert<MissionClass>().Ref.QueueMission(data.WreckMission, true);
                        // // 不可选择
                        // pWreak.Ref.Deactivate();
                        // if (pWreak.TryGetComponent<DestroyAnimsScript>(out DestroyAnimsScript script))
                        // {
                        //     script.IAmWreak = true;
                        // }
                        return true;
                    }
                }
                else
                {
                    // 绘制动画
                    // Logger.Log("Techno IsAlive={0} IsActive={1}, IsOnMap={2}", pTechno.Ref.Base.IsAlive, pTechno.Ref.Base.IsActive(), pTechno.Ref.Base.IsOnMap);
                    string[] destroyAnims = data.DestroyAnims;
                    if (null != destroyAnims && destroyAnims.Length > 0)
                    {
                        int facing = destroyAnims.Length;
                        int index = 0;
                        if (!data.DestroyAnimsRandom && facing % 8 == 0)
                        {
                            // uint bits = (uint)Math.Round(Math.Sqrt(facing), MidpointRounding.AwayFromZero);
                            // double face = pTechno.Ref.GetRealFacing().target().GetValue(bits);
                            // double x = (face / (1 << (int)bits)) * facing;
                            // index = (int)Math.Round(x, MidpointRounding.AwayFromZero);
                            // Logger.Log("Index={0}/{1}, x={2}, bits={3}, face={4}, ", index, facing, x, bits, face);
                            index = ExHelper.Dir2FacingIndex(pTechno.Ref.Facing.current(), facing);
                            index = (int)(facing / 8) + index;
                            if (index >= facing)
                            {
                                index = 0;
                            }
                        }
                        else
                        {
                            index = MathEx.Random.Next(0, destroyAnims.Length - 1);
                            // Logger.Log("随机选择摧毁动画{0}/{1}", index, facing);
                        }
                        string animID = destroyAnims[index];
                        // Logger.Log("选择摧毁动画{0}/{1}[{2}], HouseClass={3}", index, facing, animID, pTechno.Ref.Owner.IsNull ? "Null" : pTechno.Ref.Owner.Ref.Type.Ref.Base.ID);
                        Pointer<AnimTypeClass> pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(animID);
                        if (!pAnimType.IsNull)
                        {
                            // Logger.Log("AnimType AltPalette={0}, MakeInf={1}, Next={2}", pAnimType.Ref.AltPalette, pAnimType.Ref.MakeInfantry, !pAnimType.Ref.Next.IsNull);
                            pAnimType.Ref.AltPalette = true;
                            Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, location);
                            // Logger.Log("Anim Owner={0}, IsPlaying={1}, PaletteName={2}, TintColor={3}, HouseColorIndex={4}", !pAnim.Ref.Owner.IsNull, pAnim.Ref.IsPlaying, pAnim.Ref.PaletteName, pAnim.Ref.TintColor, pHouse.Ref.ColorSchemeIndex);
                            pAnim.Ref.Owner = pTechno.Ref.Owner;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

    }
}
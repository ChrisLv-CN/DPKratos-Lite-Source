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
        public State<DestroyAnimData> DestroyAnimState = new State<DestroyAnimData>();
        private SwizzleablePointer<HouseClass> pKillerHouse = new SwizzleablePointer<HouseClass>(IntPtr.Zero);

        public void InitState_DestroyAnim()
        {
            // 初始化状态机
            DestroyAnimData data = Ini.GetConfig<DestroyAnimData>(Ini.RulesDependency, section).Data;
            if (data.Enable)
            {
                DestroyAnimState.Enable(data);
            }
        }

        public unsafe void OnReceiveDamage2_DestroyAnim(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            if (damageState == DamageState.NowDead)
            {
                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 被殴打致死，弹头{pWH.Ref.Base.ID}，攻击者{pAttacker}，攻击者所属{pAttackingHouse}");
                this.pKillerHouse.Pointer = pAttackingHouse;
                // 检查弹头上的AE，被弹头击杀时，弹头事件不会赋予AE，因此要单独附加
                AttachEffectTypeData aeTypeData = Ini.GetConfig<AttachEffectTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID).Data;
                if (null != aeTypeData.AttachEffectTypes && aeTypeData.AttachEffectTypes.Length > 0 && pTechno.TryGetAEManager(out AttachEffectScript aeManager))
                {
                    aeManager.Attach(aeTypeData, pAttacker, pAttackingHouse, true);
                }
                // 检查弹头上的状态，优先级最高
                DestroyAnimData data = Ini.GetConfig<DestroyAnimData>(Ini.RulesDependency, pWH.Ref.Base.ID).Data;
                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 被殴打致死，弹头{pWH.Ref.Base.ID} data.Enable = {data.Enable} {data.CanAffectType(pTechno)}");
                if (null != data && data.Enable && data.CanAffectType(pTechno))
                {
                    DestroyAnimState.Enable(data);
                }
            }
        }

        public unsafe bool PlayDestroyAnims()
        {
            if (DestroyAnimState.IsActive())
            {
                DestroyAnimData data = DestroyAnimState.Data;
                if (data.CanAffectType(pTechno))
                {
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 播放死亡动画，攻击者所属{pKillerHouse.Pointer}");
                    if (!data.WreckType.IsNullOrEmptyOrNone() && !pTechno.InAir())
                    {
                        CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                        Pointer<HouseClass> pHouse = pTechno.Ref.Owner;
                        switch (data.Owner)
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
                            return true;
                        }
                    }
                    else
                    {
                        // 绘制动画
                        // Logger.Log("Techno IsAlive={0} IsActive={1}, IsOnMap={2}", pTechno.Ref.Base.IsAlive, pTechno.Ref.Base.IsActive(), pTechno.Ref.Base.IsOnMap);
                        string[] destroyAnims = data.Anims;
                        if (null != destroyAnims && destroyAnims.Length > 0 && (data.PlayInAir || !pTechno.InAir()))
                        {
                            int facing = destroyAnims.Length;
                            int index = 0;
                            if (!data.Random && facing % 8 == 0)
                            {
                                // 0的方向是游戏中的北方，是↗，素材0帧是朝向0点，是↑
                                index = FLHHelper.Dir2FacingIndex(pTechno.Ref.Facing.current(), facing);
                                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 单位朝向{index}/{facing}");
                                index = (int)(facing / 8) + index;
                                if (index >= facing)
                                {
                                    index = 0;
                                }
                            }
                            else
                            {
                                index = MathEx.Random.Next(0, facing);
                                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 随机选择摧毁动画{index}/{facing}");
                            }
                            string animID = destroyAnims[index];
                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 选择摧毁动画{index}/{facing}[{animID}], HouseClass={(pTechno.Ref.Owner.IsNull ? "Null" : pTechno.Ref.Owner.Ref.Type.Ref.Base.ID)}");
                            Pointer<AnimTypeClass> pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(animID);
                            if (!pAnimType.IsNull)
                            {
                                CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                                // Logger.Log("AnimType AltPalette={0}, MakeInf={1}, Next={2}", pAnimType.Ref.AltPalette, pAnimType.Ref.MakeInfantry, !pAnimType.Ref.Next.IsNull);
                                pAnimType.Ref.AltPalette = true;
                                Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, location);
                                // Logger.Log("Anim Owner={0}, IsPlaying={1}, PaletteName={2}, TintColor={3}, HouseColorIndex={4}", !pAnim.Ref.Owner.IsNull, pAnim.Ref.IsPlaying, pAnim.Ref.PaletteName, pAnim.Ref.TintColor, pHouse.Ref.ColorSchemeIndex);
                                Pointer<HouseClass> pHouse = pTechno.Ref.Owner;
                                switch (data.Owner)
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
                                pAnim.Ref.Owner = pHouse;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

    }
}

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

        public DamageReactionState DamageReactionState = new DamageReactionState();

        public void InitState_DamageReaction()
        {
            // 初始化状态机
            DamageReactionData data = Ini.GetConfig<DamageReactionData>(Ini.RulesDependency, section).Data;
            if (data.Enable)
            {
                DamageReactionState.Enable(data);
            }
        }

        public void OnUpdate_DamageReaction()
        {
            DamageReactionState.Update(pTechno.Ref.Veterancy.IsElite());
        }

        public void OnReceiveDamage_DamageReaction(Pointer<int> pDamage, int distanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
                    Pointer<ObjectClass> pAttacker, bool ignoreDefenses, bool preventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {

            // 无视防御的真实伤害不做任何响应
            if (!ignoreDefenses)
            {
                if (DamageReactionState.Reaction(out DamageReactionEntity reactionData) && pWH.CanReaction(out DamageReactionMode[] ignoreModes))
                {
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 收到伤害 [{(ignoreModes == null ? "null" : string.Join(",", ignoreModes))}]");
                    int damage = pDamage.Data;
                    bool action = false;
                    switch (reactionData.Mode)
                    {
                        case DamageReactionMode.REDUCE:
                            action = null == ignoreModes || !ignoreModes.Contains(DamageReactionMode.REDUCE);
                            if (action)
                            {
                                // 调整伤害系数
                                pDamage.Ref = (int)(damage * reactionData.ReducePercent);
                                // Logger.Log($"{Game.CurrentFrame} {pTechno} {pTechno.Ref.Type.Ref.Base.Base.ID} 响应 伤害{damage} 调整伤害系数 {reactionData.ReducePercent}");
                            }
                            break;
                        case DamageReactionMode.FORTITUDE:
                            if (damage >= reactionData.MaxDamage)
                            {
                                action = null == ignoreModes || !ignoreModes.Contains(DamageReactionMode.FORTITUDE);
                                if (action)
                                {
                                    // 伤害大于阈值，降低为固定值
                                    pDamage.Ref = reactionData.MaxDamage;
                                }
                                // Logger.Log($"{Game.CurrentFrame} {pTechno} {pTechno.Ref.Type.Ref.Base.Base.ID} 响应 刚毅盾");
                            }
                            break;
                        case DamageReactionMode.PREVENT:
                            // 伤害大于血量，致死，消除伤害
                            // 计算实际伤害
                            int realDamage = pTechno.GetRealDamage(damage, pWH, ignoreDefenses, distanceFromEpicenter);
                            if (realDamage >= pTechno.Ref.Base.Health)
                            {
                                action = null == ignoreModes || !ignoreModes.Contains(DamageReactionMode.PREVENT);
                                if (action)
                                {
                                    // 回避致命伤害
                                    pDamage.Ref = 0;
                                    // Logger.Log($"{Game.CurrentFrame} {pTechno} {pTechno.Ref.Type.Ref.Base.Base.ID} 响应 免死");
                                }
                            }
                            break;
                        default:
                            action = null == ignoreModes || !ignoreModes.Contains(DamageReactionMode.EVASION);
                            // Logger.Log($"{Game.CurrentFrame} {pTechno} {pTechno.Ref.Type.Ref.Base.Base.ID} 响应 闪避");
                            if (action)
                            {
                                pDamage.Ref = 0; // 成功闪避，消除伤害
                            }
                            break;
                    }
                    if (action)
                    {
                        // Logger.Log($"{Game.CurrentFrame} 成功激活一次响应，模式{reactionData.Mode}，调整后伤害值{pDamage.Ref}, CanPlayAnim = {DamageReactionState.CanPlayAnim()}");
                        // 成功激活一次响应
                        DamageReactionState.ActionOnce();
                        // 附加AE
                        string[] triggeredAttachEffects = reactionData.TriggeredAttachEffects;
                        if (null != triggeredAttachEffects && triggeredAttachEffects.Any() && pTechno.TryGetAEManager(out AttachEffectScript aeManager))
                        {
                            if (reactionData.TriggeredAttachEffectsFromAttacker)
                            {
                                aeManager.Attach(triggeredAttachEffects, pAttacker, pAttackingHouse);
                            }
                            else
                            {
                                aeManager.Attach(triggeredAttachEffects);
                            }
                        }
                        // 播放响应动画
                        if (!string.IsNullOrEmpty(reactionData.Anim) && DamageReactionState.CanPlayAnim())
                        {
                            Pointer<AnimTypeClass> pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(reactionData.Anim);
                            if (!pAnimType.IsNull)
                            {
                                CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                                if (default != reactionData.AnimFLH)
                                {
                                    location = FLHHelper.GetFLHAbsoluteCoords(pTechno, reactionData.AnimFLH);
                                }
                                Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, location);
                                pAnim.Ref.SetOwnerObject(pTechno.Convert<ObjectClass>());
                                pAnim.SetAnimOwner(pTechno);
                                DamageReactionState.AnimPlay();
                            }
                        }
                        // 显示DamageText
                        if (reactionData.ActionText)
                        {
                            WarheadTypeExt whExt = WarheadTypeExt.ExtMap.Find(pWH);
                            if (!SkipDrawDamageText(pWH, out DamageTextData damageTextData))
                            {
                                DamageText data = null;
                                switch (reactionData.TextStyle)
                                {
                                    case DamageTextStyle.DAMAGE:
                                        data = damageTextData.Damage;
                                        break;
                                    case DamageTextStyle.REPAIR:
                                        data = damageTextData.Repair;
                                        break;
                                    default:
                                        if (pDamage.Ref >= 0)
                                        {
                                            data = damageTextData.Damage;
                                        }
                                        else
                                        {
                                            data = damageTextData.Repair;
                                        }
                                        break;
                                }
                                if (!data.Hidden)
                                {
                                    CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                                    DamageText temp = data.Clone();
                                    if (!string.IsNullOrEmpty(reactionData.CustomSHP))
                                    {
                                        // 自定义SHP
                                        temp.UseSHP = true;
                                        temp.SHPDrawStyle = SHPDrawStyle.TEXT; // 使用指定的帧
                                        temp.SHPFileName = reactionData.CustomSHP;
                                        temp.ZeroFrameIndex = reactionData.CustomSHPIndex;
                                        OrderDamageText("WWSB", location, temp);
                                        // Logger.Log($"{Game.CurrentFrame} 使用自定义SHP {reactionData.CustomSHP} {reactionData.CustomSHPIndex}");
                                    }
                                    else if (!string.IsNullOrEmpty(reactionData.CustomText))
                                    {
                                        // 自定义文字
                                        temp.UseSHP = false;
                                        OrderDamageText(reactionData.CustomText, location, temp);
                                        // Logger.Log($"{Game.CurrentFrame} 使用自定义文字 {reactionData.CustomText}");
                                    }
                                    else
                                    {
                                        // 使用默认设置
                                        OrderDamageText(reactionData.DefaultText.ToString(), location, temp);
                                        // Logger.Log($"{Game.CurrentFrame} 使用默认设置 {reactionData.DefaultText}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}

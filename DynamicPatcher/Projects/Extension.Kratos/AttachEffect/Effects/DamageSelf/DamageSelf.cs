using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class AttachEffect
    {
        public DamageSelf DamageSelf;

        private void InitDamageSelf()
        {
            this.DamageSelf = AEData.DamageSelfData.CreateEffect<DamageSelf>();
            RegisterEffect(DamageSelf);
        }
    }


    [Serializable]
    public class DamageSelf : Effect<DamageSelfData>
    {
        private SwizzleablePointer<WarheadTypeClass> pWH = new SwizzleablePointer<WarheadTypeClass>(IntPtr.Zero);
        private BulletDamageData bulletDamageData = new BulletDamageData(1);
        private TimerStruct ROFTimer;

        public override void OnEnable()
        {
            // 排除附着平民抛射体
            if (pOwner.CastToBullet(out Pointer<BulletClass> pBullet))
            {
                if (Data.DeactiveWhenCivilian && AE.pSourceHouse.IsCivilian())
                {
                    this.active = false;
                    return;
                }
            }
            // 伤害弹头
            pWH.Pointer = RulesClass.Instance.Ref.C4Warhead;
            if (!Data.Warhead.IsNullOrEmptyOrNone())
            {
                Pointer<WarheadTypeClass> pCustomWH = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.Warhead);
                if (!pCustomWH.IsNull)
                {
                    pWH.Pointer = pCustomWH;
                }
            }
            // 抛射体伤害
            bulletDamageData.Damage = Data.Damage;
            bulletDamageData.Eliminate = false; // 非一击必杀
            bulletDamageData.Harmless = false; // 非和平处置
        }

        public override void OnUpdate(CoordStruct location, bool isDead)
        {
            if (!active)
            {
                return;
            }
            if (isDead)
            {
                Disable(location);
                return;
            }
            if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                if (ROFTimer.Expired())
                {
                    // 排除平民
                    Pointer<HouseClass> pHouse = pTechno.Ref.Owner;
                    if (!Data.DeactiveWhenCivilian || !pHouse.IsCivilian())
                    {
                        ROFTimer.Start(Data.ROF);

                        int realDamage = Data.Damage;
                        if (Data.Peaceful)
                        {
                            // 静默击杀，需要计算实际伤害

                            // 计算实际伤害
                            realDamage = pTechno.GetRealDamage(realDamage, pWH, Data.IgnoreArmor);

                            if (realDamage >= pTechno.Ref.Base.Health)
                            {
                                // Logger.Log($"{Game.CurrentFrame} {pTechno}[{pTechno.Ref.Data.Ref.Base.Base.ID}] 收到自伤 {realDamage} 而死，设置了平静的移除");
                                // 本次伤害足够打死目标，移除单位
                                // pTechno.Ref.Base.Remove();
                                // pTechno.Ref.Base.UnInit();
                                // 设置DestroySelf来移除单位
                                pTechno.GetStatus().DestroySelfState.DestroyNow(true);
                                Disable(location);
                                return;
                            }
                        }

                        // 伤害的来源
                        Pointer<ObjectClass> pDamageMaker = IntPtr.Zero;
                        if (!AE.pSource.IsNull && AE.pSource != pTechno)
                        {
                            pDamageMaker = AE.pSource.Convert<ObjectClass>();
                        }

                        if (realDamage < 0 || pTechno.Ref.CloakStates == CloakStates.UnCloaked || Data.Decloak)
                        {
                            // 维修或者显形直接炸
                            pTechno.Ref.Base.ReceiveDamage(Data.Damage, 0, pWH, pDamageMaker, Data.IgnoreArmor, pTechno.Ref.Type.Ref.Crewed, AE.pSourceHouse);
                        }
                        else
                        {
                            // 不显形不能使用ReceiveDamage，改成直接扣血
                            if (!Data.Peaceful)
                            {
                                // 非静默击杀，实际伤害未计算过
                                realDamage = pTechno.GetRealDamage(realDamage, pWH, Data.IgnoreArmor);
                            }

                            // 扣血
                            if (realDamage >= pTechno.Ref.Base.Health)
                            {
                                // 本次伤害足够打死目标
                                pTechno.Ref.Base.ReceiveDamage(realDamage, 0, pWH, pDamageMaker, true, pTechno.Ref.Type.Ref.Crewed, AE.pSourceHouse);
                            }
                            else
                            {
                                // 血量可以减到负数不死
                                pTechno.Ref.Base.Health -= realDamage;
                            }
                        }

                        // 播放弹头动画
                        if (Data.WarheadAnim)
                        {
                            Pointer<AnimClass> pAnim = pWH.Pointer.PlayWarheadAnim(location, realDamage);
                            if (!pAnim.IsNull)
                            {
                                pAnim.Ref.Owner = AE.pSourceHouse;
                            }
                        }

                    }
                }
            }
            else if (pOwner.CastToBullet(out Pointer<BulletClass> pBullet))
            {
                if (ROFTimer.Expired())
                {
                    ROFTimer.Start(Data.ROF);

                    pBullet.GetStatus().TakeDamage(bulletDamageData);

                    // 播放弹头动画
                    if (Data.WarheadAnim)
                    {
                        Pointer<AnimClass> pAnim = pWH.Pointer.PlayWarheadAnim(location, bulletDamageData.Damage);
                        if (!pAnim.IsNull)
                        {
                            pAnim.Ref.Owner = AE.pSourceHouse;
                        }
                    }

                }
            }
            else
            {
                Disable(location);
            }

        }
    }
}

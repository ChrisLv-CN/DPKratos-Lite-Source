using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    // [UpdateAfter(typeof(TechnoStatusScript))]
    public class TechnoAntiBulletScript : TechnoScriptable
    {
        public TechnoAntiBulletScript(TechnoExt owner) : base(owner) { }

        private AntiBulletData data;
        public AntiBulletData Data
        {
            get
            {
                if (null == data)
                {
                    data = Ini.GetConfig<AntiBulletData>(Ini.RulesDependency, section).Data;
                }
                return data;
            }
        }

        private TimerStruct delayTimer;

        public override void Awake()
        {
            if (null == Data || !Data.Enable || NoPassengers() || WeaponNoAA())
            {
                // Logger.Log($"{Game.CurrentFrame} [{section}] 关闭 AntiMissile. {NoPassengers()} {PrimaryWeaponNoAA()}");
                Data.Enable = false;
                GameObject.RemoveComponent(this);
                return;
            }
        }

        private bool NoPassengers()
        {
            // 没有乘客位
            return Data.ForPassengers && pTechno.Ref.Type.Ref.Passengers <= 0;
        }

        private bool WeaponNoAA()
        {
            bool noAA = true;
            // 检查单位时候具有防空武器
            if (Data.Self)
            {
                Pointer<WeaponTypeClass> pPrimary = pTechno.Ref.Type.Ref.get_Primary();
                bool noPrimary = pPrimary.IsNull || pPrimary.Ref.Projectile.IsNull;
                Pointer<WeaponTypeClass> pSecondary = pTechno.Ref.Type.Ref.get_Secondary();
                bool noSecondary = pSecondary.IsNull || pSecondary.Ref.Projectile.IsNull;

                if (Data.Weapon == 0 && noPrimary)
                {
                    Logger.LogWarning($"{Game.CurrentFrame} Techno [{section}] has no Primary weapon, disable AntiMissile.");
                }
                else if (Data.Weapon == 1 && noSecondary)
                {
                    Logger.LogWarning($"{Game.CurrentFrame} Techno [{section}] has no Secondary weapon, disable AntiMissile.");
                }
                else if (noPrimary && noSecondary)
                {
                    Logger.LogWarning($"{Game.CurrentFrame} Techno [{section}] has no Any weapon, disable AntiMissile.");
                }
                else
                {
                    bool primaryAA = !noPrimary && pPrimary.Ref.Projectile.Ref.AA;
                    bool secondaryAA = !noSecondary && pSecondary.Ref.Projectile.Ref.AA;
                    if (Data.Weapon == 0 && !primaryAA)
                    {
                        Logger.LogWarning($"{Game.CurrentFrame} Techno [{section}] Primary weapon has no AA, disable AntiMissile.");
                    }
                    else if (Data.Weapon == 1 && !secondaryAA)
                    {
                        Logger.LogWarning($"{Game.CurrentFrame} Techno [{section}] Secondary weapon has no AA, disable AntiMissile.");
                    }
                    else if (!primaryAA && !secondaryAA)
                    {
                        Logger.LogWarning($"{Game.CurrentFrame} Techno [{section}] All weapon has no AA, disable AntiMissile.");
                    }
                    else
                    {
                        noAA = false;
                    }
                }
            }
            return noAA && !Data.ForPassengers;
        }

        public override void OnUpdate()
        {
            if (!pTechno.IsDeadOrInvisible())
            {
                if (Data.Enable)
                {
                    if (delayTimer.Expired())
                    {
                        int scanRange = Data.Range;
                        if (pTechno.Ref.Veterancy.IsElite())
                        {
                            scanRange = Data.EliteRange;
                        }

                        // Logger.Log($"{Game.CurrentFrame} 开始搜索抛射体，Range = {AntiBulletData.Range}，EliteRange = {AntiBulletData.EliteRange}，Rate = {AntiBulletData.Rate}");
                        CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                        // 搜索周围的所有的抛射体
                        BulletClass.Array.FindObject((pBullet) =>
                        {
                            if (pBullet.TryGetStatus(out BulletStatusScript bulletStatus) && bulletStatus.LifeData.Interceptable)
                            {
                                bool attackHouse = false;
                                if (Data.ScanAll)
                                {
                                    // 抛射体是否攻击自己阵营或者友军阵营
                                    Pointer<AbstractClass> pTarget = pBullet.Ref.Target;
                                    if (!pTarget.IsNull && pTarget.CastToTechno(out Pointer<TechnoClass> pBulletTarget))
                                    {
                                        Pointer<HouseClass> pBulletTargetHouse = pBulletTarget.Ref.Owner;
                                        attackHouse = pBulletTargetHouse == pTechno.Ref.Owner || pBulletTargetHouse.Ref.IsAlliedWith(pTechno.Ref.Owner);
                                    }
                                    else
                                    {
                                        // 抛射体的所属是敌人
                                        Pointer<HouseClass> pBulletHouse = pBullet.GetHouse();
                                        attackHouse = pBulletHouse.IsNull || !pBulletHouse.Ref.IsAlliedWith(pTechno.Ref.Owner);
                                    }
                                }
                                else if (pBullet.Ref.Target == pTechno.Convert<AbstractClass>())
                                {
                                    // 抛射体是否在攻击自己
                                    attackHouse = true;
                                }
                                if (attackHouse)
                                {
                                    // 确认目标
                                    // Logger.Log($"{Game.CurrentFrame} 确认目标 {pBullet}");
                                    delayTimer.Start(Data.Rate);
                                    if (Data.ForPassengers)
                                    {
                                        pTechno.Ref.SetTargetForPassengers(pBullet.Convert<AbstractClass>());
                                    }
                                    if (Data.Self && (pTechno.Ref.Target.IsNull || pTechno.Ref.Target.Ref.IsDead()))
                                    {
                                        pTechno.Ref.SetTarget(pBullet.Convert<AbstractClass>());
                                    }
                                    return true;
                                }
                            }
                            return false;
                        }, location, scanRange);

                    }
                }
            }
        }
    }
}

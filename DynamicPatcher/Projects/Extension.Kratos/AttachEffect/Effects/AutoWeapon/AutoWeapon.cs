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
        public AutoWeapon AutoWeapon;

        private void InitAutoWeapon()
        {
            this.AutoWeapon = AEData.AutoWeaponData.CreateEffect<AutoWeapon>();
            RegisterEffect(AutoWeapon);
        }
    }


    [Serializable]
    public class AutoWeapon : Effect<AutoWeaponData>
    {
        private Dictionary<string, TimerStruct> weaponsROF = new Dictionary<string, TimerStruct>();

        public override void OnUpdate(CoordStruct location, bool isDead)
        {
            if (!active)
            {
                return;
            }
            if (isDead)
            {
                Disable(AE.Location);
                return;
            }
            if (Data.Powered && AE.AEManager.PowerOff)
            {
                // 需要电力，但是建筑没电了
                return;
            }

            AutoWeaponEntity data = Data.Data;

            Pointer<TechnoClass> pReceiverOwner = IntPtr.Zero; // 附着的对象，如果是Bullet类型，则是Bullet的发射者
            Pointer<HouseClass> pReceiverHouse = IntPtr.Zero; // 附着的对象的所属
            Pointer<AbstractClass> pReceiverTarget = IntPtr.Zero; // 附着的对象当前的目标

            // 获取附属对象的所属，开火FLH，预设目标FLH
            bool isOnBullet = false;
            if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                pReceiverOwner = pTechno;
                pReceiverHouse = pReceiverOwner.Ref.Owner;
                pReceiverTarget = pReceiverOwner.Ref.Target;
                // 获取武器设置
                if (pReceiverOwner.Ref.Veterancy.IsElite())
                {
                    data = Data.EliteData;
                }
            }
            else if (pOwner.CastToBullet(out Pointer<BulletClass> pBullet))
            {
                isOnBullet = true;
                pReceiverOwner = pBullet.Ref.Owner;
                pReceiverHouse = pBullet.GetHouse();
                pReceiverTarget = pBullet.Ref.Target;
            }

            if (null == data || !(data.WeaponIndex > -1 || null != data.WeaponTypes))
            {
                // 没有可以使用的武器
                return;
            }

            // 获取武器设置
            int weaponIndex = data.WeaponIndex;
            string[] weaponTypes = data.WeaponTypes;
            int randomNum = data.RandomTypesNum;
            CoordStruct fireFLH = data.FireFLH;
            CoordStruct targetFLH = data.TargetFLH;

            bool attackerInvisible = AE.pSource.IsDead() || AE.pSource.Ref.IsImmobilized || AE.pSource.WhoIsShooter().IsDeadOrInvisible();
            bool bulletOwnerInvisible = isOnBullet && (pReceiverOwner.IsDeadOrInvisible() || pReceiverOwner.Ref.IsImmobilized || pReceiverOwner.WhoIsShooter().IsDeadOrInvisible());
            // Logger.Log($"{Game.CurrentFrame} - [{pOwner.Ref.Type.Ref.Base.ID}]{pOwner} ({(isDead ? "Dead" : "Alive")}) 上的 AutoFire, AE.pSource = {AE.pSource.Pointer} pSource.IsDead() = {AE.pSource.Pointer.IsDead()}, attackerInvisible = {attackerInvisible}, bulletOwnerInvisible = {bulletOwnerInvisible}");
            // 攻击者标记下，攻击者死亡或不存在，如果在抛射体上，而抛射体的发射者死亡或不存在，AE结束，没有启用标记，却设置了反向，同样结束AE
            if (Data.IsAttackerMark ? (attackerInvisible || bulletOwnerInvisible) : !Data.ReceiverAttack)
            {
                // if (Data.IsAttackerMark)
                // {
                //     Logger.LogWarning($"{Game.CurrentFrame} - [{pOwner.Ref.Type.Ref.Base.ID}]{pOwner} ({(isDead ? "Dead" : "Alive")}) 上的 AutoFire, 启用攻击者标记，攻击者死亡或不存在 attackerInvisible = {attackerInvisible}，如果在抛射体上，而抛射体的发射者死亡或不存在 bulletOwnerInvisible = {bulletOwnerInvisible}，结束AE");
                // }
                // else
                // {
                //     Logger.LogWarning($"{Game.CurrentFrame} - [{pOwner.Ref.Type.Ref.Base.ID}]{pOwner} ({(isDead ? "Dead" : "Alive")}) 上的 AutoFire, 未启用攻击者标记，设置了反向 ReceiverAttack = {Data.ReceiverAttack}，结束AE");
                // }
                Disable(AE.Location);
                return;
            }

            // 检查平民
            if (Data.DeactiveWhenCivilian && pReceiverHouse.IsCivilian())
            {
                // Logger.LogWarning($"{Game.CurrentFrame} - [{pOwner.Ref.Type.Ref.Base.ID}]{pOwner} ({(isDead ? "Dead" : "Alive")}) 上的 AutoFire, 检测到所属变成平民，停止AutoWeapon活动");
                return;
            }

            // 进入发射流程
            bool weaponLaunch = false;
            bool needFakeTarget = false;
            // 装订射击诸元
            if ((needFakeTarget = TryGetShooterAndTarget(pReceiverOwner, pReceiverHouse, pReceiverTarget,
                out Pointer<ObjectClass> pShooter, out Pointer<TechnoClass> pAttacker, out Pointer<HouseClass> pAttackingHouse, out Pointer<AbstractClass> pTarget,
                out bool dontMakeFakeTarget))
                && dontMakeFakeTarget
            )
            {
                // 目标为空，并且不构建假目标，发射终止
                // Logger.LogWarning($"{Game.CurrentFrame} - [{pOwner.Ref.Type.Ref.Base.ID}]{pOwner} ({(isDead ? "Dead" : "Alive")}) 上的 AutoFire, 检测到所目标不存在，且不构建假目标，发射终止");
                return;
            }
            if (pShooter.IsDeadOrInvisible())
            {
                // 发射武器的对象不存在，发射终止
                // Logger.LogWarning($"{Game.CurrentFrame} - [{pOwner.Ref.Type.Ref.Base.ID}]{pOwner} ({(isDead ? "Dead" : "Alive")}) 上的 AutoFire, 检测到发射武器不存在，发射终止");
                return;
            }
            // 发射武器是单位本身的武器还是自定义武器
            if (weaponIndex >= 0 && pShooter.CastToTechno(out Pointer<TechnoClass> pShooterTechno))
            {
                // 发射单位自身的武器
                // 获取发射单位的ROF加成
                double rofMult = pShooterTechno.GetROFMult();
                rofMult *= AE.AEManager.CountAttachStatusMultiplier().ROFMultiplier;
                // Logger.Log($"{Game.CurrentFrame} [{pShooter.Ref.Type.Ref.Base.Base.ID}] 即将向 [{pTarget.Convert<ObjectClass>().Ref.Type.Ref.Base.ID}] 发射自身的武器 {weaponIndex}");
                // 检查武器是否存在，是否ROF结束
                Pointer<WeaponStruct> pWeaponStruct = pShooterTechno.Ref.GetWeapon(weaponIndex);
                Pointer<WeaponTypeClass> pWeapon = IntPtr.Zero;
                if (!pWeaponStruct.IsNull && !(pWeapon = pWeaponStruct.Ref.WeaponType).IsNull && CheckROF(pWeapon))
                {
                    // 可以发射
                    // Logger.Log($"{Game.CurrentFrame} [{pShooter.Ref.Type.Ref.Base.Base.ID}] 向 [{pTarget.Convert<ObjectClass>().Ref.Type.Ref.Base.ID}] 发射自身的武器 {weaponIndex}");

                    if (needFakeTarget && !pReceiverHouse.IsNull)
                    {
                        // 需要创建假目标
                        pTarget = MakeFakeTarget(pReceiverHouse, pShooter, fireFLH, targetFLH);
                    }
                    if (!pTarget.IsNull)
                    {
                        // 发射武器
                        AttachFireScript attachFire = pShooter.FindOrAllocate<AttachFireScript>();
                        if (null != attachFire)
                        {
                            attachFire.FireCustomWeapon(pAttacker, pTarget, pAttackingHouse, pWeapon, pWeapon.GetData(), fireFLH);
                        }
                        weaponLaunch = true;
                        // 进入冷却
                        ResetROF(pWeapon, rofMult);
                    }
                }
            }
            else if (null != weaponTypes && weaponTypes.Length > 0)
            {
                // 发射自定义的武器
                // Logger.Log($"{Game.CurrentFrame} - [{pOwner.Ref.Type.Ref.Base.ID}]{pOwner} ({(isDead ? "Dead" : "Alive")}) 准备发射自动武器 [{string.Join(",", weaponTypes)}]");
                // 随机发射武器
                if (randomNum > 0)
                {
                    List<string> randomWeaponTypes = new List<string>();
                    int max = weaponTypes.Length;
                    for (int i = 0; i < randomNum; i++)
                    {
                        int index = MathEx.Random.Next(0, max);
                        randomWeaponTypes.Add(weaponTypes[index]);
                    }
                    weaponTypes = randomWeaponTypes.ToArray();
                }
                // 获取ROF加成
                double rofMult = 1.0f;
                if (!pAttacker.IsNull)
                {
                    rofMult = pAttacker.GetROFMult(); // 获取阵营和精英加成
                    rofMult *= AE.AEManager.CountAttachStatusMultiplier().ROFMultiplier; // 获取AE加成
                }
                // 正式发射武器
                foreach (string weaponId in weaponTypes)
                {

                    if (!string.IsNullOrEmpty(weaponId))
                    {
                        // 进行ROF检查
                        Pointer<WeaponTypeClass> pWeapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(weaponId);
                        if (!pWeapon.IsNull)
                        {
                            if (CheckROF(pWeapon) && !pReceiverHouse.IsNull)
                            {
                                // 可以发射
                                FireBulletToTarget callback = null;
                                if (needFakeTarget && !pReceiverHouse.IsNull)
                                {
                                    // 需要创建假目标
                                    pTarget = MakeFakeTarget(pReceiverHouse, pShooter, fireFLH, targetFLH);
                                    callback = FireBulletToTargetCallback;
                                }
                                if (!pTarget.IsNull)
                                {
                                    // Logger.Log($"{Game.CurrentFrame} - [{pShooter.Ref.Type.Ref.Base.ID}]{pShooter} 发射自动武器 [{weaponId}], 攻击者 [{(pAttacker.IsNull ? "Null" : pAttacker.Ref.Type.Ref.Base.Base.ID)}]{pAttacker}, 目标 [{(pTarget.CastToObject(out Pointer<ObjectClass> pTargetObject) ? pTarget.Ref.WhatAmI() : pTargetObject.Ref.Type.Ref.Base.ID)}]{pTarget}");
                                    // 发射自定义武器
                                    AttachFireScript attachFire = pShooter.FindOrAllocate<AttachFireScript>();
                                    if (null != attachFire)
                                    {
                                        attachFire.FireCustomWeapon(pAttacker, pTarget, pAttackingHouse, weaponId, fireFLH, !Data.IsOnTurret, Data.IsOnTarget, callback);
                                    }
                                    weaponLaunch = true;
                                    // 进入冷却
                                    ResetROF(pWeapon, rofMult);
                                }
                            }
                        }
                    }
                }
            }
            if (weaponLaunch && Data.FireOnce)
            {
                // 武器已成功发射，销毁AE
                Disable(AE.Location);
            }
        }

        private bool CheckROF(Pointer<WeaponTypeClass> pWeapon)
        {
            bool canFire = false;
            string weaponId = pWeapon.Ref.Base.ID;
            WeaponTypeData weaponData = pWeapon.GetData();
            // 进行ROF检查
            canFire = !weaponData.UseROF;
            if (!canFire)
            {
                if (weaponsROF.TryGetValue(weaponId, out TimerStruct rofTimer))
                {
                    if (rofTimer.Expired())
                    {
                        canFire = true;
                    }
                }
                else
                {
                    canFire = true;
                }
            }
            return canFire;
        }

        private void ResetROF(Pointer<WeaponTypeClass> pWeapon, double rofMult)
        {
            string weaponId = pWeapon.Ref.Base.ID;
            int rof = (int)(pWeapon.Ref.ROF * rofMult);
            if (weaponsROF.TryGetValue(weaponId, out TimerStruct rofTimer))
            {
                rofTimer.Start(rof);
                weaponsROF[weaponId] = rofTimer;
            }
            else
            {
                weaponsROF.Add(weaponId, new TimerStruct(rof));
            }
        }

        // 发射武器前设定攻击者和目标
        private bool TryGetShooterAndTarget(Pointer<TechnoClass> pReceiverOwner, Pointer<HouseClass> pReceiverHouse, Pointer<AbstractClass> pReceiverTarget,
            out Pointer<ObjectClass> pShooter, out Pointer<TechnoClass> pAttacker, out Pointer<HouseClass> pAttackingHouse, out Pointer<AbstractClass> pTarget,
            out bool dontMakeFakeTarget)
        {
            // 默认情况下，由标记持有者朝向预设位置开火
            pShooter = pOwner;
            pAttacker = pReceiverOwner;
            pAttackingHouse = pReceiverHouse;
            pTarget = IntPtr.Zero;
            dontMakeFakeTarget = false;

            // 更改射手
            if (!Data.ReceiverAttack)
            {
                // IsAttackerMark=yes时ReceiverAttack和ReceiverOwnBullet默认值为no
                // 若无显式修改，此时应为攻击者朝AE附属对象进行攻击
                // 由攻击者开火，朝向AE附属对象进行攻击
                pShooter = AE.pSource.Convert<ObjectClass>();
                pAttacker = AE.pSource;
                pAttackingHouse = AE.pSourceHouse;
                pTarget = pOwner.Convert<AbstractClass>();
                // Logger.Log($"{Game.CurrentFrame} 由单位 [{pShooter.Ref.Type.Ref.Base.ID}]{pShooter} 朝向持有者 [{pReceiver.Ref.Type.Ref.Base.ID}]{pReceiver} 发射武器, 武器属于攻击者 [{pAttacker.Ref.Type.Ref.Base.Base.ID}]{pAttacker}");
            }

            // 更改所属
            if (Data.ReceiverOwnBullet)
            {
                pAttacker = pReceiverOwner;
                pAttackingHouse = pReceiverHouse;
            }
            else
            {
                pAttacker = AE.pSource;
                pAttackingHouse = AE.pSourceHouse;
                // Logger.Log($"{Game.CurrentFrame} 武器所属变更为攻击者 [{pAttacker.Ref.Type.Ref.Base.Base.ID}]");
            }

            // 设定目标
            if (Data.IsAttackerMark)
            {
                // IsAttackerMark=yes时ReceiverAttack和ReceiverOwnBullet默认值为no
                // 若无显式修改，此时应为攻击者朝AE附属对象进行攻击
                // 只有显式修改 ReceiverAttack时，说明是由AE附属对象朝向攻击者攻击
                // 修改目标为攻击者
                if (Data.ReceiverAttack)
                {
                    pTarget = AE.pSource.Convert<AbstractClass>();
                    // Logger.Log($"{Game.CurrentFrame} 武器的目标变更为AE来源 [{AE.pSource.Ref.Type.Ref.Base.Base.ID}]");
                }
            }
            else if (Data.FireToTarget)
            {
                pTarget = pReceiverTarget;
                // Logger.Log($"{Game.CurrentFrame} 设定目标为附属对象的目标 [{(pTarget.IsNull ? "null" : pTarget.Ref.WhatAmI())}]");
                // 如果附属对象的目标不存在，此时应为无法开火，固定返回true不创建假目标
                dontMakeFakeTarget = true;
            }

            // 检查攻击目标是否藏在载具内，设定攻击目标为载具
            if (!pTarget.IsNull && pTarget.CastToTechno(out Pointer<TechnoClass> pTagretTechno))
            {
                pTarget = pTagretTechno.WhoIsShooter().Convert<AbstractClass>();
                // Logger.Log($"{Game.CurrentFrame} 设定的目标 若藏在载具内 查找到载具 [{pTarget.Convert<ObjectClass>().Ref.Type.Ref.Base.ID}] 设定为新的目标");
            }

            return pTarget.IsNull;
        }


        // 将假想敌设置在抛射体扩展上，以便在抛射体注销时销毁假想敌
        private bool FireBulletToTargetCallback(int index, int burst, Pointer<BulletClass> pBullet, Pointer<AbstractClass> pTarget)
        {
            if (pBullet.TryGetStatus(out BulletStatusScript bulletStatus))
            {
                bulletStatus.SetFakeTarget(pTarget.Convert<ObjectClass>());
            }
            return false;
        }

        // private void GetFireLocation(Pointer<ObjectClass> pShooter, CoordStruct fireFLH, Pointer<AbstractClass> pTarget, CoordStruct targetFLH, out CoordStruct forceFirePos, out CoordStruct fakeTargetPos)
        // {
        //     forceFirePos = default;
        //     fakeTargetPos = default;

        //     if (pShooter.CastToTechno(out Pointer<TechnoClass> pTechno))
        //     {
        //         // 武器从单位身上射出，按照单位获取开火坐标
        //         GetFireLocation(pTechno, fireFLH, targetFLH, out forceFirePos, out fakeTargetPos);
        //     }
        //     else if (pShooter.CastToBullet(out Pointer<BulletClass> pBullet))
        //     {
        //         // 武器从抛射体上射出，按照抛射体速度朝向获取开火坐标
        //         CoordStruct sourcePos = pBullet.Ref.Base.Base.GetCoords();
        //         DirStruct bulletDir = new DirStruct();
        //         if (!Data.IsOnWorld)
        //         {
        //             bulletDir = FLHHelper.Point2Dir(pBullet.Ref.SourceCoords, pBullet.Ref.TargetCoords);
        //         }
        //         // 增加抛射体偏移值取下一帧所在实际位置
        //         sourcePos += pBullet.Ref.Velocity.ToCoordStruct();
        //         forceFirePos = FLHHelper.GetFLHAbsoluteCoords(sourcePos, fireFLH, bulletDir);
        //         fakeTargetPos = FLHHelper.GetFLHAbsoluteCoords(sourcePos, targetFLH, bulletDir);
        //     }
        // }

        private Pointer<AbstractClass> MakeFakeTarget(Pointer<HouseClass> pHouse, Pointer<ObjectClass> pShooter, CoordStruct fireFLH, CoordStruct targetFLH)
        {
            CoordStruct targetPos = default;
            // 确定假想敌的位置
            if (Data.IsOnWorld)
            {
                // 绑定世界坐标，以射手为参考移动位置
                targetPos = FLHHelper.GetFLHAbsoluteCoords(pShooter.Ref.Base.GetCoords(), targetFLH, default);
            }
            else
            {
                // 以射手为参考获取相对位置
                targetPos = pShooter.GetFLHAbsoluteCoords(targetFLH, Data.IsOnTurret);
            }
            // 创建假想敌
            Pointer<OverlayTypeClass> pOverlayType = OverlayTypeClass.ABSTRACTTYPE_ARRAY.Array[0];
            if (!pOverlayType.IsNull)
            {
                Pointer<ObjectClass> pFakeTarget = pOverlayType.Ref.Base.CreateObject(pHouse);
                // Logger.Log("创建一个假想敌, {0}", pTarget.Ref.Base.WhatAmI());
                pFakeTarget.Ref.SetLocation(targetPos);
                pFakeTarget.Ref.InLimbo = false;
                pFakeTarget.Ref.IsVisible = false;
                return pFakeTarget.Convert<AbstractClass>();
            }
            return IntPtr.Zero;
        }

        public override void OnRemove()
        {
            Disable(AE.Location);
        }

        public override void OnReceiveDamageDestroy()
        {
            Disable(AE.Location);
        }

    }
}

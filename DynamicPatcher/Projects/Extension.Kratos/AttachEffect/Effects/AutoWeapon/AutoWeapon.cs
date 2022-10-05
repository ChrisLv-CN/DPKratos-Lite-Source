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

            AutoWeaponEntity data = Data.Data;

            double rofMult = 1;

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
                rofMult = pReceiverOwner.GetROFMult(); // ExHelper.GetROFMult(pReceiverOwner);
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

            // bool attackerInvisible = pAttacker.IsNull || pAttacker.Ref.Base.Health <= 0 || !pAttacker.Ref.Base.IsAlive || pAttacker.Ref.Base.InLimbo || pAttacker.Ref.IsImmobilized || !pAttacker.Ref.Transporter.IsNull;
            bool attackerInvisible = AE.pSource.Pointer.IsDeadOrInvisible() || AE.pSource.Ref.IsImmobilized || !AE.pSource.Ref.Transporter.IsNull;
            bool bulletOwnerInvisible = isOnBullet && (pReceiverOwner.IsDeadOrInvisible() || pReceiverOwner.Ref.IsImmobilized || !pReceiverOwner.Ref.Transporter.IsNull);
            // 攻击者标记下，攻击者死亡或不存在，如果在抛射体上，而抛射体的发射者死亡或不存在，AE结束，没有启用标记，却设置了反向，同样结束AE
            if (Data.IsAttackerMark ? (attackerInvisible || bulletOwnerInvisible) : !Data.ReceiverAttack)
            {
                Disable(AE.Location);
                return;
            }

            // 检查平民
            if (Data.DeactiveWhenCivilian && pReceiverHouse.IsCivilian())
            {
                return;
            }

            // 进入发射流程
            bool weaponLaunch = false;
            bool needFakeTarget = false;
            // 装订射击诸元
            if ((needFakeTarget = TryGetShooterAndTarget(pOwner, pReceiverOwner, pReceiverHouse, pReceiverTarget, out Pointer<TechnoClass> pShooter, out Pointer<TechnoClass> pWeaponOwner, out Pointer<AbstractClass> pTarget, out bool dontMakeFakeTarget)) && dontMakeFakeTarget)
            {
                // 目标为空，并且不构建假目标，发射终止
                return;
            }
            // 发射武器是单位本身的武器还是自定义武器
            if (weaponIndex >= 0)
            {
                // 发射单位自身的武器
                if (!pShooter.IsNull)
                {
                    // Logger.Log("{0}朝{1}发射自身的武器{2}", pShooter.Ref.Data.Ref.Base.Base.ID, pTarget.Ref.Data.Ref.Base.ID, weaponIndex);
                    // 检查武器是否存在，是否ROF结束
                    Pointer<WeaponStruct> pWeaponStruct = pShooter.Ref.GetWeapon(weaponIndex);
                    if (!pWeaponStruct.IsNull && !pWeaponStruct.Ref.WeaponType.IsNull && CheckROF(pWeaponStruct.Ref.WeaponType))
                    {
                        // 可以发射
                        // Logger.Log("{0}朝{1}发射自身的武器{2}", pShooter.Ref.Data.Ref.Base.Base.ID, pTarget.Ref.Data.Ref.Base.ID, weaponIndex);
                        // 准备发射，获取发射位置
                        GetFireLocation(pReceiverOwner, pShooter, fireFLH, pTarget, targetFLH, out CoordStruct forceFirePos, out CoordStruct fakeTargetPos);
                        if (needFakeTarget && !pReceiverHouse.IsNull && default != fakeTargetPos)
                        {
                            // 需要创建假目标
                            pTarget = MakeFakeTarget(pReceiverHouse, fakeTargetPos);
                        }
                        if (!pTarget.IsNull)
                        {
                            // 发射武器
                            if (pShooter.TryGetComponent<AttachFireScript>(out AttachFireScript attachFire))
                            {
                                attachFire.FireOwnWeapon(weaponIndex, pTarget);
                            }
                            weaponLaunch = true;
                            // 进入冷却
                            ResetROF(pWeaponStruct.Ref.WeaponType, rofMult);
                        }
                    }
                }
            }
            else if (null != weaponTypes && weaponTypes.Length > 0 && !pShooter.IsNull)
            {
                // 发射自定义的武器
                // 准备发射，获取发射位置
                GetFireLocation(pReceiverOwner, pShooter, fireFLH, pTarget, targetFLH, out CoordStruct forceFirePos, out CoordStruct fakeTargetPos);
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
                // 正式发射武器
                foreach (string weaponId in weaponTypes)
                {

                    if (!string.IsNullOrEmpty(weaponId) && !pReceiverOwner.IsNull)
                    {
                        // 进行ROF检查
                        Pointer<WeaponTypeClass> pWeapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(weaponId);
                        if (!pWeapon.IsNull)
                        {
                            if (CheckROF(pWeapon) && !pReceiverHouse.IsNull)
                            {
                                // 可以发射
                                FireBulletToTarget callback = null;
                                if (needFakeTarget && !pReceiverHouse.IsNull && default != fakeTargetPos)
                                {
                                    // 需要创建假目标
                                    pTarget = MakeFakeTarget(pReceiverHouse, fakeTargetPos);
                                    callback = FireBulletToTarget;
                                }
                                if (!pTarget.IsNull)
                                {
                                    // 发射自定义武器
                                    if (pShooter.TryGetComponent<AttachFireScript>(out AttachFireScript attachFire))
                                    {
                                        attachFire.FireCustomWeapon(pWeaponOwner, pTarget, pWeaponOwner.Ref.Owner, weaponId, fireFLH, rofMult, callback);
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
            WeaponTypeData weaponData = Ini.GetConfig<WeaponTypeData>(Ini.RulesDependency, pWeapon.Ref.Base.ID).Data;
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

        //TODO 攻击者标记工作不正常
        // 发射武器前设定攻击者和目标
        private bool TryGetShooterAndTarget(Pointer<ObjectClass> pReceiver, Pointer<TechnoClass> pReceiverOwner, Pointer<HouseClass> pReceiverHouse, Pointer<AbstractClass> pReceiverTarget, out Pointer<TechnoClass> pShooter, out Pointer<TechnoClass> pWeaponOwner, out Pointer<AbstractClass> pTarget, out bool dontMakeFakeTarget)
        {
            // 默认情况下，由标记持有者朝向预设位置开火
            pShooter = pReceiverOwner;
            pWeaponOwner = pReceiverOwner;
            pTarget = IntPtr.Zero;
            dontMakeFakeTarget = false;

            // 更改射手
            if (!Data.ReceiverAttack)
            {
                // IsAttackerMark=yes时ReceiverAttack和ReceiverOwnBullet默认值为no
                // 若无显式修改，此时应为攻击者朝AE附属对象进行攻击
                // 由攻击者开火，朝向AE附属对象进行攻击
                pShooter = AE.pSource;
                pWeaponOwner = AE.pSource;
                pTarget = pReceiver.Convert<AbstractClass>();
                // Logger.Log("由攻击者{0}朝向持有者{1}开火", pShooter.Ref.Data.Ref.Base.Base.ID, pTarget.Ref.Data.Ref.Base.ID);
            }

            // 更改所属
            if (Data.ReceiverOwnBullet)
            {
                pWeaponOwner = pReceiverOwner;
            }
            else
            {
                pWeaponOwner = AE.pSource;
                // Logger.Log("武器所属变更为攻击者{0}", pShooter.Ref.Data.Ref.Base.Base.ID);
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
                    pTarget = AE.pSource.Pointer.Convert<AbstractClass>();
                }
            }
            else if (Data.FireToTarget)
            {
                pTarget = pReceiverTarget;
                // Logger.Log("设定目标为附属对象的目标{0}", pTarget.IsNull ? "null" : pTarget.Ref.WhatAmI());
                // 如果附属对象的目标不存在，此时应为无法开火，固定返回true不创建假目标
                dontMakeFakeTarget = true;
            }
            return pTarget.IsNull;
        }


        // 将假想敌设置在抛射体扩展上，以便在抛射体注销时销毁假想敌
        private bool FireBulletToTarget(int index, int burst, Pointer<BulletClass> pBullet, Pointer<AbstractClass> pTarget)
        {
            if (pBullet.TryGetStatus(out BulletStatusScript bulletStatus))
            {
                bulletStatus.pFakeTarget.Pointer = pTarget.Convert<ObjectClass>();
            }
            return false;
        }

        private void GetFireLocation(Pointer<TechnoClass> pReceiverOwner, Pointer<TechnoClass> pShooter, CoordStruct fireFLH, Pointer<AbstractClass> pTarget, CoordStruct targetFLH, out CoordStruct forceFirePos, out CoordStruct fakeTargetPos)
        {
            forceFirePos = default;
            fakeTargetPos = default;
            CoordStruct sourcePos = pOwner.Ref.Base.GetCoords();

            if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                GetFireLocation(pShooter, sourcePos, fireFLH, targetFLH, ref forceFirePos, ref fakeTargetPos);
            }
            else if (pOwner.CastToBullet(out Pointer<BulletClass> pBullet))
            {
                // pReceiverOwner 是抛射体的发射者，pShooter也可能是抛射体的发射者
                if (pReceiverOwner == pShooter && pBullet.Convert<AbstractClass>() != pTarget)
                {
                    // 附加的对象是抛射体，默认武器的发射者为抛射体的发射者，从抛射体所在的位置向目标发射
                    DirStruct bulletDir = new DirStruct();
                    if (!Data.IsOnWorld)
                    {
                        bulletDir = ExHelper.Point2Dir(pBullet.Ref.SourceCoords, pBullet.Ref.TargetCoords);
                    }
                    // 增加抛射体偏移值取下一帧所在实际位置
                    sourcePos += pBullet.Ref.Velocity.ToCoordStruct();
                    forceFirePos = ExHelper.GetFLHAbsoluteCoords(sourcePos, fireFLH, bulletDir);
                    fakeTargetPos = ExHelper.GetFLHAbsoluteCoords(sourcePos, targetFLH, bulletDir);
                }
                else
                {
                    // 武器的发射者与抛射体的发射者不是同一个人，以发射者计算开火坐标和目标坐标
                    GetFireLocation(pShooter, sourcePos, fireFLH, targetFLH, ref forceFirePos, ref fakeTargetPos);
                }
            }
        }

        private void GetFireLocation(Pointer<TechnoClass> pShooter, CoordStruct sourcePos, CoordStruct fireFLH, CoordStruct targetFLH, ref CoordStruct forceFirePos, ref CoordStruct fakeTargetPos)
        {

            // 武器的发射者与抛射体的发射者不是同一个人，以发射者计算开火坐标和目标坐标
            if (Data.IsOnWorld)
            {
                // 绑定世界坐标
                DirStruct dir = new DirStruct();
                forceFirePos = ExHelper.GetFLHAbsoluteCoords(sourcePos, fireFLH, dir);
                fakeTargetPos = ExHelper.GetFLHAbsoluteCoords(sourcePos, targetFLH, dir);
            }
            else
            {
                // 绑定单位身上或炮塔
                fakeTargetPos = ExHelper.GetFLHAbsoluteCoords(pShooter, targetFLH, Data.IsOnTurret);
            }
        }

        private Pointer<AbstractClass> MakeFakeTarget(Pointer<HouseClass> pHouse, CoordStruct targetPos)
        {
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

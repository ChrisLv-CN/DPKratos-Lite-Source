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

    public partial class AnimStatusScript
    {
        private IConfigWrapper<AnimDamageData> _animDamageData;
        private AnimDamageData animDamageData
        {
            get
            {
                if (null == _animDamageData)
                {
                    _animDamageData = Ini.GetConfig<AnimDamageData>(Ini.ArtDependency, section);
                }
                return _animDamageData.Data;
            }
        }

        public TechnoExt Creater;
        private Pointer<TechnoClass> creater => null != Creater ? Creater.OwnerObject : default;
        private bool createrIsDeadth = false;

        private TimerStruct weaponDelay;
        private bool initDelayFlag = false;

        public void SetCreater(Pointer<TechnoClass> pTechno)
        {
            if (!pTechno.IsNull)
            {
                Creater = TechnoExt.ExtMap.Find(pTechno);
            }
        }

        public bool TryGetCreater(out Pointer<TechnoClass> pCreater)
        {
            pCreater = creater;
            return !pCreater.IsNull;
        }

        public void OnUpdate_Damage()
        {
            if (pAnim.IsNull)
            {
                return;
            }
            if (!initDelayFlag)
            {
                initDelayFlag = true;
                weaponDelay.Start(animDamageData.InitDelay);
            }
            if (!createrIsDeadth)
            {
                if (creater.IsNull)
                {
                    if (!(createrIsDeadth = !animDamageData.KillByCreater))
                    {
                        Pointer<TechnoClass> pCreater = IntPtr.Zero;
                        Pointer<ObjectClass> pOwner = pAttachOwner;
                        if (pOwner.IsNull)
                        {
                            pOwner = pAnim.Ref.OwnerObject;
                        }
                        if (!pOwner.IsNull)
                        {
                            if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
                            {
                                pCreater = pTechno;
                            }
                            else if (pOwner.CastToBullet(out Pointer<BulletClass> pBullet) && pBullet.TryGetStatus(out BulletStatusScript bulletStatus))
                            {
                                pCreater = bulletStatus.pSource;
                            }
                        }
                        if (!pCreater.IsDead())
                        {
                            Creater = TechnoExt.ExtMap.Find(pCreater);
                        }
                        else
                        {
                            createrIsDeadth = true;
                        }
                    }
                }
                else if (createrIsDeadth = creater.IsDead())
                {
                    Creater = null;
                }
            }
        }

        public void Explosion_Damage(bool isBounce = false, bool bright = false)
        {
            if (pAnim.IsNull)
            {
                return;
            }
            Pointer<AnimTypeClass> pAnimType = pAnim.Ref.Type;
            if (!pAnimType.IsNull)
            {
                CoordStruct location = pAnim.Ref.Base.Base.GetCoords();
                if (isBounce)
                {
                    location = pAnim.Ref.Bounce.GetCoords();
                }
                int damage = (int)pAnimType.Ref.Damage;
                if (damage != 0 || (!animDamageData.Weapon.IsNullOrEmptyOrNone() && animDamageData.UseWeaponDamage))
                {
                    // 制造伤害
                    string weaponType = animDamageData.Weapon;
                    Pointer<WarheadTypeClass> pWH = pAnimType.Ref.Warhead;
                    // 检查动画类型有没有写弹头
                    if (!weaponType.IsNullOrEmptyOrNone())
                    {
                        // 用武器
                        Pointer<WeaponTypeClass> pWeapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(weaponType);
                        if (!pWeapon.IsNull)
                        {
                            // 使用武器的伤害数值
                            if (animDamageData.UseWeaponDamage)
                            {
                                damage = pWeapon.Ref.Damage;
                            }
                            if (damage != 0)
                            {
                                if (weaponDelay.Expired())
                                {
                                    // Logger.Log($"{Game.CurrentFrame} - 动画 {pAnim} [{pAnimType.Ref.Base.Base.ID}] 用武器播放伤害 TypeDamage = {damage}, AnimDamage = {pAnim.Ref.Damage}, Weapon = {weaponType}");
                                    pWH = pWeapon.Ref.Warhead;
                                    bool isBright = bright || pWeapon.Ref.Bright; // 原游戏中弹头上的bright是无效的
                                    Pointer<BulletTypeClass> pBulletType = pWeapon.Ref.Projectile;
                                    Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(IntPtr.Zero, creater, damage, pWH, pWeapon.Ref.Speed, isBright);
                                    pBullet.Ref.WeaponType = pWeapon;
                                    pBullet.SetSourceHouse(pAnim.Ref.Owner);
                                    pBullet.Ref.Detonate(location);
                                    pBullet.Ref.Base.UnInit();
                                    weaponDelay.Start(animDamageData.Delay);
                                }
                            }
                        }
                    }
                    else if (!pWH.IsNull)
                    {
                        // 用弹头
                        if (weaponDelay.Expired())
                        {
                            // Logger.Log($"{Game.CurrentFrame} - 动画 {pAnim} [{pAnimType.Ref.Base.Base.ID}] 用弹头播放伤害 TypeDamage = {damage}, AnimDamage = {pAnim.Ref.Damage}, Warhead = {pAnimType.Ref.Warhead}");
                            MapClass.DamageArea(location, damage, creater, pWH, true, pAnim.Ref.Owner);
                            weaponDelay.Start(animDamageData.Delay);
                            if (bright)
                            {
                                MapClass.FlashbangWarheadAt(damage, pWH, location);
                            }
                            // 播放弹头动画
                            if (animDamageData.PlayWarheadAnim)
                            {
                                LandType landType = LandType.Clear;
                                if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
                                {
                                    landType = pCell.Ref.LandType;
                                }
                                Pointer<AnimTypeClass> pWHAnimType = MapClass.SelectDamageAnimation(damage, pWH, landType, location);
                                // Logger.Log($"{Game.CurrentFrame} - Anim {pAnim} [{pAnimType.Ref.Base.Base.ID}] play warhead's Anim [{(pWHAnimType.IsNull ? "null" : pWHAnimType.Ref.Base.Base.ID)}]");
                                if (!pWHAnimType.IsNull)
                                {
                                    Pointer<AnimClass> pWHAnim = YRMemory.Create<AnimClass>(pWHAnimType, location);
                                    pWHAnim.Ref.Owner = pAnim.Ref.Owner;
                                }
                            }
                        }
                    }

                }

            }
        }

    }
}

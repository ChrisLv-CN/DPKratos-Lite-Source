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

        private TechnoExt createrExt;
        private Pointer<TechnoClass> creater => null != createrExt ? createrExt.OwnerObject : default;
        private bool createrIsDeadth = false;

        private TimerStruct weaponDelay;
        private bool initDelayFlag = false;

        public void SetCreater(Pointer<TechnoClass> pTechno)
        {
            if (animDamageData.KillByCreater && !pTechno.IsNull)
            {
                createrExt = TechnoExt.ExtMap.Find(pTechno);
            }
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
                    if (animDamageData.KillByCreater && !pAnim.Ref.OwnerObject.IsNull && pAnim.Ref.OwnerObject.CastToTechno(out Pointer<TechnoClass> pTechno) && !pTechno.IsDead())
                    {
                        createrExt = TechnoExt.ExtMap.Find(pTechno);
                    }
                    else
                    {
                        createrIsDeadth = true;
                    }
                }
                else if (creater.IsDead())
                {
                    createrExt = null;
                    createrIsDeadth = true;
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
                    // ????????????
                    string weaponType = animDamageData.Weapon;
                    Pointer<WarheadTypeClass> pWH = pAnimType.Ref.Warhead;
                    // ????????????????????????????????????
                    if (!weaponType.IsNullOrEmptyOrNone())
                    {
                        // ?????????
                        Pointer<WeaponTypeClass> pWeapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(weaponType);
                        if (!pWeapon.IsNull)
                        {
                            // ???????????????????????????
                            if (animDamageData.UseWeaponDamage)
                            {
                                damage = pWeapon.Ref.Damage;
                            }
                            if (damage != 0)
                            {
                                if (weaponDelay.Expired())
                                {
                                    // Logger.Log($"{Game.CurrentFrame} - ?????? {pAnim} [{pAnimType.Ref.Base.Base.ID}] ????????????????????? TypeDamage = {damage}, AnimDamage = {pAnim.Ref.Damage}, Weapon = {weaponType}");
                                    pWH = pWeapon.Ref.Warhead;
                                    bool isBright = bright || pWeapon.Ref.Bright; // ????????????????????????bright????????????
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
                        // ?????????
                        if (weaponDelay.Expired())
                        {
                            // Logger.Log($"{Game.CurrentFrame} - ?????? {pAnim} [{pAnimType.Ref.Base.Base.ID}] ????????????????????? TypeDamage = {damage}, AnimDamage = {pAnim.Ref.Damage}, Warhead = {pAnimType.Ref.Warhead}");
                            MapClass.DamageArea(location, damage, creater, pWH, true, pAnim.Ref.Owner);
                            weaponDelay.Start(animDamageData.Delay);
                            if (bright)
                            {
                                MapClass.FlashbangWarheadAt(damage, pWH, location);
                            }
                            // ??????????????????
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

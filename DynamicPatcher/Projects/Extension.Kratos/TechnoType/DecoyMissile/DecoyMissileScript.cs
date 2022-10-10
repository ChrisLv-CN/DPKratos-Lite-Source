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
    public class DecoyMissileScript : TechnoScriptable
    {
        public DecoyMissileScript(TechnoExt owner) : base(owner) { }

        public DecoyMissile decoyMissile;

        public override void Awake()
        {
            DecoyMissileData data = Ini.GetConfig<DecoyMissileData>(Ini.RulesDependency, section).Data;
            if (!data.Enable)
            {
                GameObject.RemoveComponent(this);
                return;
            }
            Pointer<WeaponTypeClass> pWeapon = IntPtr.Zero;
            Pointer<WeaponTypeClass> pEliteWeapon = IntPtr.Zero;
            if (data.Weapon.IsNullOrEmptyOrNone())
            {
                pWeapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(data.Weapon);
                pEliteWeapon = pWeapon;
            }
            if (data.EliteWeapon.IsNullOrEmptyOrNone())
            {
                pEliteWeapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(data.EliteWeapon);
            }
            if (pWeapon.IsNull && pEliteWeapon.IsNull)
            {
                GameObject.RemoveComponent(this);
                return;
            }
            if (default == data.Velocity && pWeapon.Ref.Projectile.Ref.Arcing)
            {
                data.Velocity = data.FLH;
            }
            decoyMissile = new DecoyMissile(data, pWeapon, pEliteWeapon, pTechno.Ref.Veterancy.IsElite());
        }

        public override void OnUpdate()
        {
            if (null != decoyMissile && decoyMissile.Enable && !decoyMissile.Weapon.IsNull && !decoyMissile.EliteWeapon.IsNull)
            {
                CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                Pointer<HouseClass> pHouse = pTechno.Ref.Owner;
                Pointer<WeaponTypeClass> pWeapon = decoyMissile.FindWeapon(pTechno.Ref.Veterancy.IsElite());
                int distance = pWeapon.Ref.Range;
                // change decoy speed and target.
                for (int i = 0; i < decoyMissile.Decoys.Count; i++)
                {
                    // Check life count down;
                    DecoyBullet decoy = decoyMissile.Decoys[i];
                    if (decoy.IsNotDeath())
                    {
                        // Check distance to Change speed and target point
                        Pointer<BulletClass> pBullet = decoy.Bullet;
                        if (null != pBullet && !pBullet.IsNull)
                        {
                            int speed = pBullet.Ref.Speed - 5;
                            pBullet.Ref.Speed = speed < 10 ? 10 : speed;
                            if (speed > 10 && decoy.LaunchPort.DistanceFrom(pBullet.Ref.Base.Location) <= distance)
                            {
                                pBullet.Ref.Base.Location += new CoordStruct(0, 0, 64);
                            }

                        }
                    }
                    decoyMissile.Decoys[i] = decoy;
                }

                // remove dead decoy
                decoyMissile.ClearDecoy();

                // Fire decoy
                if (decoyMissile.Fire)
                {

                    if (decoyMissile.DropOne())
                    {
                        FacingStruct facing = pTechno.Ref.GetRealFacing();

                        CoordStruct flhL = decoyMissile.Data.FLH;
                        if (flhL.Y > 0)
                        {
                            flhL.Y = -flhL.Y;
                        }
                        CoordStruct flhR = decoyMissile.Data.FLH;
                        if (flhR.Y < 0)
                        {
                            flhR.Y = -flhR.Y;
                        }

                        CoordStruct portL = ExHelper.GetFLH(location, flhL, facing.target());
                        CoordStruct portR = ExHelper.GetFLH(location, flhR, facing.target());

                        CoordStruct targetFLHL = flhL + new CoordStruct(0, -distance * 2, 0);
                        CoordStruct targetFLHR = flhR + new CoordStruct(0, distance * 2, 0);
                        CoordStruct targetL = ExHelper.GetFLH(location, targetFLHL, facing.target());
                        CoordStruct targetR = ExHelper.GetFLH(location, targetFLHR, facing.target());

                        CoordStruct vL = decoyMissile.Data.Velocity;
                        if (vL.Y > 0)
                        {
                            vL.Y = -vL.Y;
                        }
                        vL.Z *= 2;
                        CoordStruct vR = decoyMissile.Data.Velocity;
                        if (vR.Y < 0)
                        {
                            vR.Y = -vR.Y;
                        }
                        vR.Z *= 2;
                        CoordStruct velocityL = ExHelper.GetFLH(new CoordStruct(), vL, facing.target());
                        CoordStruct velocityR = ExHelper.GetFLH(new CoordStruct(), vR, facing.target());

                        for (int i = 0; i < 2; i++)
                        {
                            CoordStruct initTarget = targetL;
                            CoordStruct port = portL;
                            BulletVelocity velocity = new BulletVelocity(velocityL.X, velocityL.Y, velocityL.Z);
                            if (i > 0)
                            {
                                initTarget = targetR;
                                port = portR;
                                velocity = new BulletVelocity(velocityR.X, velocityR.Y, velocityR.Z);
                            }
                            Pointer<CellClass> pCell = MapClass.Instance.GetCellAt(initTarget);
                            Pointer<BulletClass> pBullet = pWeapon.Ref.Projectile.Ref.CreateBullet(pCell.Convert<AbstractClass>(), pTechno, pWeapon.Ref.Damage, pWeapon.Ref.Warhead, pWeapon.Ref.Speed, pWeapon.Ref.Bright);
                            pBullet.Ref.WeaponType = pWeapon;
                            pBullet.Ref.MoveTo(port, velocity);
                            decoyMissile.AddDecoy(pBullet, port, decoyMissile.Data.Life);
                        }
                    }
                    ExHelper.FindBulletTargetMe(pTechno, (pBullet) =>
                    {
                        CoordStruct pos = pBullet.Ref.Base.Location;
                        Pointer<BulletClass> pDecoy = decoyMissile.CloseEnoughDecoy(pos, location.DistanceFrom(pos));
                        if (!pDecoy.IsDeadOrInvisible()
                            && pDecoy.Ref.Base.Location.DistanceFrom(pBullet.Ref.Base.Location) <= distance * 2)
                        {
                            pBullet.Ref.SetTarget(pDecoy.Convert<AbstractClass>());
                            return true;
                        }
                        return false;
                    });

                }
                else
                {
                    ExHelper.FindBulletTargetMe(pTechno, (pBullet) =>
                    {
                        if (location.DistanceFrom(pBullet.Ref.Base.Location) <= distance)
                        {
                            decoyMissile.Fire = true;
                            CoordStruct pos = pBullet.Ref.Base.Location;
                            Pointer<BulletClass> pDecoy = decoyMissile.CloseEnoughDecoy(pos, location.DistanceFrom(pos));
                            if (!pDecoy.IsDeadOrInvisible())
                            {
                                pBullet.Ref.SetTarget(pDecoy.Convert<AbstractClass>());
                            }
                            return true;
                        }
                        return false;
                    });

                }
            }
        }
    }
}
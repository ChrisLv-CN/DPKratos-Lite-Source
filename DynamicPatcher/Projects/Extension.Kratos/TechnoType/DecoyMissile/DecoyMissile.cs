using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class DecoyMissile
    {
        public bool Enable;

        // data
        public DecoyMissileData Data;

        public SwizzleablePointer<WeaponTypeClass> Weapon;
        public SwizzleablePointer<WeaponTypeClass> EliteWeapon;

        public SwizzleablePointer<WeaponTypeClass> UseWeapon;
        // public int Damage;
        // public int Burst;
        // public int ROF;
        // public int Distance;

        // control
        public int Delay;
        public int Bullets;
        public int Reloading;
        public bool Fire;

        public bool Elite;

        public List<BulletExt> Decoys;

        public DecoyMissile(DecoyMissileData data, Pointer<WeaponTypeClass> pWeapon, Pointer<WeaponTypeClass> pEliteWeapon, bool elite = false)
        {
            this.Enable = true;
            this.Data = data;
            this.Weapon = new SwizzleablePointer<WeaponTypeClass>(pWeapon);
            this.EliteWeapon = new SwizzleablePointer<WeaponTypeClass>(pEliteWeapon);
            if (elite)
            {
                this.UseWeapon = this.EliteWeapon;
            }
            else
            {
                this.UseWeapon = this.Weapon;
            }
            this.Delay = data.Delay;
            this.Bullets = UseWeapon.Ref.Burst;
            this.Reloading = 0;
            this.Fire = data.AlwaysFire;
            this.Elite = elite;
            Decoys = new List<BulletExt>();
        }

        public Pointer<WeaponTypeClass> FindWeapon(bool elite)
        {
            if (this.Elite != elite)
            {
                if (elite)
                {
                    this.UseWeapon = this.EliteWeapon;
                }
                else
                {
                    this.UseWeapon = this.Weapon;
                }
                this.Elite = elite;
            }
            return this.UseWeapon;
        }

        public bool DropOne()
        {
            if (--this.Reloading <= 0 && --this.Delay <= 0)
            {
                if (--this.Bullets >= 0)
                {
                    this.Delay = Data.Delay;
                    return true;
                }
                Reload();
            }
            return false;
        }

        public void Reload()
        {
            this.Bullets = this.UseWeapon.Ref.Burst;
            this.Reloading = this.UseWeapon.Ref.ROF;
            this.Fire = this.Data.AlwaysFire;
        }

        public void AddDecoy(Pointer<BulletClass> pDecoy, CoordStruct launchPos, int life)
        {
            if (null == Decoys)
            {
                Decoys = new List<BulletExt>();
            }
            BulletExt decoy = BulletExt.ExtMap.Find(pDecoy);
            if (null != decoy)
            {
                MissileTrajectoryScript script = decoy.GameObject.GetComponent<MissileTrajectoryScript>();
                if (null != script)
                {
                    script.IsDecoy = true;
                    script.LaunchPos = launchPos;
                    script.LifeTimer.Start(life);

                    Decoys.Add(decoy);
                }
            }
        }

        public void ClearDecoy()
        {
            Decoys.RemoveAll((deocy) =>
            {
                return null == deocy || deocy.OwnerObject.IsDeadOrInvisible();
            });
        }

        public Pointer<BulletClass> RandomDecoy()
        {
            int count = 0;
            if (null != Decoys && (count = Decoys.Count) > 0)
            {
                int ans = MathEx.Random.Next(count);
                BulletExt decoy = Decoys[ans == count ? ans - 1 : ans];
                Decoys.Remove(decoy);
                return decoy.OwnerObject;
            }
            return Pointer<BulletClass>.Zero;
        }

        public Pointer<BulletClass> CloseEnoughDecoy(CoordStruct pos, double min)
        {
            Pointer<BulletClass> pDecoy = IntPtr.Zero;
            double distance = min;
            for (int i = 0; i < Decoys.Count; i++)
            {
                BulletExt decoy = Decoys[i];
                Pointer<BulletClass> pBullet = IntPtr.Zero;
                if (null != decoy && !(pBullet = decoy.OwnerObject).IsDeadOrInvisible())
                {
                    double x = pos.DistanceFrom(pBullet.Ref.Base.Base.GetCoords());
                    if (x < distance)
                    {
                        distance = x;
                        pDecoy = pBullet;
                    }
                }
            }
            return pDecoy;
        }
    }

}

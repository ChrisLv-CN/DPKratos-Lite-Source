using DynamicPatcher;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Ext
{

    [Serializable]
    public class SimulateBurst
    {
        public SwizzleablePointer<WeaponTypeClass> pWeaponType; // 武器指针
        public TechnoExt AttackerExt;
        public Pointer<TechnoClass> pAttacker => null != AttackerExt ? AttackerExt.OwnerObject : default; // 武器的所属对象指针
        public SwizzleablePointer<AbstractClass> pTarget; // 目标指针
        public SwizzleablePointer<HouseClass> pAttackingHouse; // 武器所属阵营
        public CoordStruct FLH; // 发射的位置
        public FireBulletToTarget Callback; // 武器发射后回调函数

        // Burst控制参数
        public int Burst; // 总数
        public int MinRange; // 最近射程
        public int Range; // 射程

        public WeaponTypeData WeaponTypeData; // 武器控制

        public int FlipY; // 左右发射位点标签
        public int Index; // 当前发射的序号

        private TimerStruct timer;
        private int flag;

        public SimulateBurst(TechnoExt attackerExt, Pointer<AbstractClass> pTarget, Pointer<HouseClass> pAttackingHouse,
            Pointer<WeaponTypeClass> pWeaponType, CoordStruct flh,
            int burst, int minRange, int range, WeaponTypeData weaponTypeData, int flipY, FireBulletToTarget callback)
        {
            this.AttackerExt = attackerExt;
            this.pTarget = new SwizzleablePointer<AbstractClass>(pTarget);
            this.pAttackingHouse = new SwizzleablePointer<HouseClass>(pAttackingHouse);

            this.pWeaponType = new SwizzleablePointer<WeaponTypeClass>(pWeaponType);
            this.FLH = flh;

            this.Burst = burst;
            this.MinRange = minRange;
            this.Range = range;

            this.WeaponTypeData = weaponTypeData;

            this.FlipY = flipY;

            this.flag = flipY;
            this.Index = 0;

            this.Callback = callback;

            this.timer.Start(WeaponTypeData.SimulateBurstDelay);
        }

        public SimulateBurst Clone()
        {
            SimulateBurst newObj = new SimulateBurst(AttackerExt, pTarget, pAttackingHouse, pWeaponType, FLH, Burst, MinRange, Range, WeaponTypeData, FlipY, Callback);
            newObj.Index = Index;
            return newObj;
        }

        public bool CanFire()
        {
            if (timer.Expired())
            {
                timer.Start(WeaponTypeData.SimulateBurstDelay);
                return true;
            }
            return false;
        }

        public void CountOne()
        {
            Index++;
            switch (WeaponTypeData.SimulateBurstMode)
            {
                case 1:
                    // 左右切换
                    FlipY *= -1;
                    break;
                case 2:
                    // 左一半右一半
                    FlipY = (Index < Burst / 2f) ? flag : -flag;
                    break;
                default:
                    break;
            }
        }
    }

}

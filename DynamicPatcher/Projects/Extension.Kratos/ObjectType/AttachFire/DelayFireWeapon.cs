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
    public class DelayFireWeapon
    {
        public bool Invalid;

        public bool FireOwnWeapon;
        public int WeaponIndex;
        public SwizzleablePointer<WeaponTypeClass> pWeapon;
        public SwizzleablePointer<AbstractClass> pTarget;
        public CoordStruct FLH;
        public WeaponTypeData weaponTypeData;

        private int delay;
        private TimerStruct timer;
        private int count;

        public DelayFireWeapon(Pointer<AbstractClass> pTarget, int delay, int count)
        {
            this.pTarget = new SwizzleablePointer<AbstractClass>(pTarget);
            this.delay = delay;
            this.timer.Start(delay);
            this.count = count;
        }

        public DelayFireWeapon(int weaponIndex, Pointer<AbstractClass> pTarget, int delay = 0, int count = 1) : this(pTarget, delay, count)
        {
            this.FireOwnWeapon = true;
            this.WeaponIndex = weaponIndex;
            this.pWeapon = new SwizzleablePointer<WeaponTypeClass>(IntPtr.Zero);
        }

        public DelayFireWeapon(Pointer<WeaponTypeClass> pWeapon, WeaponTypeData weaponTypeData, Pointer<AbstractClass> pTarget, int delay = 0, int count = 1) : this(pTarget, delay, count)
        {
            this.FireOwnWeapon = false;
            this.WeaponIndex = -1;
            this.pWeapon = new SwizzleablePointer<WeaponTypeClass>(pWeapon);
            this.weaponTypeData = weaponTypeData;
        }

        public bool TimesUp()
        {
            return timer.Expired();
        }

        public void ReduceOnce()
        {
            count--;
            timer.Start(delay);
        }

        public bool NotDone()
        {
            return count > 0;
        }

    }

}

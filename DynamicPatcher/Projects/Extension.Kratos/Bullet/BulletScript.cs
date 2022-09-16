using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using System.Threading.Tasks;
using Extension.INI;

namespace Extension.Script
{

    [Serializable]
    [GlobalScriptable(typeof(BulletExt))]
    public class BulletScript : BulletScriptable
    {
        public BulletScript(BulletExt owner) : base(owner) { }


        public SwizzleablePointer<HouseClass> pHouse = new SwizzleablePointer<HouseClass>(HouseClass.FindSpecial());

        public override void Awake()
        {
            Logger.Log($"{Game.CurrentFrame} + Bullet 全局主程，记录下抛射体的所属");
            Pointer<TechnoClass> pShooter = Owner.OwnerObject.Ref.Owner;
            if (pShooter.IsNotNull && pShooter.Ref.Owner.IsNotNull)
            {
                pHouse.Pointer = pShooter.Ref.Owner;
            }

        }

    }
}
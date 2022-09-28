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

    public partial class BulletStatusScript
    {

        public BlackHoleState BlackHoleState = new BlackHoleState();

        public bool CaptureByBlackHole;

        public void Awake_BlackHole()
        {
            // 初始化状态机
            BlackHoleData BlackHoleData = Ini.GetConfig<BlackHoleData>(Ini.RulesDependency, section).Data;
            if (BlackHoleData.Enable)
            {
                BlackHoleState.Enable(BlackHoleData);
            }
        }

        public void OnUpdate_BlackHole()
        {
            // 被黑洞吸收中
            if (CaptureByBlackHole)
            {
                Pointer<AbstractClass> pTarget = pBullet.Ref.Target;
                if (pTarget.IsNull || (pTarget.CastToObject(out Pointer<ObjectClass> pObject) && pObject.BlackHoleStateDone()))
                {
                    // 目标不存在或者黑洞的黑洞效果关闭
                    CaptureByBlackHole = false;
                    // 如果不是导弹类型，Cancel其目标
                    if (pBullet.Ref.Type.Ref.Arcing || pBullet.Ref.Type.Ref.ROT <= 1)
                    {
                        pBullet.Ref.SetTarget(IntPtr.Zero);
                    }
                }
                else
                {
                    CoordStruct targetPos = pTarget.Ref.GetCoords();
                    // 重算向量，朝向目标
                    pBullet.RecalculateBulletVelocity(targetPos);
                }
            }
            // 黑洞吸人
            if (BlackHoleState.IsReady())
            {
                BlackHoleState.Capture(pBullet.Convert<ObjectClass>(), pSourceHouse);
            }
        }

        public void SetBlackHole(Pointer<ObjectClass> pBlackHole)
        {
            pBullet.Ref.SetTarget(pBlackHole.Convert<AbstractClass>());
            this.CaptureByBlackHole = true;
        }

    }
}

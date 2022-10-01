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

    public partial class BulletStatusScript : IBlackHoleVictim
    {

        public BlackHoleState BlackHoleState = new BlackHoleState();

        public bool CaptureByBlackHole;

        private SwizzleablePointer<ObjectClass> pBlackHole = new SwizzleablePointer<ObjectClass>(IntPtr.Zero);
        private BlackHoleData blackHoleData;
        private TimerStruct blackHoleDamageDelay;

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
            // 黑洞吸人
            if (BlackHoleState.IsReady())
            {
                BlackHoleState.StartCapture(pBullet.Convert<ObjectClass>(), pSourceHouse);
            }
            // 被黑洞吸收中
            if (CaptureByBlackHole)
            {
                if (pBlackHole.IsNull
                   || !pBlackHole.Pointer.TryGetBlackHoleState(out BlackHoleState blackHoleState)
                   || !blackHoleState.IsActive()
                   || OutOfBlackHole(blackHoleState))
                {
                    CancelBlackHole();
                }
                else
                {
                    CoordStruct sourcePos = pBullet.Ref.Base.Base.GetCoords();
                    CoordStruct targetPos = pBlackHole.Ref.Base.GetCoords();
                    // 获取偏移量
                    targetPos += blackHoleData.Offset;
                    // 获取偏移量
                    targetPos += blackHoleData.Offset;
                    // 获取捕获速度
                    int speed = blackHoleData.GetCaptureSpeed(1);
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 自身速度 {pTechno.Ref.Type.Ref.Speed} 捕获速度 {speed} 质量{pTechno.Ref.Type.Ref.Weight} 黑洞捕获速度 {blackHoleData.CaptureSpeed}");
                    CoordStruct nextPosFLH = new CoordStruct(speed, 0, 0);
                    DirStruct nextPosDir = ExHelper.Point2Dir(sourcePos, targetPos);
                    CoordStruct nextPos = ExHelper.GetFLHAbsoluteCoords(sourcePos, nextPosFLH, nextPosDir);
                    // 计算Z值
                    int deltaZ = sourcePos.Z - targetPos.Z;
                    if (deltaZ < 0)
                    {
                        // 目标点在上方
                        int offset = -deltaZ > 20 ? 20 : -deltaZ;
                        nextPos.Z += offset;
                    }
                    else if (deltaZ > 0)
                    {
                        // 目标点在下方
                        int offset = deltaZ > 20 ? 20 : deltaZ;
                        nextPos.Z -= offset;
                    }
                    // 被黑洞吸走
                    pBullet.Ref.Base.SetLocation(nextPos);
                }
                // 黑洞伤害
                if (null != blackHoleData && blackHoleData.AllowDamageBullet && blackHoleData.Damage != 0 && !BlackHoleState.IsActive())
                {
                    if (blackHoleDamageDelay.Expired())
                    {
                        blackHoleDamageDelay.Start(blackHoleData.DamageDelay);
                        // Logger.Log($"{Game.CurrentFrame} 黑洞对 [{section}]{pTechno} 造成伤害 准备中 Damage = {blackHoleData.Damage}, ROF = {blackHoleData.DamageDelay}, WH = {blackHoleData.DamageWH}");
                        TakeDamage(blackHoleData.Damage, false, false, true);
                    }
                }
            }
        }

        public void SetBlackHole(Pointer<ObjectClass> pBlackHole, BlackHoleData blackHoleData)
        {
            this.CaptureByBlackHole = true;
            this.pBlackHole.Pointer = pBlackHole;
            this.blackHoleData = blackHoleData;
        }

        public void CancelBlackHole()
        {
            if (CaptureByBlackHole && !pBullet.IsDeadOrInvisible() && !LifeData.IsDetonate)
            {
                // Arcing摔地上，导弹不管
            }
            this.CaptureByBlackHole = false;
            this.blackHoleData = null;
            this.pBlackHole.Pointer = IntPtr.Zero;
        }

        private bool OutOfBlackHole(BlackHoleState blackHoleState)
        {
            CoordStruct taregtPos = pBlackHole.Ref.Base.GetCoords();
            CoordStruct sourcePos = pBullet.Ref.Base.Base.GetCoords();
            return blackHoleState.IsOutOfRange(taregtPos.DistanceFrom(sourcePos));
        }

    }
}

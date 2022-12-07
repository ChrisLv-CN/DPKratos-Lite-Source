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
    /// <summary>
    /// AE的行为与脚本一致，区别在于不作为Component挂载，用外部脚本调用相同的事件
    /// </summary>
    [Serializable]
    public partial class AttachEffect : IEffectAction
    {

        public AttachEffectData AEData;

        public AttachEffectScript AEManager;

        public Pointer<ObjectClass> pOwner => AEManager.pOwner; // AE附着对象

        public TechnoExt SourceExt; // AE来源
        public Pointer<TechnoClass> pSource => null != SourceExt ? SourceExt.OwnerObject : default;
        public HouseExt HouseExt; // AE来源所属
        public Pointer<HouseClass> pSourceHouse => null != HouseExt ? HouseExt.OwnerObject : default;

        public CoordStruct WarheadLocation;
        public bool FromWarhead;

        public int AEMode;
        public bool FromPassenger;

        public CoordStruct Location;

        public bool NonInheritable;

        private bool active;
        private int duration; // 寿命
        private bool immortal; // 永生
        private TimerStruct lifeTimer;
        private TimerStruct initialDelayTimer;
        private bool delayToEnable; // 延迟激活中

        private List<IEffect> effects = new List<IEffect>();

        public AttachEffect(AttachEffectData data)
        {
            this.AEData = data;

            int initDelay = AEData.InitialRandomDelay.GetRandomValue(AEData.InitialDelay);
            this.delayToEnable = initDelay > 0;
            if (delayToEnable)
            {
                this.initialDelayTimer.Start(initDelay);
            }
            this.duration = AEData.Duration;
            this.immortal = AEData.HoldDuration;

            //TODO 增加新的AE

            InitStand(); // 替身需要第一个初始化
            InitDestroySelf(); // state AffectWho

            InitAnimation();
            InitAttackBeacon(); // state AffectWho
            InitAutoWeapon();
            InitBlackHole(); // state AffectWho
            InitBounce();
            InitBroadcast();
            InitCrateBuff(); // state AffectWho
            InitDamageReaction();
            InitDamageSelf(); // state AffectWho
            InitDeselect(); // state AffectWho
            InitDestroyAnim();
            InitDisableWeapon(); // state AffectWho
            InitExtraFire(); // state AffectWho
            InitFireSuper(); // state AffectWho
            InitGiftBox(); // state AffectWho
            InitMark();
            InitPaintball(); // always same JoJo and Stand
            InitPump();
            InitRevenge(); // state AffectWho
            InitScatter();
            InitStack();
            InitTeleport();
            InitTransform();
            InitOverrideWeapon(); // state AffectWho
        }

        public void SetupLifeTimer()
        {
            if (!this.immortal)
            {
                this.lifeTimer.Start(this.duration);
            }
        }

        public void RegisterEffect(IEffect effect)
        {
            if (null != effect)
            {
                // Logger.Log($"{Game.CurrentFrame}, 注册AE系统 {Type.Name}, 模块 {behaviour.GetType().Name}");
                effects.Add(effect);
            }
        }

        public void Enable(AttachEffectScript AEManager, Pointer<TechnoClass> pSource, Pointer<HouseClass> pSourceHouse, CoordStruct warheadLocation, int aeMode, bool fromPassenger)
        {
            this.active = true;
            this.AEManager = AEManager;
            this.SourceExt = TechnoExt.ExtMap.Find(pSource);
            this.HouseExt = HouseExt.ExtMap.Find(pSourceHouse);
            this.WarheadLocation = warheadLocation;
            this.FromWarhead = default != WarheadLocation;
            this.AEMode = aeMode;
            this.FromPassenger = fromPassenger;
            if (!delayToEnable || initialDelayTimer.Expired())
            {

                EnableEffects();
            }
        }

        private void EnableEffects()
        {
            delayToEnable = false;
            SetupLifeTimer();
            foreach (IEffect effect in effects)
            {
                effect?.Enable(this);
            }
        }

        public void Enable(AttachEffect ae) { }

        public void Disable(CoordStruct location)
        {
            this.active = false;
            if (delayToEnable)
            {
                return;
            }
            foreach (IEffect effect in effects)
            {
                effect?.Disable(location);
            }
        }

        public bool IsActive()
        {
            if (active)
            {
                // Logger.Log("AE Type {0} {1} and {2}", Type.Name, IsDeath() ? "is death" : "not dead", IsAlive() ? "is alive" : "not alive");
                active = delayToEnable || (!IsDeath() && IsAlive());
            }
            return active;
        }

        public bool IsAnyAlive()
        {
            foreach (IEffect effect in effects)
            {
                if (effect.IsAlive())
                {
                    // Logger.Log($"{Game.CurrentFrame} - [{pOwner.Ref.Type.Ref.Base.ID}]{pOwner} 上的 AE [{AEData.Name}] 模块 {effect.GetType().Name} 狗带了");
                    return true;
                }
            }
            return false;
        }

        public bool IsAlive()
        {
            if (FromPassenger)
            {
                if (pSource.IsDead() || pSource.Ref.Transporter.IsNull)
                {
                    // Logger.Log($"{Game.CurrentFrame} [{pOwner.Ref.Type.Ref.Base.ID}]{pOwner} 上的 AE 来自乘客{pSource}，乘客下车了，结束所有AE");
                    return false;
                }
            }
            if (AEMode >= 0)
            {
                List<int> passengerIds = AEManager.PassengerIds;
                if (null == passengerIds || !passengerIds.Any() || !passengerIds.Contains(AEMode))
                {
                    // Logger.Log($"{Game.CurrentFrame} [{pOwner.Ref.Type.Ref.Base.ID}]{pOwner} 上的 AEMode = {AEMode}，乘客[{(null != passengerIds ? string.Join(",", passengerIds) : "null")}]下车了，结束所有AE");
                    return false;
                }
            }
            foreach (IEffect effect in effects)
            {
                if (!effect.IsAlive())
                {
                    // Logger.Log($"{Game.CurrentFrame} - [{pOwner.Ref.Type.Ref.Base.ID}]{pOwner.Pointer} 上的 AE [{AEData.Name}] 模块 {effect.GetType().Name} 狗带了");
                    return false;
                }
            }
            return true;
        }

        private bool IsDeath()
        {
            // Logger.Log("AE Type {0} duration {1} time left {2}", Type.Name, duration, lifeTimer.TimeLeft);
            return !immortal && lifeTimer.Expired();
        }

        private void ForceStartLifeTimer(int timeLeft)
        {
            this.immortal = false;
            this.lifeTimer.Start(timeLeft);
            // Logger.Log("启动{0}生命计时器，生命{1}，计时{2}", Name, duration, timeLeft);
        }

        public bool IsSameGroup(AttachEffectData otherType)
        {
            return this.AEData.Group > -1 && otherType.Group > -1 && this.AEData.Group == otherType.Group;
        }

        public void MergeDuation(int otherDuration)
        {
            if (delayToEnable || otherDuration == 0)
            {
                // Logger.Log("{0}延迟激活中，不接受时延修改", Name);
                return;
            }
            // 重设时间
            if (otherDuration < 0)
            {
                // 剩余时间
                int timeLeft = immortal ? this.duration : lifeTimer.GetTimeLeft();
                // 削减生命总长
                this.duration += otherDuration;
                if (this.duration <= 0 || timeLeft <= 0)
                {
                    // 削减的时间超过总长度，直接减没了
                    this.active = false;
                }
                else
                {
                    // Logger.Log("削减{0}持续时间{1}，{2}生命{3}，当前剩余{4}", Name, otherDuration, this.immortal ? "无限" : "", this.duration, timeLeft);
                    timeLeft += otherDuration;
                    if (timeLeft <= 0)
                    {
                        // 削减完后彻底没了
                        this.active = false;
                    }
                    else
                    {
                        // 还有剩
                        // 重设时间
                        ForceStartLifeTimer(timeLeft);
                    }
                }
            }
            else
            {
                // 累加持续时间
                this.duration += otherDuration;
                if (!immortal)
                {
                    int timeLeft = lifeTimer.GetTimeLeft();
                    // Logger.Log("增加{0}持续时间{1}，当前剩余{2}", Name, otherDuration, timeLeft);
                    timeLeft += otherDuration;
                    ForceStartLifeTimer(timeLeft);
                }
            }
        }

        public void ResetDuration()
        {
            SetupLifeTimer();

            foreach (IEffect effect in effects)
            {
                effect?.ResetDuration();
            }
        }

        public void OnRender(CoordStruct location)
        {
            if (delayToEnable)
            {
                return;
            }

            foreach (IEffect effect in effects)
            {
                effect?.OnRender(location);
            }
        }

        public void OnRenderEnd(CoordStruct location)
        {
            if (delayToEnable)
            {
                return;
            }
            foreach (IEffect effect in effects)
            {
                effect?.OnRenderEnd(location);
            }
        }

        public void OnUpdate(CoordStruct location, bool isDead)
        {
            this.Location = location;
            if (delayToEnable)
            {
                if (initialDelayTimer.InProgress())
                {
                    return;
                }
                EnableEffects();
            }

            foreach (IEffect effect in effects)
            {
                effect?.OnUpdate(location, isDead);
            }
        }

        public void OnLateUpdate(CoordStruct location, bool isDead)
        {
            if (delayToEnable)
            {
                return;
            }
            foreach (IEffect effect in effects)
            {
                effect?.OnLateUpdate(location, isDead);
            }
        }

        public void OnWarpUpdate(CoordStruct location, bool isDead)
        {
            if (delayToEnable)
            {
                return;
            }
            foreach (IEffect effect in effects)
            {
                effect?.OnWarpUpdate(location, isDead);
            }
        }

        public void OnTemporalUpdate(Pointer<TemporalClass> pTemporal)
        {
            if (delayToEnable)
            {
                return;
            }
            foreach (IEffect effect in effects)
            {
                effect?.OnTemporalUpdate(pTemporal);
            }
        }

        public void OnTemporalEliminate(Pointer<TemporalClass> pTemporal)
        {
            if (delayToEnable)
            {
                return;
            }
            foreach (IEffect effect in effects)
            {
                effect?.OnTemporalEliminate(pTemporal);
            }
        }

        public void OnRocketExplosion()
        {
            if (delayToEnable)
            {
                return;
            }
            foreach (IEffect effect in effects)
            {
                effect?.OnRocketExplosion();
            }
        }

        public void OnPut(Pointer<CoordStruct> location, DirType dirType)
        {
            if (delayToEnable)
            {
                return;
            }
            foreach (IEffect effect in effects)
            {
                effect?.OnPut(location, dirType);
            }
        }

        public void OnRemove()
        {
            if (delayToEnable)
            {
                return;
            }
            foreach (IEffect effect in effects)
            {
                effect?.OnRemove();
            }
        }

        public void OnReceiveDamage(Pointer<int> pDamage, int distanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool ignoreDefenses, bool preventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (delayToEnable)
            {
                return;
            }
            foreach (IEffect effect in effects)
            {
                effect?.OnReceiveDamage(pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse);
            }
        }

        public void OnReceiveDamageDestroy()
        {
            if (delayToEnable)
            {
                return;
            }
            foreach (IEffect effect in effects)
            {
                effect?.OnReceiveDamageDestroy();
            }
        }

        public void OnGuardCommand()
        {
            if (delayToEnable)
            {
                return;
            }
            foreach (IEffect effect in effects)
            {
                effect?.OnGuardCommand();
            }
        }

        public void OnStopCommand()
        {
            if (delayToEnable)
            {
                return;
            }
            foreach (IEffect effect in effects)
            {
                effect?.OnStopCommand();
            }
        }

    }
}
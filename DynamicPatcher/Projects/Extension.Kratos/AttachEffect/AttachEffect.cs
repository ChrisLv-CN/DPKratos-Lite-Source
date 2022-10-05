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

        public SwizzleablePointer<ObjectClass> pOwner; // AE附着对象
        public SwizzleablePointer<HouseClass> pSourceHouse; // AE来源所属
        public SwizzleablePointer<TechnoClass> pSource; // AE来源

        public CoordStruct Location;

        public bool Active;
        private int duration; // 寿命
        private bool immortal; // 永生
        private TimerStruct lifeTimer;
        private TimerStruct initialDelayTimer;
        private bool delayToEnable; // 延迟激活中

        private List<IEffect> effects = new List<IEffect>();

        public AttachEffect(AttachEffectData data)
        {
            this.AEData = data;

            this.pOwner = new SwizzleablePointer<ObjectClass>(IntPtr.Zero);
            this.pSourceHouse = new SwizzleablePointer<HouseClass>(IntPtr.Zero);
            this.pSource = new SwizzleablePointer<TechnoClass>(IntPtr.Zero);
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
            InitDestroySelf();

            InitAnimation();
            InitAttackBeacon();
            InitAutoWeapon();
            InitBlackHole();
            InitCrateBuff();
            // InitDamageReaction();
            InitDamageSelf(); // AffectWho
            InitDeselect(); // AffectWho
            InitDisableWeapon(); // AffectWho
            InitExtraFire(); // AffectWho
            InitFireSuper(); // AffectWho
            InitGiftBox(); // AffectWho
            InitPaintball();
            // InitTransform();
            InitOverrideWeapon(); // AffectWho
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

        public void Enable(Pointer<ObjectClass> pOwner, Pointer<HouseClass> pSourceHouse, SwizzleablePointer<TechnoClass> pSource)
        {
            this.Active = true;
            this.pOwner.Pointer = pOwner;
            this.pSourceHouse.Pointer = pSourceHouse;
            this.pSource.Pointer = pSource;
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
            this.Active = false;
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
            if (Active)
            {
                // Logger.Log("AE Type {0} {1} and {2}", Type.Name, IsDeath() ? "is death" : "not dead", IsAlive() ? "is alive" : "not alive");
                Active = delayToEnable || (!IsDeath() && IsAlive());
            }
            return Active;
        }

        public bool IsAnyAlive()
        {
            foreach (IEffect effect in effects)
            {
                if (effect.IsAlive())
                {
                    // Logger.Log($"{Game.CurrentFrame} - AE {Name} 模块 {effect.GetType().Name} 狗带了");
                    return true;
                }
            }
            return false;
        }

        public bool IsAlive()
        {
            CheckSourceAlive();
            foreach (IEffect effect in effects)
            {
                if (!effect.IsAlive())
                {
                    // Logger.Log($"{Game.CurrentFrame} - AE {Name} 模块 {effect.GetType().Name} 狗带了");
                    return false;
                }
            }
            return true;
            // return (null == Animation || Animation.IsAlive())
            //     && (null == AttachStatus || AttachStatus.IsAlive())
            //     && (null == AutoWeapon || AutoWeapon.IsAlive())
            //     && (null == BlackHole || BlackHole.IsAlive())
            //     && (null == DestroySelf || DestroySelf.IsAlive())
            //     && (null == Paintball || Paintball.IsAlive())
            //     && (null == Stand || Stand.IsAlive())
            //     && (null == Transform || Transform.IsAlive())
            //     && (null == Weapon || Weapon.IsAlive());
        }

        private void CheckSourceAlive()
        {
            if (!pSource.IsNull && pSource.Pointer.IsDead())
            {
                pSource.Pointer = IntPtr.Zero;
            }
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
                    this.Active = false;
                }
                else
                {
                    // Logger.Log("削减{0}持续时间{1}，{2}生命{3}，当前剩余{4}", Name, otherDuration, this.immortal ? "无限" : "", this.duration, timeLeft);
                    timeLeft += otherDuration;
                    if (timeLeft <= 0)
                    {
                        // 削减完后彻底没了
                        this.Active = false;
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
            CheckSourceAlive();
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
            CheckSourceAlive();
            if (delayToEnable)
            {
                return;
            }

            foreach (IEffect effect in effects)
            {
                effect?.OnLateUpdate(location, isDead);
            }
        }

        public void OnTemporalUpdate(Pointer<TemporalClass> pTemporal)
        {
            CheckSourceAlive();
            if (delayToEnable)
            {
                return;
            }
            foreach (IEffect effect in effects)
            {
                effect?.OnTemporalUpdate(pTemporal);
            }
        }

        public void OnPut(Pointer<CoordStruct> location, DirType dirType)
        {
            CheckSourceAlive();
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
            CheckSourceAlive();
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
            CheckSourceAlive();
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
            CheckSourceAlive();
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
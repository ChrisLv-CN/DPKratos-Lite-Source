using System.Reflection;
using System.Collections;
using DynamicPatcher;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Ext
{

    [Serializable]
    public partial class AttachEffect : IAttachEffectBehaviour
    {
        public string Name;
        public AttachEffectType Type;

        public SwizzleablePointer<HouseClass> pHouse;
        public SwizzleablePointer<TechnoClass> pAttacker;
        public bool Active;
        private int duration; // 寿命
        private bool immortal; // 永生
        private TimerStruct lifeTimer;
        private TimerStruct initialDelayTimer;
        private bool delayToEnable; // 延迟激活中

        public AttachEffect(AttachEffectType type)
        {
            this.Name = type.Name;
            this.Type = type;
            this.pHouse = new SwizzleablePointer<HouseClass>(IntPtr.Zero);
            this.pAttacker = new SwizzleablePointer<TechnoClass>(IntPtr.Zero);
            int initDelay = type.InitialDelay;
            this.delayToEnable = false;
            if (type.InitialRandomDelay)
            {
                initDelay = MathEx.Random.Next(type.InitialMinDelay, type.InitialMaxDelay);
            }
            if (initDelay > 0)
            {
                this.initialDelayTimer = new TimerStruct(initDelay);
                this.delayToEnable = true;
            }
            this.duration = type.Duration;
            this.immortal = Type.HoldDuration;
            // if (!this.immortal)
            // {
            //     this.lifeTimer.Start(this.duration);
            // }
            InitAnimation();
            InitAttachStatus();
            InitAutoWeapon();
            InitBlackHole();
            InitDestroySelf();
            InitPaintball();
            InitStand();
            InitTransform();
        }

        public void SetupLifeTimer()
        {
            if (!this.immortal)
            {
                this.lifeTimer.Start(this.duration);
            }
        }

        /// <summary>
        /// 激活
        /// </summary>
        public void Enable(Pointer<ObjectClass> pObject, Pointer<HouseClass> pHouse, Pointer<TechnoClass> pAttacker)
        {
            this.Active = true;
            this.pHouse.Pointer = pHouse;
            this.pAttacker.Pointer = pAttacker;
            if (!delayToEnable || initialDelayTimer.Expired())
            {
                EnableEffects(pObject, pHouse, pAttacker);
            }
        }

        private void EnableEffects(Pointer<ObjectClass> pObject, Pointer<HouseClass> pHouse, Pointer<TechnoClass> pAttacker)
        {
            delayToEnable = false;
            SetupLifeTimer();
            // 激活动画
            Animation?.Enable(pObject, pHouse, pAttacker);
            // 激活属性加成
            AttachStatus?.Enable(pObject, pHouse, pAttacker);
            // 激活自动武器
            AutoWeapon?.Enable(pObject, pHouse, pAttacker);
            // 激活黑洞
            BlackHole?.Enable(pObject, pHouse, pAttacker);
            // 激活自毁
            DestroySelf?.Enable(pObject, pHouse, pAttacker);
            // 激活彩弹
            Paintball?.Enable(pObject, pHouse, pAttacker);
            // 激活替身
            Stand?.Enable(pObject, pHouse, pAttacker);
            // 激活变形
            Transform?.Enable(pObject, pHouse, pAttacker);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Disable(CoordStruct location)
        {
            this.Active = false;
            if (delayToEnable)
            {
                return;
            }
            Animation?.Disable(location);
            AttachStatus?.Disable(location);
            AutoWeapon?.Disable(location);
            BlackHole?.Disable(location);
            DestroySelf?.Disable(location);
            Paintball?.Disable(location);
            Stand?.Disable(location);
            Transform?.Disable(location);
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

        public bool IsAlive()
        {
            return (null == Animation || Animation.IsAlive())
                && (null == AttachStatus || AttachStatus.IsAlive())
                && (null == AutoWeapon || AutoWeapon.IsAlive())
                && (null == BlackHole || BlackHole.IsAlive())
                && (null == DestroySelf || DestroySelf.IsAlive())
                && (null == Paintball || Paintball.IsAlive())
                && (null == Stand || Stand.IsAlive())
                && (null == Transform || Transform.IsAlive());
        }

        private bool IsDeath()
        {
            // Logger.Log("AE Type {0} duration {1} time left {2}", Type.Name, duration, lifeTimer.TimeLeft);
            return duration <= 0 || (!immortal && lifeTimer.Expired());
        }

        private void ForceStartLifeTimer(int timeLeft)
        {
            this.immortal = false;
            this.lifeTimer.Start(timeLeft);
            // Logger.Log("启动{0}生命计时器，生命{1}，计时{2}", Name, duration, timeLeft);
        }

        public bool IsSameGroup(AttachEffectType otherType)
        {
            return this.Type.Group > -1 && otherType.Group > -1 && this.Type.Group == otherType.Group;
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
            Animation?.ResetDuration();
            AttachStatus?.ResetDuration();
            AutoWeapon?.ResetDuration();
            BlackHole?.ResetDuration();
            DestroySelf?.ResetDuration();
            Paintball?.ResetDuration();
            Stand?.ResetDuration();
            Transform?.ResetDuration();
        }

        public void OnUpdate(Pointer<ObjectClass> pObject, bool isDead, AttachEffectManager manager)
        {
            if (delayToEnable)
            {
                if (initialDelayTimer.InProgress())
                {
                    return;
                }
                EnableEffects(pObject, pHouse, pAttacker);
            }
            Animation?.OnUpdate(pObject, isDead, manager);
            AttachStatus?.OnUpdate(pObject, isDead, manager);
            AutoWeapon?.OnUpdate(pObject, isDead, manager);
            BlackHole?.OnUpdate(pObject, isDead, manager);
            DestroySelf?.OnUpdate(pObject, isDead, manager);
            Paintball?.OnUpdate(pObject, isDead, manager);
            Stand?.OnUpdate(pObject, isDead, manager);
            Transform?.OnUpdate(pObject, isDead, manager);
        }

        public void OnTemporalUpdate(TechnoExt ext, Pointer<TemporalClass> pTemporal)
        {
            if (delayToEnable)
            {
                return;
            }
            Animation?.OnTemporalUpdate(ext, pTemporal);
            AttachStatus?.OnTemporalUpdate(ext, pTemporal);
            AutoWeapon?.OnTemporalUpdate(ext, pTemporal);
            BlackHole?.OnTemporalUpdate(ext, pTemporal);
            DestroySelf?.OnTemporalUpdate(ext, pTemporal);
            Paintball?.OnTemporalUpdate(ext, pTemporal);
            Stand?.OnTemporalUpdate(ext, pTemporal);
            Transform?.OnTemporalUpdate(ext, pTemporal);
        }

        public void OnPut(Pointer<ObjectClass> pObject, Pointer<CoordStruct> pCoord, Direction faceDir)
        {
            if (delayToEnable)
            {
                return;
            }
            Animation?.OnPut(pObject, pCoord, faceDir);
            AttachStatus?.OnPut(pObject, pCoord, faceDir);
            AutoWeapon?.OnPut(pObject, pCoord, faceDir);
            BlackHole?.OnPut(pObject, pCoord, faceDir);
            DestroySelf?.OnPut(pObject, pCoord, faceDir);
            Paintball?.OnPut(pObject, pCoord, faceDir);
            Stand?.OnPut(pObject, pCoord, faceDir);
            Transform?.OnPut(pObject, pCoord, faceDir);
        }

        public void OnRemove(Pointer<ObjectClass> pObject)
        {
            if (delayToEnable)
            {
                return;
            }
            Animation?.OnRemove(pObject);
            AttachStatus?.OnRemove(pObject);
            AutoWeapon?.OnRemove(pObject);
            BlackHole?.OnRemove(pObject);
            DestroySelf?.OnRemove(pObject);
            Paintball?.OnRemove(pObject);
            Stand?.OnRemove(pObject);
            Transform?.OnRemove(pObject);
        }

        public void OnReceiveDamage(Pointer<ObjectClass> pObject, Pointer<int> pDamage, int distanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool ignoreDefenses, bool preventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (delayToEnable)
            {
                return;
            }
            Animation?.OnReceiveDamage(pObject, pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse);
            AttachStatus?.OnReceiveDamage(pObject, pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse);
            AutoWeapon?.OnReceiveDamage(pObject, pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse);
            BlackHole?.OnReceiveDamage(pObject, pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse);
            DestroySelf?.OnReceiveDamage(pObject, pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse);
            Paintball?.OnReceiveDamage(pObject, pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse);
            Stand?.OnReceiveDamage(pObject, pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse);
            Transform?.OnReceiveDamage(pObject, pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse);
        }

        public void OnDestroy(Pointer<ObjectClass> pObject)
        {
            if (delayToEnable)
            {
                return;
            }
            Animation?.OnDestroy(pObject);
            AttachStatus?.OnDestroy(pObject);
            AutoWeapon?.OnDestroy(pObject);
            BlackHole?.OnDestroy(pObject);
            DestroySelf?.OnDestroy(pObject);
            Paintball?.OnDestroy(pObject);
            Stand?.OnDestroy(pObject);
            Transform?.OnDestroy(pObject);
        }

        public void OnGuardCommand()
        {
            if (delayToEnable)
            {
                return;
            }
            Animation?.OnGuardCommand();
            AttachStatus?.OnGuardCommand();
            AutoWeapon?.OnGuardCommand();
            BlackHole?.OnGuardCommand();
            DestroySelf?.OnGuardCommand();
            Paintball?.OnGuardCommand();
            Stand?.OnGuardCommand();
            Transform?.OnGuardCommand();
        }

        public void OnStopCommand()
        {
            if (delayToEnable)
            {
                return;
            }
            Animation?.OnStopCommand();
            AttachStatus?.OnStopCommand();
            AutoWeapon?.OnStopCommand();
            BlackHole?.OnStopCommand();
            DestroySelf?.OnStopCommand();
            Paintball?.OnStopCommand();
            Stand?.OnStopCommand();
            Transform?.OnStopCommand();
        }

    }
}
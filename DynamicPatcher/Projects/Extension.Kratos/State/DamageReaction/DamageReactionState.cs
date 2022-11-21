using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class DamageReactionState : State<DamageReactionData>
    {
        public bool forceDone;

        private bool isElite;
        private DamageReactionEntity data;

        private int count;
        private int delay;
        private TimerStruct delayTimer;

        private int animDelay;
        private TimerStruct animDelayTimer;

        public override void OnEnable()
        {
            this.forceDone = false;
            this.data = GetDamageReactionEntity(isElite);
            this.count = 0;
            this.delay = 0;
            this.delayTimer.Stop();
            this.animDelay = 0;
            this.animDelayTimer.Stop();
        }

        public void Update(bool isElite)
        {
            if (IsActive())
            {
                this.data = GetDamageReactionEntity(isElite);
                if (this.isElite != isElite)
                {
                    // 重置计数器
                    if (null != data && data.ResetTimes)
                    {
                        count = 0;
                    }
                }
                if (IsDone() || forceDone)
                {
                    Disable();
                }
            }
            this.isElite = isElite;
        }

        private DamageReactionEntity GetDamageReactionEntity(bool isElite)
        {
            if (isElite && null != Data.EliteData)
            {
                return Data.EliteData;
            }
            return Data.Data;
        }

        public bool Reaction(out DamageReactionEntity reactionData)
        {
            reactionData = data;
            // 检查有效性和冷却
            if (IsActive() && Timeup() && null != data && !IsDone())
            {
                return data.Chance.Bingo();
            }
            return false;
        }

        public void ActionOnce()
        {
            count++;
            if (null != data)
            {
                this.delay = data.Delay;
                forceDone = data.ActiveOnce;
            }
            else
            {
                this.delay = -1;
            }
            if (this.delay > 0)
            {
                delayTimer.Start(delay);
            }
        }

        private bool Timeup()
        {
            return delay <= 0 || delayTimer.Expired();
        }

        private bool IsDone()
        {
            if (data != null)
            {
                return data.TriggeredTimes > 0 && count >= data.TriggeredTimes;
            }
            return false;
        }

        public bool CanPlayAnim()
        {

            return animDelay <= 0 || animDelayTimer.Expired();
        }

        public void AnimPlay()
        {
            this.animDelay = null != data ? data.AnimDelay : -1;
            if (animDelay > 0)
            {
                animDelayTimer.Start(animDelay);
            }
        }

    }

}

using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{
    public interface IState
    {
        public void EnableAndReplace(int duration, string token, IStateData data);

        public void ResetDuration(string token, int duration);

        public void Enable(int duration, string token, IStateData data);

        public void Disable(string token);

        public bool IsActive();

    }

    [Serializable]
    public class State<T> : IState where T : IStateData, new()
    {
        public string Token;
        public AttachEffect AE;

        public T Data;

        private bool active;
        private bool infinite;
        private TimerStruct timer;

        private bool resetFlag;

        private int frame; // 当前帧

        public State()
        {
            this.active = false;
            this.infinite = false;
            this.timer.Start(0);

            this.resetFlag = false;
        }

        // 由AE亲自开启
        public void EnableAndReplace<TT>(Effect<TT> effect) where TT : EffectData, IStateData, new()
        {
            // 强制关闭原有的
            Disable();
            // 附加新的
            this.AE = effect.AE;
            Enable(AE.GetDuration(), effect.Token, effect.Data);
        }

        // 由AE给替身开启
        public void EnableAndReplace(int duration, string token, IStateData data)
        {
            // 强制关闭原有的
            Disable();
            // 附加新的
            Enable(duration, token, data);
        }

        // 不管来源直接开启
        public void Enable(IStateData data)
        {
            this.AE = null;
            Enable(-1, null, data);
        }

        public void Enable(int duration, string token, IStateData data)
        {
            this.Token = token;
            this.Data = (T)data;
            this.active = duration != 0;
            ResetDuration(duration);
            this.frame = Game.CurrentFrame;
            // Logger.Log($"{Game.CurrentFrame} Enable State {(null != Data ? Data.GetType().Name : "Null")}, duration = {duration}, token {Token}");
            this.resetFlag = true;
            OnEnable();
        }

        public void ResetDuration(string token, int duration)
        {
            if (Token == token)
            {
                ResetDuration(duration);
            }
        }

        public void ResetDuration(int duration)
        {
            if (duration < 0)
            {
                infinite = true;
                timer.Stop();
            }
            else
            {
                infinite = false;
                StartTimer(duration);
            }
        }

        public virtual void StartTimer(int duration)
        {
            timer.Start(duration);
        }

        public virtual void OnEnable() { }

        public void Disable()
        {
            Disable(this.Token);
        }

        public void Disable(string token)
        {
            if (this.Token == token)
            {
                this.active = false;
                this.infinite = false;
                this.timer.Stop();
                // 关闭AE
                if (null != AE && AE.IsActive())
                {
                    AE.Disable(AE.Location);
                    this.AE = null;
                }
                // Logger.Log($"{Game.CurrentFrame} Disable State {(null != Data ? Data.GetType().Name : "Null")}, token {Token}");
                OnDisable();
            }
        }

        public virtual void OnDisable() { }

        public bool IsActive()
        {
            // 当前帧内持续有效，下一帧检查计时器
            if (active)
            {
                int currentFrame = Game.CurrentFrame;
                if (frame != currentFrame)
                {
                    frame = currentFrame;
                    active = infinite || timer.InProgress();
                    // if (!active && !Token.IsNullOrEmptyOrNone() && null != Data)
                    // {
                    //     Logger.Log($"{Game.CurrentFrame}, State {(null != Data ? Data.GetType().Name : "Null")} Time's up, token {Token}");
                    // }
                }
            }
            return active;
        }

        public bool IsReset()
        {
            bool reset = resetFlag;
            if (resetFlag)
            {
                resetFlag = false;
            }
            return reset;
        }

    }
}

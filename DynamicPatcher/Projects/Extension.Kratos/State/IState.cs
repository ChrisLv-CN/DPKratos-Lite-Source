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

        protected bool active;
        protected bool infinite;
        protected TimerStruct timer;

        private int frame; // 当前帧

        public State()
        {
            this.active = false;
            this.infinite = false;
            this.timer.Start(0);
        }

        public void EnableAndReplace<TT>(Effect<TT> effect) where TT : EffectData, IStateData, new()
        {
            // 激活新的效果，关闭旧的效果
            if (!string.IsNullOrEmpty(Token) && Token != effect.Token && null != AE && AE.IsActive())
            {
                AE.Disable(AE.Location);
            }
            this.AE = effect.AE;
            Enable(AE.AEData.GetDuration(), effect.Token, effect.Data);
        }

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
            if (duration < 0)
            {
                infinite = true;
                timer.Start(0);
            }
            else
            {
                infinite = false;
                timer.Start(duration);
            }
            this.frame = Game.CurrentFrame;
            // Logger.Log($"{Game.CurrentFrame} Enable State {(null != Data ? Data.GetType().Name : "Null")}, duration = {duration}, token {Token}");
            OnEnable();
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
                this.timer.Start(0);
                // 关闭AE
                if (null != AE && AE.IsActive())
                {
                    AE.Disable(AE.Location);
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

    }
}

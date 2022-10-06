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

        public void Enable(IStateData data)
        {
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
            // Logger.Log($"{Game.CurrentFrame} Enable State {(null != Data ? Data.GetType().Name : "Null")}, token {Token}");
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

                // Logger.Log($"{Game.CurrentFrame} Disable State {(null != Data ? Data.GetType().Name : "Null")}, token {Token}");
                OnDisable();
            }
        }

        public virtual void OnDisable() { }

        public bool IsActive()
        {
            // 当前帧内持续有效，下一帧检查计时器
            bool isActiveNow = active;
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
            return isActiveNow;
        }

    }
}

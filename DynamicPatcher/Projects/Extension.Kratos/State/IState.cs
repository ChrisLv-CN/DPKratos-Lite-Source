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
            // Logger.Log($"{Game.CurrentFrame}, Enable AE State {Data.GetType().Name}, token {Token}");
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

                // Logger.Log($"{Game.CurrentFrame}, Disable AE State {Data.GetType().Name}, token {Token}");
                OnDisable();
            }
        }

        public virtual void OnDisable() { }

        public bool IsActive()
        {
            return infinite || timer.InProgress();
        }

    }

    [Serializable]
    public class AEState<T> : State<T> where T : IStateData, new()
    {

        // public AttachEffect AE;

        // public void EnableAndReplace<TT>(Effect<TT> effect) where TT : IEffectType, IStateData, new()
        // {
        //     // 激活新的效果，关闭旧的效果
        //     if (!string.IsNullOrEmpty(Token) && Token != effect.Token && null != AE && AE.IsActive())
        //     {
        //         AE.Disable(AE.Location);
        //     }
        //     this.AE = effect.AE;
        //     Enable(effect.AEType.GetDuration(), effect.Token, effect.Type);
        // }

    }

}

using DynamicPatcher;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Ext
{
    public interface IEffect : IEffectAction
    {
        void SetData<T>(T data) where T : EffectData, IEffectData, new();
    }

    /// <summary>
    /// AE的效果模块
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public abstract class Effect<T> : IEffect where T : EffectData, IEffectData, new()
    {
        public string Token;
        public T Data;

        protected AttachEffect AE;
        protected Pointer<ObjectClass> pOwner => AE.pOwner;

        protected bool active;

        public Effect()
        {
            this.Token = Guid.NewGuid().ToString();
            this.AE = null;
            this.active = false;
        }

        public void SetData<TT>(TT data) where TT : EffectData, IEffectData, new()
        {
            this.Data = data as T;
        }


        // 返回AE是否还存活
        public virtual bool IsAlive() { return this.active; }
        // AE激活，开始生效
        public void Enable(AttachEffect ae)
        {
            this.AE = ae;
            this.active = true;
            OnEnable();
        }
        public virtual void OnEnable() { }
        // AE关闭，销毁相关资源
        public void Disable(CoordStruct location)
        {
            this.active = false;
            OnDisable(location);
        }
        public virtual void OnDisable(CoordStruct location) { }
        // 重置计时器
        public virtual void ResetDuration() { }

        // 渲染
        public virtual void OnRender(CoordStruct location) { }
        public virtual void OnRenderEnd(CoordStruct location) { }
        // 更新
        public virtual void OnUpdate(CoordStruct location, bool isDead) { }
        public virtual void OnLateUpdate(CoordStruct location, bool isDead) { }
        // 被超时空冻结更新
        public virtual void OnTemporalUpdate(Pointer<TemporalClass> pTemporal) { }
        // 挂载AE的单位出现在地图上
        public virtual void OnPut(Pointer<CoordStruct> pCoord, DirType dirType) { }
        // 挂载AE的单位从地图隐藏
        public virtual void OnRemove() { }
        // 收到伤害
        public virtual void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse) { }
        // 收到伤害导致死亡
        public virtual void OnReceiveDamageDestroy() { }
        // 按下G键
        public virtual void OnGuardCommand() { }
        // 按下S键
        public virtual void OnStopCommand() { }

    }


}

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
    public interface IEffectAction
    {
        // 返回AE是否还存活
        bool IsAlive();
        // AE激活
        void Enable(AttachEffect AE);
        // AE关闭
        void Disable(CoordStruct location);
        // 重置计时器
        void ResetDuration();

        // 渲染
        void OnGScreenRender(CoordStruct location);
        // 更新
        void OnUpdate(CoordStruct location, bool isDead);
        // 被超时空冻结更新
        void OnWarpUpdate(CoordStruct location, bool isDead);
        // 被超时空兵攻击
        void OnTemporalUpdate(Pointer<TemporalClass> pTemporal);
        void OnTemporalEliminate(Pointer<TemporalClass> pTemporal);
        // 子机导弹爆炸
        void OnRocketExplosion();
        // 挂载AE的单位出现在地图上
        void OnPut(Pointer<CoordStruct> pCoord, DirType dirType);
        // 挂载AE的单位从地图隐藏
        void OnRemove();
        // 收到伤害
        void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse);
        // 收到实际伤害
        void OnReceiveDamage2(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse);
        // 收到伤害导致死亡
        void OnReceiveDamageDestroy();
        // 按下G键
        void OnGuardCommand();
        // 按下S键
        void OnStopCommand();
    }

}

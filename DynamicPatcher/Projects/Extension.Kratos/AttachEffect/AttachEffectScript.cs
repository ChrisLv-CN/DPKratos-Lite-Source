using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.EventSystems;
using Extension.INI;
using Extension.Utilities;
using System.Runtime.InteropServices.ComTypes;

namespace Extension.Script
{

    [Serializable]
    public class LocationMark
    {
        public CoordStruct Location;
        public DirStruct Direction;

        public LocationMark(CoordStruct location, DirStruct direction)
        {
            this.Location = location;
            this.Direction = direction;
        }
    }

    /// <summary>
    /// AEManager
    /// </summary>
    [Serializable]
    [GlobalScriptable(typeof(TechnoExt), typeof(BulletExt))]
    [UpdateBefore(typeof(TechnoStatusScript), typeof(BulletStatusScript))]
    public partial class AttachEffectScript : ObjectScriptable
    {
        public AttachEffectScript(IExtension owner) : base(owner) { }

        public Pointer<ObjectClass> pOwner => pObject;

        public IConfigWrapper<AttachEffectTypeData> _aeTypeData = null;
        public AttachEffectTypeData aeTypeData
        {
            get
            {
                if (null == _aeTypeData)
                {
                    _aeTypeData = Ini.GetConfig<AttachEffectTypeData>(Ini.RulesDependency, section);
                }
                return _aeTypeData.Data;
            }
        }
        public List<AttachEffect> AttachEffects; // 所有有效的AE
        public Dictionary<string, TimerStruct> DisableDelayTimers; // 同名AE失效后再赋予的计时器
        public Dictionary<string, int> AEStacks; // 同名AE的叠加层数

        private AbstractType _absType;
        public AbstractType AbsType
        {
            get
            {
                if (default == _absType)
                {
                    _absType = pOwner.Ref.Base.WhatAmI();
                }
                return _absType;
            }
        }

        public bool IsBullet => AbsType == AbstractType.Bullet;
        public bool IsBuilding => AbsType == AbstractType.Building;
        public bool IsFoot => !IsBullet && !IsBuilding;

        public bool InBuilding
        {
            get
            {
                if (!isDead && IsBuilding)
                {
                    Pointer<BuildingClass> pBuilding = pObject.Convert<BuildingClass>();
                    MissionClass mission = pBuilding.Ref.BaseMission;
                    // Logger.Log($"{Game.CurrentFrame} 建筑 [{section}]{pOwner} 是否在建筑, BState = {pBuilding.Ref.BState}, Mission = {mission.CurrentMission}, MissionStatus = {mission.MissionStatus}");
                    return pBuilding.Ref.BState == BStateType.CONSTRUCTION && mission.CurrentMission != Mission.Selling;
                }
                return false;
            }
        }

        public bool InSelling
        {
            get
            {
                if (IsBuilding)
                {
                    Pointer<BuildingClass> pBuilding = pObject.Convert<BuildingClass>();
                    MissionClass mission = pBuilding.Ref.BaseMission;
                    // Logger.Log($"{Game.CurrentFrame} 建筑 [{section}]{pOwner} 是否在出售, BState = {pBuilding.Ref.BState}, Mission = {mission.CurrentMission}, MissionStatus = {mission.MissionStatus}");
                    return pBuilding.Ref.BState == BStateType.CONSTRUCTION && mission.CurrentMission == Mission.Selling && mission.MissionStatus > 0;
                }
                return false;
            }
        }

        public bool PowerOff;

        public List<int> PassengerIds; // 乘客持有的ID
        private List<TechnoExt> passengerMark = new List<TechnoExt>();

        private CoordStruct location;

        private List<LocationMark> locationMarks;
        private CoordStruct lastLocation; // 使者的上一次位置
        private int locationMarkDistance; // 多少格记录一个位置
        private double totleMileage; // 总里程

        private IConfigWrapper<AttachEffectTypeTypeData> _aeTypeTypeData = null;
        private AttachEffectTypeTypeData aeTypeTypeData
        {
            get
            {
                if (null == _aeTypeTypeData)
                {
                    _aeTypeTypeData = Ini.GetConfig<AttachEffectTypeTypeData>(Ini.RulesDependency, section);
                }
                return _aeTypeTypeData.Data;
            }
        }
        private bool attachEffectOnceFlag = false; // 已经在Update事件中附加过一次section上写的AE

        private bool _isDead = false;
        private bool isDead
        {
            get
            {
                if (!_isDead)
                {
                    if (IsBullet)
                    {
                        _isDead = pObject.Convert<BulletClass>().IsDead();
                    }
                    else
                    {
                        // 出售中的建筑也判定为死亡
                        _isDead = pObject.Convert<TechnoClass>().IsDead() || InSelling;
                    }
                }
                return _isDead;
            }
        }


        private int locationSpace; // 替身火车的车厢间距

        private bool initEffectFlag;

        // 将AE转移给其他对象
        public void InheritedTo(AttachEffectScript heir, bool heirIsNew)
        {
            // 转移给继任者
            heir.AttachEffects = this.AttachEffects;
            heir.DisableDelayTimers = this.DisableDelayTimers;
            heir.AEStacks = this.AEStacks;
            // 更改AE记录的附着对象，并移除不被继承的状态类型的AE
            for (int i = Count() - 1; i >= 0; i--)
            {
                AttachEffect ae = AttachEffects[i];
                // 移除不可继承的状态类型的AE，如礼盒，因为礼盒的状态机不会被继承
                if (ae.NonInheritable)
                {
                    // 强制移除礼盒AE
                    AttachEffects.Remove(ae);
                    DisableDelayTimers.Remove(ae.AEData.Name);
                    AEStacks.Remove(ae.AEData.Name);
                }
                else
                {
                    // 修改AE的附着对象
                    ae.AEManager = heir;
                    // 移除不可继承的AE
                    if (!ae.AEData.Inheritable)
                    {
                        ae.Disable(location); // 关闭继承者的状态机
                        AttachEffects.Remove(ae);
                        DisableDelayTimers.Remove(ae.AEData.Name);
                        AEStacks.Remove(ae.AEData.Name);
                    }
                }
            }

            heir.locationMarks = this.locationMarks;
            heir.lastLocation = this.lastLocation;
            heir.locationMarkDistance = this.locationMarkDistance;
            heir.totleMileage = this.totleMileage;

            // 继承后应允许添加新Type的AE，所以Flag应该为false，而不是继承过去
            heir.attachEffectOnceFlag = !heirIsNew ? this.attachEffectOnceFlag : false;

            heir.locationSpace = this.locationSpace;

            // 转移完成后，重置
            OnAwake();
        }

        public override void Awake()
        {
            OnAwake();
            // Logger.Log($"{Game.CurrentFrame} 脚本激活时注册");
            // 注册Render事件
            EventSystem.GScreen.AddTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
            EventSystem.Techno.AddTemporaryHandler(EventSystem.Techno.TypeChangeEvent, OnTransform);
        }

        public override void LoadFromStream(IStream stream)
        {
            base.LoadFromStream(stream);
            // Logger.Log($"{Game.CurrentFrame} 读入时重新注册");
            // 注册Render事件
            EventSystem.GScreen.AddTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
            EventSystem.Techno.AddTemporaryHandler(EventSystem.Techno.TypeChangeEvent, OnTransform);
            for (int i = Count() - 1; i >= 0; i--)
            {
                AttachEffect ae = AttachEffects[i];
                if (ae.IsActive())
                {
                    ae.LoadFromStream(stream);
                }
            }
        }

        private void OnAwake()
        {
            this.AttachEffects = new List<AttachEffect>();
            this.DisableDelayTimers = new Dictionary<string, TimerStruct>();
            this.AEStacks = new Dictionary<string, int>();

            this.location = pOwner.Ref.Base.GetCoords();

            this.locationMarks = new List<LocationMark>();
            this.lastLocation = default;
            this.locationMarkDistance = 16;
            this.totleMileage = 0;

            this.attachEffectOnceFlag = false;

            this.locationSpace = 512;
        }

        public int Count()
        {
            return AttachEffects.Count();
        }

        public void SetLocationSpace(int cabinLenght)
        {
            this.locationSpace = cabinLenght;

            if (cabinLenght < locationMarkDistance)
            {
                this.locationMarkDistance = cabinLenght;
            }
        }

        /// <summary>
        /// 从单位自身的section中获取AE清单并附加
        /// 不判断概率
        /// </summary>
        /// <param name="typeData">section的AE清单</param>
        /// <param name="pSource">AE来源，即攻击者</param>
        /// <param name="pSourceHouse">来源所属</param>
        /// <param name="fromWarhead">来自弹头，attachEffectOnceFlag应该传false</param>
        /// <param name="warheadLocation">弹头的位置</param>
        private void Attach(AttachEffectTypeData typeData)
        {
            // 清单中有AE类型
            if (null != typeData.AttachEffectTypes && typeData.AttachEffectTypes.Any())
            {
                Attach(typeData.AttachEffectTypes, null, pOwner, default, attachEffectOnceFlag);
            }

            if (typeData.StandTrainCabinLength > 0)
            {
                SetLocationSpace(typeData.StandTrainCabinLength);
            }
        }

        /// <summary>
        /// 遍历AE清单并逐个附加
        /// 弹头爆炸赋予受害者AE
        /// </summary>
        /// <param name="aeTypes"></param>
        /// <param name="pSource"></param>
        /// <param name="pSourceHouse">来源所属</param>
        /// <param name="attachOnceFlag"></param>
        /// <param name="warheadLocation">弹头的位置</param>
        /// <param name="aeMode">分组编号</param>
        /// <param name="fromPassenger">绑定乘客</param>
        public void Attach(string[] aeTypes, double[] aeChances, Pointer<ObjectClass> pSource = default, Pointer<HouseClass> pSourceHouse = default, bool attachOnceFlag = false, CoordStruct warheadLocation = default, int aeMode = -1, bool fromPassenger = false)
        {
            if (null != aeTypes && aeTypes.Any())
            {
                // Logger.Log($"{Game.CurrentFrame} 为 [{section}]{pOwner} 附加 AE 清单 [{string.Join(",", aeTypes)}]. attachOnceFlag = {attachOnceFlag}, 来源 {pSource}");
                int i = 0;
                foreach (string type in aeTypes)
                {
                    if (aeChances.Bingo(i))
                    {
                        Attach(type, pSource, pSourceHouse, attachOnceFlag, warheadLocation, aeMode, fromPassenger);
                    }
                    i++;
                }
            }
        }

        /// <summary>
        /// 按照AE的section来添加AE
        /// </summary>
        /// <param name="type">AE的section</param>
        /// <param name="pSource">AE来源</param>
        /// <param name="pSourceHouse">来源所属</param>
        /// <param name="attachOnceFlag"></param>
        /// <param name="warheadLocation">弹头的位置</param>
        /// <param name="aeMode">分组编号</param>
        /// <param name="fromPassenger">绑定乘客</param>
        public void Attach(string type, Pointer<ObjectClass> pSource = default, Pointer<HouseClass> pSourceHouse = default, bool attachOnceFlag = false, CoordStruct warheadLocation = default, int aeMode = -1, bool fromPassenger = false)
        {
            IConfigWrapper<AttachEffectData> aeDate = Ini.GetConfig<AttachEffectData>(Ini.RulesDependency, type);
            // Logger.Log($"{Game.CurrentFrame} 为 [{section}]{pOwner} 附加 AE [{type}]. attachOnce = {aeDate.Data.AttachOnceInTechnoType} attachOnceFlag = {attachOnceFlag}, 来源 {pSource}");
            if (attachOnceFlag && aeDate.Data.AttachOnceInTechnoType)
            {
                return;
            }
            Attach(aeDate.Data, pSource, pSourceHouse, warheadLocation, aeMode, fromPassenger);
        }

        /// <summary>
        /// 附加AE
        /// </summary>
        /// <param name="aeData">要附加的AE类型</param>
        public void Attach(AttachEffectData aeData)
        {
            // 写在type上附加的AE，所属是自己，攻击者是自己
            Attach(aeData, pOwner);
        }

        /// <summary>
        /// 附加AE
        /// </summary>
        /// <param name="aeData">要附加的AE类型</param>
        /// <param name="pSource">来源</param>
        /// <param name="pSourceHouse">来源所属</param>
        /// <param name="warheadLocation">弹头的位置</param>
        /// <param name="aeMode">分组编号</param>
        /// <param name="fromPassenger">绑定乘客</param>
        public void Attach(AttachEffectData data, Pointer<ObjectClass> pSource, Pointer<HouseClass> pSourceHouse = default, CoordStruct warheadLocation = default, int aeMode = -1, bool fromPassenger = false)
        {
            if (!data.Enable)
            {
                Logger.LogWarning($"Attempt to attach an invalid AE [{data.Name}] to [{section}]");
                return;
            }
            // 检查是否穿透铁幕
            if (!data.PenetratesIronCurtain && pObject.Ref.IsIronCurtained())
            {
                return;
            }
            // 是否在名单上
            if (!data.CanAffectType(pOwner))
            {
                // Logger.Log($"{Game.CurrentFrame} 单位 [{section}] 不在AE [{data.Name}] 的名单内，不能赋予");
                return;
            }
            // 是否需要标记
            if (!data.IsOnMark(this))
            {
                return;
            }
            // 是否有排斥的AE
            if (data.Contradiction(this))
            {
                return;
            }
            Pointer<TechnoClass> pAttacker = IntPtr.Zero;
            Pointer<HouseClass> pAttackingHouse = pSourceHouse;
            // 调整所属
            if (pSource.IsNull)
            {
                // Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pObject} 添加AE类型[{data.Name}]，未标记来源，来源设置为自身");
                pSource = pOwner;
            }
            if (pSource.CastToTechno(out Pointer<TechnoClass> pSourceTechno))
            {
                pAttacker = pSourceTechno;
                if (pAttackingHouse.IsNull)
                {
                    pAttackingHouse = pAttacker.Ref.Owner;
                }
            }
            else if (pSource.CastToBullet(out Pointer<BulletClass> pSourceBullet))
            {
                pAttacker = pSourceBullet.Ref.Owner;
                if (pAttackingHouse.IsNull)
                {
                    pAttackingHouse = pSourceBullet.GetSourceHouse();
                }
            }
            else
            {
                Logger.LogWarning($"Attach AE [{data.Name}] to [{section}] form a unknow source [{pSource.Ref.Base.WhatAmI()}]");
                return;
            }
            // 调整所属
            if (pOwner != pSource && data.ReceiverOwn)
            {
                // Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pObject} 添加AE类型[{data.Name}]，来源为外部，更改所属为接受者");
                // 所属设为接受者
                if (pOwner.CastToTechno(out Pointer<TechnoClass> pOwnerTechno))
                {
                    pAttackingHouse = pOwnerTechno.Ref.Owner;
                }
                else if (pOwner.CastToBullet(out Pointer<BulletClass> pOwnerBullet))
                {
                    pAttackingHouse = pOwnerBullet.GetSourceHouse();
                }
            }
            // 调整攻击者
            if (!pAttacker.IsDead() && !fromPassenger && data.FromTransporter)
            {
                pAttacker = pAttacker.WhoIsShooter();
            }
            // 检查叠加
            bool add = data.Cumulative == CumulativeMode.YES;
            if (!add)
            {
                // 不同攻击者是否叠加
                bool isAttackMark = fromPassenger || data.Cumulative == CumulativeMode.ATTACKER && !pAttacker.IsNull && pAttacker.Ref.Base.IsAlive;
                // 攻击者标记AE名称相同，但可以来自不同的攻击者，可以叠加，不检查Delay
                // if (isAttackMark)
                // {
                //     Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pObject} 添加AE类型[{data.Name}] 来源 {pAttacker} fromPassenger = {fromPassenger}");
                // }
                // 检查冷却计时器
                if (!isAttackMark && DisableDelayTimers.TryGetValue(data.Name, out TimerStruct delayTimer) && delayTimer.InProgress())
                {
                    // Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pObject} 添加AE类型[{data.Name}]，该类型尚在冷却中，无法添加");
                    return;
                }
                bool find = false;
                // 检查持续时间，增减Duration
                for (int i = Count() - 1; i >= 0; i--)
                {
                    AttachEffect temp = AttachEffects[i];
                    if (data.Group < 0)
                    {
                        // 找同名
                        if (temp.AEData.Name == data.Name)
                        {
                            find = true;
                            // 找到了
                            if (isAttackMark)
                            {
                                if (temp.pSource == pAttacker)
                                {
                                    // 是攻击者标记，且相同的攻击者，重置持续时间
                                    if (temp.AEData.ResetDurationOnReapply)
                                    {
                                        temp.ResetDuration();
                                        AttachEffects[i] = temp;
                                    }
                                    break;
                                }
                                else
                                {
                                    find = false;
                                }
                            }
                            else
                            {
                                // 不是攻击者标记，重置持续时间
                                if (temp.AEData.ResetDurationOnReapply)
                                {
                                    temp.ResetDuration();
                                    AttachEffects[i] = temp;
                                }
                            }
                        }
                    }
                    else
                    {
                        // 找同组
                        if (temp.IsSameGroup(data))
                        {
                            // 找到了同组
                            find = true;
                            if (data.OverrideSameGroup)
                            {
                                // 替换
                                // Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pObject} 添加AE类型[{data.Name}]，发现同组{data.Group}已存在有AE类型[{temp.AEData.Name}]，执行替换");
                                // 关闭发现的同组
                                temp.Disable(location);
                                add = true;
                                continue; // 全部替换
                            }
                            else
                            {
                                // Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pObject} 添加AE类型[{data.Name}]，发现同组{data.Group}已存在有AE类型[{temp.AEData.Name}]，调整持续时间{data.Duration}");
                                // 调整持续时间
                                temp.MergeDuation(data.Duration);
                                AttachEffects[i] = temp;
                            }
                        }
                    }
                }
                // 没找到同类或同组，可以添加新的实例
                add = add || !find;
            }
            // 可以添加AE
            if (add && data.GetDuration() != 0 && StackNotFull(data))
            {
                AttachEffect ae = data.CreateAE();
                // 入队
                int index = FindInsertIndex(ae);
                // Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pObject} 添加AE类型[{data.Name}]，加入队列，插入位置{index}, 持续时间 {data.GetDuration()}, 来源 {(pAttacker.IsNull ? "" : pAttacker.Ref.Type.Ref.Base.Base.ID)} {pAttacker}, 所属 {pAttackingHouse}");
                AttachEffects.Insert(index, ae);
                AddStackCount(ae); // 叠层计数
                // 激活
                ae.Enable(this, pAttacker, pAttackingHouse, warheadLocation, aeMode, fromPassenger);
            }
        }

        // 关闭并移除指定名称的AE
        public void Disable(string[] aeTypes)
        {
            if (null != aeTypes && aeTypes.Any())
            {
                for (int i = Count() - 1; i >= 0; i--)
                {
                    AttachEffect ae = AttachEffects[i];
                    if (aeTypes.Contains(ae.AEData.Name))
                    {
                        ae.Disable(location);
                        AttachEffects.Remove(ae);
                        ReduceStackCount(ae);
                    }
                }
            }
        }

        private bool StackNotFull(AttachEffectData data)
        {
            if (data.MaxStack > 0)
            {
                string name = data.Name;
                if (AEStacks.ContainsKey(name))
                {
                    return AEStacks[name] < data.MaxStack;
                }
            }
            return true;
        }

        private void AddStackCount(AttachEffect ae)
        {
            string name = ae.AEData.Name;
            if (AEStacks.ContainsKey(name))
            {
                AEStacks[name]++;
            }
            else
            {
                AEStacks.Add(name, 1);
            }
        }

        private void ReduceStackCount(AttachEffect ae)
        {
            string name = ae.AEData.Name;
            if (AEStacks.ContainsKey(name))
            {
                AEStacks[name]--;
                if (AEStacks[name] < 1)
                {
                    AEStacks.Remove(name);
                }
            }
        }

        /// <summary>
        /// 根据火车的位置插入AE列表中
        /// </summary>
        /// <param name="ae"></param>
        /// <returns></returns>
        public int FindInsertIndex(AttachEffect ae)
        {
            StandData standData = null;
            if (null != ae.Stand && (standData = ae.Stand.Data).IsTrain)
            {
                int index = -1;
                // 插入头还是尾
                if (standData.CabinHead)
                {
                    // 插入队列末位
                    // 检查是否有分组
                    if (standData.CabinGroup > -1)
                    {
                        // 倒着找自己的分组
                        for (int j = Count() - 1; j >= 0; j--)
                        {
                            AttachEffect temp = AttachEffects[j];
                            Stand tempStand = null;
                            if (null != (tempStand = temp.Stand))
                            {
                                if (standData.CabinGroup == tempStand.Data.CabinGroup)
                                {
                                    // 找到组员
                                    index = j;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // 插入队列首位
                    index = 0;
                    // 检查是否有分组
                    if (standData.CabinGroup > -1)
                    {
                        // 顺着找自己的分组
                        for (int j = 0; j < Count(); j++)
                        {
                            AttachEffect temp = AttachEffects[j];
                            Stand tempStand = null;
                            if (null != (tempStand = temp.Stand))
                            {
                                if (standData.CabinGroup == tempStand.Data.CabinGroup)
                                {
                                    // 找到组员
                                    index = j;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (index > -1)
                {
                    return index;
                }
            }
            return 0;
        }

        private CoordStruct MarkLocation()
        {
            CoordStruct location = pOwner.Ref.Base.GetCoords();
            if (default == lastLocation)
            {
                lastLocation = location;
            }
            double mileage = location.DistanceFrom(lastLocation);
            if (mileage > locationMarkDistance)
            {
                lastLocation = location;
                double tempMileage = totleMileage + mileage;
                // 记录下当前的位置信息
                LocationMark locationMark = pOwner.GetRelativeLocation(default, 0, false, false);
                // 入队
                locationMarks.Insert(0, locationMark);

                // 检查数量超过就删除最后一个
                if (tempMileage > (Count() + 1) * locationSpace)
                {
                    locationMarks.RemoveAt(locationMarks.Count - 1);
                }
                else
                {
                    totleMileage = tempMileage;
                }
            }
            return location;
        }

        /// <summary>
        /// 更新火车替身的车厢位置
        /// </summary>
        /// <param name="stand"></param>
        /// <param name="markIndex"></param>
        /// <returns></returns>
        private bool UpdateTrainStandLocation(Stand stand, ref int markIndex)
        {
            if (stand.Data.IsTrain)
            {
                // 查找可以用的记录点
                double length = 0;
                LocationMark preMark = null;
                for (int j = markIndex; j < locationMarks.Count; j++)
                {
                    markIndex = j;
                    LocationMark mark = locationMarks[j];
                    if (null == preMark)
                    {
                        preMark = mark;
                        continue;
                    }
                    length += mark.Location.DistanceFrom(preMark.Location);
                    preMark = mark;
                    if (length >= locationSpace)
                    {
                        break;
                    }
                }

                if (null != preMark)
                {
                    stand.UpdateLocation(preMark);
                    return true;
                }
            }
            return false;
        }

        public bool HasSpace()
        {
            return totleMileage > Count() * locationSpace;
        }

        public bool HasStand()
        {
            foreach (AttachEffect ae in AttachEffects)
            {
                if (null != ae.Stand && ae.IsActive())
                {
                    return true;
                }
            }
            return false;
        }

        public void EnableAEStatsToStand(int duration, string token, IStateData data, bool resetDuration)
        {
            foreach (AttachEffect ae in AttachEffects)
            {
                Stand stand = ae.Stand;
                if (null != stand && ae.IsActive())
                {
                    Pointer<TechnoClass> pStand = stand.pStand;
                    if (!pStand.IsDead() && pStand.TryGetStatus(out TechnoStatusScript status))
                    {
                        IState state = null;
                        // Logger.Log($"{Game.CurrentFrame} - 同步开启AE {ae.Name} 的替身状态 {data.GetType().Name} token {token}");
                        if (data is DestroySelfData)
                        {
                            // 自毁
                            state = status.DestroySelfState;
                        }
                        else if (data is GiftBoxData)
                        {
                            // 同步礼盒
                            state = status.GiftBoxState;
                        }
                        else if (data is DisableWeaponData)
                        {
                            // 同步禁武
                            state = status.DisableWeaponState;
                        }
                        else if (data is OverrideWeaponData)
                        {
                            // 同步替武
                            state = status.OverrideWeaponState;
                        }
                        else if (data is FireSuperData)
                        {
                            // 同步发射超武
                            state = status.FireSuperState;
                        }
                        else if (data is DeselectData)
                        {
                            // 同步禁止选择
                            state = status.DeselectState;
                        }
                        else if (data is DestroyAnimData)
                        {
                            // 死亡动画
                            state = status.DestroyAnimState;
                        }

                        if (null != state)
                        {
                            if (resetDuration)
                            {
                                state.ResetDuration(token, duration);
                            }
                            else
                            {
                                state.EnableAndReplace(duration, token, data);
                            }
                        }
                    }
                }
            }
        }

        public ImmuneData GetImmageData()
        {
            ImmuneData data = new ImmuneData();
            // 统计AE
            foreach (AttachEffect ae in AttachEffects)
            {
                if (null != ae.Immune && ae.Immune.IsAlive())
                {
                    data.Psionics |= ae.Immune.Data.Psionics;
                    data.PsionicWeapons |= ae.Immune.Data.PsionicWeapons;
                    data.Radiation |= ae.Immune.Data.Radiation;
                    data.Poison |= ae.Immune.Data.Poison;
                    data.EMP |= ae.Immune.Data.EMP;
                    data.Parasite |= ae.Immune.Data.Parasite;
                    data.Temporal |= ae.Immune.Data.Temporal;
                    data.IsLocomotor |= ae.Immune.Data.IsLocomotor;
                }
            }
            data.Enable = data.Psionics || data.PsionicWeapons || data.Radiation || data.Poison || data.EMP || data.Parasite || data.Temporal || data.IsLocomotor;
            return data;
        }

        public CrateBuffData CountAttachStatusMultiplier()
        {
            CrateBuffData multiplier = new CrateBuffData();
            if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno)
                && pTechno.AmIStand(out TechnoStatusScript status, out StandData standData)
                && standData.IsVirtualTurret
                && !status.MyMasterIsAnim
                && !status.MyMaster.IsNull
                && status.MyMaster.TryGetAEManager(out AttachEffectScript aem)
            )
            {
                // 替身是虚拟炮塔，buff加成取JOJO身上的
                multiplier = aem.CountAttachStatusMultiplier();
                return multiplier;
            }
            // 统计AE加成
            foreach (AttachEffect ae in AttachEffects)
            {
                if (null != ae.CrateBuff && ae.CrateBuff.IsAlive())
                {
                    multiplier.FirepowerMultiplier *= ae.CrateBuff.Data.FirepowerMultiplier;
                    multiplier.ArmorMultiplier *= ae.CrateBuff.Data.ArmorMultiplier;
                    multiplier.SpeedMultiplier *= ae.CrateBuff.Data.SpeedMultiplier;
                    multiplier.ROFMultiplier *= ae.CrateBuff.Data.ROFMultiplier;
                    multiplier.Cloakable |= ae.CrateBuff.Data.Cloakable;
                    multiplier.ForceDecloak |= ae.CrateBuff.Data.ForceDecloak;
                    // Logger.Log("Count {0}, ae {1}", multiplier, ae.AttachStatus.Type);
                }
            }
            return multiplier;
        }

        public bool TryGetMarks(out HashSet<string> marks)
        {
            marks = new HashSet<string>();
            // 获取所有标记
            foreach (AttachEffect ae in AttachEffects)
            {
                if (null != ae.Mark && ae.Mark.IsAlive())
                {
                    foreach (string name in ae.Mark.Data.Names)
                    {
                        if (!name.IsNullOrEmptyOrNone())
                        {
                            marks.Add(name);
                        }
                    }
                }
            }
            return marks.Any();
        }

        public bool IsOnMark(string[] marks)
        {
            return TryGetMarks(out HashSet<string> hasMarks) && (marks.Intersect(hasMarks).Count() > 0);
        }

        private bool CheckPassanger(Pointer<TechnoClass> pTechno, out List<int> passengerIds, Found<ObjectClass> foundPassanger = null)
        {
            passengerIds = null;
            if (pTechno.Ref.Passengers.NumPassengers > 0)
            {
                passengerIds = new List<int>();
                // 遍历所有乘客，获得乘客的AEMode序号集
                Pointer<ObjectClass> pPassenger = pTechno.Ref.Passengers.FirstPassenger.Convert<ObjectClass>();
                do
                {
                    if (!pPassenger.IsNull && !pPassenger.IsDead())
                    {
                        // 查找该乘客身上的AEMode设置
                        if (pPassenger.Convert<TechnoClass>().TryGetAEManager(out AttachEffectScript pAEM))
                        {
                            int aeMode = pAEM.aeTypeData.AEMode;
                            if (aeMode >= 0)
                            {
                                passengerIds.Add(aeMode);
                            }
                        }
                        if (null != foundPassanger)
                        {
                            foundPassanger(pPassenger);
                        }
                    }
                } while (!pPassenger.IsNull && !(pPassenger = pPassenger.Ref.NextObject).IsNull);
            }
            return null != passengerIds && passengerIds.Any();
        }

        /// <summary>
        /// 广播渲染事件，调整替身的位置，OnRender不在视野不执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnGScreenRender(object sender, EventArgs args)
        {
            if (((GScreenEventArgs)args).IsBeginRender)
            {
                if (!isDead)
                {
                    location = MarkLocation();
                }
                // 专门执行替身的定位工作
                Dictionary<string, CoordStruct> standPosMarks = new Dictionary<string, CoordStruct>();
                int markIndex = 0;
                for (int i = Count() - 1; i >= 0; i--)
                {
                    AttachEffect ae = AttachEffects[i];
                    if (ae.IsActive())
                    {
                        Stand stand = ae.Stand;
                        if (null != stand && stand.IsAlive())
                        {
                            // 调整位置
                            if (!UpdateTrainStandLocation(stand, ref markIndex))
                            {
                                // 获取挂载对象当前的位置和方向
                                LocationMark locationMark = pObject.GetRelativeLocation(stand.Offset, stand.Data.Direction, stand.Data.IsOnTurret, stand.Data.IsOnWorld);
                                // 堆叠偏移
                                if (default != stand.Data.StackOffset)
                                {
                                    string aeName = ae.AEData.Name;
                                    if (standPosMarks.ContainsKey(aeName))
                                    {
                                        CoordStruct location = standPosMarks[aeName];
                                        location += stand.Data.StackOffset;
                                        locationMark.Location = location;
                                        standPosMarks[aeName] = location;
                                    }
                                    else
                                    {
                                        standPosMarks.Add(aeName, locationMark.Location);
                                    }
                                }
                                stand.UpdateLocation(locationMark);
                            }
                        }
                        ae.OnGScreenRender(location);
                    }
                }
            }
        }

        /// <summary>
        /// 广播变形事件，重新读取类型信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnTransform(object sender, EventArgs args)
        {
            if (!IsBullet)
            {
                Pointer<TechnoClass> pTechno = ((TechnoTypeChangeEventArgs)args).pTechno;
                if (!pTechno.IsNull && pTechno.Convert<ObjectClass>() == pOwner)
                {
                    attachEffectOnceFlag = false;
                    _aeTypeTypeData = null;
                    _aeTypeData = null;
                    // Logger.Log($"{Game.CurrentFrame} [{section}]发生了类型改变，当前类型[{pObject.Ref.Type.Ref.Base.ID}]");
                    // 移除不保留的AE
                    for (int i = Count() - 1; i >= 0; i--)
                    {
                        AttachEffect ae = AttachEffects[i];
                        if (ae.IsActive() && ae.AEData.DiscardOnTransform)
                        {
                            ae.Disable(location);
                            AttachEffects.Remove(ae);
                            ReduceStackCount(ae);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 清理记录的位置信息
        /// </summary>
        public void ClearLocationMark()
        {
            if (null == this.locationMarks)
            {
                this.locationMarks = new List<LocationMark>();
            }
            else
            {
                this.locationMarks.Clear();
            }
            this.lastLocation = default;
            this.totleMileage = 0;
        }

        public override void OnUpdate()
        {
            // 添加Section上记录的AE
            if (!isDead)
            {
                // 记录位置
                location = pOwner.Ref.Base.GetCoords();
                // 检查电力
                if (!IsBullet)
                {
                    PowerOff = pOwner.Convert<TechnoClass>().Ref.Owner.Ref.NoPower;
                    if (!PowerOff && IsBuilding)
                    {
                        // 关闭当前建筑电源
                        PowerOff = !pOwner.Convert<BuildingClass>().Ref.HasPower;
                    }
                }
                // 添加section自带AE，无分组的
                Attach(aeTypeData);
                // 检查乘客并附加乘客带来的AE
                if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
                {
                    CheckPassanger(pTechno, out PassengerIds, (pPassenger) =>
                    {
                        // 由于乘客不会在非OpenTopped的载具内执行update事件，因此由乘客向载具赋予AE的任务由载具执行
                        UploadAttachTypeData uploadTypeData = Ini.GetConfig<UploadAttachTypeData>(Ini.RulesDependency, pPassenger.Ref.Type.Ref.Base.ID).Data;
                        if (uploadTypeData.Enable)
                        {
                            foreach (UploadAttachData loadData in uploadTypeData.Datas.Values)
                            {
                                if (loadData.Enable
                                    && loadData.CanAffectType(pTechno)
                                    && (loadData.AffectInAir || !pTechno.InAir())
                                    && (loadData.AffectStand || !pTechno.AmIStand())
                                    && loadData.IsOnMark(this)
                                )
                                {
                                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pOwner} 获得来自乘客 [{pPassenger.Ref.Type.Ref.Base.ID}]{pPassenger} 的赋予[{string.Join(",", loadData.AttachEffects)}]");
                                    Attach(loadData.AttachEffects, null, pPassenger, default, false, default, -1, loadData.SourceIsPassenger);
                                }
                            }
                        }
                        return false;
                    });
                }
                // 添加分组的
                if (aeTypeTypeData.Enable)
                {
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pOwner} 添加分组AE，一共有{aeTypeTypeData.Datas.Count()}组");
                    foreach (AttachEffectTypeData typeData in aeTypeTypeData.Datas.Values)
                    {
                        if (typeData.AttachByPassenger)
                        {
                            // 需要乘客激活
                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pOwner} 添加分组AE，一共有{aeTypeTypeData.Datas.Count()}组，需要乘客才能激活，收集到乘客ID有 {PassengerIds.Count()} 个，[{string.Join(", ", PassengerIds)}]");
                            int aeMode = typeData.AEModeIndex;
                            if (null != PassengerIds && PassengerIds.Any() && PassengerIds.Contains(aeMode))
                            {
                                // 乘客中有该组的序号
                                Attach(typeData.AttachEffectTypes, null, pOwner, default, false, default, aeMode);
                            }
                        }
                        else
                        {
                            // 不需要乘客激活
                            Attach(typeData.AttachEffectTypes, null, pOwner);
                        }
                    }
                }
                this.attachEffectOnceFlag = true;
            }
            // 逐个触发有效的AEbuff，并移除无效的AEbuff
            for (int i = Count() - 1; i >= 0; i--)
            {
                AttachEffect ae = AttachEffects[i];
                if (ae.IsActive())
                {
                    // Logger.Log($"{Game.CurrentFrame} - {pOwner} [{pOwner.Ref.Type.Ref.Base.ID}] {ae.Type.Name} 执行更新");
                    ae.OnUpdate(location, isDead);
                }
                else
                {
                    AttachEffectData data = ae.AEData;
                    int delay = data.RandomDelay.GetRandomValue(ae.AEData.Delay);
                    if (delay > 0)
                    {
                        DisableDelayTimers[data.Name] = new TimerStruct(delay);
                    }
                    // Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pObject} 持有AE类型[{data.Name}] 失效，从列表中移除，不可再赋予延迟 {delay}");
                    ae.Disable(location);
                    AttachEffects.Remove(ae);
                    ReduceStackCount(ae);
                    // 如果有Next，则赋予新的AE
                    string nextAE = data.Next;
                    if (!string.IsNullOrEmpty(nextAE) && !isDead && !pOwner.IsInvisible())
                    {
                        // Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pObject} 添加AE类型[{data.Name}]的Next类型[{nextAE}]");
                        Attach(nextAE, ae.pSource.Convert<ObjectClass>(), ae.pSourceHouse, false);
                    }
                }
            }
        }

        public override void OnWarpUpdate()
        {
            if (!isDead)
            {
                location = pOwner.Ref.Base.GetCoords();
            }
            for (int i = Count() - 1; i >= 0; i--)
            {
                AttachEffect ae = AttachEffects[i];
                if (ae.IsActive())
                {
                    ae.OnWarpUpdate(location, isDead);
                }
                else
                {
                    AttachEffectData data = ae.AEData;
                    int delay = data.RandomDelay.GetRandomValue(ae.AEData.Delay);
                    if (delay > 0)
                    {
                        DisableDelayTimers[data.Name] = new TimerStruct(delay);
                    }
                    // Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pObject} 持有AE类型[{data.Name}] 失效，从列表中移除，不可再赋予延迟 {delay}");
                    ae.Disable(location);
                    AttachEffects.Remove(ae);
                    ReduceStackCount(ae);
                    // 如果有Next，则赋予新的AE
                    string nextAE = data.Next;
                    if (!string.IsNullOrEmpty(nextAE))
                    {
                        // Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pObject} 添加AE类型[{data.Name}]的Next类型[{nextAE}]");
                        Attach(nextAE, ae.pOwner, ae.pSourceHouse, false);
                    }
                }
            }
        }

        public override void OnTemporalUpdate(Pointer<TemporalClass> pTemporal)
        {
            for (int i = Count() - 1; i >= 0; i--)
            {
                AttachEffect ae = AttachEffects[i];
                if (ae.IsActive())
                {
                    ae.OnTemporalUpdate(pTemporal);
                }
            }
        }

        public override void OnTemporalEliminate(Pointer<TemporalClass> pTemporal)
        {
            // OnUnInit();
            for (int i = Count() - 1; i >= 0; i--)
            {
                AttachEffect ae = AttachEffects[i];
                if (ae.IsActive())
                {
                    ae.OnTemporalEliminate(pTemporal);
                }
            }
        }

        public override void OnRocketExplosion()
        {
            // Logger.Log($"{Game.CurrentFrame} 导弹爆炸");
            foreach (AttachEffect ae in AttachEffects)
            {
                if (ae.IsActive())
                {
                    ae.OnRocketExplosion();
                }
            }
        }

        public override void OnPut(Pointer<CoordStruct> pCoord, ref DirType dirType)
        {
            this.location = pCoord.Data;
            if (!initEffectFlag)
            {
                initEffectFlag = true;
                InitEffect_DamageSelf();
            }
            foreach (AttachEffect ae in AttachEffects)
            {
                if (ae.IsActive())
                {
                    ae.OnPut(pCoord, dirType);
                }
            }
        }

        public override void OnRemove()
        {
            location = pOwner.Ref.Base.GetCoords();
            ClearLocationMark();
            foreach (AttachEffect ae in AttachEffects)
            {
                if (ae.AEData.DiscardOnEntry)
                {
                    ae.Disable(location);
                }
                else
                {
                    if (ae.IsActive())
                    {
                        ae.OnRemove();
                    }
                }
            }
        }

        public override void CanFire(Pointer<AbstractClass> pTarget, Pointer<WeaponTypeClass> pWeapon, ref bool ceaseFire)
        {
            ImmuneData data = null;
            if (!pWeapon.IsNull && !pWeapon.Ref.Warhead.IsNull
                && !pTarget.IsNull && pTarget.CastToTechno(out Pointer<TechnoClass> pTargetTechno)
                && pTargetTechno.TryGetAEManager(out AttachEffectScript aeManager)
                && (data = aeManager.GetImmageData()).Enable)
            {
                // 免疫超时空和磁电
                if ((pWeapon.Ref.Warhead.Ref.Temporal && data.Temporal) || (pWeapon.Ref.Warhead.Ref.IsLocomotor && data.IsLocomotor))
                {
                    ceaseFire = true;
                }
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            Pointer<TechnoClass> pTechno = pOwner.Convert<TechnoClass>();
            Pointer<WeaponStruct> pWeapon = pTechno.Ref.GetWeapon(weaponIndex);
            if (!pWeapon.IsNull && !pWeapon.Ref.WeaponType.IsNull)
            {
                FeedbackAttach(pOwner, pWeapon.Ref.WeaponType);
            }
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int distanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool ignoreDefenses, bool preventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            foreach (AttachEffect ae in AttachEffects)
            {
                if (ae.IsActive())
                {
                    ae.OnReceiveDamage(pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse);
                }
            }

        }

        public override void OnReceiveDamage2(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH,
            DamageState damageState,
            Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            foreach (AttachEffect ae in AttachEffects)
            {
                if (ae.IsActive())
                {
                    ae.OnReceiveDamage2(pRealDamage, pWH, damageState, pAttacker, pAttackingHouse);
                }
            }
        }


        public override void OnDetonate(Pointer<CoordStruct> pCoords, ref bool skip)
        {
            // 不在该事件检索和附加AE，统一用 MapClass_DamageArea 检索和附加
            DestroyAll(pCoords.Data);
        }

        public override void OnReceiveDamageDestroy()
        {
            DestroyAll(pObject.Ref.Base.GetCoords());
        }

        public void DestroyAll(CoordStruct location)
        {
            foreach (AttachEffect ae in AttachEffects)
            {
                if (ae.IsActive())
                {
                    ae.OnReceiveDamageDestroy();
                }
            }
        }

        public override void OnUnInit()
        {
            for (int i = Count() - 1; i >= 0; i--)
            {
                AttachEffect ae = AttachEffects[i];
                if (ae.IsAnyAlive())
                {
                    // Logger.Log($"{Game.CurrentFrame} - {ae.Type.Name} 注销，执行关闭");
                    ae.Disable(lastLocation);
                }
                // Logger.Log($"{Game.CurrentFrame} - {ae.Type.Name} 注销，移出列表");
                AttachEffects.Remove(ae);
                ReduceStackCount(ae);
            }
            AttachEffects.Clear();
            EventSystem.GScreen.RemoveTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
            EventSystem.Techno.RemoveTemporaryHandler(EventSystem.Techno.TypeChangeEvent, OnTransform);
        }

        public override void OnGuardCommand()
        {
            foreach (AttachEffect ae in AttachEffects)
            {
                if (ae.IsActive())
                {
                    ae.OnGuardCommand();
                }
            }
        }

        public override void OnStopCommand()
        {
            foreach (AttachEffect ae in AttachEffects)
            {
                if (ae.IsActive())
                {
                    ae.OnStopCommand();
                }
            }
        }

        public static void FeedbackAttach(Pointer<ObjectClass> pShooter, Pointer<WeaponTypeClass> pWeapon)
        {
            if (!pShooter.IsNull && !pWeapon.IsNull && pShooter.TryGetAEManager(out AttachEffectScript shooterAEM))
            {
                FeedbackAttachTypeData typeData = Ini.GetConfig<FeedbackAttachTypeData>(Ini.RulesDependency, pWeapon.Ref.Base.ID).Data;
                if (typeData.Enable)
                {
                    // Logger.Log($"{Game.CurrentFrame} {pShooter} 通过武器 {pWeapon} 附加 AE，一共 {typeData.Datas.Count} 组");
                    bool isTechno = pShooter.CastToTechno(out Pointer<TechnoClass> pTechno);
                    Pointer<BulletClass> pBullet = IntPtr.Zero;
                    if (!isTechno && pShooter.CastToBullet(out pBullet))
                    {
                        return;
                    }
                    Pointer<TechnoClass> pTransporter = isTechno ? pTechno.Ref.Transporter : default;
                    pTransporter.TryGetAEManager(out AttachEffectScript transporterAEM);
                    // Logger.Log($"{Game.CurrentFrame} {pShooter} 通过武器 {pWeapon} 附加 AE, 附加对象 {(isTechno ? "Techno" : "Bullet")}, 拿到AEM {Pointer<AttachEffectScript>.AsPointer(ref shooterAEM)}");
                    foreach (FeedbackAttachData data in typeData.Datas.Values)
                    {
                        if (data.Enable)
                        {
                            // 检查所属是否平民
                            if (isTechno && data.DeactiveWhenCivilian && pTechno.Ref.Owner.IsCivilian())
                            {
                                // Logger.Log($"{Game.CurrentFrame} {pShooter} 通过武器 {pWeapon} 不能附加，因为是平民");
                                continue;
                            }
                            if (isTechno)
                            {
                                if (data.AffectTechno)
                                {
                                    // 是否在载具内
                                    bool inTransporter = !pTransporter.IsNull;
                                    if (!inTransporter || data.AttachToTransporter)
                                    {
                                        Pointer<TechnoClass> pTempTechno = pTechno;
                                        AttachEffectScript tempAEM = shooterAEM;
                                        if (inTransporter)
                                        {
                                            if (null == transporterAEM)
                                            {
                                                continue;
                                            }
                                            pTempTechno = pTransporter;
                                            tempAEM = transporterAEM;
                                        }
                                        // Logger.Log($"{Game.CurrentFrame} {pShooter} 通过武器 {pWeapon} 准备为 {pTempTechno} 附加AE，载具内 {inTransporter}");
                                        // 检查是否可以影响
                                        if (data.CanAffectType(pTempTechno)
                                            && (data.AffectInAir || !pTempTechno.InAir())
                                            && (data.AffectStand || !pTempTechno.AmIStand())
                                            && data.IsOnMark(tempAEM)
                                        )
                                        {
                                            // Logger.Log($"{Game.CurrentFrame} 为武器发射者 {pTempTechno} 附加AE [{string.Join(", ", data.AttachEffects)}], 管理器{Pointer<AttachEffectScript>.AsPointer(ref tempAEM)}");
                                            tempAEM.Attach(data.AttachEffects, data.AttachChances);
                                        }
                                    }
                                }
                            }
                            else if (data.AffectBullet && !pBullet.IsNull)
                            {
                                if (data.CanAffectType(pBullet) && data.IsOnMark(shooterAEM))
                                {
                                    shooterAEM.Attach(data.AttachEffects, data.AttachChances);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 搜索爆炸位置物体并附加AE
        /// </summary>
        /// <param name="location"></param>
        /// <param name="damage"></param>
        /// <param name="pWH"></param>
        /// <param name="pAttackingHouse"></param>
        /// <param name="exclude"></param>
        public static void FindAndAttach(CoordStruct location, int damage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            AttachEffectTypeData aeTypeData = pWH.GetAEData();
            if (null != aeTypeData.AttachEffectTypes && aeTypeData.AttachEffectTypes.Length > 0)
            {
                bool fullAirspace = aeTypeData.AttachFullAirspace;
                bool findTechno = false;
                bool findBullet = false;
                // 快速检索是否需要查找单位或者抛射体清单
                foreach (string ae in aeTypeData.AttachEffectTypes)
                {
                    AttachEffectData aeData = Ini.GetConfig<AttachEffectData>(Ini.RulesDependency, ae).Data;
                    findTechno |= aeData.AffectTechno;
                    findBullet |= aeData.AffectBullet;
                }

                WarheadTypeData warheadTypeData = pWH.GetData();
                if (findTechno)
                {
                    double cellSpread = pWH.Ref.CellSpread;
                    bool affectInAir = warheadTypeData.AffectInAir;
                    // 检索爆炸范围内的单位类型
                    List<Pointer<TechnoClass>> pTechnoList = FinderHelper.GetCellSpreadTechnos(location, cellSpread, fullAirspace, affectInAir, false);
                    // 检索爆炸范围内的替身
                    if (warheadTypeData.AffectStand)
                    {
                        // 检索爆炸范围内的替身
                        List<Pointer<TechnoClass>> pStandArray = new List<Pointer<TechnoClass>>();
                        foreach (TechnoExt standExt in TechnoStatusScript.StandArray.Keys)
                        {
                            pStandArray.Add(standExt.OwnerObject);
                        }
                        // foreach(TechnoExt standExt in TechnoStatusScript.ImmuneStandArray.Keys)
                        // {
                        //     pStandArray.Add(standExt.OwnerObject);
                        // }
                        HashSet<Pointer<TechnoClass>> pStandList = new HashSet<Pointer<TechnoClass>>();
                        // 过滤掉不在范围内的
                        pStandArray.FindTechno((pTarget) =>
                        {
                            if (affectInAir || !pTarget.Ref.Base.Base.IsInAir())
                            {
                                pStandList.Add(pTarget);
                            }
                            return false;
                        }, location, pWH.Ref.CellSpread);
                        // 合并搜索到的单位和替身清单并去重
                        pTechnoList = pTechnoList.Union(pStandList).ToList<Pointer<TechnoClass>>();
                    }
                    // Logger.Log($"{Game.CurrentFrame} 弹头[{pWH.Ref.Base.ID}] {pWH} 爆炸半径{pWH.Ref.CellSpread}, 影响的单位{pTechnoList.Count()}个，附加AE [{string.Join(", ", aeTypeData.AttachEffectTypes)}]");
                    foreach (Pointer<TechnoClass> pTarget in pTechnoList)
                    {
                        // 检查死亡，过滤掉发射者
                        if (pTarget.IsDeadOrInvisible() || (!warheadTypeData.AffectShooter && pTarget.Convert<ObjectClass>() == pAttacker))
                        {
                            continue;
                        }
                        // 过滤替身和虚单位
                        if (!warheadTypeData.AffectStand && pTarget.TryGetStatus(out var status) && (status.AmIStand() || status.VirtualUnit))
                        {
                            continue;
                        }
                        int distanceFromEpicenter = (int)location.DistanceFrom(pTarget.Ref.Base.Base.GetCoords());
                        Pointer<HouseClass> pTargetHouse = pTarget.Ref.Owner;
                        // Logger.Log($"{Game.CurrentFrame} - 弹头[{pWH.Ref.Base.ID}] {pWH} 可以影响 [{pTarget.Ref.Type.Ref.Base.Base.ID}] {pWH.CanAffectHouse(pAttackingHouse, pTargetHouse, warheadTypeData)}, 可以伤害 {pTarget.CanDamageMe(damage, (int)distanceFromEpicenter, pWH, out int r)}, 实际伤害 {r}");
                        // 可影响可伤害
                        if (pWH.CanAffectHouse(pAttackingHouse, pTargetHouse, warheadTypeData)// 检查所属权限
                            && pTarget.CanDamageMe(damage, (int)distanceFromEpicenter, pWH, out int realDamage)// 检查护甲
                        )
                        {
                            // 赋予AE
                            if (pTarget.TryGetAEManager(out AttachEffectScript aeManager))
                            {
                                // Logger.Log($"{Game.CurrentFrame} - 弹头[{pWH.Ref.Base.ID}] {pWH} 为 [{pTarget.Ref.Type.Ref.Base.Base.ID}]{pTarget} 附加AE [{string.Join(", ", aeTypeData.AttachEffectTypes)}] Attacker {pAttacker} AttackingHouse {pAttackingHouse} ");
                                aeManager.Attach(aeTypeData.AttachEffectTypes, aeTypeData.AttachEffectChances, pAttacker, pAttackingHouse, false, location);
                            }
                        }
                    }
                }

                // 检索爆炸范围内的抛射体类型
                if (findBullet)
                {
                    BulletClass.Array.FindObject((pTarget) =>
                    {
                        if (!pTarget.IsDeadOrInvisible() && (warheadTypeData.AffectShooter || pTarget.Convert<ObjectClass>() != pAttacker))
                        {
                            // 可影响
                            Pointer<HouseClass> pTargetSourceHouse = pTarget.GetSourceHouse();
                            if (pWH.CanAffectHouse(pAttackingHouse, pTargetSourceHouse, warheadTypeData))
                            {
                                // 赋予AE
                                if (pTarget.TryGetAEManager(out AttachEffectScript aeManager))
                                {
                                    aeManager.Attach(aeTypeData.AttachEffectTypes, aeTypeData.AttachEffectChances, pAttacker, pAttackingHouse, false, location);
                                }
                            }
                        }
                        return false;
                    }, location, pWH.Ref.CellSpread, 0, fullAirspace);
                }
            }
        }

    }

}

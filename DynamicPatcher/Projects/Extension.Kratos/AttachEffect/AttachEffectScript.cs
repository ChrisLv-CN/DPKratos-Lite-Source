using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

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

        public List<AttachEffect> AttachEffects; // 所有有效的AE
        public Dictionary<string, TimerStruct> DisableDelayTimers; // 同名AE失效后再赋予的计时器

        private CoordStruct location;

        private List<LocationMark> locationMarks;
        private CoordStruct lastLocation; // 使者的上一次位置
        private int locationMarkDistance; // 多少格记录一个位置
        private double totleMileage; // 总里程

        private AttachEffectTypeData aeTypeData => Ini.GetConfig<AttachEffectTypeData>(Ini.RulesDependency, section).Data;
        private bool attachEffectOnceFlag = false; // 已经在Update事件中附加过一次section上写的AE
        private bool renderFlag = false; // Render比Update先执行，在附着对象Render时先调整替身位置，Update就不用调整
        private bool isDead = false;

        private int locationSpace; // 替身火车的车厢间距

        private bool initEffectFlag;

        // 将AE转移给其他对象
        public void InheritedTo(AttachEffectScript heir)
        {
            // 转移给继任者
            heir.AttachEffects = this.AttachEffects;
            heir.DisableDelayTimers = this.DisableDelayTimers;
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
                    }
                }
            }

            heir.locationMarks = this.locationMarks;
            heir.locationMarkDistance = this.locationMarkDistance;
            heir.totleMileage = this.totleMileage;

            heir.attachEffectOnceFlag = this.attachEffectOnceFlag;

            heir.locationSpace = this.locationSpace;

            // 转移完成后，重置
            Awake();
        }

        public override void Awake()
        {
            this.AttachEffects = new List<AttachEffect>();
            this.DisableDelayTimers = new Dictionary<string, TimerStruct>();

            this.location = pOwner.Ref.Base.GetCoords();

            this.locationMarks = new List<LocationMark>();
            this.locationMarkDistance = 16;
            this.totleMileage = 0;

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
        /// 弹头爆炸赋予受害者AE
        /// </summary>
        /// <param name="typeData">section的AE清单</param>
        /// <param name="pSource">AE来源，即攻击者</param>
        /// <param name="pSourceHouse">来源所属</param>
        /// <param name="fromWarhead">来自弹头，attachEffectOnceFlag应该传false</param>
        public void Attach(AttachEffectTypeData typeData, Pointer<ObjectClass> pSource, Pointer<HouseClass> pSourceHouse = default, bool fromWarhead = false)
        {
            // 清单中有AE类型
            if (null != typeData.AttachEffectTypes && typeData.AttachEffectTypes.Length > 0)
            {
                Attach(typeData.AttachEffectTypes, pSource, pSourceHouse, !fromWarhead && attachEffectOnceFlag);
            }

            if (typeData.StandTrainCabinLength > 0)
            {
                SetLocationSpace(typeData.StandTrainCabinLength);
            }
        }

        /// <summary>
        /// 遍历AE清单并逐个附加
        /// </summary>
        /// <param name="aeTypes"></param>
        /// <param name="pSource"></param>
        /// <param name="pSourceHouse">来源所属</param>
        /// <param name="attachOnceFlag"></param>
        public void Attach(string[] aeTypes, Pointer<ObjectClass> pSource = default, Pointer<HouseClass> pSourceHouse = default, bool attachOnceFlag = false)
        {
            if (null != aeTypes && aeTypes.Length > 0)
            {
                // Logger.Log($"{Game.CurrentFrame} 为 [{section}]{pOwner} 附加 AE 清单 [{string.Join(",", aeTypes)}]. attachOnceFlag = {attachOnceFlag}");
                foreach (string type in aeTypes)
                {
                    Attach(type, pSource, pSourceHouse, attachOnceFlag);
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
        public void Attach(string type, Pointer<ObjectClass> pSource = default, Pointer<HouseClass> pSourceHouse = default, bool attachOnceFlag = false)
        {
            IConfigWrapper<AttachEffectData> aeDate = Ini.GetConfig<AttachEffectData>(Ini.RulesDependency, type);
            if (attachOnceFlag && aeDate.Data.AttachOnceInTechnoType)
            {
                return;
            }
            // Logger.Log("AE {0} AttachOnceInTechnoType = {1}, AttachOnceFlag = {2}", aeType.Name, aeType.AttachOnceInTechnoType, attachOnceFlag);
            Attach(aeDate.Data, pSource, pSourceHouse);
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
        public void Attach(AttachEffectData data, Pointer<ObjectClass> pSource, Pointer<HouseClass> pSourceHouse = default)
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
            if (!IsOnMark(data))
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
            if (pOwner != pSource && data.OwnerTarget)
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
            if (!pAttacker.IsDead() && data.FromTransporter)
            {
                pAttacker = pAttacker.WhoIsShooter();
            }
            // 检查叠加
            bool add = data.Cumulative == CumulativeMode.YES;
            if (!add)
            {
                // 不同攻击者是否叠加
                bool isAttackMark = data.Cumulative == CumulativeMode.ATTACKER && !pAttacker.IsNull && pAttacker.Ref.Base.IsAlive;
                // 攻击者标记AE名称相同，但可以来自不同的攻击者，可以叠加，不检查Delay
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
                            // 找到了
                            find = true;
                            if (isAttackMark)
                            {
                                if (temp.pSource.Pointer == pAttacker)
                                {
                                    // 是攻击者标记，且相同的攻击者，重置持续时间
                                    if (temp.AEData.ResetDurationOnReapply)
                                    {
                                        temp.ResetDuration();
                                        AttachEffects[i] = temp;
                                    }
                                    break;
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
            if (add && data.GetDuration() != 0)
            {
                AttachEffect ae = data.CreateAE();
                // 入队
                int index = FindInsertIndex(ae);
                // Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pObject} 添加AE类型[{data.Name}]，加入队列，插入位置{index}, 持续时间 {data.GetDuration()}, 来源 {(pAttacker.IsNull ? "" : pAttacker.Ref.Type.Ref.Base.Base.ID)} {pAttacker}, 所属 {pAttackingHouse}");
                AttachEffects.Insert(index, ae);
                // 激活
                ae.Enable(this, pAttacker, pAttackingHouse);
            }
        }

        // 关闭并移除指定名称的AE
        public void Remove(string[] aeTypes)
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
                        DisableDelayTimers.Remove(ae.AEData.Name);
                    }
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

        private void UpdateStandLocation(Stand stand, ref int markIndex)
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
                    LocationMark forward = preMark;
                    int forwardIndex = markIndex - 1;
                    if (forwardIndex > -1 && forwardIndex < locationMarks.Count)
                    {
                        forward = locationMarks[forwardIndex];
                    }
                    stand.UpdateLocation(preMark, forward);
                    return;
                }
            }
            // 获取挂载对象的位置和方向
            LocationMark locationMark = pObject.GetRelativeLocation(stand.Data.Offset, stand.Data.Direction, stand.Data.IsOnTurret, stand.Data.IsOnWorld);
            stand.UpdateLocation(locationMark, locationMark);
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

        public void EnableAEStatsToStand(int duration, string token, IStateData data)
        {
            foreach (AttachEffect ae in AttachEffects)
            {
                Stand stand = ae.Stand;
                if (null != stand && ae.IsActive())
                {
                    Pointer<TechnoClass> pStand = stand.pStand;
                    if (pStand.TryGetStatus(out TechnoStatusScript status))
                    {
                        // Logger.Log($"{Game.CurrentFrame} - 同步开启AE {ae.Name} 的替身状态 {data.GetType().Name} token {token}");
                        if (data is DestroySelfData)
                        {
                            // 自毁
                            status.DestroySelfState.Enable(duration, token, data);
                        }
                        else if (data is GiftBoxData)
                        {
                            // 同步礼盒
                            status.GiftBoxState.Enable(duration, token, data);
                        }
                        else if (data is DisableWeaponData)
                        {
                            // 同步禁武
                            status.DisableWeaponState.Enable(duration, token, data);
                        }
                        else if (data is OverrideWeaponData)
                        {
                            // 同步替武
                            status.OverrideWeaponState.Enable(duration, token, data);
                        }
                        else if (data is FireSuperData)
                        {
                            // 同步发射超武
                            status.FireSuperState.Enable(duration, token, data);
                        }
                        else if (data is DeselectData)
                        {
                            // 同步禁止选择
                            status.DeselectState.Enable(duration, token, data);
                        }
                        else if (data is RevengeData)
                        {
                            // 同步复仇
                            status.RevengeState.Enable(duration, token, data);
                        }
                    }
                }
            }
        }

        public CrateBuffData CountAttachStatusMultiplier()
        {
            CrateBuffData multiplier = new CrateBuffData();
            if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno)
                && pTechno.AmIStand(out TechnoStatusScript status, out StandData standData)
                && standData.IsVirtualTurret
                && !status.MyMasterIsAnim
                && !status.MyMaster.IsNull
                && status.MyMaster.Pointer.TryGetAEManager(out AttachEffectScript aem)
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

        private bool IsOnMark(AttachEffectData data)
        {
            return null == data.OnlyAffectMarks || !data.OnlyAffectMarks.Any()
                || (TryGetMarks(out HashSet<string> marks)
                    && (data.OnlyAffectMarks.Intersect(marks).Count() > 0)
                );
        }

        public override void OnRender()
        {
            isDead = pObject.IsDead();
            location = pOwner.Ref.Base.GetCoords();
            for (int i = Count() - 1; i >= 0; i--)
            {
                AttachEffect ae = AttachEffects[i];
                if (ae.IsActive())
                {
                    ae.OnRender(location);
                }
            }
        }

        public override void OnRenderEnd()
        {
            renderFlag = !isDead;
            if (renderFlag)
            {
                // 记录下位置
                location = MarkLocation();
                // 更新替身的位置
                int markIndex = 0;
                for (int i = Count() - 1; i >= 0; i--)
                {
                    AttachEffect ae = AttachEffects[i];
                    if (ae.IsActive())
                    {
                        // 如果是替身，额外执行替身的定位操作
                        if (null != ae.Stand && ae.Stand.IsAlive())
                        {
                            UpdateStandLocation(ae.Stand, ref markIndex); // 调整位置
                        }
                        ae.OnRenderEnd(location);
                    }
                }
            }
        }

        public override void OnUpdate()
        {
            isDead = pObject.IsDead();
            // 添加Section上记录的AE
            if (!isDead && !pObject.IsInvisible() && null != aeTypeData)
            {
                Attach(aeTypeData, pOwner);
                this.attachEffectOnceFlag = true;
            }

            // 记录下位置
            if (renderFlag)
            {
                location = pOwner.Ref.Base.GetCoords();
            }
            else
            {
                location = MarkLocation();
            }
            // 逐个触发有效的AEbuff，并移除无效的AEbuff
            int markIndex = 0;
            for (int i = Count() - 1; i >= 0; i--)
            {
                AttachEffect ae = AttachEffects[i];
                if (ae.IsActive())
                {
                    if (!renderFlag && null != ae.Stand && ae.Stand.IsAlive())
                    {
                        // 替身不需要渲染时，在update中调整替身的位置
                        UpdateStandLocation(ae.Stand, ref markIndex);
                    }
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
                    // 如果有Next，则赋予新的AE
                    string nextAE = data.Next;
                    if (!string.IsNullOrEmpty(nextAE))
                    {
                        // Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pObject} 添加AE类型[{data.Name}]的Next类型[{nextAE}]");
                        Attach(nextAE, ae.pOwner, ae.pSourceHouse, false);
                    }
                }
            }
            renderFlag = false;
        }

        public override void OnLateUpdate()
        {
            isDead = pObject.IsDead();
            location = pOwner.Ref.Base.GetCoords();
            for (int i = Count() - 1; i >= 0; i--)
            {
                AttachEffect ae = AttachEffects[i];
                if (ae.IsActive())
                {
                    ae.OnLateUpdate(location, isDead);
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

        public override void OnDetonate(Pointer<CoordStruct> pCoords)
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
            }
            AttachEffects.Clear();
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

        /// <summary>
        /// 搜索爆炸位置物体并附加AE
        /// </summary>
        /// <param name="location"></param>
        /// <param name="damage"></param>
        /// <param name="pWH"></param>
        /// <param name="pAttackingHouse"></param>
        /// <param name="exclude"></param>
        public static void FindAndAttach(CoordStruct location, int damage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse, Pointer<ObjectClass> exclude = default)
        {
            AttachEffectTypeData aeTypeData = Ini.GetConfig<AttachEffectTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID).Data;
            if (null != aeTypeData.AttachEffectTypes && aeTypeData.AttachEffectTypes.Length > 0)
            {
                bool findTechno = false;
                bool findBullet = false;
                // 快速检索是否需要查找单位或者抛射体清单
                foreach (string ae in aeTypeData.AttachEffectTypes)
                {
                    AttachEffectData aeData = Ini.GetConfig<AttachEffectData>(Ini.RulesDependency, ae).Data;
                    findTechno |= aeData.AffectTechno;
                    findBullet |= aeData.AffectBullet;
                }

                WarheadTypeData warheadTypeData = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID).Data;
                if (findTechno)
                {
                    // 检索爆炸范围内的单位类型
                    List<Pointer<TechnoClass>> pTechnoList = FinderHelper.GetCellSpreadTechnos(location, pWH.Ref.CellSpread, warheadTypeData.AffectInAir, false);

                    // Logger.Log($"{Game.CurrentFrame} 弹头[{pWH.Ref.Base.ID}] {pWH} 爆炸半径{pWH.Ref.CellSpread}, 影响的单位{pTechnoList.Count()}个，附加AE [{string.Join(", ", aeTypeData.AttachEffectTypes)}]");
                    foreach (Pointer<TechnoClass> pTarget in pTechnoList)
                    {
                        // 检查死亡
                        if (pTarget.IsDeadOrInvisible() || pTarget.Convert<ObjectClass>() == exclude)
                        {
                            continue;
                        }
                        // 过滤替身和虚单位
                        if (pTarget.TryGetStatus(out var status) && (!status.MyMaster.IsNull || status.MyMasterIsAnim || status.VirtualUnit))
                        {
                            continue;
                        }
                        int distanceFromEpicenter = (int)location.DistanceFrom(pTarget.Ref.Base.Base.GetCoords());
                        Pointer<HouseClass> pTargetHouse = pTarget.Ref.Owner;
                        // Logger.Log($"{Game.CurrentFrame} - 弹头[{pWH.Ref.Base.ID}] {pWH} 可以影响 [{pTarget.Ref.Type.Ref.Base.Base.ID}] {pWH.CanAffectHouse(pAttackingHouse, pTargetHouse, warheadTypeData)}, 可以伤害 {pTarget.CanDamageMe(damage, (int)distanceFromEpicenter, pWH, out int r)}, 实际伤害 {r}");
                        // 可影响可伤害
                        if (pWH.CanAffectHouse(pAttackingHouse, pTargetHouse, warheadTypeData)// 检查所属权限
                            && pTarget.CanDamageMe(damage, (int)distanceFromEpicenter, pWH, out int realDamage)// 检查护甲
                            && (pTarget.Ref.Base.Health - realDamage) > 0 // 收到本次伤害后会死，就不再进行赋予
                        )
                        {
                            // 赋予AE
                            if (pTarget.TryGetAEManager(out AttachEffectScript aeManager))
                            {
                                // Logger.Log($"{Game.CurrentFrame} - 弹头[{pWH.Ref.Base.ID}] {pWH} 为 [{pTarget.Ref.Type.Ref.Base.Base.ID}]{pTarget} 附加AE [{string.Join(", ", aeTypeData.AttachEffectTypes)}] Attacker {pAttacker} AttackingHouse {pAttackingHouse} ");
                                aeManager.Attach(aeTypeData, pAttacker, pAttackingHouse, true);
                            }
                        }
                    }
                }

                // 检索爆炸范围内的抛射体类型
                if (findBullet)
                {
                    BulletClass.Array.FindObject((pTarget) =>
                    {
                        if (!pTarget.IsDeadOrInvisible() && pTarget.Convert<ObjectClass>() != exclude)
                        {
                            // 可影响
                            Pointer<HouseClass> pTargetSourceHouse = pTarget.GetSourceHouse();
                            if (pWH.CanAffectHouse(pAttackingHouse, pTargetSourceHouse, warheadTypeData))
                            {
                                // 赋予AE
                                if (pTarget.TryGetAEManager(out AttachEffectScript aeManager))
                                {
                                    aeManager.Attach(aeTypeData, pAttacker, pAttackingHouse, true);
                                }
                            }
                        }
                        return false;
                    }, location, pWH.Ref.CellSpread);
                }
            }
        }

    }

}

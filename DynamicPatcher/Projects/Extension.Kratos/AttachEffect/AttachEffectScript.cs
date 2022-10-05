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
    [UpdateAfter(typeof(TechnoStatusScript), typeof(BulletStatusScript))]
    public class AttachEffectScript : ObjectScriptable
    {
        public AttachEffectScript(IExtension owner) : base(owner) { }

        public Pointer<ObjectClass> pOwner => pObject;

        public List<AttachEffect> AttachEffects; // 所有有效的AE
        public Dictionary<string, TimerStruct> DisableDelayTimers; // 同名AE失效后再赋予的计时器

        private List<LocationMark> locationMarks;
        private CoordStruct lastLocation; // 使者的上一次位置
        private int locationMarkDistance; // 多少格记录一个位置
        private double totleMileage; // 总里程

        private bool attachEffectOnceFlag = false; // 已经在Update事件中附加过一次section上写的AE
        private bool renderFlag = false; // Render比Update先执行，在附着对象Render时先调整替身位置，Update就不用调整
        private bool isDead = false;

        private int locationSpace; // 替身火车的车厢间距

        // 将AE转移给其他对象
        public void InheritedTo(AttachEffectScript heir)
        {
            heir.AttachEffects = this.AttachEffects;
            heir.DisableDelayTimers = this.DisableDelayTimers;

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

            this.locationMarks = new List<LocationMark>();
            this.locationMarkDistance = 16;
            this.totleMileage = 0;

            this.locationSpace = 512;
        }

        public int Count()
        {
            return AttachEffects.Count;
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
        /// </summary>
        /// <param name="typeData">section的AE清单</param>
        /// <param name="pHouse">强制所属</param>
        public void Attach(AttachEffectTypeData typeData, bool fromDamage = false)
        {
            // 清单中有AE类型
            if (null != typeData.AttachEffectTypes && typeData.AttachEffectTypes.Length > 0)
            {
                // 写在type上附加的AE，所属是自己，攻击者是自己
                Pointer<HouseClass> pHouse = IntPtr.Zero;
                Pointer<TechnoClass> pAttacker = IntPtr.Zero;
                if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
                {
                    pHouse = pTechno.Ref.Owner;
                    pAttacker = pTechno;
                }
                else if (pOwner.CastToBullet(out Pointer<BulletClass> pBullet))
                {
                    pHouse = pBullet.GetSourceHouse();
                    pAttacker = pBullet.Ref.Owner;
                }
                Attach(typeData.AttachEffectTypes, pHouse, pAttacker, attachEffectOnceFlag, fromDamage);
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
        /// <param name="pHouse"></param>
        /// <param name="attachOnceFlag"></param>
        public void Attach(string[] aeTypes, Pointer<HouseClass> pHouse, Pointer<TechnoClass> pAttacker, bool attachOnceFlag, bool fromDamage = false)
        {
            if (null != aeTypes && aeTypes.Length > 0)
            {
                foreach (string type in aeTypes)
                {
                    // Logger.Log("事件{0}添加AE类型{1}", onUpdate ? "OnUpdate" : "OnInit", type);
                    Attach(type, pHouse, pAttacker, attachOnceFlag, fromDamage);
                }
            }
        }

        /// <summary>
        /// 按照AE的section来添加AE
        /// </summary>
        /// <param name="type">AE的section</param>
        /// <param name="pHouse">强制所属</param>
        /// <param name="pAttacker">AE来源</param>
        /// <param name="attachOnceFlag"></param>
        public void Attach(string type, Pointer<HouseClass> pHouse, Pointer<TechnoClass> pAttacker, bool attachOnceFlag, bool fromDamage = false)
        {
            IConfigWrapper<AttachEffectData> aeDate = Ini.GetConfig<AttachEffectData>(Ini.RulesDependency, type);
            if (attachOnceFlag && aeDate.Data.AttachOnceInTechnoType)
            {
                return;
            }
            // Logger.Log("AE {0} AttachOnceInTechnoType = {1}, AttachOnceFlag = {2}", aeType.Name, aeType.AttachOnceInTechnoType, attachOnceFlag);
            Attach(aeDate.Data, pHouse, pAttacker, fromDamage);
        }

        /// <summary>
        /// 附加AE
        /// </summary>
        /// <param name="aeData">要附加的AE类型</param>
        public void Attach(AttachEffectData aeData, bool fromDamage = false)
        {
            // 写在type上附加的AE，所属是自己，攻击者是自己
            Pointer<HouseClass> pHouse = IntPtr.Zero;
            Pointer<TechnoClass> pAttacker = IntPtr.Zero;
            if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                pHouse = pTechno.Ref.Owner;
                pAttacker = pTechno;
            }
            else if (pOwner.CastToBullet(out Pointer<BulletClass> pBullet))
            {
                pHouse = pBullet.GetSourceHouse();
                pAttacker = pBullet.Ref.Owner;
            }
            Attach(aeData, pHouse, pAttacker, fromDamage);
        }

        /// <summary>
        /// 附加AE
        /// </summary>
        /// <param name="aeData">要附加的AE类型</param>
        /// <param name="pHouse">指定的所属</param>
        /// <param name="pAttacker">来源</param>
        public void Attach(AttachEffectData data, Pointer<HouseClass> pHouse, Pointer<TechnoClass> pAttacker, bool fromDamage)
        {
            if (!data.Enable)
            {
                Logger.LogWarning($"Attempt to attach an invalid AE [{data.Name}] to [{section}]");
                return;
            }
            // 是否在名单上
            if (!data.CanAffectType(pOwner))
            {
                // Logger.Log($"{Game.CurrentFrame} 单位 [{section}] 不在AE [{aeData.Data.Name}] 的名单内，不能赋予");
                return;
            }
            // 通过伤害赋予的AE没有通过伤害进行赋予，不能赋予
            if (data.AttachWithDamage != fromDamage)
            {
                // Logger.Log($"{Game.CurrentFrame} AE类 [{aeData.Data.Name}] 仅能赋予伤害来源{data.AttachWithDamage}与当前来源{fromDamage}不符，不能赋予单位 [{section}]");
                return;
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
                        if (temp.AEData == data)
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
                                CoordStruct location = pOwner.Ref.Base.GetCoords();
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
                // Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pObject} 添加AE类型[{data.Name}]，加入队列，插入位置{index}");
                AttachEffects.Insert(index, ae);
                // 激活
                ae.Enable(pOwner, pHouse, pAttacker);
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
                    stand.UpdateLocation(preMark);
                    return;
                }
            }
            // 获取挂载对象的位置和方向
            LocationMark locationMark = pObject.GetRelativeLocation(stand.Data.Offset, stand.Data.Direction, stand.Data.IsOnTurret, stand.Data.IsOnWorld);
            stand.UpdateLocation(locationMark);
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

        public CrateBuffData CountAttachStatusMultiplier()
        {
            CrateBuffData multiplier = new CrateBuffData();
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

        public override void OnRender()
        {
            isDead = pObject.IsDead();
            CoordStruct location = pOwner.Ref.Base.GetCoords();
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
                CoordStruct location = MarkLocation();
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
            if (!pObject.IsDeadOrInvisible())
            {
                AttachEffectTypeData aeTypeData = Ini.GetConfig<AttachEffectTypeData>(Ini.RulesDependency, section).Data;
                Attach(aeTypeData);
                this.attachEffectOnceFlag = true;
            }

            // 记录下位置
            CoordStruct location = pOwner.Ref.Base.GetCoords();
            if (!renderFlag)
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
                        Attach(nextAE, ae.pSourceHouse, ae.pSource, false);
                    }
                }
            }
            renderFlag = false;
        }

        public override void OnLateUpdate()
        {
            isDead = pObject.IsDead();
            CoordStruct location = pOwner.Ref.Base.GetCoords();
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

        public override void OnPut(Pointer<CoordStruct> pCoord, DirType dirType)
        {
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
            CoordStruct location = pOwner.Ref.Base.GetCoords();
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
            // 受到伤害，为我自己添加AE
            AttachEffectTypeData aeTypeData = Ini.GetConfig<AttachEffectTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID).Data;
            if (null != aeTypeData.AttachEffectTypes && aeTypeData.AttachEffectTypes.Length > 0)
            {
                if (pObject.CastToTechno(out Pointer<TechnoClass> pTechno))
                {
                    if (pTechno.CanAffectMe(pAttackingHouse, pWH) && pTechno.CanDamageMe(pDamage.Data, distanceFromEpicenter, pWH, out int realDamage))
                    {
                        if (!pAttacker.CastToTechno(out Pointer<TechnoClass> pAttackerTechno))
                        {
                            pAttackerTechno = pTechno;
                        }
                        Attach(aeTypeData.AttachEffectTypes, pAttackingHouse, pAttackerTechno, false, true);
                        if (aeTypeData.StandTrainCabinLength > 0)
                        {
                            SetLocationSpace(aeTypeData.StandTrainCabinLength);
                        }
                    }
                }
                else if (pObject.CastToBullet(out Pointer<BulletClass> pBullet))
                {
                    Pointer<HouseClass> pSourceHouse = pBullet.GetSourceHouse();
                    if (pWH.CanAffectHouse(pSourceHouse, pAttackingHouse))
                    {
                        if (!pAttacker.CastToTechno(out Pointer<TechnoClass> pAttackerTechno))
                        {
                            pAttackerTechno = pBullet.Ref.Owner;
                        }
                        Attach(aeTypeData.AttachEffectTypes, pAttackingHouse, pAttackerTechno, false, true);
                        if (aeTypeData.StandTrainCabinLength > 0)
                        {
                            SetLocationSpace(aeTypeData.StandTrainCabinLength);
                        }
                    }
                }
            }
            foreach (AttachEffect ae in AttachEffects)
            {
                if (ae.IsActive())
                {
                    ae.OnReceiveDamage(pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse);
                }
            }

        }

        /// <summary>
        /// 抛射体爆炸，搜索附近单位并为他们附加AE
        /// </summary>
        /// <param name="pCoords"></param>
        public override void OnDetonate(Pointer<CoordStruct> pCoords)
        {

            Pointer<BulletClass> pBullet = pOwner.Convert<BulletClass>();
            Pointer<WarheadTypeClass> pWH = pBullet.Ref.WH;
            if (!pWH.IsNull)
            {
                AttachEffectTypeData aeTypeData = Ini.GetConfig<AttachEffectTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID).Data;
                if (null != aeTypeData.AttachEffectTypes && aeTypeData.AttachEffectTypes.Length > 0)
                {
                    // 爆炸的位置离目标位置非常近视为命中
                    CoordStruct targetLocation = pCoords.Data;
                    // 炸膛或者垂直抛射体不检查与目标的贴近距离
                    bool snapped = pBullet.Ref.Type.Ref.Dropping || pBullet.Ref.Type.Ref.Vertical;
                    int snapDistance = 64;
                    Pointer<AbstractClass> pBulletTarget = pBullet.Ref.Target;
                    if (!pBulletTarget.IsNull && pBullet.Ref.Base.DistanceFrom(pBulletTarget.Convert<ObjectClass>()) < snapDistance)
                    {
                        targetLocation = pBulletTarget.Convert<AbstractClass>().Ref.GetCoords();
                        snapped = true;
                    }
                    if (!pBullet.TryGetStatus(out BulletStatusScript status) || !status.LifeData.SkipAE)
                    {
                        // 抛射体所属阵营
                        Pointer<HouseClass> pAttackingHouse = pBullet.GetSourceHouse();
                        // 搜索并附加效果
                        FindAndAttach(targetLocation, pBullet.Ref.Base.Health, pWH, pAttackingHouse, pObject);
                    }
                }
            }
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
            CoordStruct location = pOwner.Ref.Base.GetCoords();
            for (int i = Count() - 1; i >= 0; i--)
            {
                AttachEffect ae = AttachEffects[i];
                if (ae.IsAnyAlive())
                {
                    // Logger.Log($"{Game.CurrentFrame} - {ae.Type.Name} 注销，执行关闭");
                    ae.Disable(location);
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
        public static void FindAndAttach(CoordStruct location, int damage, Pointer<WarheadTypeClass> pWH, Pointer<HouseClass> pAttackingHouse, Pointer<ObjectClass> exclude = default)
        {
            AttachEffectTypeData aeTypeData = Ini.GetConfig<AttachEffectTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID).Data;
            if (null != aeTypeData.AttachEffectTypes && aeTypeData.AttachEffectTypes.Length > 0)
            {
                WarheadTypeData warheadTypeData = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID).Data;

                // 检索爆炸范围内的单位类型
                List<Pointer<TechnoClass>> pTechnoList = ExHelper.GetCellSpreadTechnos(location, pWH.Ref.CellSpread, warheadTypeData.AffectsAir, false);
                // Logger.Log("弹头{0}半径{1}, 影响的单位{2}个", pWH.Ref.Base.ID, pWH.Ref.CellSpread, pTechnoList.Count);
                foreach (Pointer<TechnoClass> pTarget in pTechnoList)
                {
                    // 检查死亡
                    if (pTarget.IsDeadOrInvisible() || pTarget.Convert<ObjectClass>() == exclude)
                    {
                        continue;
                    }

                    int distanceFromEpicenter = (int)location.DistanceFrom(pTarget.Ref.Base.Location);
                    Pointer<HouseClass> pTargetHouse = pTarget.Ref.Owner;
                    // 可影响可伤害
                    if (pWH.CanAffectHouse(pAttackingHouse, pTargetHouse, warheadTypeData)// 检查所属权限
                        && pTarget.CanDamageMe(damage, (int)distanceFromEpicenter, pWH, out int realDamage)// 检查护甲
                        && (pTarget.Ref.Base.Health - realDamage) > 0 // 收到本次伤害后会死，就不再进行赋予
                    )
                    {
                        // 赋予AE
                        if (pTarget.TryGetAEManager(out AttachEffectScript aeManager))
                        {
                            aeManager.Attach(aeTypeData);
                        }
                    }
                }

                // 检索爆炸范围内的抛射体类型
                if (warheadTypeData.AffectsBullet)
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
                                    aeManager.Attach(aeTypeData);
                                }
                            }
                        }
                        return false;
                    }, location, pWH.Ref.CellSpread);
                }
            }
        }

        /// <summary>
        /// 查找爆炸位置的替身并对其造成伤害
        /// </summary>
        /// <param name="location"></param>
        /// <param name="damage"></param>
        /// <param name="pAttacker"></param>
        /// <param name="pWH"></param>
        /// <param name="pAttackingHouse"></param>
        /// <param name="exclue"></param>
        public static void FindAndDamageStand(CoordStruct location, int damage, Pointer<ObjectClass> pAttacker,
           Pointer<WarheadTypeClass> pWH, Pointer<HouseClass> pAttackingHouse, Pointer<ObjectClass> exclude = default)
        {
            // 虽然不知道为什么但是有可能会出现空指针
            if (pWH.IsNull)
            {
                return;
            }

            double spread = pWH.Ref.CellSpread * 256;

            HashSet<DamageGroup> stands = new HashSet<DamageGroup>();
            TechnoClass.Array.FindObject((pTechno) =>
            {
                // Stand always not on map.
                if (!pTechno.IsDeadOrInvisible() && pTechno.Convert<ObjectClass>() != exclude && !pTechno.Ref.Base.IsOnMap && !(pTechno.Ref.Base.IsIronCurtained() || pTechno.Ref.IsForceShilded))
                {
                    if (pTechno.AmIStand(out StandData standData))
                    {
                        // 检查距离
                        CoordStruct targetPos = pTechno.Ref.Base.Base.GetCoords();
                        double dist = targetPos.DistanceFrom(location);
                        if (pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Aircraft && pTechno.InAir(true))
                        {
                            dist *= 0.5;
                        }
                        if (dist <= spread)
                        {
                            // 找到一个最近的替身，检查替身是否可以受伤，以及弹头是否可以影响该替身
                            if (!standData.Immune
                                && pTechno.CanAffectMe(pAttackingHouse, pWH)// 检查所属权限
                                && pTechno.CanDamageMe(damage, (int)dist, pWH, out int realDamage)// 检查护甲
                            )
                            {
                                DamageGroup damageGroup = new DamageGroup();
                                damageGroup.Target = pTechno;
                                damageGroup.Distance = dist;
                                stands.Add(damageGroup);
                            }
                        }
                    }
                }
                return false;
            });

            foreach (DamageGroup damageGroup in stands)
            {
                damageGroup.Target.Ref.Base.ReceiveDamage(damage, (int)damageGroup.Distance, pWH, pAttacker, false, false, pAttackingHouse);
            }
        }

    }

}

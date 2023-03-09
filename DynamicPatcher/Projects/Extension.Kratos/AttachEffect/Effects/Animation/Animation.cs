using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class AttachEffect
    {
        public Animation Animation;

        private void InitAnimation()
        {
            this.Animation = AEData.AnimationData.CreateEffect<Animation>();
            RegisterEffect(Animation);
        }
    }


    [Serializable]
    public class Animation : Effect<AnimationData>
    {
        private AnimExt animExt;
        private Pointer<AnimClass> pAnim
        {
            get
            {
                if (null == animExt)
                {
                    return IntPtr.Zero;
                }
                return animExt.OwnerObject;
            }
        }
        private BlitterFlags animFlags;

        private bool OnwerIsDead;
        private bool OnwerIsCloakable;

        public Animation() : base()
        {
            this.OnwerIsDead = false;
        }

        public override void OnEnable()
        {
            // 激活动画
            // Logger.Log("效果激活，播放激活动画{0}", Data.ActiveAnim);
            if (null != Data.ActiveAnim)
            {
                Pointer<AnimTypeClass> pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.ActiveAnim.Type);
                if (!pAnimType.IsNull)
                {
                    // 获取激活动画的位置
                    OffsetData offsetData = Data.ActiveAnim.Offset;
                    LocationMark locationMark = pOwner.GetRelativeLocation(offsetData);
                    Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, locationMark.Location);
                    pAnim.Ref.SetOwnerObject(pOwner);
                    pAnim.SetAnimOwner(pOwner);
                }
            }
            // 持续动画
            CreateIdleAnim();
        }

        private void CreateIdleAnim(bool fource = false, CoordStruct location = default)
        {
            if (!pAnim.IsNull)
            {
                // Logger.Log($"{Game.CurrentFrame} AE[{AE.AEData.Name}]持续动画[{Data.IdleAnim}]已存在，清除再重新创建");
                KillIdleAnim();
            }
            if (!fource && (null == Data.IdleAnim || pOwner.IsInvisible() || (Data.IdleAnim.RemoveInCloak && OnwerIsCloakable)))
            {
                // Logger.Log($"{Game.CurrentFrame} AE[{AE.AEData.Name}]持续动画[{Data.IdleAnim}]无法创建");
                return;
            }
            // 创建动画
            // Logger.Log($"{Game.CurrentFrame} AE[{AE.AEData.Name}]创建持续动画[{Data.IdleAnim}]");
            Pointer<AnimTypeClass> pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.IdleAnim.Type);
            if (!pAnimType.IsNull)
            {
                OffsetData offsetData = Data.IdleAnim.Offset;
                if (default == location)
                {
                    location = pOwner.GetRelativeLocation(offsetData).Location;
                }
                Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, location);
                // Logger.Log($"{Game.CurrentFrame} AE[{AEData.Name}]成功创建持续动画[{Data.IdleAnim}], 指针 {pAnim}");
                // pAnim.Ref.SetOwnerObject(pObject); // 当单位隐形时，附着在单位上的动画会被游戏注销
                // Logger.Log(" - 将动画{0}赋予对象", Data.IdleAnim);
                pAnim.Ref.Loops = 0xFF;
                // Logger.Log(" - 设置动画{0}的剩余迭代次数为{1}", Data.IdleAnim, 0xFF);
                pAnim.SetAnimOwner(pOwner);
                pAnim.Show(Data.IdleAnim.Visibility);
                this.animFlags = pAnim.Ref.AnimFlags; // 记录下动画的渲染参数

                this.animExt = AnimExt.ExtMap.Find(pAnim);
                // 设置动画的附着对象，由动画自身去位移
                AnimStatusScript status = animExt.GameObject.GetComponent<AnimStatusScript>();
                status.AttachOwner = AE.AEManager.Owner;
                status.AttachOffsetData = offsetData;
                // Logger.Log(" - 缓存动画{0}的实例对象指针", Data.IdleAnim);
            }
        }

        private void KillIdleAnim()
        {
            if (!pAnim.IsNull)
            {
                // 不将动画附着于单位上，动画就不会自行注销，需要手动注销
                pAnim.Ref.TimeToDie = true;
                pAnim.Ref.Base.UnInit(); // 包含了SetOwnerObject(0) 0x4255B0
                // Logger.Log("{0} - 已销毁动画{1}实例", Game.CurrentFrame, Data.IdleAnim);
                animExt = null;
                // Logger.Log("{0} - 成功移除持续动画{1}", Game.CurrentFrame, Data.IdleAnim);
            }
        }

        public void UpdateLocationOffset(CoordStruct offset)
        {
            if (!pAnim.IsNull && pAnim.TryGetStatus(out AnimStatusScript status))
            {
                // 调整动画的位置，加上偏移值
                status.Offset = offset;
            }
        }

        public override void OnDisable(CoordStruct location)
        {
            // Logger.Log($"{Game.CurrentFrame} AE[{AEData.Name}]效果结束，移除持续动画[{Data.IdleAnim}]");
            KillIdleAnim();
            // Logger.Log($"{Game.CurrentFrame} AE[{AEData.Name}]效果结束，播放结束动画[{Data.DoneAnim}]");
            // 结束动画
            if (null != Data.DoneAnim)
            {
                Pointer<AnimTypeClass> pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.DoneAnim.Type);
                if (!pAnimType.IsNull)
                {
                    // 获取结束动画的位置
                    if (!pOwner.IsNull)
                    {
                        OffsetData offsetData = Data.DoneAnim.Offset;
                        location = pOwner.GetRelativeLocation(offsetData).Location;
                    }
                    Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, location);
                    // pAnim.Ref.SetOwnerObject(pObject);
                }
            }
        }

        public override void OnPut(Pointer<CoordStruct> pCoord, DirType dirType)
        {
            // Logger.Log($"{Game.CurrentFrame}，单位[{AE.pOwner.Ref.Type.Ref.Base.ID}]{AE.pOwner}显现，创建持续动画{Data.IdleAnim}");
            CreateIdleAnim(true, pCoord.Ref);
        }

        public override void OnUpdate(CoordStruct location, bool isDead)
        {
            this.OnwerIsDead = isDead;
            if (!isDead && pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                // Logger.Log($"{Game.CurrentFrame} AE[{AE.AEData.Name}]附着单位 [{pOwner.Ref.Type.Ref.Base.ID}]{pOwner}  隐形 = {pTechno.Ref.Cloakable}，隐形状态 {pTechno.Ref.CloakStates}");
                switch (pTechno.Ref.CloakStates)
                {
                    case CloakStates.UnCloaked:
                        // 显形状态
                        if (OnwerIsCloakable)
                        {
                            OnwerIsCloakable = false;
                            if (Data.IdleAnim.RemoveInCloak)
                            {
                                CreateIdleAnim();
                            }
                            else if (Data.IdleAnim.TranslucentInCloak)
                            {
                                // 恢复不透明
                                pAnim.Ref.AnimFlags = animFlags;
                            }
                        }
                        break;
                    default:
                        // 进入隐形或处于正在隐\显形
                        if (!OnwerIsCloakable)
                        {
                            OnwerIsCloakable = true;
                            if (Data.IdleAnim.RemoveInCloak)
                            {
                                KillIdleAnim();
                            }
                            else if (Data.IdleAnim.TranslucentInCloak)
                            {
                                // 半透明
                                pAnim.Ref.AnimFlags |= BlitterFlags.TransLucent50;
                            }
                        }
                        break;
                }
            }
        }

        public override void OnRemove()
        {
            // Logger.Log($"{Game.CurrentFrame} 单位[{AE.pOwner.Ref.Type.Ref.Base.ID}]{AE.pOwner}隐藏，移除持续动画{Data.IdleAnim}");
            KillIdleAnim();
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            // Logger.Log("播放受击动画{0}", Data.HitAnim);
            // 受击动画
            if (null != Data.HitAnim)
            {
                Pointer<AnimTypeClass> pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.HitAnim.Type);
                if (!pAnimType.IsNull)
                {
                    // 获取受击动画的位置
                    OffsetData offsetData = Data.HitAnim.Offset;
                    LocationMark locationMark = pOwner.GetRelativeLocation(offsetData);
                    Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, locationMark.Location);

                    pAnim.SetAnimOwner(pOwner);
                    pAnim.Show(Data.HitAnim.Visibility);

                    this.animExt = AnimExt.ExtMap.Find(pAnim);
                    // 设置动画的附着对象，由动画自身去位移
                    AnimStatusScript status = animExt.GameObject.GetComponent<AnimStatusScript>();
                    status.AttachOwner = AE.AEManager.Owner;
                    status.AttachOffsetData = offsetData;
                }
            }
        }

        public override void OnReceiveDamageDestroy()
        {
            // 单位被杀死时，附着动画会自动remove，0x4A9770
            // Logger.Log("{0} 单位{1}被炸死，不移除持续动画{2}", Game.CurrentFrame, pOwner, Data.IdleAnim);
            // this.Disable(pOwner.Ref.Location);
            this.OnwerIsDead = true;
        }

    }
}
